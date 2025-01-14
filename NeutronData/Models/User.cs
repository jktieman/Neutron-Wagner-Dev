using System;
using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;

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
        public string EmpId { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Disabled { get; set; }
        public ICollection<Role> Roles { get; set; }
        public ICollection<Group> Groups { get; set; }
        public int LanguageId { get; set; }
        [ForeignKey("LanguageId")]
        public virtual Language Language { get; set; }
        public string UserInfo => $"{Firstname} {Lastname}";
        public string Fullname => ($"{Lastname},{Firstname}");

        public bool IsSupervisor()
        {
            if (Groups is not { Count: > 0 }) return false;
            return Groups.Any(group => group.Name.Contains("Supervisor"));
        }
    }
}
