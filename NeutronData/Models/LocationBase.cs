using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class LocationBase
    {
        public virtual int Station { get; set; }
        public virtual int Car { get; set; }
        public virtual int Bin { get; set; }
        public virtual int Level { get; set; }
        public virtual int Part { get; set; }
    }
}
