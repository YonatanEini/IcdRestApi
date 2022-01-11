using HandleIcdLibrary;
using HandleKafkaLibrary.TopicsEnum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HandleKafkaLibrary.CosumersProperties
{
    /// <summary>
    /// Udp client properties
    /// </summary>
    public class SocketClientsProperties : ClientPropertiesBase
    {
        public SocketClientsProperties() : base() {;}
        public SocketClientsProperties(int port, string ip, List<KafkaTopicsEnum> topic) : base(port, ip, topic) {;}
    }
}
