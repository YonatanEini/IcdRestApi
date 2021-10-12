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
    /// client Udp properties
    /// </summary>
    public class UdpClientProperties:ClientPropertiesBase
    {
        public UdpClientProperties() : base() {;}
        public UdpClientProperties(int port, string ip, List<KafkaTopicsEnum> topic) : base(port, ip, topic) {;}
    }
}
