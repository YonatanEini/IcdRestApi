using HandleIcdLibrary;
using HandleKafkaLibrary.ClientConsumers;
using HandleKafkaLibrary.ClientsProperties;
using HandleKafkaLibrary.CosumersProperties;
using Newtonsoft.Json;
using Splunk.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary.ClientDataReceivers
{
    public class SplunkClient : ClientDataReceiverBase
    {
        public HttpEventCollectorSender SplunkEventEmitter { get; set; }
        public bool IsAlive { get; set; }
        public SplunkClient(ClientPropertiesBase splunkProperties) : base(splunkProperties)
        {
            //converting base class properties to splunk class properties
            SplunkClientProperties clientProperties = base.ClientProperties as SplunkClientProperties;
            var middleware = new HttpEventCollectorResendMiddleware(1);
            this.SplunkEventEmitter = new HttpEventCollectorSender(new Uri($"http://{clientProperties.Ip}:{clientProperties.Port}"),
               clientProperties.HttpEventCollectorToken,  
                null,
                HttpEventCollectorSender.SendMode.Sequential,
                0,
                0,
                0,
                middleware.Plugin
            );
            this.IsAlive = true;
        }
        public override async Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token)
        {
            if (!token.IsCancellationRequested && this.IsAlive)
            {
                string decodedFrameJson = JsonConvert.SerializeObject(decodedFrame);
                await SubmitEvent(decodedFrameJson);
                Console.WriteLine($"Data => Arrived To {this.GetType()}");
                return true;
            }
            return false;
        }
        /// <summary>
        /// Submitting One Event To Splunk
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task SubmitEvent(string data)
        {
            //sending http event with decoded frame as data
            if (this.IsAlive)
            {
                this.SplunkEventEmitter.OnError += o =>
                {
                    this.IsAlive = false;
                    return;
                };
                this.SplunkEventEmitter.Send(Guid.NewGuid().ToString(), "INFO", null, new { DecodedFrameData = data });
                var flushTask = this.SplunkEventEmitter.FlushAsync();
                flushTask.Start();
                await flushTask;
                flushTask.Dispose();
            }
        }
        public override bool CompareProperties(ClientPropertiesBase properties)
        {
            if (base.CompareProperties(properties) && properties is SplunkClientProperties)
            {
                SplunkClientProperties clientProperties = (SplunkClientProperties)base.ClientProperties;
                SplunkClientProperties otherClientProps = (SplunkClientProperties)properties;
                return clientProperties.HttpEventCollectorToken == otherClientProps.HttpEventCollectorToken;
            }
            return false;
        }
        public override string ToString()
        {
            return "Splunk client: " + base.ToString() + "is cancelled";
        }
    }
}
