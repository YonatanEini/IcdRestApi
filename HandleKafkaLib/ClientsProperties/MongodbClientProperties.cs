using HandleIcdLibrary;
using HandleKafkaLibrary.CosumersProperties;
using HandleKafkaLibrary.TopicsEnum;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HandleKafkaLib.CosumersProperties
{
    /// <summary>
    /// MongoDB Client properties 
    /// </summary>
    public class MongodbClientProperties:ClientPropertiesBase
    {
        [Required(ErrorMessage = "DB name is required")]
        public string DataBaseName { get; set; }
        [Required(ErrorMessage = "DB collection name is required")]
        public string CollectionName { get; set; }
        public MongodbClientProperties():base()
        {
            this.DataBaseName = "";
            this.CollectionName = "";
        }
        public MongodbClientProperties(string ip, int port, string DBName, string collectionName, 
            List<KafkaTopicsEnum> topic) :base(port, ip, topic)
        {
            this.DataBaseName = DBName;
            this.CollectionName = collectionName;
            this.ConsumerTopic = topic;
        }
    }
}
