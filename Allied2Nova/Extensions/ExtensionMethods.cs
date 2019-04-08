using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Allied2Nova.Extensions
{
    public static class ExtensionMethods
    {
        public static IEnumerable<string> SplitToLines(this string input)
        {
            if (input == null)
            {
                yield break;
            }
            using (System.IO.StringReader reader = new System.IO.StringReader(input))
            {
                string line;
                while( (line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static string ReplaceAt(this string str, int index, string replace)
        {
            string s = str.Remove(index, 3);

            return s.Insert(index, replace);
        }
    }
}
