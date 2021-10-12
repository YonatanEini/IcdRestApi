using Confluent.Kafka;
using HandleIcdLibrary;
using HandleKafkaLibrary;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.TopicsEnum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using KafkaNet;
using KafkaNet.Model;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLib
{
    public class KafkaConsumerCreator
    {
        private readonly ConsumerConfig _config;
        public KafkaConsumerCreator(ConsumerConfig config)
        {
            this._config = config;
        }
        public void ConsumeFromKafkaTopicAsync(KafkaTopicsEnum topic)
        {
            KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
            List<ClientDataReceiverBase> consumerClients = consumerManager.TopicClientsDict[topic]; //consumer-client list
            Task.Run(() => // consumer task
            {
                   List<string> topics = new List<string>()
                   {
                        topic + "-Uplink",
                        topic + "-Downlink"
                   };
                using (var consumer = new ConsumerBuilder<Null, string>(_config).Build())
                {
                    consumer.Subscribe(topics); //to listen both uplink and downlink kafka topics
                    while(consumerClients.Count > 0) //while there are clients on the topic
                    {
                        try
                        {
                            var data = consumer.Consume(consumerManager.TopicCancellationTokenDict[topic].Token); // read data from kafka
                            DecodedFrameDto decodedFrame = JsonConvert.DeserializeObject<DecodedFrameDto>(data.Value);// KafkaData => DecodedFrameDto                    
                            SendDataToClientsTask(consumerClients, decodedFrame, topic);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Kafka Conusmer On Topic: {topic} is close");
                            //await until cancellation TOKEN isn't cancelled
                        }
                    }
                }
                consumerManager.TopicClientsDict.Remove(topic);
                consumerManager.TopicCancellationTokenDict.Remove(topic);
            });
        }
        public void SendDataToClientsTask(List<ClientDataReceiverBase> consumerClients, DecodedFrameDto decodedFrame, KafkaTopicsEnum topic)
        {
            KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
            List<ClientDataReceiverBase> kafkaConsumerClients = consumerManager.TopicClientsDict[topic]; //consumer-client list
            Task.Run(() => //sending data to clients Task (could be long time)
            {
                Console.WriteLine("SENDING DATA TO CLIENTS =>");
                Parallel.ForEach(kafkaConsumerClients, async client => //every iteration is a Task
                {
                    await client.ReceiveDecodedFrameAsync(decodedFrame, consumerManager.TopicCancellationTokenDict[topic].Token); //send the decodedFrame to the client
                });
            }); //data sent to all clients
        }
    }
    
}
