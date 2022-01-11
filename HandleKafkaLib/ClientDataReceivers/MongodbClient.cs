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
    /// mongodb client  - inserts decoded frames from kafkaConsumerManager into DB.
    /// created the mongodb connection only once
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
            //checkes if the properties are mongodb properties
            MongodbClientProperties mongoProperties = base.ClientProperties as MongodbClientProperties;
            string MongoClientConnectionString = "mongodb://" + base.ClientProperties.Ip + ":" + base.ClientProperties.Port;
            //connecting to mongoDB
            this.MongoClient = new MongoClient(MongoClientConnectionString);
            this.ClientDataBase = this.MongoClient.GetDatabase(mongoProperties.DataBaseName);
            this.ClientMongoCollection = this.ClientDataBase.GetCollection<BsonDocument>(mongoProperties.CollectionName);
        }
        public override async Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                var bsonDocument = decodedFrame.ToBsonDocument();
                await this.ClientMongoCollection.InsertOneAsync(bsonDocument, token);
                Console.WriteLine($"Data => Arrived To {this.GetType()}");
                return true;
            }
            return false;
        }
        public bool CheckMongoDBconnection()
        {
            bool isMongoLive = this.ClientDataBase.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
            if (isMongoLive)
            {
                return true;
            }
            else
            {
                Console.WriteLine("Mongodb Client Failed -> cancelling the request");
                return false;
            }
        }
        public override bool CompareProperties(ClientPropertiesBase properties)
        {
            if (base.CompareProperties(properties) && base.ClientProperties is MongodbClientProperties && properties is MongodbClientProperties)
            {
                MongodbClientProperties clientProperties = (MongodbClientProperties)base.ClientProperties;
                MongodbClientProperties otherClientProps = (MongodbClientProperties)properties;
                return clientProperties.DataBaseName == otherClientProps.DataBaseName && clientProperties.CollectionName == otherClientProps.CollectionName;
            }
            return false;
        }
        public override string ToString()
        {
            //checkes if the properties are mongodb properties
            MongodbClientProperties mongoProperties = base.ClientProperties as MongodbClientProperties; 
            if (mongoProperties != null)
            {
                return "Mongodb client: " + base.ToString() + " data base Name: " + mongoProperties.DataBaseName + " collection Name: " + mongoProperties.CollectionName + "ic cancelled";
            }
            return base.ToString();
        }
    }
}
