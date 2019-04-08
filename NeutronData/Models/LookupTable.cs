using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class LookupTable
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public string TableName { get; set; }
    }
}
