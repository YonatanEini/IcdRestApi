using IcdFilesRestApi.Controllers.RequestsHandlers;
using System;
using System.Collections.Generic;

namespace IcdFilesRestApi
{
    public class IcdDataInitialiazer
    {
        public string IcdFilesPath { get; set; }
        public Dictionary<string, string[]> CommunicationIcdDict { get; set; }
        public string DestinationPath { get; set; }
    }
}