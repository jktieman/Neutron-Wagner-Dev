namespace Neutron.Controllers
{
    public class Ipti_BLI
    {
        private int _address;
        private string _text;
        private int _beacon;
        private string _bayId = "01";
        private string _turnOn = "33";
        private string _turnOff = "14";
        private const string _fourSpaces = "    ";
        private const string _endOfLine = "00000000120012000";


        public Ipti_BLI(int address, string text)
        {
            _address = address;
            _beacon = 0;
            _text = text;
        }

        public Ipti_BLI(int address, int beacon, string text)
        {
            _address = address;
            _beacon = beacon;   // No Arrow - 0, Arrow up - 2, Arrow down - 8 
            _text = text;
        }

        public int BLI_Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public int BLI_Beacon
        {
            get
            {
                return _beacon;
            }
            set
            {
                _beacon = value;
            }
        }

        public string BLI_Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        public string TurnOff => _bayId + _turnOff + BLI_Address.ToString().PadLeft(2, '0');
        
        public string TurnOn => _bayId + _turnOn + BLI_Address.ToString().PadLeft(2, '0') 
                   + BLI_Text.PadLeft(4, ' ') 
                   + _fourSpaces + _endOfLine;
        
    }
}