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
        private readonly ISizeCodeManager _sizeCodeManager;
        private readonly IHeightCodeManager _heightCodeManager;
        private readonly IVelocityCodeManager _velocityCodeManager;
        private readonly GenericRepository<Location> _repoLocation;

        public RandomLocationManager(ISizeCodeManager sizeCodeManager, IHeightCodeManager heightCodeManager, IVelocityCodeManager velocityCodeManager, Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            _sizeCodeManager = sizeCodeManager;
            _heightCodeManager = heightCodeManager;
            _velocityCodeManager = velocityCodeManager;
            _repoLocation = new GenericRepository<Location>(contextFactory);
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
                rec.AreaId = novaRandomLocation.SystemNumber.ParseInt();
                rec.Loc1 = novaRandomLocation.Car.ParseInt();
                rec.Loc2 = novaRandomLocation.Bin.ParseInt();
                rec.Loc3 = novaRandomLocation.Lvl.ParseInt();
                rec.Loc4 = novaRandomLocation.Prt.ParseInt();
                rec.Loc5 = 1;
                rec.Slot = string.Empty;
                rec.LocationCode = string.Empty;
                rec.SizeCodeId = _sizeCodeManager.Get(novaRandomLocation.Size).Id;
                rec.VelocityCodeId = _velocityCodeManager.Get(novaRandomLocation.Velocity).Id;
                rec.HeightCodeId = _heightCodeManager.Get(novaRandomLocation.Height).Id;
                rec.InUse = false;

                _repoLocation.Insert(rec);
            }
        }

        private void Modify(NovaRandomLocation novaRandomLocation)
        {
            //var rec = Get(novaRandomLocation);
            //if (rec != null)
            //{
            //    rec.AreaId = novaRandomLocation.SystemNumber.ParseInt();
            //    rec.Loc1 = novaRandomLocation.Car.ParseInt();
            //    rec.Loc2 = novaRandomLocation.Bin.ParseInt();
            //    rec.Loc3 = novaRandomLocation.Lvl.ParseInt();
            //    rec.Loc4 = novaRandomLocation.Prt.ParseInt();
            //    rec.SizeCodeId = _sizeCodeManager.Get(novaRandomLocation.Size).Id;
            //    rec.VelocityCodeId = _velocityCodeManager.Get(novaRandomLocation.Velocity).Id;
            //    rec.HeightCodeId = _heightCodeManager.Get(novaRandomLocation.Height).Id;
            //    rec.InUse = false;

            //    _repoLocation.Update(rec);
            //}

            var rec = Get(novaRandomLocation);
            if (rec != null)
            {
                // Parse values once to avoid repeated parsing
                var areaId = novaRandomLocation.SystemNumber.ParseInt();
                var loc1 = novaRandomLocation.Car.ParseInt();
                var loc2 = novaRandomLocation.Bin.ParseInt();
                var loc3 = novaRandomLocation.Lvl.ParseInt();
                var loc4 = novaRandomLocation.Prt.ParseInt();
                var sizeCodeId = _sizeCodeManager.Get(novaRandomLocation.Size).Id;
                var velocityCodeId = _velocityCodeManager.Get(novaRandomLocation.Velocity).Id;
                var heightCodeId = _heightCodeManager.Get(novaRandomLocation.Height).Id;
                // Update record
                UpdateLocationRecord(rec, areaId, loc1, loc2, loc3, loc4, sizeCodeId, velocityCodeId, heightCodeId);
                // Save changes
                _repoLocation.Update(rec);
            }
            else
            {
                // Log or handle the case where the record is not found
                Console.WriteLine(@"Record not found for the given location.");
            }
        }

        // Encapsulated update logic
        private void UpdateLocationRecord(
            Location rec,
            int areaId,
            int loc1,
            int loc2,
            int loc3,
            int loc4,
            int sizeCodeId,
            int velocityCodeId,
            int heightCodeId)
        {
            rec.AreaId = areaId;
            rec.Loc1 = loc1;
            rec.Loc2 = loc2;
            rec.Loc3 = loc3;
            rec.Loc4 = loc4;
            rec.SizeCodeId = sizeCodeId;
            rec.VelocityCodeId = velocityCodeId;
            rec.HeightCodeId = heightCodeId;
            rec.InUse = false;
        }


        private void Delete(NovaRandomLocation novaRandomLocation)
        {
            var rec = Get(novaRandomLocation);
            _repoLocation.Delete(rec.Id);
        }


        private Location Get(NovaRandomLocation novaRandomLocation)
        {
            // var rec = _repoLocation.All().FirstOrDefault(r => r.AreaId == novaRandomLocation.SystemNumber.ParseInt() && r.Loc1 == novaRandomLocation.Car.ParseInt() && r.Loc2 == novaRandomLocation.Bin.ParseInt() && r.Loc3 == novaRandomLocation.Lvl.ParseInt() && r.Loc4 == novaRandomLocation.Prt.ParseInt());

            // Parse values once to avoid repeated parsing
            var systemNumber = novaRandomLocation.SystemNumber.ParseInt();
            var car = novaRandomLocation.Car.ParseInt();
            var bin = novaRandomLocation.Bin.ParseInt();
            var lvl = novaRandomLocation.Lvl.ParseInt();
            var prt = novaRandomLocation.Prt.ParseInt();
            // Query the database efficiently
            var rec = _repoLocation.All(r => r.AreaId == systemNumber &&
                 r.AreaId == systemNumber &&
                                     r.Loc1 == car &&
                                     r.Loc2 == bin &&
                                     r.Loc3 == lvl &&
                                     r.Loc4 == prt).FirstOrDefault();


            return rec;
        }
    }
}
