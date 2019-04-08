using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    //Carousel Sku Definition 
    public class NDefDb
    {
        [StringLength(35)]
        public string Sku { get; set; }
        [StringLength(30)]
        public string Des { get; set; }
        [StringLength(6)]
        public string UnitOfIssue { get; set; }
        public int Cap { get; set; }
        public int Trigger { get; set; }
        public int OcTrig { get; set; }
        public byte SizeClass { get; set; }
        public byte VelClass { get; set; }
        public int RandomSku { get; set; }
        public int SegRcvg { get; set; }
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
        public Single Weight { get; set; }
        public int Scale { get; set; }
        public Single Cube { get; set; }
        public Single Length { get; set; }
        [StringLength(1)]
        public string CCClass { get; set; }
        [StringLength(8)]
        public string LastCC { get; set; }
        public double SysCap { get; set; }
        public int SysTrig { get; set; }
        [StringLength(1)]
        public string SelClass { get; set; }
        public byte HeightClass { get; set; }
        public int SizeFirst { get; set; }
        public int AutoQuart { get; set; }
        public int Station { get; set; }
        public int Id { get; set; }
    }
}
