using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronMaintenance.Models;

namespace NeutronMaintenance
{
    public class RandomLocationManager : ILocationManager
    {
        private readonly IVelocityCodeManager _velocityCodeManager;
        private readonly GenericRepository<Location> _repoLocation;

        //private readonly GenericRepository<ItemDefinition> _repoItemDefinition;
        //private readonly GenericRepository<SizeCode> _repoSizeCode;
        //private readonly GenericRepository<VelocityCode> _repoVelocityCode;
        //private readonly GenericRepository<HeightCode> _repoHeightCode;
        //private readonly GenericRepository<LocationCode> _repoLocationCode;
        //private readonly GenericRepository<Inventory> _repoInventory;

        public RandomLocationManager(IVelocityCodeManager velocityCodeManager)
        {
            _velocityCodeManager = velocityCodeManager;
            _repoLocation = new GenericRepository<Location>(new NeutronDb());

            //_repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
            //_repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
            //_repoVelocityCode = new GenericRepository<VelocityCode>(new NeutronDb());
            //_repoHeightCode = new GenericRepository<HeightCode>(new NeutronDb());
            //_repoLocationCode = new GenericRepository<LocationCode>(new NeutronDb());
            //_repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        }

        public void Process(NovaRandomLocation novaRandomLocation)
        {
            if (novaRandomLocation == null) return;
            switch (novaRandomLocation.Operation)
            {
                case "A":
                    {
                        Add(novaRandomLocation);
                        break;
                    }
                case "M":
                    {
                        Modify(novaRandomLocation);
                        break;
                    }
                case "D":
                    {
                        Delete(novaRandomLocation);
                        break;
                    }
            }
        }

        private void Add(NovaRandomLocation novaRandomLocation)
        {
            var rec = Get(novaRandomLocation);
            if (rec == null)
            {
                rec = new Location();
                rec.StationId = novaRandomLocation.SystemNumber.ParseInt();
                rec.Loc1 = novaRandomLocation.Car.ParseInt();
                rec.Loc2 = novaRandomLocation.Bin.ParseInt();
                rec.Loc3 = novaRandomLocation.Lvl.ParseInt();
                rec.Loc4 = novaRandomLocation.Prt.ParseInt();
                rec.VelocityCodeId = _velocityCodeManager.Get(novaRandomLocation.Velocity).Id;
            }
            else
            {

            }
        }

        private void Modify(NovaRandomLocation novaRandomLocation)
        {
            var rec = Get(novaRandomLocation);
            if(rec == null) return;
            var velocity = _velocityCodeManager.Get(novaRandomLocation.Velocity);
           // rec.VelocityCode = novaRandomLocation.Velocity

        }

        private void Delete(NovaRandomLocation novaRandomLocation)
        {
            var rec = Get(novaRandomLocation);
        }


        private Location Get(NovaRandomLocation novaRandomLocation)
        {
            return _repoLocation.All().FirstOrDefault(r => r.StationId == novaRandomLocation.SystemNumber.ParseInt() && r.Loc1 == novaRandomLocation.Car.ParseInt() && r.Loc2 == novaRandomLocation.Bin.ParseInt() && r.Loc3 == novaRandomLocation.Lvl.ParseInt() && r.Loc4 == novaRandomLocation.Prt.ParseInt());
        }
    }
}
