using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    // Off Carousel Sku Definition
    public class OCType
    {
        [StringLength(35)]
        public string Sku { get; set; }
        [StringLength(1)]
        public string SizeClass { get; set; } = "5";
        [StringLength(1)]
        public string Class { get; set; } = "A";
        [StringLength(1)]
        public string VelClass { get; set; } = "5";
        [StringLength(1)]
        public string PSZ { get; set; } = "Z";
        [StringLength(30)]
        public string Des { get; set; }
        [StringLength(6)]
        public string UnitOfIssue { get; set; } = "EACH";
        public int SerialNumber { get; set; }
        public Single Cube { get; set; }
        public Single Length { get; set; }
        public Single Weight { get; set; }
        [StringLength(1)]
        public string CCClass { get; set; } = string.Empty;
        [StringLength(8)]
        public string LastCC { get; set; } = string.Empty;
        public int Discrete { get; set; }
        public int Trigger { get; set; }
        public int Cap { get; set; }
        public int AutoQuart { get; set; }
        public int Months1 { get; set; }
        public int Months2 { get; set; }
        public int Months3 { get; set; }
        public int Months4 { get; set; }
        public int Months5 { get; set; }
        public int Months6 { get; set; }
        public int Months7 { get; set; }
        public int Months8 { get; set; }
        public int Months9 { get; set; }
        public int Months10 { get; set; }
        public int Months11 { get; set; }
        public int Months12 { get; set; }
        public int Station { get; set; }
        public int Id { get; set; }
    }
}
