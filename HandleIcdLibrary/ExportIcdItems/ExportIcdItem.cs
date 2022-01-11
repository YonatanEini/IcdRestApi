using System;
using System.Collections.Generic;

namespace HandleIcdLibrary
{
    public class ExportIcdItem
    {
        /// <summary>
        /// singleton
        /// </summary>
        private ExportIcdItem()
        {
        }
        private static ExportIcdItem _instance = null;
        public static ExportIcdItem GetInstance()
        {
            _instance = _instance ?? new ExportIcdItem();
            return _instance;
        }
        /// <summary>
        ///   export icd item from a Icd file line
        /// </summary>
        /// <param name="line"></param>
        /// <returns> icd item </returns>
        public IcdItem ExportFromLine(string line)
        {
            string[] lineElements = line.Split('\t');
            int location = int.Parse(lineElements[1]);
            int minVal = int.Parse(lineElements[6]);
            int maxVal = int.Parse(lineElements[7]) + 1;
            string name = lineElements[2];
            string mask = lineElements[3];
            int endBit = int.Parse(lineElements[5]);
            int startBit = ExportStartBit(lineElements[4], endBit);
            return new IcdItem(location, name, mask, startBit, endBit, minVal, maxVal);
        }
        /// <summary>
        /// Export id
        /// </summary>
        /// <param name="line"></param>
        /// <returns>id of icd item </returns>
        public int ExportId(string line)
        {
            string[] lineElements = line.Split('\t');
            return int.Parse(lineElements[0]);
        }
        private int ExportStartBit(string startBit, int endBit)
        {
            if (string.IsNullOrEmpty(startBit)) return 0;
            if (endBit == 1)
            {
                return int.Parse(startBit);
            }
            string[] elements = startBit.Split('-');
            return int.Parse(elements[0]);
        }
    }
}
