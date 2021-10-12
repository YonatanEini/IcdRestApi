
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
    public class CommuncationsIcdDict
    {
        public Dictionary<string, string[]> CommunicationsIcdDict { get; set; }
        public string IcdFilesPath { get; set; }

        private CommuncationsIcdDict(IcdDataInitialiazer IcdFilesInitilalizer)
        {

            this.IcdFilesPath = IcdFilesInitilalizer.IcdFilesPath;
            this.CommunicationsIcdDict = IcdFilesInitilalizer.CommunicationIcdDict;
        }
        public static CommuncationsIcdDict _instance = null;

        public static CommuncationsIcdDict GetInstance(IcdDataInitialiazer dataBaseInitilalizer)
        {
            _instance = _instance ?? new CommuncationsIcdDict(dataBaseInitilalizer);
            return _instance;
        }
        public string GetCommunicationName(string communicationName, EnumCommunicationType type)
        {
            if (this.CommunicationsIcdDict.ContainsKey(communicationName))
            {
                if (type.Equals(EnumCommunicationType.Uplink))
                    return CommunicationsIcdDict[communicationName][0];
                return CommunicationsIcdDict[communicationName][1];
            }
            return null;
        }
        public static void AppendToFile(string communicationName, string IcdData, string clientFilesPath)
        {
            string filePath = Path.Combine(clientFilesPath, communicationName + ".txt");
            File.AppendAllText(filePath, IcdData);
        }
    }
}
