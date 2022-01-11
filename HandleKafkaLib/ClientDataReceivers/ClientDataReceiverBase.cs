using HandleIcdLibrary;
using HandleKafkaLibrary.CosumersProperties;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary.ClientConsumers
{
    /// <summary>
    /// Base class for clients that receive data from kafkaConsumerManager
    /// </summary>
    public abstract class ClientDataReceiverBase : IReceiveData
    {
        public ClientPropertiesBase ClientProperties { get; set; }
        protected ClientDataReceiverBase()
        {
            this.ClientProperties = null;
        }
        protected ClientDataReceiverBase(ClientPropertiesBase consumerProperties)
        {
            this.ClientProperties = consumerProperties;
        }
        public abstract Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token);
        public virtual bool CompareProperties(ClientPropertiesBase properties) //with linq
        {
            return this.ClientProperties.Port == properties.Port && this.ClientProperties.Ip == properties.Ip 
                && this.ClientProperties.ConsumerTopic.All(properties.ConsumerTopic.Contains);
        }
        public override string ToString()
        {
            return "ip: "  + this.ClientProperties.Ip + "," + " port: " + this.ClientProperties.Port +
                "," + "topics: " + string.Join(" ", this.ClientProperties.ConsumerTopic) + ",";
        }
    }
}
