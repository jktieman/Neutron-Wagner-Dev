using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;

namespace NeutronData.Repositories
{
    public class AreaRepository : IAreaRepository
    {
        public AreaRepository() { }
        public List<Area> GetAreasByStationId(int workstationId)
        {
            var areas = new List<Area>();
            using (var db = new NeutronDb())
            {
                var recs = db.WorkstationAreas.Where(r => r.WorkstationId == workstationId).ToList();
                if (recs != null)
                {
                    foreach (var workstationArea in recs)
                    {
                         var area = db.Areas.FirstOrDefault(r => r.Id == workstationArea.AreaId);
                    areas.Add(area);
                    }
                }
            }

            return areas;
        }
    }
}
