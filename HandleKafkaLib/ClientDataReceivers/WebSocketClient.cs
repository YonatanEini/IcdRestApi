using HandleIcdLibrary;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.CosumersProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace HandleKafkaLibrary.ClientDataReceivers
{
    /// <summary>
    /// web socket client  - receives data from kafkaConsumerManager
    /// creates the web socket connection only once
    /// </summary>
    public class WebSocketClient : ClientDataReceiverBase
    {
        public WebSocket SocketClient { get; set; }
        public bool IsAlive { get; set; }
        public WebSocketClient() : base()
        {
            this.SocketClient = null;
            this.IsAlive = false;
        }
        public WebSocketClient(ClientPropertiesBase webSocketProperties) : base(webSocketProperties)
        {
            this.IsAlive = true;
        }
        public override async Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token)
        {
            if (this.IsAlive)
            {
                //checking if the WebSocketServer is up
                if (SocketClient.Ping() && !token.IsCancellationRequested)
                {
                    this.IsAlive = await Task.Factory.StartNew(() => SendDataTask(decodedFrame));
                    Console.WriteLine($"Data => Arrived To {this.GetType()}");
                    return true;
                }
                else
                {
                    Console.WriteLine("WebSocketServer isn't available => ending Connection");
                    this.IsAlive = false;
                }
            }
            return false;
        }
        public bool SendDataTask(DecodedFrameDto decodedFrame)
        {
            string decodedFrameJson = JsonConvert.SerializeObject(decodedFrame);
            byte[] data = Encoding.ASCII.GetBytes(decodedFrameJson);
            try
            {
                this.SocketClient.Send(decodedFrameJson);
                return true;
            }
            catch(WebSocketException)
            {
                Console.WriteLine("WebSocketServer isn't available => ending Connection");
                return false;
            }
        }
        public bool ConnectToServer()
        {
            try
            {
                // endpoint where WebSocketServer is - Checking Poperties and Connecion to the server
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(base.ClientProperties.Ip), base.ClientProperties.Port);
                this.SocketClient = new WebSocket($"ws://{base.ClientProperties.Ip}:{base.ClientProperties.Port}/DecodedFrames");
                this.SocketClient.Connect();
                return this.SocketClient.Ping();
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("ip address or port number isn't valid");
            }
            catch (FormatException)
            {
                Console.WriteLine("ip address or port are not in the correct format");
            }
            return false;
        }
        public override bool CompareProperties(ClientPropertiesBase properties)
        {
            return base.ClientProperties is SocketClientsProperties && properties is SocketClientsProperties
                && base.CompareProperties(properties);
        }
        public override string ToString()
        {
            return "WebSocket client: " + base.ToString() + "is cancelled";
        }
    }
}
