using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class PrintJob : IEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string JobNum { get; set; }
        public bool PickDocument { get; set; }
        public bool ToteLabel { get; set; }
        
    }
}
