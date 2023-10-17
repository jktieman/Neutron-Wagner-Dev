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
        private readonly NeutronDb _db = new NeutronDb();

        public void ProcessAkaDefinitions(List<AkaLoad> akaDefinitions, IDynamicLogger logger)
        {
            try
            {
                logger.LogDetailAsync($"Process Aka Definitions");
                foreach (var item in akaDefinitions)
                {
                    logger.LogDetailAsync($"Aka Definition Start: {item.Item}  {item.AkaSku}");

                    var rec = _db.AkaTypes.FirstOrDefault(r => r.Item == item.Item && r.Aka == item.AkaSku);

                    if (rec == null)
                    {
                        var aka = new AkaType
                        {
                            Aka = item.AkaSku,
                            Item = item.Item
                        };

                        _db.AkaTypes.Add(aka);
                        _db.SaveChanges();
                        
                    }
                    logger.LogDetailAsync($"Aka Definition End =====>>>  {item.Item}  {item.AkaSku}");
                }
            }
            catch (Exception ex)
            {
                logger.LogDetailAsync($"Aka Definition Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }
    }
}
