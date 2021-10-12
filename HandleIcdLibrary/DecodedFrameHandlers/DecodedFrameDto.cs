using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HandleIcdLibrary
{
    /// <summary>
    /// write object to kafka
    /// </summary>
    public class DecodedFrameDto
    {
        [BsonElement("DecodedItems")]
        [Required(ErrorMessage = "Decoded items are required")]
        public List<DecodedItem> DecodedItems { get; set; }
        [BsonElement("DecodingTime")]
        [Required(ErrorMessage = "Decoded date is required")]
        public DateTime DecodingTime { get; set; }
        [BsonElement("Icdfile")]
        [Required(ErrorMessage = "Icd file is required")]
        public string Icdfile { get; set; }
        [Required(ErrorMessage = "Hour is required")]
        public string Hour { get; set; } //for powerbi
        public DecodedFrameDto()
        {
            this.DecodedItems = new List<DecodedItem>();
            this.DecodingTime = new DateTime();
            this.Icdfile = "";
            this.Hour = this.DecodingTime.ToString("HH:mm:ss");
        }
        public DecodedFrameDto(List<DecodedItem> decodedItems, DateTime decodedTime, string Icd)
        {
            this.DecodedItems = decodedItems;
            this.DecodingTime = decodedTime;
            this.Icdfile = Icd;
            this.Hour = this.DecodingTime.ToString("HH:mm:ss");
        }
    }
}
