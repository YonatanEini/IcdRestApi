using HandleIcdLibrary;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.CosumersProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary.ClientDataReceivers
{
    public class HttpProtocolClient : ClientDataReceiverBase
    {
        public HttpClient HttpProtocolclient { get; set; }
        public bool IsAlive { get; set; }
        public HttpProtocolClient()
        {
            this.HttpProtocolclient = null;
            this.IsAlive = true;
        }
        public HttpProtocolClient(ClientPropertiesBase httpProperties) : base(httpProperties)
        {
            this.IsAlive = true;
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            this.HttpProtocolclient = new HttpClient();
        }
        public override async Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token)
        {
            if (this.IsAlive && !token.IsCancellationRequested)
            {
                var json = JsonConvert.SerializeObject(decodedFrame, Formatting.Indented);
                var stringContent = new StringContent(json, Encoding.UTF32, "application/json");
                try
                {
                    //sending the http post request {HttpEndPoint: http api name}
                    var response = await this.HttpProtocolclient.PostAsync("http://" + base.ClientProperties.Ip +
                        ":" + base.ClientProperties.Port, stringContent);
                    Console.WriteLine($"Data => Arrived To {this.GetType()}");
                    return true;
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("Pandas HTTP client was null");
                    this.IsAlive = false;
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("unable to create pandas Http connection => invalid parameters or Http server is down");
                    this.IsAlive = false;
                }
            }
            return false;
        }
        public async Task<bool> CheckHttpPropertiesAsync()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(base.ClientProperties.Ip), base.ClientProperties.Port);
                await this.HttpProtocolclient.GetAsync("http://" + base.ClientProperties.Ip + ":" + base.ClientProperties.Port);
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
            catch (HttpRequestException)
            {
                Console.WriteLine("http end point doesn't exsist in this properties");
            }
            return false;
        }
        public override bool CompareProperties(ClientPropertiesBase properties)
        {
            return base.ClientProperties is HttpClientProperties && properties is HttpClientProperties
                && base.CompareProperties(properties);
        }
        public override string ToString()
        {
            return "Http Client: " + "," + base.ToString() + " is cancelled";
        }
    }
}
