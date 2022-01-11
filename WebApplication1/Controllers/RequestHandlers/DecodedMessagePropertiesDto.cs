using IcdFilesRestApi.Controllers.RequestsHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DecodedIcd.Controllers.RequestHandlers
{
    public class DecodedMessagePropertiesDto
    {
        [Required(ErrorMessage = "Communication Name is required")]
        public string CommunicationType { get; set; }
        [Required(ErrorMessage = "Data type is required")]
        public EnumCommunicationType DataDirection { get; set; }
        [Required(ErrorMessage = "TransmissionRate is required")]
        [Range(500,60000)] //transmission rate is between 0.5s to 1 Minute
        public int TransmissionRate { get; set; }
        public DecodedMessagePropertiesDto()
        {
        }
        public DecodedMessagePropertiesDto(string communicationType, EnumCommunicationType direction, int rate)
        {
            this.CommunicationType = communicationType;
            this.DataDirection = direction;
            this.TransmissionRate = rate;
        }
        public override string ToString()
        {
            return "CommunicationType: " + this.CommunicationType + " DataDirection: " + 
                this.DataDirection + " transmition rate: " + this.TransmissionRate;
        }
    }
}
