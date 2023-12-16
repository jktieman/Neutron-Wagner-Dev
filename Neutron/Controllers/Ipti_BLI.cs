namespace Neutron.Controllers
{
    public class Ipti_BLI
    {

        private string _bayId;
        private string _turnOnMicro = "10";
        private string _turnOn = "33";
        private string _turnOff = "14";
        private string _turnAllOff = "1400";
        private const string FourSpaces = "    ";
        private const string EndOfLine = "00000000120012000";


        public Ipti_BLI(int bayController, int address, string text)
        {
            _bayId = bayController.ToString().PadLeft(2, '0');
            BLI_Address = address;
            BLI_Beacon = 0;
            BLI_Text = text;
        }

        public Ipti_BLI(int bayController, int address, int beacon, string text)
        {
            _bayId = bayController.ToString().PadLeft(2, '0');
            BLI_Address = address;
            BLI_Beacon = beacon;   // No Arrow - 0, Arrow up - 2, Arrow down - 8 
            BLI_Text = text;
        }

        public string BLI_BayController => _bayId;

        public int BLI_Address { get; set; }

        public int BLI_Beacon { get; set; }

        public string BLI_Text { get; set; }

        public string TurnOff => _bayId + _turnOff + BLI_Address.ToString().PadLeft(2, '0');
        
        //public string TurnOn => _bayId + _turnOn + BLI_Address.ToString().PadLeft(2, '0') 
        //           + BLI_Text.PadLeft(4, ' ') 
        //           + FourSpaces + EndOfLine;

        public string TurnOn => _bayId + _turnOnMicro + BLI_Address.ToString().PadLeft(2, '0')
                                     + "43300430000"
                                     + BLI_Text.PadLeft(2, '0');
                                
        public string TurnAllOff => _bayId + _turnAllOff;

    }
}