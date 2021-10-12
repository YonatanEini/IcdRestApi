using DecodedIcd.Controllers.RequestHandlers;
using HandleKafkaLib.CosumersProperties;
using HandleKafkaLibrary.CosumersProperties;
using HandleKafkaLibrary.TopicsEnum;
using IcdFilesRestApi.Controllers.RequestsHandlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
//using Xunit;

namespace UnittestRestApi
{
    public class HttpRequestTest
    {
        private HttpClient _client;
        public HttpRequestTest()
        {
            this._client = new TestClientProvider().Client;
        }
        //[Fact]
        public async Task TestGetCommunicationNamesRequests()
        {
            var respons = await this._client.GetAsync("/api/decodedIcd");
            //  Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
        }
        //[Fact]

        public async Task TestGetActiveRequests()
        {
            var respons = await this._client.GetAsync("/api/decodedIcd/requests");
            // Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
        }
        //[Fact]
        public async Task TestPostCancelDecodingRequest()
        {
            var respons = await this._client.PostAsync("/api/decodedIcd/CancelRequest", new StringContent(JsonConvert.SerializeObject(new DecodedMessagePropertiesDTO() { CommunicationType = "F-15", DataDirection = EnumCommunicationType.Uplink, TransmissionRate = 5000 })
                , Encoding.UTF8, "application/json"));
            //Assert.Equal(HttpStatusCode.BadRequest, respons.StatusCode);
        }
        //  [Fact]
        public async Task TestPostDecodingRequest()
        {
            var respons = await this._client.PostAsync("/api/decodedIcd", new StringContent(JsonConvert.SerializeObject(new DecodedMessagePropertiesDTO() { CommunicationType = "F-15", DataDirection = EnumCommunicationType.Uplink, TransmissionRate = 5000 })
                , Encoding.UTF8, "application/json"));
            //   Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
        }
        //  [Fact]
        public async Task TestPostMongoDBconsumerRequest()
        {

            List<KafkaTopicsEnum> topics = new List<KafkaTopicsEnum>() { KafkaTopicsEnum.LandingBox};
            var respons = await this._client.PostAsync("/api/decodedIcd/ClientRequest", new StringContent(JsonConvert.SerializeObject(new MongodbClientProperties() { Port = 27017, Ip = "127.0.0.1", DataBaseName = "DecodedFrameDB", CollectionName = "FramesCollection", ConsumerTopic = topics })
                , Encoding.UTF8, "application/json"));
            //  Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
        }
        //   [Fact]
        public async Task TestPostUdpconsumerRequest()
        {
            List<KafkaTopicsEnum> topics = new List<KafkaTopicsEnum>() { KafkaTopicsEnum.FiberBoxDown };
            var respons = await this._client.PostAsync("/api/decodedIcd/UdpClientRequest", new StringContent(JsonConvert.SerializeObject(new UdpClientProperties() { Port = 11000, Ip = "127.0.0.1", ConsumerTopic = topics })
                , Encoding.UTF8, "application/json"));
            //   Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
        }
        //   [Fact]
        public async Task TestPostCancelMongoConsumerRequest()
        {
            List<KafkaTopicsEnum> topics = new List<KafkaTopicsEnum>() { KafkaTopicsEnum.LandingBox };
            var respons = await this._client.PostAsync("/api/decodedIcd/CancelMongoClient", new StringContent(JsonConvert.SerializeObject(new MongodbClientProperties() { Port = 27017, Ip = "127.0.0.1", DataBaseName = "DecodedFrameDB", CollectionName = "FramesCollection", ConsumerTopic = topics })
                , Encoding.UTF8, "application/json"));
            //    Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
        }
        //  [Fact]
        public async Task TestPostCancelUdpConsumerRequest()
        {
            List<KafkaTopicsEnum> topics = new List<KafkaTopicsEnum>() { KafkaTopicsEnum.FiberBoxDown };
            var respons = await this._client.PostAsync("/api/decodedIcd/UdpClientRequest", new StringContent(JsonConvert.SerializeObject(new UdpClientProperties() { Port = 11000, Ip = "127.0.0.1", ConsumerTopic = topics })
                , Encoding.UTF8, "application/json"));
            //  Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
        }
    }
}


