using Confluent.Kafka;
using HandleKafkaLib;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.CosumersProperties;
using HandleKafkaLibrary.TopicsEnum;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary
{
    public class KafkaConsumerManager
    {
        // {LandingBox: udpClient, MongoClient, HttpClient}
        public Dictionary<KafkaTopicsEnum, List<ClientDataReceiverBase>> TopicClientsDict { get; set; }
        // {FlightBoxUp: Token}
        public Dictionary<KafkaTopicsEnum, CancellationTokenSource> TopicCancellationTokenDict { get; set; }
        //run-time cancelled requests
        public List<string> CancelledRequestDiscriptionList { get; set; }
        /// <summary>
        /// singleton
        /// </summary>
        private KafkaConsumerManager()
        {
            this.TopicClientsDict = new Dictionary<KafkaTopicsEnum, List<ClientDataReceiverBase>>();
            this.TopicCancellationTokenDict = new Dictionary<KafkaTopicsEnum, CancellationTokenSource>();
            this.CancelledRequestDiscriptionList = new List<string>();
        }
        private static KafkaConsumerManager _instance = null;
        public static KafkaConsumerManager GetInstance()
        {
            _instance = _instance ?? new KafkaConsumerManager();
            return _instance;
        }
        public void AddConsumer(ClientDataReceiverBase clientProperties, ConsumerConfig config)
        {
            foreach (var topic in clientProperties.ClientProperties.ConsumerTopic)
            {
                // checks if there is a topic consumer
                if (this.TopicClientsDict.ContainsKey(topic)) 
                {
                    // adding the client to topic consumer list
                    this.TopicClientsDict[topic].Add(clientProperties);
                }
                else
                {
                    // creating new topic consumer
                    List<ClientDataReceiverBase> clients = new List<ClientDataReceiverBase>() { clientProperties };
                    this.TopicClientsDict.Add(topic, clients);
                    if (!this.TopicCancellationTokenDict.ContainsKey(topic))  
                    {
                        //to stay with the producer cancellation token
                        this.TopicCancellationTokenDict.Add(topic, new CancellationTokenSource()); 
                    }
                    //starting kafka consumer Task
                    KafkaConsumerCreator consumer = new KafkaConsumerCreator(config);
                    Task.Factory.StartNew(() => consumer.ConsumeFromKafkaTopicAsync(topic));
                }
            }
        }
        public void AddTopicCancellationToken(string topic, CancellationTokenSource token)
        {
            KafkaTopicsEnum kafkaEnumTopic;
            KafkaTopicsEnum.TryParse(topic, out kafkaEnumTopic);
            if(this.TopicCancellationTokenDict.ContainsKey(kafkaEnumTopic))
            {
                //update to producer cancellationToken (consumer opened before the producer)
                this.TopicCancellationTokenDict[kafkaEnumTopic] = token; 
            }
            else
            {
                //create new CancellationToken (producer before the consumer)
                this.TopicCancellationTokenDict.Add(kafkaEnumTopic, token); 
            }
        }
        
    }
}
