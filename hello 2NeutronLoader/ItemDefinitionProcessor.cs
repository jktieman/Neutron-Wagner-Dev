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
                itemDef = _repoItemDefinition.FindBy(r => r.Item == partNum).FirstOrDefault();
                if (itemDef == null)
                {
                    itemDef = CreateNewItemDefinition(new HostOrder {PartNum = partNum, PartDesc = description });
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
            IJsonData jsonData = new JsonData();

            var newDefinition = new ItemDefinition();
            var def = new ItemDefinition();
            try
            {
                def = _jsonData.LoadFile<ItemDefinition>();
                if (def != null)
                {
                    newDefinition = new ItemDefinition
                    {
                        StationId = def.StationId
                        ,
                        Item = hostOrder.PartNum
                        ,
                        Description = hostOrder.PartDesc
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
                    string msg = "A default Item Definition must be set up in ";
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
           // IJsonData jsonData = new JsonData();

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
                    string msg = "A default Item Definition must be set up in ";
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
