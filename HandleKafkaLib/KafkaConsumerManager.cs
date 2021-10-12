using Confluent.Kafka;
using HandleKafkaLib;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.CosumersProperties;
using HandleKafkaLibrary.TopicsEnum;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace HandleKafkaLibrary
{
    public class KafkaConsumerManager
    {
        public Dictionary<KafkaTopicsEnum, List<ClientDataReceiverBase>> TopicClientsDict { get; set; } 
        public Dictionary<KafkaTopicsEnum, CancellationTokenSource> TopicCancellationTokenDict { get; set; }
        public List<KafkaTopicsEnum> ConsumersOnPending { get; set; }
        /// <summary>
        /// singleton
        /// </summary>
        private KafkaConsumerManager()
        {
            this.TopicClientsDict = new Dictionary<KafkaTopicsEnum, List<ClientDataReceiverBase>>();
            this.TopicCancellationTokenDict = new Dictionary<KafkaTopicsEnum, CancellationTokenSource>();
        }
        private static KafkaConsumerManager _instance = null;
        public static KafkaConsumerManager GetInstance()
        {
            _instance = _instance ?? new KafkaConsumerManager();
            return _instance;
        }
        public void AddConsumer(ClientDataReceiverBase clientProperties, ConsumerConfig config)
        {
            foreach (var topic in clientProperties.ConsumerProperties.ConsumerTopic)
            {
                if (this.TopicClientsDict.ContainsKey(topic)) //checks if there is a consumer on the topic already
                {
                    // adding to topic consumer list
                    this.TopicClientsDict[topic].Add(clientProperties);
                }
                else
                {
                    // creating new topic listener
                    List<ClientDataReceiverBase> clients = new List<ClientDataReceiverBase>() { clientProperties };
                    this.TopicClientsDict.Add(topic, new List<ClientDataReceiverBase>(clients));
                    if(!this.TopicCancellationTokenDict.ContainsKey(topic))
                    {
                        this.TopicCancellationTokenDict.Add(topic, new CancellationTokenSource()); //to stay with the producer cancellation token
                    }
                    KafkaConsumerCreator consumer = new KafkaConsumerCreator(config);//creating kafka consumer Task
                    consumer.ConsumeFromKafkaTopicAsync(topic);
                }
            }
        }
        public void CancelKafkaConsumer(string topic)
        {
            KafkaTopicsEnum enumTopic;
            KafkaTopicsEnum.TryParse(topic, out enumTopic);
            if(this.TopicCancellationTokenDict.ContainsKey(enumTopic))
            {
                this.TopicCancellationTokenDict[enumTopic].Cancel();
                this.TopicCancellationTokenDict.Remove(enumTopic);
            }
        }
        public void AddCancellationToken(string topic, CancellationTokenSource token)
        {
            KafkaTopicsEnum kafkaEnumTopic;
            KafkaTopicsEnum.TryParse(topic, out kafkaEnumTopic);
            if(this.TopicCancellationTokenDict.ContainsKey(kafkaEnumTopic))
            {
                this.TopicCancellationTokenDict[kafkaEnumTopic] = token; //update to producer cancellationToken
            }
            else
            {
                this.TopicCancellationTokenDict.Add(kafkaEnumTopic, token);
            }
        }
        
    }
}
