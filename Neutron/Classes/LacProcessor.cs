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
        public List<Carrier> LacProfile { get; set; }

        public LacProcessor()
        {
            LacProfile = new List<Carrier>();
            ReprocessLacSet();
        }

        public void ReprocessLacSet(int userId = 0)
        {
            LacProfile = new List<Carrier>();
            if (userId > 0)
            {
                try
                {
                    using (var context = new SecureDb())
                    {
                        var param = new SqlParameter("@UserId", userId);
                         var carriers = context.Database.SqlQuery<Carrier>("usp_GetLacSet @UserId", param).ToList();
                        foreach (var carrier in carriers)
                        {
                            LacProfile.Add(carrier);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Get All Inventory Views Error. {ex.Message}\r\n {ex.InnerException}");
                }
            }
        }

        public bool MovePermitted(int station, int device, int carrier)
        {
            var result = false;
            var x = LacProfile.Find(r =>
                r.CarrierNumber == carrier && r.DeviceNumber == device && r.StationNumber == station);
            if (x != null) result = true;
            return result;
        }
    }
}
