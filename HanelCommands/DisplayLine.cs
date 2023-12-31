using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands
{
    public class DisplayLine
    {
        public string Line { get; set; }
        public string Column { get; set; }
        public string Text { get; set; }

        public DisplayLine(string line, string column, string text)
        {
            Line = line.PadLeft(2, '0');
            Column = column.PadLeft(3, '0');
            Text = text;
        }

        public override string ToString()
        {
            return $"{Line}{Column}{Text}";
        }
    }
}
