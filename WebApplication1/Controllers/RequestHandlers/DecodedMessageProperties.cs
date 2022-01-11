﻿using IcdFilesRestApi.Controllers.RequestsHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecodedIcd.Controllers.RequestHandlers
{
    public class DecodedMessageProperties
    {
        public string CommunicationType { get; set; }
        public EnumCommunicationType DataDirection { get; set; }
        public int TransimisionRate { get; set; }
        public DecodedMessageProperties()
        {
        }
        public DecodedMessageProperties(string communicationType, EnumCommunicationType direction, int rate)
        {
            this.CommunicationType = communicationType;
            this.DataDirection = direction;
            this.TransimisionRate = rate;
        }
        public override string ToString()
        {
            return "CommunicationType: " + this.CommunicationType + " DataDirection: " + 
                this.DataDirection + " transmition rate: " + this.TransimisionRate;
        }
    }
}
