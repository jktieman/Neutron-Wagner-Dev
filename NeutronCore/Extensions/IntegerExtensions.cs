namespace NeutronCore.Extensions
{
    public static class IntegerExtensions
    {
        public static int ParseInt(this string value, int defaultIntValue = 0)
        {
            decimal parsedInt;
            if (decimal.TryParse(value, out parsedInt))
            {
                return (int)parsedInt;
            }

            return defaultIntValue;
        }

        public static int? ParseNullableInt(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return value.ParseInt();
        }
    }
}
