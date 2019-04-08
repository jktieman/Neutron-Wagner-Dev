using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class ReplenHostOrder : IHostOrder
    {
        public int Id { get; set; }
        public string TypeCode { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string JobNum { get; set; }
        public string PrimeBin { get; set; }
        public string NewBin { get; set; }
        public string Qty { get; set; }
        public string TroubleBit { get; set; }
        public string DateTime { get; set; }
        public string EmpId { get; set; }
        public string LineStatusId { get; set; }
        public ReplenOrderDetail OrderDetail { get; set; }
    }
}