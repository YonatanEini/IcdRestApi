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
        /// creates decodedFrame {name, random value} from {id-icdItem} dictionary
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DecodedFrameDto> CreateRandomFrameAsyncTask(CancellationToken token)
        {
            DecodedFrameDto frame = new DecodedFrameDto();
            await Task.Run(async () =>
            {
                 Icdframe icdItems = new Icdframe();
                //creates ICD frame from icd file
                 await icdItems.CreateIcdFrame(_filePath, token); 
                 foreach (var icdItem in icdItems._frameDict.Values)
                 {
                     DecodedItem newDecodeItem = new DecodedItem(icdItem.Name, GetRndNumber(icdItem));
                     _frameDecodedItems.Add(newDecodeItem);
                 }
                 DateTime decodedDate = DateTime.Now;
                 string icdFileName = Path.GetFileName(_filePath);
                 frame = new DecodedFrameDto(_frameDecodedItems, decodedDate, icdFileName);
            }, token);
            if(!token.IsCancellationRequested)
            {
                //task completed
                return frame; 
            }
            //cancellation token activated
            return null; 
        }
        public int GetRndNumber(IcdItem icdItem)
        {
            return _rnd.Next(icdItem.MinValue, icdItem.MaxValue);
        }
    }
}
