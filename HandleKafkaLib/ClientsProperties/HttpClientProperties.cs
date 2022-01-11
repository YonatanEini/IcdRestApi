using HandleIcdLibrary;
using HandleKafkaLibrary.TopicsEnum;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HandleKafkaLibrary.CosumersProperties
{
    /// <summary>
    /// http client properties
    /// </summary>
    public class HttpClientProperties : ClientPropertiesBase
    {
        public string ApiName { get; set; }
        public HttpClientProperties() : base() 
        {
            this.ApiName = " ";
        }
        public HttpClientProperties(int port, string ip, List<KafkaTopicsEnum> topic, string apiName) : base(port, ip, topic) 
        {
            this.ApiName = apiName;
        }
    }
}
