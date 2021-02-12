using System;
using System.ComponentModel;

namespace NeutronCore.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription<TEnum>(this TEnum value)
                 where TEnum : struct
        {
            var type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException(message: "Enumeration Value must be of the Enum Type"
                    , paramName: nameof(value));
            }

            var fi = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(
                    typeof(DescriptionAttribute), inherit: false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
