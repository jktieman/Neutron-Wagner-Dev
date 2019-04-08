using AlliedLogger;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronMaintenance.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeutronMaintenance
{
    public class AkaDefinitionUpdate
    {
        readonly NeutronDb db = new NeutronDb();

        public void ProcessAkaDefinitions(List<AkaLoad> akaDefinitions, DynamicLogger logger)
        {
            try
            {
                logger.Log($"Process Aka Definitions");
                foreach (var item in akaDefinitions)
                {
                    logger.Log($"Aka Definition Start: {item.Item}  {item.AkaSku}");

                    AkaType rec = db.AkaTypes.Where(r => r.Item == item.Item && r.Aka == item.AkaSku).FirstOrDefault();

                    if (rec == null)
                    {
                        var aka = new AkaType();
                        aka.Aka = item.AkaSku;
                        aka.Item = item.Item;

                        db.AkaTypes.Add(aka);
                        db.SaveChanges();
                        
                    }
                    logger.Log($"Aka Definition End =====>>>  {item.Item}  {item.AkaSku}");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Aka Definition Error: {ex.Message} \r\n {ex.InnerException}");
            }
        }
    }
}
