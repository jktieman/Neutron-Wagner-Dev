using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    //Carousel Location
    public class NLocDb
    {
        public int Part { get; set; }
        public int Level { get; set; }
        public int Bin { get; set; }
        public int Car { get; set; }
        public byte SizeClass { get; set; }
        public byte VelClass { get; set; }
        public byte HeightClass { get; set; }
        public int Station { get; set; }
        public string Slot { get; set; }
        public int Id { get; set; }
    }
}
