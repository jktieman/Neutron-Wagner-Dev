using AlliedLogger;
using JsonManager;
using NeutronCore;
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
        private readonly GenericRepository<Location> _repoLocation;
        private ISlotNameFactory _slotNameFactory;
        private readonly IJsonData _jsonData;
        private IDynamicLogger _logger;

        public LocationProcessor(IJsonData jsonData, Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            
            _jsonData = jsonData;
            _repoLocation = new GenericRepository<Location>(contextFactory);
            _logger = NeutronCore.Global.Logger.SetupLogger("LocationProcessor");
        }
        public Location GetOrCreate(HostOrder hostOrder)
        {
            var itemDef = new Location();
            try
            {
                itemDef = _repoLocation.FindBy(r => r.Slot == hostOrder.PrimeBin).FirstOrDefault() ?? CreateNewLocation(hostOrder);
            }
            catch (Exception ex)
            {
             _ = _logger.LogDetailAsync($"Error Finding Location - {hostOrder.PrimeBin}.  {ex.Message}{Environment.NewLine} {ex.InnerException}");

            }
            return itemDef;
        }

        private Location CreateNewLocation(HostOrder hostOrder)
        {
            _slotNameFactory = Type2SlotNameFactory.GetInstance();
             LocationDetail locDetail = _slotNameFactory.GetLocationDetails(hostOrder.PrimeBin).LocationDetail;

                var newDefinition = new Location();
            var def = new Location();
            try
            {
                def = _jsonData.LoadFile<Location>();
                if (def != null)
                {
                    newDefinition = new Location
                    {
                        AreaId = def.AreaId
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
                        LocationCode = def.LocationCode
                        ,
                        InUse = true
                    };
                    _repoLocation.Insert(newDefinition);
                }
                else
                {
                 _ = _logger.LogDetailAsync(msg: "No Default Location.  Create Location Failed.");
                    string msg = "A default Location must be set up in ";
                    msg += "order to create definitions during the Order Load process.";
                    msg += "The Slot number MUST be called, DEFAULT .";
                    MessageBox.Show(msg);
                }
            }
            catch (Exception ex)
            {
             _ = _logger.LogDetailAsync($"Unable to create New Location.  {ex.Message} {Environment.NewLine}{ex.InnerException.Message}");
            }
            return newDefinition;
        }

        private void CreateSlotNameFactory(string slotType)
        {
            switch (slotType)
            {
                case "Default":
                    _slotNameFactory = DefaultSlotNameFactory.GetInstance();
                    break;
                case "T101-01-01":
                    _slotNameFactory = Type1SlotNameFactory.GetInstance();
                    break;
                case "V101":
                    _slotNameFactory = Type2SlotNameFactory.GetInstance();
                    break;
               default:
                    _slotNameFactory = DefaultSlotNameFactory.GetInstance();
                    break;
            }
        }
    }
}
