using Confluent.Kafka;
using HandleIcdLibrary;
using HandleKafkaLibrary;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.CosumersProperties;
using KafkaNet;
using KafkaNet.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLib
{
    public class KafkaProducer
    {
        private readonly ProducerConfig _config;
        public KafkaProducer(ProducerConfig config)
        {
            this._config = config;
        }
        /// <summary>
        /// singleton
        /// </summary>
        private static KafkaProducer _instance = null;
        public static KafkaProducer GetInstance(ProducerConfig config)
        {
            _instance = _instance ?? new KafkaProducer(config);
            return _instance;
        }
        /// <summary>
        /// producing decodedFrame to kafka topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="frame"></param>
        /// 
        public async Task WriteToKafkaAsync(string topic, DecodedFrameDto frame, CancellationToken token)
        {
            using (var producer = new ProducerBuilder<Null, string>(_config).Build())
            {
                try
                {
                    token.ThrowIfCancellationRequested(); 
                    string decodedFrameJson = JsonConvert.SerializeObject(frame); //convert decoded frame to JSON
                    var deliveryReport = await producer.ProduceAsync(topic, new Message<Null, string> { Value = decodedFrameJson }); //produce to kafka
                    if (deliveryReport.Status == PersistenceStatus.Persisted)
                    {
                        Console.WriteLine($"KAFKA => Delivered '{deliveryReport.Value}' to '{deliveryReport.Topic}'"); //data send successfuly tp kafka
                        producer.Flush(TimeSpan.FromSeconds(10));
                    }
                }
                catch (Exception e) //Cancellation token or kafka message Time-Out
                {
                    Console.WriteLine($"ENABLE TO SEND DATA TO KAFKA ON TOPIC {topic}");
                }
            }
        }
    }
}
