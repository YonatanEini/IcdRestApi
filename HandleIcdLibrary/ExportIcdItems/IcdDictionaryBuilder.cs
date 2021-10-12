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
        /// builds Icd Items Dictionary
        /// </summary>
        private readonly string _filePath;
        public IcdDictionaryBuilder(string path)
        {
            this._filePath = path;
        }
        /// <summary>
        /// builds IcdItems Dictionarty {key = id, value = IcdIem}
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Dictionary<int, IcdItem>> InsertValuesAsync(CancellationToken token)
        {
            Dictionary<int, IcdItem> icdItemsDictionary = new Dictionary<int, IcdItem>();
            ExportIcdItem ItemsExported = ExportIcdItem.GetInstance();
            string[] fileLines = await File.ReadAllLinesAsync(_filePath);
            await Task.Run(() => //could be many lines in the ICD file
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
