using AlliedLogger;
using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = NeutronCore.Global.Logger;

namespace ReplenService
{
    public class ReplenRepository
    {
        // get Replenishments from database
        private List<Replenishment> _replenishments;
        private readonly DynamicLogger _logger;

        public ReplenRepository()
        {
            _logger = (DynamicLogger)Logger.SetupLogger("ReplenRepository");
        }
        
       

        public List<Replenishment> GetReplenishments()
        {
            
            _replenishments = new List<Replenishment>();

            try
            {
                using (var context = new NeutronDb())
                {
                    _replenishments = context.Database.SqlQuery<Replenishment>("usp_CreateReplenishments").ToList();
                }

            }
            catch (Exception ex)
            {
               _logger.LogDetailAsync($"Get Replenishments Error. {Environment.NewLine} {ex.Message}  {Environment.NewLine}{ex.InnerException} ");
            }

            return _replenishments;
        }
    }
}
