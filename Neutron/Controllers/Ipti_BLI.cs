namespace Neutron.Controllers
{
    public class Ipti_BLI
    {
        private string _bayId = "01";
        private string _turnOn = "33";
        private string _turnOff = "14";
        private const string FourSpaces = "    ";
        private const string EndOfLine = "00000000120012000";


        public Ipti_BLI(int address, string text)
        {
            BLI_Address = address;
            BLI_Beacon = 0;
            BLI_Text = text;
        }

        public Ipti_BLI(int address, int beacon, string text)
        {
            BLI_Address = address;
            BLI_Beacon = beacon;   // No Arrow - 0, Arrow up - 2, Arrow down - 8 
            BLI_Text = text;
        }

        public int BLI_Address { get; set; }

        public int BLI_Beacon { get; set; }

        public string BLI_Text { get; set; }

        public string TurnOff => _bayId + _turnOff + BLI_Address.ToString().PadLeft(2, '0');
        
        public string TurnOn => _bayId + _turnOn + BLI_Address.ToString().PadLeft(2, '0') 
                   + BLI_Text.PadLeft(4, ' ') 
                   + FourSpaces + EndOfLine;
        
    }
}