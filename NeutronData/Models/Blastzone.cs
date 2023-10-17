using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class Blastzone : IBlastzone
    {
        private readonly int _bayController;
        private string _bayId = "02";
        private string _turnOn = "33";
        private string _turnOff = "14";
        private const string FourSpaces = "    ";
        private const string EndOfLine = "00000000120012000";


        public Blastzone(int bayController, int address, string text)
        {
            _bayController = bayController;
            BZ_Address = address;
            BZ_Beacon = 0;
            BZ_Text = text;
        }

        public Blastzone(int bayController, int address, int beacon, string text)
        {
            _bayController = bayController;
            BZ_Address = address;
            BZ_Beacon = beacon;   // No Arrow - 0, Arrow up - 2, Arrow down - 8 
            BZ_Text = text;
        }

        public int BZ_Address { get; set; }

        public int BZ_Beacon { get; set; }

        public string BZ_Text { get; set; }

        public string TurnOff => _bayId + _turnOff + BZ_Address.ToString().PadLeft(2, '0');

        public string TurnOn => _bayId + _turnOn + BZ_Address.ToString().PadLeft(2, '0')
                                + BZ_Text.PadLeft(4, ' ')
                                + FourSpaces + EndOfLine;

    }
}
