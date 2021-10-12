using HandleIcdLibrary;
using HandleKafkaLibrary.TopicsEnum;
using System;
using System.Collections.Generic;

namespace HandleKafkaLibrary.CosumersProperties
{
    class HttpClientProperties : ClientPropertiesBase
    {
        public HttpClientProperties() : base() {; }
        public HttpClientProperties(int port, string ip, List<KafkaTopicsEnum> topic) : base(port, ip, topic) {; }
    }
}
