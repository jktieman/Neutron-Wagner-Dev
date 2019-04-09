using AlliedLogger;
using JsonManager;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using SlotNameFactory;
using System;
using System.Linq;
using System.Windows.Forms;


namespace NeutronLoader
{
    public class LocationProcessor
    {
        private readonly GenericRepository<Location> repoLocation = new GenericRepository<Location>(new NeutronDb());
        private ISlotNameFactory _slotNameFactory;

        public Location GetOrCreate(HostOrder hostOrder)
        {
            var itemDef = new Location();
            try
            {
                itemDef = repoLocation.FindBy(r => r.Slot == hostOrder.PrimeBin).FirstOrDefault();
                if (itemDef == null)
                {
                    itemDef = CreateNewLocation(hostOrder);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Finding Location - {hostOrder.PrimeBin}.  {ex.Message} \r\n {ex.InnerException}");

            }
            return itemDef;
        }

        private Location CreateNewLocation(HostOrder hostOrder)
        {
            IJsonData jsonData = new JsonData();
            _slotNameFactory = new Type2SlotNameFactory();

            LocationDetail locDetail = _slotNameFactory.GetLocationDetails(hostOrder.PrimeBin).LocationDetail;

                var newDefinition = new Location();
            var def = new Location();
            try
            {
                def = jsonData.LoadFile<Location>();
                if (def != null)
                {
                    newDefinition = new Location
                    {
                        StationId = def.StationId
                        ,
                        Slot = hostOrder.PrimeBin
                        ,
                        Loc1 = locDetail.Loc1
                        ,
                        Loc2 = locDetail.Loc2
                        ,
                        Loc3 = locDetail.Loc3
                        ,
                        Loc4 = locDetail.Loc4
                        ,
                        Loc5 = locDetail.Loc5
                        ,
                        SizeCodeId = def.SizeCodeId
                        ,
                        VelocityCodeId = def.VelocityCodeId
                        ,
                        HeightCodeId = def.HeightCodeId
                        ,
                        LocationCodeId = def.LocationCodeId
                        ,
                        InUse = true
                    };
                    repoLocation.Insert(newDefinition);
                }
                else
                {
                    Logger.Log(msg: "No Default Location.  Create Location Failed.");
                    string msg = "A default Location must be set up in ";
                    msg += "order to create definitions during the Order Load process.";
                    msg += "The Slot number MUST be called, DEFAULT .";
                    MessageBox.Show(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to create New Location. " + ex.Message + ex.InnerException.Message);
            }
            return newDefinition;
        }

        private void CreateSlotNameFactory(string slotType)
        {
            switch (slotType)
            {
                case "Default":
                    _slotNameFactory = new DefaultSlotNameFactory();
                    break;
                case "T101-01-01":
                    _slotNameFactory = new Type1SlotNameFactory();
                    break;
                case "V101":
                    _slotNameFactory = new Type2SlotNameFactory();
                    break;
               default:
                    _slotNameFactory = new DefaultSlotNameFactory();
                    break;
            }
        }
    }
}
