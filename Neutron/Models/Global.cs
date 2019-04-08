using System;

namespace Neutron.Models
{
    public static class Global
    {
        public static char SOH = Convert.ToChar(value: 1);
        public static char STX = Convert.ToChar(value: 2);
        public static char ETX = Convert.ToChar(value: 3);
        public static char ACK = Convert.ToChar(value: 6);
    }
}
