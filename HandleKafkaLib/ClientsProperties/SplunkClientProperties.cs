using HandleKafkaLibrary.CosumersProperties;
using HandleKafkaLibrary.TopicsEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace HandleKafkaLibrary.ClientsProperties
{
    public class SplunkClientProperties : ClientPropertiesBase
    {
        public string HttpEventCollectorToken { get; set; }
        public SplunkClientProperties() : base()
        {
            this.HttpEventCollectorToken = " ";
        }
        public SplunkClientProperties(string ipAdress, int portNumber, List<KafkaTopicsEnum> topics, string token) 
            : base(portNumber, ipAdress, topics)
        {
            this.HttpEventCollectorToken = token;
        }
    }
}
