using Microsoft.AspNetCore.Mvc.Testing;
using RESTFulSense.Clients;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication1;
using WebApplication1.Controllers.RequestHandlers;
using Xunit;

namespace IcdRestApi.Initest
{
    public class UnitTest1
    {
        private readonly HttpClient _httpClient;
        private readonly IRESTFulApiFactoryClient apiFactoryClient;
        private string relativeUrl = "api/DecodedIcd";
        public UnitTest1()
        {
            var appFactory = new WebApplicationFactory<Startup>();
            this._httpClient = appFactory.CreateClient();
            this.apiFactoryClient = new RESTFulApiFactoryClient(this._httpClient);
        }
        public async ValueTask<DecodedMessageProperties> PostMessageAsync(DecodedMessageProperties properties) =>
            await this.apiFactoryClient.PostContentAsync(relativeUrl, properties);
    }
}
