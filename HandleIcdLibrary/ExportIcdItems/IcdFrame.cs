using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandleIcdLibrary
{
    public class Icdframe
    {
        public Dictionary<int, IcdItem> _frameDict;
        public Icdframe()
        {
            this._frameDict = new Dictionary<int, IcdItem>();
        }
        public async Task CreateIcdFrame(string filePath, CancellationToken token)
        {
            IcdDictionaryBuilder dictionaryBuilder = new IcdDictionaryBuilder(filePath);
            this._frameDict = await dictionaryBuilder.InsertValuesAsync(token);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in _frameDict.Values)
            {
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }
    }
}
