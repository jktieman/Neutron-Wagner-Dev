using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class User: IEntity
    {
        public User()
        {
            Roles = new List<Role>();
            Groups = new List<Group>();
        }
        public int Id { get; set; }
        public string EmpId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Pin { get; set; }
        public string Password { get; set; }
        public bool Disabled { get; set; }
        public ICollection<Role> Roles { get; set; }
        public ICollection<Group> Groups { get; set; }
        public int LanguageId { get; set; }
        [ForeignKey("LanguageId")]
        public virtual Language Language { get; set; }
        public string UserInfo => $"{Firstname} {Lastname}";
        public string Fullname => ($"{Lastname},{Firstname}");
    }
}
