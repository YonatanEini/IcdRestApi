using HandleIcdLibrary;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HandleKafkaLibrary.CosumersProperties
{
    interface IReceiveData
    {
        public Task<bool> ReceiveDecodedFrameAsync(DecodedFrameDto decodedFrame, CancellationToken token);
        public bool CompareProperties(ClientPropertiesBase properties);
    }
}
