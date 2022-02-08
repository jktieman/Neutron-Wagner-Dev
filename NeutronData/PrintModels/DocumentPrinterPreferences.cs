namespace NeutronData.PrintModels
{
    public class DocumentPrinterPreferences
    {
        public string PrinterName { get; set; }
        public int LeftMargin { get; set; }
        public int TopMargin { get; set; }
        public int RightMargin { get; set; }
        public int BottomMargin { get; set; }
        public bool Landscape { get; set; }
    }
}
