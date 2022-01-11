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
using HandleKafkaLibrary.ClientDataReceivers;
using System.Net;
using HandleKafkaLibrary.ClientsProperties;

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
        public DecodedIcdController(IOptions<IcdDataInitialiazer> filesPath, ProducerConfig producerConfig, 
            ConsumerConfig consumerConfig)
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
                //database is empty
                Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable; 
            return this.CommuncationIcdInitialize.CommunicationsIcdDict.Keys;
        }
        /// <summary>
        /// returns the cancelled request list
        /// </summary>
        /// <returns></returns>
        [HttpGet("CancelledRequests")]
        // GET api/DecodedIcd/CancelledRequests
        public IEnumerable<string> GetCancelledRequests()
        {
            KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
            List<string> CloneDiscriptionList = new List<string>(consumerManager.CancelledRequestDiscriptionList);
            if(CloneDiscriptionList.Count == 0)
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            consumerManager.CancelledRequestDiscriptionList.Clear();
            return CloneDiscriptionList;
        }
        // GET api/DecodedIcd/requests
        [HttpGet("requests")]
        public IEnumerable<DecodedMessagePropertiesDto> GetActiveRequest()
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
            if(ModelState.IsValid && 
                this.HandleClientRequests.CheckClientMessageProperties(CommuncationIcdInitialize, decodedMessageProps))
            {
                Task.Factory.StartNew(() => this.HandleClientRequests.ProduceDecodedMessageAsync(decodedMessageProps, 
                    this.CommuncationIcdInitialize, _IcdInitilailizer, this._kafkaProducerconfig));
                Response.StatusCode = ((int)HttpStatusCode.Accepted);
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.BadRequest);
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
                //checking mongodb connection
                if (mongodbConsumer.CheckMongoDBconnection()) 
                {
                    consumerManager.AddConsumer(mongodbConsumer, this._kafkaConsuemerConfig);
                    Response.StatusCode = (int)HttpStatusCode.Created;
                }
                else
                {
                    Response.StatusCode = ((int)HttpStatusCode.BadRequest);
                }
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.BadRequest);
            } 
        }
        /// <summary>
        /// creates Udp client Consumer
        /// </summary>
        /// <param name="properties"></param>
        [HttpPost("UdpClientRequest")]
        public void Post([FromBody] SocketClientsProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                UdpProtocolClient udpClient = new UdpProtocolClient(properties);
                if(udpClient.CheckUdpConection())
                {
                    consumerManager.AddConsumer(udpClient, this._kafkaConsuemerConfig);
                    Response.StatusCode = ((int)HttpStatusCode.Created);
                }
                else
                {
                    Response.StatusCode = ((int)HttpStatusCode.BadRequest);
                }
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.BadRequest);
            }
        }
        [HttpPost("TcpClientRequest")]
        public void TcpClientPostRequest([FromBody] SocketClientsProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                TcpProtocolClient tcpClient = new TcpProtocolClient(properties);
                if (tcpClient.ConnectToServer())
                {
                    consumerManager.AddConsumer(tcpClient, this._kafkaConsuemerConfig);
                    Response.StatusCode = ((int)HttpStatusCode.Created);
                }
                else
                {
                    Response.StatusCode = ((int)HttpStatusCode.BadRequest);
                }
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.BadRequest);
            }
        }
        [HttpPost("WebSocketClientRequest")]
        public void WebSocketClientPostRequest([FromBody] SocketClientsProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                WebSocketClient webClient = new WebSocketClient(properties);
                if (webClient.ConnectToServer())
                {
                    consumerManager.AddConsumer(webClient, this._kafkaConsuemerConfig);
                    Response.StatusCode = ((int)HttpStatusCode.Created);
                }
                else
                {
                    Response.StatusCode = ((int)HttpStatusCode.BadRequest);
                }
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.BadRequest);
            }
        }
        [HttpPost("HttpClientRequest")]
        public void Post([FromBody] HttpClientProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                MultipartContentHttpClient httpClient = new MultipartContentHttpClient(properties);
                if (httpClient.CheckHttpProperties())
                {
                    consumerManager.AddConsumer(httpClient, this._kafkaConsuemerConfig);
                    Response.StatusCode = ((int)HttpStatusCode.Accepted);
                }
                else
                {
                    Response.StatusCode = ((int)HttpStatusCode.BadRequest);
                }
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.BadRequest);
            }
        }
        [HttpPost("HttpProtocolClientRequest")]
        public async Task HttpClientRequestPostAsync([FromBody] HttpClientProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                HttpProtocolClient httpClient = new HttpProtocolClient(properties);
                bool httpPropertiesValidation = await httpClient.CheckHttpPropertiesAsync();
                if (httpPropertiesValidation == true)
                {
                    consumerManager.AddConsumer(httpClient, this._kafkaConsuemerConfig);
                    Response.StatusCode = ((int)HttpStatusCode.Accepted);
                }
                else
                {
                    Response.StatusCode = ((int)HttpStatusCode.BadRequest);
                }
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.BadRequest);
            }
        }
        
        [HttpPost("SplunkClientRequest")]
        public void SplunkClientRequestPost([FromBody] SplunkClientProperties properties)
        {
            if (ModelState.IsValid)
            {
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                SplunkClient httpClient = new SplunkClient(properties);
                consumerManager.AddConsumer(httpClient, this._kafkaConsuemerConfig);
                Response.StatusCode = ((int)HttpStatusCode.Accepted);
                /*
                else
                {
                    Response.StatusCode = ((int)HttpStatusCode.BadRequest);
                }
                */
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.BadRequest);
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
                //assuming succeed
                Response.StatusCode = ((int)HttpStatusCode.Created); 
                var cancelProducerTask = Parallel.ForEach(CancelledRequests, communicationName =>
                {
                    KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                    if (!this.HandleClientRequests.StopRequest(communicationName))
                    {
                        Response.StatusCode = ((int)HttpStatusCode.FailedDependency);
                    }
                });
                if(!cancelProducerTask.IsCompleted)
                {
                    Response.StatusCode = ((int)HttpStatusCode.RequestTimeout);
                }
            }
            else
            {
                Response.StatusCode = ((int)HttpStatusCode.FailedDependency);
            }
        }
        // POST api/DecodedIcd/CancelMongoClient
        [HttpPost("CancelMongoClient")]
        public void CancelMongoClient([FromBody] MongodbClientProperties properties)
        {
           if(ModelState.IsValid)
           {
                Response.StatusCode =(int)this.HandleClientRequests.SearchClientProperties(properties);
           }
           else
           {
                Response.StatusCode = (int)HttpStatusCode.FailedDependency;
           }
        }
        // POST api/DecodedIcd/CancelUdpClient
        [HttpPost("CancelUdpClient")]
        public void CancelUdpClient([FromBody] SocketClientsProperties properties)
        {
            if (ModelState.IsValid)
            {
                Response.StatusCode = (int)this.HandleClientRequests.SearchClientProperties(properties);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.FailedDependency;
            }
        }
        [HttpPost("CancelHttpClient")]
        public void CancelHttpClient([FromBody] HttpClientProperties properties)
        {
            if (ModelState.IsValid)
            {
                Response.StatusCode = (int)this.HandleClientRequests.SearchClientProperties(properties);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.FailedDependency;
            }
        }
        // DELETE api/DecodedIcd/{communcationName}
        [HttpDelete("{communcationName}")]
        public void Delete(string communcationName)
        {
            if (this.CommuncationIcdInitialize.CommunicationsIcdDict.ContainsKey(communcationName))
            {
                this.CommuncationIcdInitialize.CommunicationsIcdDict.Remove(communcationName);
                Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
      
    }
}
