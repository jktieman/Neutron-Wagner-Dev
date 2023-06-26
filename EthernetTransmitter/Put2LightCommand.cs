using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronCore.Extensions;

namespace EthernetTransmitter
{
    public class Put2LightCommand
    {
        public readonly char SOH = Convert.ToChar(1);
        public readonly char SOT = Convert.ToChar(2);
        public readonly char ETX = Convert.ToChar(3);
        public readonly char ACK = Convert.ToChar(6);

        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public Put2LightCommand()
        {

        }
        public Put2LightCommand(int intProperty, string stringProperty)
        {
            IntProperty = intProperty;
            StringProperty = GetCommand(stringProperty);
        }

        public string GetCommand(string stringProperty)
        {
            return SOH + stringProperty + stringProperty.ToHex() + ETX;
        }

        internal string GetAck(string cmd)
        {
            var h = $"{cmd}{ACK}".ToHex();

            return $"{SOH}{cmd}{ACK}{h}{ETX}";
        }
    }
}
