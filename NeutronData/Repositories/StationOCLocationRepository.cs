using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Repositories
{
    public class StationOCLocationRepository : RepositoryBase<NeutronDb>
    {
        public IList<StationOcLocation> GetList(int station)
        {
            List<StationOcLocation> list = new List<StationOcLocation>(); 
            try
            {
                using (var context = DataContext)
                {
                   list = (from n in context.NovaRecs where n.Station == station
                           join r in context.ReserveTypes on n.Sku equals r.Sku
                           select new StationOcLocation 
                           {
                               Station = station,
                               Slot = r.Slot,
                               Sku = r.Sku,
                               Description = n.Des,
                               Qty = r.Qty    
                           })
                           .OrderBy(o => o.Slot)
                           .ToList();
                }
            }
            catch (Exception)
            {

            }
            return list;
        }
    }
}
