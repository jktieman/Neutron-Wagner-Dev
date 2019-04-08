using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Models
{
    public class Prompt
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Prompt(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
