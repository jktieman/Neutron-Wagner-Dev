using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.DataContexts;

namespace NeutronData.Models.Lookups
{
    [Table("LineStatus")]
    public class  LineStatus : ILookup, IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }
}
