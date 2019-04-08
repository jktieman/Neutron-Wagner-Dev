using System;
using System.Linq;
using System.Text;

namespace Neutron.Extensions
{
    public static class ByteExtensions
    {
        public static string ByteArrayToHexString(this byte[] bytes, bool dashes = true)
        {
            var s = BitConverter.ToString(bytes);
            if (!dashes)
            {
                s = s.Replace("-", ""); 
            }
            return s;
        }

        public static string ByteArrayToStringX2(this byte[] bytes)
        {
            return string.Join(string.Empty, Array.ConvertAll(bytes, x => x.ToString("X2")));
        }

        public static byte[] StringToByteArray(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string ByteArrayToString(this byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static string ByteArrayToRawString(this byte[] bytes)
        {
            return bytes.ToString();
        }

        public static string GetCheckDigit(this string command)
        {
            var values = command.ToCharArray();
            var total = values.Select(c => (int)c).Select(decValue => Convert.ToInt32((decimal)decValue)).Sum();
            var hex = "00" + total.ToString(format: "X");
            return hex.Substring(hex.Length - 2, length: 2);
        }
    }
}
