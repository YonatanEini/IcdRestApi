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
using System.Xml;

namespace HandleKafkaLibrary.ClientDataReceivers
{
    public class StringContentHttpClient : MultipartContentHttpClient
    {
        /// <summary>
        ///  http client that supportes StringContent  
        /// </summary>
        public StringContentHttpClient() : base()
        {
        }
        public StringContentHttpClient(ClientPropertiesBase httpProperties): base(httpProperties)
        {
        }
        public override async Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token)
        {
            if (this.IsAlive && !token.IsCancellationRequested)
            {
                var json = JsonConvert.SerializeObject(decodedFrame, Newtonsoft.Json.Formatting.Indented);
                var stringContent = new StringContent(json, Encoding.UTF32, "application/json");
                try
                {
                    //sending the http post request {HttpEndPoint: http api name}
                    var response = await base.HTTPclient.PostAsync("http://" + base.ClientProperties.Ip +
                        ":" + base.ClientProperties.Port +
                        "/pandasController", stringContent);
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
        public override string ToString()
        {
            return "Http Pandas Client: " + "," + base.ToString() + " is cancelled";
        }
    }
}

