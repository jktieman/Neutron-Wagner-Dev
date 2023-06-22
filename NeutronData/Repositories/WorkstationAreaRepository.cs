using NeutronData.Interfaces;
using System.Collections.Generic;
using System.Linq;
using NeutronData.Models;
using NeutronData.DataContexts;
using System;

namespace NeutronData.Repositories
{
    public class WorkstationAreaRepository : IWorkstationAreaRepository
    {
        public List<Area> GetAllAreas()
        {
            List<Area> areas;
            using (var db = new NeutronDb())
            {
                areas = db.WorkstationAreas.Select(r => r.Area).ToList();
            }

            return areas;
        }

        public List<Area> GetAreasByWorkstationId(int workstationId)
        {
            var areas = new List<Area>();
            using (var db = new NeutronDb())
            {
                var recs = db.WorkstationAreas.Where(r => r.WorkstationId == workstationId).ToList();
                foreach (var workstationArea in recs)
                {
                    var area = db.Areas.FirstOrDefault(r => r.Id == workstationArea.AreaId);
                    areas.Add(area);
                }
            }

            return areas;
        }

        public List<Workstation> GetWorkstationsWithAreas()
        {
            var workstations = new List<Workstation>();
            using (var db = new NeutronDb())
            {
                var recs = db.WorkstationAreas.Distinct().ToList();
                foreach (var workstationArea in recs)
                {
                    var ws = workstationArea.Workstation;
                    workstations.Add(ws);
                }
            }
            return workstations;
        }

        public int[] GetAreaIdsByWorkstationId(int workstationId)
        {
            int[] areaIds;
            using (var db = new NeutronDb())
            {
                areaIds = db.WorkstationAreas.Where(r => r.WorkstationId == workstationId).Select(r => r.AreaId).ToArray();
            }

            return areaIds;
        }
    }
}
