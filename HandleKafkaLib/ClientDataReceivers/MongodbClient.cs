using HandleIcdLibrary;
using HandleKafkaLib.CosumersProperties;
using HandleKafkaLibrary.CosumersProperties;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary.ClientConsumers
{
    /// <summary>
    /// mongodb client consumer - inserts decoded frames into DB.
    /// </summary>
    public class MongodbClient : ClientDataReceiverBase
    {
        public MongoClient MongoClient { get; set; }
        public IMongoDatabase ClientDataBase { get; set; }
        public IMongoCollection<BsonDocument> ClientMongoCollection { get; set; }
        public MongodbClient() :base()
        {
            this.MongoClient = null;
            this.ClientDataBase = null;
            this.ClientMongoCollection = null;
        }
        public MongodbClient(ClientPropertiesBase consumerProperties) : base(consumerProperties)
        {
             MongodbClientProperties mongoProperties = base.ConsumerProperties as MongodbClientProperties;
             {
                if(mongoProperties != null)
                {
                    string MongoClientConnectionString = "mongodb://" + base.ConsumerProperties.Ip + ":" + base.ConsumerProperties.Port;
                    this.MongoClient = new MongoClient(MongoClientConnectionString); //connecting to mongoDB 
                    this.ClientDataBase = this.MongoClient.GetDatabase(mongoProperties.DataBaseName); //client DateBase
                    this.ClientMongoCollection = this.ClientDataBase.GetCollection<BsonDocument>(mongoProperties.CollectionName); //client collection
                }
             }
        }
        public override async Task ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token)
        {
            if (base.ConsumerProperties is MongodbClientProperties && !token.IsCancellationRequested)
            {
                var bsonDocument = decodedFrame.ToBsonDocument();
                await this.ClientMongoCollection.InsertOneAsync(bsonDocument, token);
                Console.WriteLine($"Data => Arrived To {this.GetType()}");
            }
        }
        public override bool CompareProperties(ClientPropertiesBase properties)
        {
            if (base.CompareProperties(properties) && base.ConsumerProperties is MongodbClientProperties && properties is MongodbClientProperties)
            {
                MongodbClientProperties clientProperties = (MongodbClientProperties)base.ConsumerProperties;
                MongodbClientProperties otherClientProps = (MongodbClientProperties)properties;
                return clientProperties.DataBaseName == otherClientProps.DataBaseName && clientProperties.CollectionName == otherClientProps.CollectionName;
            }
            return false;
        }
    }
}
