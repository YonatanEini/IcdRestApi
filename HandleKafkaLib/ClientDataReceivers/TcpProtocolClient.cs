using HandleIcdLibrary;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.CosumersProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary.ClientDataReceivers
{
    /// <summary>
    /// Tcp client  - receives data from kafkaConsumerManager and sends to the Tcp server
    /// creates the Tcp connection only once
    /// </summary>
    public class TcpProtocolClient : ClientDataReceiverBase
    {
        public TcpClient TcpConsumerClient { get; set; }
        public bool IsAlive { get; set; }
        public TcpProtocolClient() : base()
        {
            this.TcpConsumerClient = null;
            this.IsAlive = false;
        }
        public TcpProtocolClient(ClientPropertiesBase tcpProperties) : base(tcpProperties)
        {
            this.TcpConsumerClient = new TcpClient();
            this.IsAlive = true;
        }
        public override async Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token)
        {
            if(this.IsAlive)
            {
                //checking if the tcp server is up
                if (this.IsAlive && !token.IsCancellationRequested)
                {
                    string decodedFrameJson = JsonConvert.SerializeObject(decodedFrame);
                    byte[] data = Encoding.ASCII.GetBytes(decodedFrameJson);
                    //Sending Message Size In Bytes
                    await CheckTcpServerConnection(data.Length.ToString());
                    await this.TcpConsumerClient.GetStream().WriteAsync(data, 0, data.Length);
                    Console.WriteLine($"Data => Arrived To {this.GetType()}");
                    return true;
                }
            }
            return false;
        }
        public bool ConnectToServer()
        {
            try
            {
                // endpoint where Tcp server is
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(base.ClientProperties.Ip), base.ClientProperties.Port);
                this.TcpConsumerClient.Connect(endPoint);
                return true;
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("the tcp connection couldn't be establish");
            }
            catch (ArgumentNullException nullException)
            {
                Console.WriteLine("tcp client was null");
            }
            catch (ArgumentOutOfRangeException rangeException)
            {
                Console.WriteLine("ip address or port number isn't valid");
            }
            catch (FormatException formatExeption)
            {
                Console.WriteLine("ip address or port are not in the correct format");
            }
            return false;
        }
        public async Task<bool> CheckTcpServerConnection(string size)
        {
            try
            {
                byte[] BytesToSend = Encoding.ASCII.GetBytes(size);
                await this.TcpConsumerClient.GetStream().WriteAsync(BytesToSend, 0, BytesToSend.Length);
                return true;
            }
            catch (System.IO.IOException closedServer)
            {
                Console.WriteLine("unable to connect the tcp server => ending connection");
            }
            catch (InvalidOperationException invalidOperation)
            {
                Console.WriteLine("Cannot send ping to server => ending connection");
            }
            catch (ArgumentNullException argNull)
            {
                Console.WriteLine("Tcp client was null");
            }
            catch (ArgumentOutOfRangeException argRange)
            {
                Console.WriteLine("Tcp client properties are invalid");
            }
            this.IsAlive = false;
            return false;
        }
        public override bool CompareProperties(ClientPropertiesBase properties)
        {
            return base.ClientProperties is SocketClientsProperties && properties is SocketClientsProperties
                && base.CompareProperties(properties);
        }
        public override string ToString()
        {
            return "Tcp client: " + base.ToString() + "is cancelled";
        }
    }
}

