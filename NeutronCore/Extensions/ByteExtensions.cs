using System;
using System.Linq;
using System.Text;

namespace NeutronCore.Extensions
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
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ByteArrayToRawString(this byte[] bytes)
        {
            return bytes.ToString();
        }

        public static string ByteArrayToHumanString(this byte[] bytes)
        {
            var txt = string.Empty;
            var arr = Encoding.UTF8.GetString(bytes);
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

            var result = $"Bay: {arr.Substring(1, 2)} Cmd: {arr.Substring(3, 2)}" +
                         $" Dis: {arr.Substring(5, 2)} Text: {txt}";

            return result;
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
