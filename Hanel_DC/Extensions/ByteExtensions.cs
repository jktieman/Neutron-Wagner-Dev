using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanel_DC.Extensions
{
    public static class ByteExtensions
    {
        public static char SOH = Convert.ToChar(1);
        public static char ETX = Convert.ToChar(3);
        public static char ACK = Convert.ToChar(6);
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

        public static string ByteArrayToHumanString(this byte[] bytes)
        {
            var txt = string.Empty;
            var result = string.Empty;
            var arr = System.Text.Encoding.UTF8.GetString(bytes);
            var len = arr.Length;
            var txtLen = len - 10;
            if (arr.Substring(3, 2) == "39")
            {
                if (len == 21)
                {
                    txt = arr.Substring(8, txtLen - 1);
                }
                txt = arr.Substring(7, txtLen);
            }

            if (txtLen > 0)
            {
                txt = arr.Substring(7);
            }

            result =
                $"Bay: {arr.Substring(1, 2)} Cmd: {arr.Substring(3, 2)}" +
                $" Dis: {arr.Substring(5, 2)} Text: {txt}";

            return result;
        }

        public static string ByteArrayToCommand(this byte[] bytes)
        {
            var txt = string.Empty;
            var arr = System.Text.Encoding.UTF8.GetString(bytes);
            var len = arr.Length;
            var txtLen = len - 4;
            return arr.Substring(1, txtLen);
        }

        public static string ByteArrayToResponse(this byte[] bytes)
        {
            var arr = System.Text.Encoding.UTF8.GetString(bytes);
            var ack = (arr.IndexOf(ACK)) - 1;
            var len = arr.Length;
            if (ack > 0) return arr.Substring(1, ack);
            var txtLen = len - 4;
            return arr.Substring(1, txtLen);
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