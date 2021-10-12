using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DecodedIcd.Controllers.RequestHandlers;
using IcdFilesRestApi;
using Confluent.Kafka;
using HandleKafkaLib.CosumersProperties;
using HandleKafkaLibrary.CosumersProperties;
using HandleKafkaLibrary;
using HandleKafkaLibrary.ClientConsumers;
using System.Threading.Tasks;
using System.Threading;

namespace DecodedIcd.Controllers
{

    [Route("api/[controller]")]
    public class DecodedIcdController : Controller
    {
        private CommuncationsIcdDict CommuncationIcdInitialize { get; set; }
        private HandleClientRequests HandleClientRequests { get; set; }
        private readonly IcdDataInitialiazer _IcdInitilailizer;
        private readonly ProducerConfig _kafkaProducerconfig;
        private readonly ConsumerConfig _kafkaConsuemerConfig;
        public DecodedIcdController(IOptions<IcdDataInitialiazer> filesPath, ProducerConfig producerConfig, ConsumerConfig consumerConfig)
        {
            this._IcdInitilailizer = filesPath.Value;
            this.HandleClientRequests = HandleClientRequests.GetInstance();
            this.CommuncationIcdInitialize = CommuncationsIcdDict.GetInstance(_IcdInitilailizer);
            this._kafkaProducerconfig = producerConfig;
            this._kafkaConsuemerConfig = consumerConfig;
        }
        /// <summary>
        /// returns all comunication types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        // GET api/DecodedIcd
        public IEnumerable<string> Get()
        {
            if (this.CommuncationIcdInitialize.CommunicationsIcdDict.Keys == null)
                Response.StatusCode = 500; //database is empty
            return this.CommuncationIcdInitialize.CommunicationsIcdDict.Keys;
        }
        // GET api/DecodedIcd/requests
        [HttpGet("requests")]
        public List<DecodedMessagePropertiesDto> GetActiveRequest()
        {
            return this.HandleClientRequests.ClientActiveRequest;
        }
        /// <summary>
        /// client Http Producer Request
        /// </summary>
        /// <param name="decodedMessageProps"></param>
        /// <returns></returns>
        // POST api/DecodedIcd
        [HttpPost]
        public void ProducerRequestPost([FromBody] DecodedMessagePropertiesDto decodedMessageProps)
        {
            if(ModelState.IsValid && this.HandleClientRequests.CheckClientMessageProperties(CommuncationIcdInitialize, decodedMessageProps))
            {
                this.HandleClientRequests.ProduceDecodedMessageTask(decodedMessageProps, this.CommuncationIcdInitialize, _IcdInitilailizer, this._kafkaProducerconfig);
                Response.StatusCode = 201;
            }
            else
            {
                Response.StatusCode = 400;
            }
        }
        /// <summary>
        /// creates MongoDB client consumer
        /// </summary>
        /// <param name="properties"></param>
        [HttpPost("MongoClientRequest")]
        public void Post([FromBody] MongodbClientProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                MongodbClient mongodbConsumer = new MongodbClient(properties);
                consumerManager.AddConsumer(mongodbConsumer, this._kafkaConsuemerConfig);
                Response.StatusCode = 201;
            }
            else
            {
                Response.StatusCode = 400;
            } 
        }
        /// <summary>
        /// creates Udp client Consumer
        /// </summary>
        /// <param name="properties"></param>
        [HttpPost("UdpClientRequest")]
        public void Post([FromBody] UdpClientProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                UdpClient udpClient = new UdpClient(properties);
                consumerManager.AddConsumer(udpClient, this._kafkaConsuemerConfig);
                Response.StatusCode = 201;
            }
            else
            {
                Response.StatusCode = 400;
            }
        }
        /// <summary>
        /// cancel producer http request
        /// </summary>
        /// <param name="CancelledRequests"></param>
        // POST api/DecodedIcd/CancelRequest
        [HttpPost("CancelRequest")] 
        public void Post([FromBody] List<string> CancelledRequests) 
        {
            if (CancelledRequests != null)
            {
                Response.StatusCode = 201; //assuming succeed
                var cancelProducerTask = Parallel.ForEach(CancelledRequests, communicationName =>
                {
                    KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                    if (!this.HandleClientRequests.StopRequest(communicationName))
                    { 
                        Response.StatusCode = 400;
                    }
                });
                if(!cancelProducerTask.IsCompleted)
                {
                    Response.StatusCode = 400;
                }
            }
            else
            {
                Response.StatusCode = 400;
            }
        }
        // POST api/DecodedIcd/CancelMongoClient
        [HttpPost("CancelMongoClient")]
        public void CancelMongoClient([FromBody] MongodbClientProperties properties)
        {
           Response.StatusCode = 400; //assuming cancel failed
           if(ModelState.IsValid)
           {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                foreach (var topic in properties.ConsumerTopic)
                {
                    if (consumerManager.TopicClientsDict.ContainsKey(topic))
                    {
                        List<ClientDataReceiverBase> topicKafkaListeners = consumerManager.TopicClientsDict[topic];
                        foreach (var client in topicKafkaListeners)
                        {
                            if (client.CompareProperties(properties))
                            {
                                consumerManager.TopicClientsDict[topic].Remove(client);
                                Response.StatusCode = 201;
                                break;
                            }
                        }
                    }
                }
           }
        }
        // POST api/DecodedIcd/CancelUdpClient
        [HttpPost("CancelUdpClient")]
        public void CancelUdpClient([FromBody] UdpClientProperties properties)
        {
            Response.StatusCode = 400; //assuming cancel failed
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                foreach (var topic in properties.ConsumerTopic)
                {
                    if (consumerManager.TopicClientsDict.ContainsKey(topic))
                    {
                        List<ClientDataReceiverBase> topicKafkaListeners = consumerManager.TopicClientsDict[topic];
                        foreach (var client in topicKafkaListeners)
                        {
                            if (client.CompareProperties(properties))
                            {
                                consumerManager.TopicClientsDict[topic].Remove(client);
                                Response.StatusCode = 201;
                                break;
                            }
                        }
                    }
                }
            }
        }
        // DELETE api/DecodedIcd/{communcationName}
        [HttpDelete("{communcationName}")]
        public void Delete(string communcationName)
        {
            if (this.CommuncationIcdInitialize.CommunicationsIcdDict.ContainsKey(communcationName))
            {
                this.CommuncationIcdInitialize.CommunicationsIcdDict.Remove(communcationName);
                Response.StatusCode = 204;
            }
            else
            {
                Response.StatusCode = 400;
            }
        }
      
    }
}
