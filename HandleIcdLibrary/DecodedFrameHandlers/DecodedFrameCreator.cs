using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HandleIcdLibrary
{
    /// <summary>
    /// creates random frames
    /// </summary>
    public class DecodedFrameCreator
    {
        private readonly Random _rnd;
        private readonly string _filePath;
        private readonly List<DecodedItem> _frameDecodedItems;
        public DecodedFrameCreator(string path)
        {
            this._filePath = path;
            _rnd = new Random();
            _frameDecodedItems = new List<DecodedItem>();
        }
        /// <summary>
        /// Building decodedFrame {name, random value}
        /// </summary>
        /// <param name="config"></param>
        /// <param name="topic"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DecodedFrameDto> CreateRandomFrameTask(ProducerConfig config, string topic, CancellationToken token)
        {
            DecodedFrameDto frame = new DecodedFrameDto();
            await Task.Run(async () =>
            {
                Icdframe icdItems = new Icdframe();
                await icdItems.CreateIcdFrame(_filePath, token); // creates ICD frame from icd file
                foreach (var icdItem in icdItems._frameDict.Values)
                {
                    DecodedItem newDecodeItem = new DecodedItem(icdItem.Name, GetRndNumber(icdItem));
                    _frameDecodedItems.Add(newDecodeItem);
                }
                DateTime decodedDate = DateTime.Now;
                string fileName = Path.GetFileName(_filePath);
                frame = new DecodedFrameDto(_frameDecodedItems, decodedDate, fileName);
            }, token);
            return frame;
        }

        public int GetRndNumber(IcdItem icdItem)
        {
            return _rnd.Next(icdItem.MinValue, icdItem.MaxValue);
        }

    }
}
