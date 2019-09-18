using AlliedLogger;
using JsonManager;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.Models.Lookups;
//using Remotion.Mixins.CodeGeneration.DynamicProxy;

namespace NeutronLoader
{
    public class ItemDefinitionProcessor
    {
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly IJsonData _jsonData;

        public ItemDefinitionProcessor(IJsonData jsonData)
        {
            _jsonData = jsonData;
        }

        public ItemDefinition GetOrCreate(HostOrder hostOrder)
        {
            ItemDefinition itemDef = null;
            try
            {
                itemDef = _repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                if (itemDef == null)
                {
                    itemDef = CreateNewItemDefinition(hostOrder);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Finding Item Definition - {hostOrder.PartNum}.  {ex.Message} \r\n {ex.InnerException}");

            }
            return itemDef;
        }

        public ItemDefinition GetOrCreate(string partNum, string description)
        {
            ItemDefinition itemDef = null;
            try
            {
                itemDef = _repoItemDefinition.FindBy(r => r.Item.ToLower().Trim() == partNum.ToLower().Trim()).FirstOrDefault();
                if (itemDef == null)
                {
                    itemDef = CreateNewItemDefinition(new HostOrder { PartNum = partNum, PartDesc = description });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Finding Item Definition - {partNum}.  {ex.Message} \r\n {ex.InnerException}");

            }
            return itemDef;
        }

        public ItemDefinition GetOrCreate(string partNum, string description, int stationId)
        {
            ItemDefinition itemDef = null;
            try
            {
                itemDef = _repoItemDefinition.FindBy(r => r.Item == partNum
                && r.StationId == stationId).FirstOrDefault();
                if (itemDef == null)
                {

                    itemDef = CreateNewItemDefinition(partNum, description, stationId);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Finding Item Definition - {partNum}.  {ex.Message} \r\n {ex.InnerException}");

            }
            return itemDef;
        }

        private ItemDefinition CreateNewItemDefinition(HostOrder hostOrder)
        {

            var newDefinition = new ItemDefinition();
            try
            {
                var def = _jsonData.LoadFile<ItemDefinition>();
                var stationId = 0;
                var sizeCodeId = 0;
                var velocityCodeId = 0;
                var heightCodeId = 0;
                var locationCodeId = 0;
                var unitOfIssueId = 0;
                var storageTypeId = 0;

                using (var db = new NeutronDb())
                {
                    //Station
                    var sta = db.Stations.Find(def.StationId);
                    if (sta != null)
                    {
                        stationId = sta.Id;
                    }
                    else
                    {
                        MessageBox.Show(@"Invalid Station Setup in default Item Definition.", @"Invalid Station",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    //SizeCode
                    var size = db.SizeCodes.Find(def.SizeCodeId);
                    if (size != null)
                    {
                        sizeCodeId = size.Id;
                    }
                    else
                    {
                        var size2 = db.SizeCodes.FirstOrDefault();
                        if (size2 != null)
                        {
                            sizeCodeId = size2.Id;
                        }
                        else
                        {
                            // Create SizeCode
                            var sizeCode = new SizeCode {Name = "Unknown", Sequence = 10};
                            db.SizeCodes.Add(sizeCode);
                            db.SaveChanges();
                            sizeCodeId = sizeCode.Id;
                        }
                    }
                    //VelocityCode
                    var vel = db.VelocityCodes.Find(def.VelocityCodeId);
                    if (vel != null)
                    {
                        velocityCodeId = vel.Id;
                    }
                    else
                    {
                        var vel2 = db.VelocityCodes.FirstOrDefault();
                        if (vel2 != null)
                        {
                            velocityCodeId = vel2.Id;
                        }
                        else
                        {
                            // Create SizeCode
                            var velocityCode = new VelocityCode() { Name = "Unknown", Sequence = 10 };
                            db.VelocityCodes.Add(velocityCode);
                            db.SaveChanges();
                            velocityCodeId = velocityCode.Id;
                        }
                    }
                    //HeightCode
                    var height = db.HeightCodes.Find(def.HeightCodeId);
                    if (height != null)
                    {
                        heightCodeId = height.Id;
                    }
                    else
                    {
                        var height2 = db.HeightCodes.FirstOrDefault();
                        if (height2 != null)
                        {
                            heightCodeId = height2.Id;
                        }
                        else
                        {
                            // Create HeightCode
                            var heightCode = new HeightCode { Name = "Unknown", Sequence = 10 };
                            db.HeightCodes.Add(heightCode);
                            db.SaveChanges();
                            heightCodeId = heightCode.Id;
                        }
                    }
                    //LocationCode
                    var loc = db.LocationCodes.Find(def.LocationCodeId);
                    if (loc != null)
                    {
                        locationCodeId = loc.Id;
                    }
                    else
                    {
                        var loc2 = db.LocationCodes.FirstOrDefault();
                        if (loc2 != null)
                        {
                            locationCodeId = loc2.Id;
                        }
                        else
                        {
                            // Create LocationCode
                            var locationCode = new LocationCode { Name = "Unknown", Sequence = 10 };
                            db.LocationCodes.Add(locationCode);
                            db.SaveChanges();
                            locationCodeId = locationCode.Id;
                        }
                    }
                    //UnitOfIssueCode
                    var unit = db.UnitOfIssues.Find(def.UnitOfIssueId);
                    if (unit != null)
                    {
                        unitOfIssueId = unit.Id;
                    }
                    else
                    {
                        var unit2 = db.UnitOfIssues.FirstOrDefault();
                        if (unit2 != null)
                        {
                            unitOfIssueId = unit2.Id;
                        }
                        else
                        {
                            // Create UnitOfIssue
                            var unitCode = new UnitOfIssue { Name = "Unknown", Sequence = 10 };
                            db.UnitOfIssues.Add(unitCode);
                            db.SaveChanges();
                            unitOfIssueId = unitCode.Id;
                        }
                    }
                    //StorageType
                    var sto = db.StorageTypes.Find(def.StorageTypeId);
                    if (sto != null)
                    {
                        storageTypeId = sto.Id;
                    }
                    else
                    {
                        var sto2 = db.StorageTypes.FirstOrDefault();
                        if (sto2 != null)
                        {
                            storageTypeId = sto2.Id;
                        }
                        else
                        {
                            // Create StorageType
                            var storageType = new StorageType { Name = "Unknown", Sequence = 10 };
                            db.StorageTypes.Add(storageType);
                            db.SaveChanges();
                            storageTypeId = storageType.Id;
                        }
                    }
                }



                if (def != null)
                {
                    newDefinition = new ItemDefinition
                    {
                        StationId = stationId
                        , Item = hostOrder.PartNum
                        , Description = hostOrder.PartDesc
                        , LocationMax = def.LocationMax
                        , LocationMin = def.LocationMin
                        , SystemMax = def.SystemMax
                        , SystemMin = def.SystemMin
                        , Weight = def.Weight
                        , Scale = def.Scale
                        , StorageTypeId = storageTypeId
                        , UnitOfIssueId = unitOfIssueId
                        , SizeCodeId = sizeCodeId
                        , VelocityCodeId = velocityCodeId
                        , HeightCodeId = heightCodeId
                        , LocationCodeId = locationCodeId
                    };
                    _repoItemDefinition.Insert(newDefinition);
                }
                else
                {
                    Logger.Log(msg: "No Default Item Definition.  Create Item Definition Failed.");
                    var msg = "A default Item Definition must be set up in ";
                    msg += "order to create definitions during the Order Load process.";
                    msg += "The Item Number MUST be called, DEFAULT .";
                    MessageBox.Show(msg);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Logger.Log("Unable to create New Item Definition. " + ex.Message + ex.InnerException.Message);
            }
            return newDefinition;
        }

        private ItemDefinition CreateNewItemDefinition(string partNum, string partDesc, int stationId)
        {

            var newDefinition = new ItemDefinition();
            var def = new ItemDefinition();
            try
            {
                def = _jsonData.LoadFile<ItemDefinition>();
                if (def != null)
                {
                    newDefinition = new ItemDefinition
                    {
                        StationId = stationId
                        ,
                        Item = partNum
                        ,
                        Description = partDesc
                        ,
                        LocationMax = def.LocationMax
                        ,
                        LocationMin = def.LocationMin
                        ,
                        SystemMax = def.SystemMax
                        ,
                        SystemMin = def.SystemMin
                        ,
                        Weight = def.Weight
                        ,
                        Scale = def.Scale
                        ,
                        StorageTypeId = def.StorageTypeId
                        ,
                        UnitOfIssueId = def.UnitOfIssueId
                        ,
                        SizeCodeId = def.SizeCodeId
                        ,
                        VelocityCodeId = def.VelocityCodeId
                        ,
                        HeightCodeId = def.HeightCodeId
                        ,
                        LocationCodeId = def.LocationCodeId
                    };
                    _repoItemDefinition.Insert(newDefinition);

                }
                else
                {
                    Logger.Log(msg: "No Default Item Definition.  Create Item Definition Failed.");
                    var msg = "A default Item Definition must be set up in ";
                    msg += "order to create definitions during the Order Load process.";
                    msg += "The Item Number MUST be called, DEFAULT .";
                    MessageBox.Show(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to create New Item Definition. " + ex.Message + ex.InnerException.Message);
            }
            return newDefinition;
        }
    }
}
