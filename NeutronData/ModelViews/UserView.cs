using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.ModelViews
{
    public class UserView
    {
        public int Id { get; set; }
        public string EmpId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Pin { get; set; }
        public bool Disabled { get; set; }
        public int HomeLocationId { get; set; }
        public string HomeLocation { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int LocationGroupId { get; set; }
        public string LocationGroupName { get; set; }
    }
}
