using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronCore.Extensions
{
    public static class StringExtensions
    {
        public static string ToHex(this string value)
        {
            var total = default(int);
            var values = value.ToCharArray();
            foreach (var c in values)
            {
                var decValue = Convert.ToInt32(c);
                total += Convert.ToInt32(decValue);
            }
            //total = values.Select(Convert.ToDecimal).Select(Convert.ToInt32).Sum();
            var hex = "00" + total.ToString("X");
            return hex.Substring(hex.Length - 2, 2);
        }

    }
}
