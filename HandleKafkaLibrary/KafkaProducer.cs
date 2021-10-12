using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandleKafkaLibrary
{
    public class KafkaProducer
    {
        private ProducerConfig _config;
        public KafkaProducer(ProducerConfig config)
        {
            this._config = config;
        }
        //singelton
        public static KafkaProducer _instance = null;
        public static KafkaProducer GetInstance(ProducerConfig config)
        {
            _instance = _instance ?? new KafkaProducer(config);
            return _instance;
        }
        public async void Write(string topic, DecodedFrame frame)
        {
            string serializedDecodedFrame = JsonConvert.SerializeObject(frame);
            using (var producer = new ProducerBuilder<Null, string>(_config).Build())
            {
                await producer.ProduceAsync(topic, new Message<Null, string> { Value = serializedDecodedFrame });
                producer.Flush(TimeSpan.FromSeconds(10));
            }
        }
    }
}
