using Neutron.Enums;
using Neutron.Interfaces;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Classes
{
    public class SecurityProcessor : ISecurityProcessor
    {
        private bool[] securityProfile;
        readonly int neutronSecurityLength = 0;

        public SecurityProcessor()
        {
            neutronSecurityLength = Enum.GetNames(typeof(NeutronSecurity)).Length + 1;
            securityProfile = new bool[neutronSecurityLength];
            ReprocessSecuritySet();
        }

        public bool[] SecurityProfile
        {
            get { return securityProfile; }
            set { securityProfile = value; }
        }

        public void ReprocessSecuritySet(string id = "")
        {
            if (string.IsNullOrEmpty(id))
            {
                for (int i = 0; i < neutronSecurityLength; i++)
                {
                    SecurityProfile[i] = false;
                }
                return;
            }

            if (id == "1111")
            {
                for (int i = 0; i < neutronSecurityLength; i++)
                {
                    SecurityProfile[i] = true;
                }
                return;
            }

            if (id == "2277") //Neutron Admin
            {
                for (int i = 0; i < neutronSecurityLength; i++)
                {
                    SecurityProfile[i] = true;
                }
                return;
            }
            else
            {
                for (int i = 0; i < neutronSecurityLength; i++)
                {
                    SecurityProfile[i] = false;
                }
                try
                {
                    if (!string.IsNullOrEmpty(id) && id != "2277")
                    {
                        using (var db = new SecureDb())
                        {
                            try
                            {
                                var secureItems = new List<SecureItem>();
                                User user = db.Users.Where(r => r.Pin == id).FirstOrDefault();
                                List<Group> groups = db.GroupUser.Where(g => g.UserId == user.Id).Select(s => s.Group).ToList();
                                foreach (var group in groups)
                                {
                                    List<SecureItem> secItems = db.GroupSecureItem.Where(g => g.GroupId == group.GroupId)
                                        .Select(s => s.SecureItem).ToList();
                                    foreach (var sec in secItems)
                                    {
                                        if (!secureItems.Contains(sec))
                                        {
                                            secureItems.Add(sec);
                                        }
                                    }
                                }
                                if (secureItems.Count > 0)
                                {
                                    foreach (var item in secureItems)
                                    {
                                        SecurityProfile[(int) item.SecureItemId] = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error finding user.  " + ex.Message);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Security Profile. " + ex.Message);
                }
            }
        }
    }
}
