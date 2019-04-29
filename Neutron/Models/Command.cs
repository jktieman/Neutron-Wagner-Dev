using System.Collections.Generic;

namespace Neutron.Models
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
