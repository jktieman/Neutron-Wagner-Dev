using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTI_Tester.Enums;
using IPTI_Tester.Models;

namespace IPTI_Tester
{
    public class Command
    {
        // 1 = Interface, 2 = Bay
        public int ControllerType { get; set; }
        public string Code { get; set; }
        public string Function { get; set; }
        public List<Prompt> Prompts { get; set; }
    }
}
