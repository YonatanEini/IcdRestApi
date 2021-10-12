using System;
using Xunit;
using FakeItEasy;
using WebApplication1;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using DecodedIcd.Controllers.RequestHandlers;
using IcdFilesRestApi.Controllers.RequestsHandlers;

namespace RestApiUnitTest
{
    public class HttpRequstTest
    {
        [Fact]
        public async Task TestGetCommunicationNamesRequests()
        {
            var client = new TestClientProvider().Client;
            var respons = await client.GetAsync("/api/DecodedIcd");
            Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
        }
        [Fact]
        public async Task TestGetActiveRequests()
        {
            var client = new TestClientProvider().Client;
            var respons = await client.GetAsync("/api/DecodedIcd/requests");
            Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
        }
        [Fact]
        public async Task TestPostCancelDecodingRequest()
        {
            await TestGetActiveRequests();
            var client = new TestClientProvider().Client;
            var respons = await client.PostAsync("/api/DecodedIcd/CancelRequest", new StringContent(JsonConvert.SerializeObject(new DecodedMessageProperties() { CommunicationType = "F-15", DataDirection = EnumCommunicationType.Uplink, TransimisionRate = 5000 })
                , Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, respons.StatusCode);
        }
        [Fact]
        public async Task TestPostDecodingRequest()
        {
            var client = new TestClientProvider().Client;
            var respons = await client.PostAsync("/api/DecodedIcd", new StringContent(JsonConvert.SerializeObject(new DecodedMessageProperties() { CommunicationType = "F-15", DataDirection = EnumCommunicationType.Uplink, TransimisionRate = 5000 })
                ,Encoding.UTF8,"application/json"));
            Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
        }
    }
}
