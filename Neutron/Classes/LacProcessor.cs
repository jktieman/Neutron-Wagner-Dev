using Neutron.Interfaces;
using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using AlliedLogger;
using NeutronData.General;
using Logger = NeutronCore.Global.Logger;

namespace Neutron.Classes
{
    public class LacProcessor : ILacProcessor
    {
        private readonly IDynamicLogger _logger;
        public bool UseLacProcessor { get; set; } = false;

        public List<Carrier> LacProfile { get; set; }
        
        public LacProcessor()
        {
            _logger = Logger.SetupLogger(@"LacProcessor");
            LacProfile = new List<Carrier>();
            ReprocessLacSet();
        }

        public void ReprocessLacSet(int userId = 0)
        {
            if (!UseLacProcessor) return;
            LacProfile = new List<Carrier>();
            if (userId > 0)
            {
                try
                {
                    using (var context = new SecureDb())
                    {
                        if (!context.CheckConnection()) return;

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

        public bool MovePermitted(int workstationId, int device, int carrier)
       {
           //if not using LacProcessor always return true to allow move
            if (!UseLacProcessor) return true;
            var result = false;
            var x = LacProfile.Find(r =>
                r.CarrierNumber == carrier && r.DeviceNumber == device && r.WorkstationId == workstationId);
            if (x != null) result = true;
            return result;
        }
    }
}
