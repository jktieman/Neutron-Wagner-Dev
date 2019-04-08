using Neutron.Interfaces;
using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Classes
{
    public class LacProcessor : ILacProcessor
    {
        public Dictionary<int, Carrier> LacProfile { get; set; }

        public LacProcessor()
        {
            LacProfile = new Dictionary<int, Carrier>();
            ReprocessLacSet();
        }

        public void ReprocessLacSet(int userId = 0)
        {
            LacProfile = new Dictionary<int, Carrier>();
            if (userId > 0)
            {
                //var recs = new List<int>();
                try
                {
                    using (var context = new SecureDb())
                    {
                        var param = new SqlParameter("@UserId", userId);
                         var carriers = context.Database.SqlQuery<Carrier>("usp_GetLacSet @UserId", param).ToList();
                        foreach (var carrier in carriers)
                        {
                            LacProfile.Add(carrier.CarrierId, carrier);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Get All Inventory Views Error. {ex.Message}\r\n {ex.InnerException}");
                }
            }
        }

        public bool LacAccess(int locationId)
        {
            return LacProfile.ContainsKey(locationId);
        }
    }
}
