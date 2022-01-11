using HandleIcdLibrary;
using HandleKafkaLibrary.CosumersProperties;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace HandleKafkaLibrary.ClientConsumers
{
    /// <summary>
    /// Udp client  - receives data from kafkaConsumerManager
    /// creates the udp connection only once
    /// </summary>
    public class UdpProtocolClient : ClientDataReceiverBase
    {
        public UdpClient UdpConsumerClient { get; set; } 
        public UdpProtocolClient() : base()
        {
            this.UdpConsumerClient = null;
        }
        public UdpProtocolClient(ClientPropertiesBase udpProperties) : base(udpProperties)
        {
            this.UdpConsumerClient = new UdpClient();

        }
        public override async Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token) 
        {
            if (!token.IsCancellationRequested)
            {
                string decodedFrameJson = JsonConvert.SerializeObject(decodedFrame);
                byte[] data = Encoding.ASCII.GetBytes(decodedFrameJson);
                await this.UdpConsumerClient.SendAsync(data, data.Length);
                Console.WriteLine($"Data => Arrived To {this.GetType()}");
                return true;
            }
            return false;
        }
        public bool CheckUdpConection()
        {
            try
            {
                // endpoint where Udp Client is
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(base.ClientProperties.Ip), base.ClientProperties.Port); 
                this.UdpConsumerClient.Connect(endPoint);
                return true;
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("the udp connection couldn't be establish");
            }
            catch (ArgumentNullException nullException)
            {
                Console.WriteLine("udp client was null");
            }
            catch (ArgumentOutOfRangeException rangeException)
            {
                Console.WriteLine("ip address or port number isn't valid");
            }
            catch (FormatException formatExeption)
            {
                Console.WriteLine("ip address wasn't in the correct format");
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
            return "Udp client: " + base.ToString() + "is cancelled";
        }
    }
}
