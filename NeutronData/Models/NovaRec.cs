using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class NovaRec
    {
        [StringLength(35)]
        public string Sku { get; set; }
        public int Part { get; set; }
        public int Level { get; set; }
        public int Bin { get; set; }
        public int Car { get; set; }
        [StringLength(30)]
        public string Des { get; set; }
        public int Qty { get; set; }
        public int Cap { get; set; }
        public int Julian { get; set; }
        [StringLength(8)]
        public string Greg { get; set; }
        public int Trigger { get; set; }
        public int Flag { get; set; }
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
        public int OcTrig { get; set; }
        [StringLength(6)]
        public string UnitOfIssue { get; set; }
        public Single Weight { get; set; }
        public int Scale { get; set; }
        public byte SizeClass { get; set; }
        public byte VelClass { get; set; }
        public int RandomAssigned { get; set; }
        public int SegRcvg { get; set; }
        public Single Cube { get; set; }
        public Single Length { get; set; }
        [StringLength(1)]
        public string CCClass { get; set; }
        [StringLength(8)]
        public string LastCC { get; set; }
        [StringLength(1)]
        public string SelClass { get; set; }
        public byte HeightClass { get; set; }
        public int SizeFirst { get; set; }
        public int Quart { get; set; }
        public int Station { get; set; }
        public string Slot { get; set; }
        public int Id { get; set; }
    }
}
