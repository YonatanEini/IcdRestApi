
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IcdFilesRestApi;
using IcdFilesRestApi.Controllers.RequestsHandlers;
using Newtonsoft.Json;

namespace DecodedIcd.Controllers.RequestHandlers
{
    /// <summary>
    /// inserts the Icd appSettings Data to class object
    /// </summary>
    public class CommuncationsIcdDict
    {
        public Dictionary<string, string[]> CommunicationsIcdDict { get; set; }
        public string IcdFilesPath { get; set; }

        private CommuncationsIcdDict(IcdDataInitialiazer IcdFilesInitilalizer)
        {
            this.IcdFilesPath = IcdFilesInitilalizer.IcdFilesPath;
            this.CommunicationsIcdDict = IcdFilesInitilalizer.CommunicationIcdDict;
        }
        private static CommuncationsIcdDict _instance = null;

        public static CommuncationsIcdDict GetInstance(IcdDataInitialiazer IcdData)
        {
            _instance = _instance ?? new CommuncationsIcdDict(IcdData);
            return _instance;
        }
        public string GetCommunicationIcdName(string communicationName, EnumCommunicationType type)
        {
            if (this.CommunicationsIcdDict.ContainsKey(communicationName))
            {
                if (type.Equals(EnumCommunicationType.Uplink))
                    return CommunicationsIcdDict[communicationName][0];
                return CommunicationsIcdDict[communicationName][1];
            }
            return null;
        }

    }
}
