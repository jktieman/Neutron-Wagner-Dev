using NeutronData.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class ItemImage : IEntity
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public string FileName { get; set; }
    }
}