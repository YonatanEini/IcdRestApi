using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WebApplication1;

namespace UnittestRestApi
{
    public class TestClientProvider
    {
        public HttpClient Client { get; set; }
        public TestClientProvider()
        {
            var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            this.Client = server.CreateClient();
        }
    }
}
