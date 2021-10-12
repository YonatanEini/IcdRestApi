using HandleIcdLibrary;
using HandleKafkaLibrary.CosumersProperties;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary.ClientConsumers
{
    /// <summary>
    /// Base class for consumers that consume data from kafkaConsumerManager
    /// </summary>
    public abstract class ClientDataReceiverBase : IReceiveData
    {
        public ClientPropertiesBase ConsumerProperties { get; set; }
        protected ClientDataReceiverBase()
        {
            this.ConsumerProperties = null;
        }
        protected ClientDataReceiverBase(ClientPropertiesBase consumerProperties)
        {
            this.ConsumerProperties = consumerProperties;
        }
        public abstract Task ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token);
        public virtual bool CompareProperties(ClientPropertiesBase properties) 
        {
            return this.ConsumerProperties.Port == properties.Port && this.ConsumerProperties.Ip == properties.Ip && this.ConsumerProperties.ConsumerTopic.All(properties.ConsumerTopic.Contains);
        }
    }
}
