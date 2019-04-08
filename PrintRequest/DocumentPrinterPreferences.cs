using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintRequest
{
    public class DocumentPrinterPreferences
    {
        public string PrinterName { get; set; }
        public int LeftMargin { get; set; }
        public int TopMargin { get; set; }
        public int RightMargin { get; set; }
        public int BottomMargin { get; set; }
    }
}
