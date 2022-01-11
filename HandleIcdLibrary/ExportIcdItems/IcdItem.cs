using System;
using System.Collections.Generic;

namespace HandleIcdLibrary
{
    /// <summary>
    /// single ICD item
    /// </summary>
    public class IcdItem
    {
        public int Location { get; set; }
        public string Name { get; set; }
        public string Mask { get; set; }
        public int StartBit { get; set; }
        public int EndBit { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public IcdItem()
        {
        }
        public IcdItem(int location, string name, string mask, int startBit, int endBit, int minValue, int maxValue)
        {
            this.Location = location;
            this.Name = name;
            this.Mask = mask;
            this.StartBit = startBit;
            this.EndBit = endBit;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }
    }
}
