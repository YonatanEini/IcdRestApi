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
        /// <summary>
        /// consume data from kafka by topic and sends it to the topic clients
        /// </summary>
        /// <param name="topic"></param>
        public async Task ConsumeFromKafkaTopicAsync(KafkaTopicsEnum topic)
        {
            KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
            //consumer-client list
            List<ClientDataReceiverBase> consumerClients = consumerManager.TopicClientsDict[topic]; 
            List<string> topics = new List<string>()
            {
                        topic + "-Uplink",
                        topic + "-Downlink"
            };
            using (var consumer = new ConsumerBuilder<Null, string>(_config).Build())
            {
                //to listen both uplink and downlink kafka topics
                consumer.Subscribe(topics);
                //while there are clients on the topic
                while (consumerClients.Count > 0) 
                {
                    try
                    {
                        // read data from kafka
                        var data = consumer.Consume(consumerManager.TopicCancellationTokenDict[topic].Token);
                        // KafkaData => DecodedFrameDto
                        DecodedFrameDto decodedFrame = JsonConvert.DeserializeObject<DecodedFrameDto>(data.Value);
                        //sending decoded frame to topic clients
                        Task sendDataTask = new Task(() => SendDataToClients(decodedFrame, topic)); 
                        sendDataTask.Start();
                    }
                    catch (ConsumeException)
                    {
                        Console.WriteLine("Consumer Error => can't consume data!");
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Cancellation Token Activated => consumer doesn't consume any data!");
                        await Task.Delay(1000);
                        //await until cancellation TOKEN isn't cancelled
                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("Token Cancelled after starting task => cancelling the task");
                    }
                }
                consumerManager.TopicClientsDict.Remove(topic);
                consumerManager.TopicCancellationTokenDict.Remove(topic);
            }
        }
        /// <summary>
        /// sends decodedFrame to the kafka topic clients
        /// </summary>
        /// <param name="decodedFrame"></param>
        /// <param name="topic"></param>
        public void SendDataToClients(DecodedFrameDto decodedFrame, KafkaTopicsEnum topic)
        {
            KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
            //checks if topic consumer is active
            if (consumerManager.TopicClientsDict.ContainsKey(topic) && 
                !consumerManager.TopicCancellationTokenDict[topic].Token.IsCancellationRequested) 
            {
                 //updated topic client list
                List<ClientDataReceiverBase> kafkaConsumerClients = consumerManager.TopicClientsDict[topic];
                Console.WriteLine("SENDING DATA TO CLIENTS =>");
                Parallel.ForEach(kafkaConsumerClients, async client =>
                {
                    //send the decodedFrame to the client {true => success, false => failed}
                    bool taskResult = await client.ReceiveDecodedFrameAsync(decodedFrame,
                        consumerManager.TopicCancellationTokenDict[topic].Token);
                    if (taskResult == false && kafkaConsumerClients.Contains(client))
                    {
                        kafkaConsumerClients.Remove(client);
                        consumerManager.CancelledRequestDiscriptionList.Add(client.ToString());
                    }
                });
            }
        }
    }
    
}
