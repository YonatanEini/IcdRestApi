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
    /// Udp client consumer - consume data from kafkaConsumerManager
    /// </summary>
    public class UdpClient : ClientDataReceiverBase
    {
        public System.Net.Sockets.UdpClient UDPclient { get; set; } 
        public UdpClient() : base()
        {
            this.UDPclient = null;
        }
        public UdpClient(ClientPropertiesBase udpProperties) : base(udpProperties)
        {
            if(base.ConsumerProperties is UdpClientProperties)
            {
                this.UDPclient = new System.Net.Sockets.UdpClient();
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(base.ConsumerProperties.Ip), base.ConsumerProperties.Port); // endpoint where Udp Client
                this.UDPclient.Connect(endPoint);
            }
        }
        public override async Task ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token) 
        {
            if (base.ConsumerProperties is UdpClientProperties && !token.IsCancellationRequested)
            {
                string decodedFrameJson = JsonConvert.SerializeObject(decodedFrame);
                byte[] data = Encoding.ASCII.GetBytes(decodedFrameJson);
                await this.UDPclient.SendAsync(data, data.Length);
                Console.WriteLine($"Data => Arrived To {this.GetType()}");
            }
        }
        public override bool CompareProperties(ClientPropertiesBase properties) 
        {
            return base.ConsumerProperties is UdpClientProperties && properties is UdpClientProperties && base.CompareProperties(properties);
        }
    }
}
