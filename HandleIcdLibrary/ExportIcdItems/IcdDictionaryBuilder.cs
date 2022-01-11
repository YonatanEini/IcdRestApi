using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HandleIcdLibrary
{
    public class IcdDictionaryBuilder
    {
        /// <summary>
        /// builds {id - IcdItem} Dictionary
        /// </summary>
        private readonly string _filePath;
        public IcdDictionaryBuilder(string path)
        {
            this._filePath = path;
        }
        /// <summary>
        /// builds IcdItems Dictionarty from icd file {key = id, value = IcdIem}
        /// </summary>
        /// <param name="token">producer cancellation token</param>
        /// <returns></returns>
        public async Task<Dictionary<int, IcdItem>> InsertValuesAsyncTask(CancellationToken token)
        {
            Dictionary<int, IcdItem> icdItemsDictionary = new Dictionary<int, IcdItem>();
            ExportIcdItem ItemsExported = ExportIcdItem.GetInstance();
            //icd file lines
            string[] fileLines = await File.ReadAllLinesAsync(_filePath);
            //could be many lines in the ICD file => Task
            await Task.Run(() => 
            {
                for (int i = 1; i < fileLines.Length - 1; i++)
                {

                    IcdItem curretnItem = ItemsExported.ExportFromLine(fileLines[i]);
                    int id = ItemsExported.ExportId(fileLines[i]);
                    icdItemsDictionary.Add(id, curretnItem);
                }
            }, token);
            return icdItemsDictionary;
        }
    }
}
