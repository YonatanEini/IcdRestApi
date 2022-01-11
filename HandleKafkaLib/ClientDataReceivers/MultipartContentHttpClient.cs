using HandleIcdLibrary;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.CosumersProperties;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary.ClientDataReceivers
{
    public class MultipartContentHttpClient : ClientDataReceiverBase
    {
        /// <summary>
        /// http client that supportes MultipartFormDataContent 
        /// </summary>
        public HttpClient HTTPclient { get; set; }
        public bool IsAlive { get; set; }
        public MultipartContentHttpClient()
        {
            this.HTTPclient = null;
            this.IsAlive = true;
        }
        public MultipartContentHttpClient(ClientPropertiesBase httpProperties) : base(httpProperties)
        {
            this.IsAlive = true;
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            this.HTTPclient = new HttpClient();
            if (base.ClientProperties.Ip == "127.0.0.1")
            {
                base.ClientProperties.Ip = "localhost";
            }
        }
        public override async Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token)
        { 
            if (this.IsAlive && !token.IsCancellationRequested)
            {
                //converting decoded frame to json format
                var json = JsonConvert.SerializeObject(decodedFrame, Formatting.Indented);
                var stringContent = new StringContent(json, Encoding.UTF32, "application/json");
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(stringContent);
                try
                {
                    //sending the http post request {HttpEndPoint: http api name}
                    var response = await this.HTTPclient.PostAsync("http://" + base.ClientProperties.Ip + 
                        ":" + base.ClientProperties.Port +
                        "/pandasController", content); 
                    Console.WriteLine($"Data => Arrived To {this.GetType()}");
                    return true;
                }
                catch(ArgumentNullException)
                {
                    Console.WriteLine("HTTP client was null");
                    this.IsAlive = false;
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("unable to create Http connection => invalid parameters or Http server is down");
                    this.IsAlive = false;
                }
            }
            return false;
        }
        public bool CheckHttpProperties()
        {
            string ipToCheck = base.ClientProperties.Ip;
            if (ipToCheck.Equals("localhost"))
                ipToCheck = "127.0.0.1";
            try
            {
                // to check http port and ip properties
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipToCheck), base.ClientProperties.Port); 
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("port or ip isn't on the correct range");
            }
            catch (FormatException)
            {
                Console.WriteLine("invalid port or ip");
            }
            return false;
        }
        public override bool CompareProperties(ClientPropertiesBase properties)
        {
            if(base.ClientProperties.Ip == "localhost")
            {
                base.ClientProperties.Ip = "127.0.0.1";
            }
            return base.ClientProperties is HttpClientProperties && properties is HttpClientProperties 
                && base.CompareProperties(properties);
        }
        public override string ToString()
        {
            return "Http Client: " + "," + base.ToString() + " is cancelled";
        }
    }
}
