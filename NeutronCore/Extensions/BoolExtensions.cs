using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronCore.Extensions
{
    public static class BoolExtensions
    {
        public static bool ToBoolean(this int value)
        {
            if (value == 0 || value == 1)
            {
                return Convert.ToBoolean(value);
            }

            throw new ArgumentException("Invalid parameter value. Only 0, 1 are allowed.");
        }

        public static bool ToBoolean(this string value)
        {
            if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase) || value == "1")
            {
                return true;
            }
            if (value.Equals("false", StringComparison.CurrentCultureIgnoreCase) || value == "0")
            {
                return false;
            }

            throw new ArgumentException("Invalid parameter value. Only true, or false are allowed.");
        }
    }
}
