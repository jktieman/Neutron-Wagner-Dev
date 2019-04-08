using System;
using System.Linq;
using System.Text;

namespace Core.Common.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder ReplaceAt(this StringBuilder sb, int index, int length, string value)
        {
            sb.Remove(index, length).Insert(index, value);
            return sb;
        }
    }
}
