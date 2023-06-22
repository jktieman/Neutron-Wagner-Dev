using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronCore.Enums;
using NeutronData.Interfaces;
using StorageDeviceType = NeutronData.Models.Lookups.StorageDeviceType;

namespace NeutronData.Models
{
    public class StorageDevice : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StorageDeviceNumber { get; set; }
        public bool IsMoveable { get; set; }
        public int StorageDeviceTypeId { get; set; }
        public int AreaId { get; set; }

        [ForeignKey("AreaId")]
        public virtual Area Area { get; set; }

        [ForeignKey("StorageDeviceTypeId")]

        public virtual StorageDeviceType StorageDeviceType { get; set; }
    }
}
