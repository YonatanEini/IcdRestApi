using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DecodedIcd.Controllers.RequestHandlers;
using IcdFilesRestApi;
using Confluent.Kafka;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading;
using HandleKafkaLib;
using HandleKafkaLib.CosumersProperties;
using HandleKafkaLibrary.CosumersProperties;
using HandleKafkaLibrary;

namespace DecodedIcd.Controllers
{

    [Route("api/[controller]")]
    public class DecodedIcdController : Controller
    {
        private CommuncationsIcdDict CommuncationIcdInitialize { get; set; }
        private HandleClientRequests HandleClientRequests { get; set; }
        private IcdDataInitialiazer _IcdInitilailizer;
        private ProducerConfig _config;
        public DecodedIcdController(IOptions<IcdDataInitialiazer> filesPath, ProducerConfig config)
        {
            this._IcdInitilailizer = filesPath.Value;
            this.HandleClientRequests = HandleClientRequests.GetInstance();
            this.CommuncationIcdInitialize = CommuncationsIcdDict.GetInstance(_IcdInitilailizer);
            this._config = config;
        }
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
        public List<DecodedMessagePropertiesDTO> GetActiveRequest()
        {
            return this.HandleClientRequests.ClientActiveRequest;
        }
        // POST api/DecodedIcd
        [HttpPost]
        public HttpResponseMessage Post([FromBody] DecodedMessagePropertiesDTO decodedMessageProps)
        {
            if(ModelState.IsValid && this.HandleClientRequests.CheckClientMessageProperties(CommuncationIcdInitialize, decodedMessageProps))
            {
                this.HandleClientRequests.WritingDecodedMessage(decodedMessageProps, this.CommuncationIcdInitialize, _IcdInitilailizer, this._config);
                Response.StatusCode = 201;
                return new HttpResponseMessage(HttpStatusCode.Created);
            }
            else
            {
                Response.StatusCode = 400;
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
        [HttpPost("ClientRequest")]
        public void Post([FromBody] MongodbClientProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                consumerManager.AddConsumer(properties);
                Response.StatusCode = 201;
            }
            else
            {
                Response.StatusCode = 400;
            } 
        }
        [HttpPost("UdpClientRequest")]
        public void Post([FromBody] UdpClientProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                consumerManager.AddConsumer(properties);
                Response.StatusCode = 201;
            }
            else
            {
                Response.StatusCode = 400;
            }
        }
        // POST api/DecodedIcd/CancelRequest
        [HttpPost("CancelRequest")] 
        public void Post([FromBody] List<string> CancelledRequests) //cancel producer
        {
            if (CancelledRequests != null)
            {
                Response.StatusCode = 201;
                foreach (var aircraftName in CancelledRequests)
                {
                    if (!this.HandleClientRequests.StopRequest(aircraftName))
                    {
                        Response.StatusCode = 400;
                        break;
                    }
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
                    if (consumerManager.TopicConsumersDict.ContainsKey(topic))
                    {
                        List<BasicClientProperties> topicKafkaListeners = consumerManager.TopicConsumersDict[topic];
                        foreach (var client in topicKafkaListeners)
                        {
                            if (client.Compare(properties))
                            {
                                consumerManager.TopicConsumersDict[topic].Remove(client);
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
                    if (consumerManager.TopicConsumersDict.ContainsKey(topic))
                    {
                        List<BasicClientProperties> topicKafkaListeners = consumerManager.TopicConsumersDict[topic];
                        foreach (var client in topicKafkaListeners)
                        {
                            if (client.Compare(properties))
                            {
                                consumerManager.TopicConsumersDict[topic].Remove(client);
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
        // PUT api/DecodedIcd/fileName
        [HttpPut("{fileName}")]
        public void Put(string fileName, [FromBody] string value)
        {
        }
    }
}
