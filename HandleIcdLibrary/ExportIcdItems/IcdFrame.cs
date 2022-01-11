using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandleIcdLibrary
{
    public class Icdframe
    {
        /// <summary>
        /// dictionary of {id - icdItem}
        /// </summary>
        public Dictionary<int, IcdItem> _frameDict;
        public Icdframe()
        {
            this._frameDict = new Dictionary<int, IcdItem>();
        }
        public async Task CreateIcdFrame(string filePath, CancellationToken token)
        {
            IcdDictionaryBuilder dictionaryBuilder = new IcdDictionaryBuilder(filePath);
            this._frameDict = await dictionaryBuilder.InsertValuesAsyncTask(token);
        }
    }
}
