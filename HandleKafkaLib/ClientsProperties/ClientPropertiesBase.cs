using HandleIcdLibrary;
using HandleKafkaLibrary.TopicsEnum;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace HandleKafkaLibrary.CosumersProperties
{
    /// <summary>
    /// base class for kafka clients properties
    /// </summary>
    [JsonConverter(typeof(ClientPropertiesBase))]
    public abstract class ClientPropertiesBase
    {
        [Required(ErrorMessage = "port is required")]
        public int Port { get; set; }
        [Required(ErrorMessage = "Ip is required")]
        public string Ip { get; set; }
        [Required(ErrorMessage = "Consumer topic is required")]
        public IEnumerable<KafkaTopicsEnum> ConsumerTopic { get; set; } 
        protected ClientPropertiesBase()
        {
            this.Port = -1;
            this.Ip = "";
            this.ConsumerTopic = new List<KafkaTopicsEnum>();
        }
        protected ClientPropertiesBase(int port, string ip, List<KafkaTopicsEnum> topic)
        {
            this.Port = port;
            this.Ip = ip;
            this.ConsumerTopic = topic;
        }
    }
}
