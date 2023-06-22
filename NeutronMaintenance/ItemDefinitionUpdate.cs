using AlliedLogger;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronMaintenance.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeutronMaintenance
{
    public class ItemDefinitionUpdate
    {
        readonly NeutronDb db = new NeutronDb();

        public void ProcessItemDefinitions(List<ItemDefinitionLoad> itemDefinitions, DynamicLogger logger)
        {
            try
            {
                logger.Log($"ProcessItemDefinitions");
                ItemDefinition itemDefinition;
                foreach (var item in itemDefinitions)
                {
                    logger.Log($"Item Definition Start: {item.Item}  {item.Description}");
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        //No id new or from host
                        itemDefinition = db.ItemDefinitions.Where(r => r.Item == item.Item.Trim()).FirstOrDefault();
                    }
                    else
                    {
                        itemDefinition = db.ItemDefinitions.Find(item.Id.ParseInt());
                    }

                    if (itemDefinition != null)
                    {
                        itemDefinition.Description = item.Description;
                    }
                    else  //ADD a new ItemDefinition
                    {
                        itemDefinition = new ItemDefinition();
                        int areaId = item.AreaId;
                        if (areaId > 0)
                        {
                            itemDefinition.AreaId = areaId;
                            int unitOfIssueId = GetUnitOfIssueId(item.UnitOfIssue);
                            if (unitOfIssueId > 0)
                            {
                                itemDefinition.UnitOfIssueId = unitOfIssueId;
                                int sizeCodeId = GetSizeCodeId(item.SizeCode);
                                if (sizeCodeId > 0)
                                {
                                    itemDefinition.SizeCodeId = sizeCodeId;
                                    int velocityCodeId = GetVelocityCodeId(item.VelocityCode);
                                    if (velocityCodeId > 0)
                                    {
                                        itemDefinition.VelocityCodeId = velocityCodeId;

                                        if (string.IsNullOrEmpty(item.HeightCode))
                                        {
                                            itemDefinition.HeightCodeId = db.HeightCodes.FirstOrDefault().Id;
                                        }
                                        else
                                        {
                                            int heightCodeId = GetHeightCodeId(item.HeightCode);
                                            if (heightCodeId > 0)
                                            {
                                                itemDefinition.HeightCodeId = heightCodeId;
                                            }
                                        }
                                        int storageTypeId = GetStorageTypeId(item.StorageType);
                                        if (storageTypeId > 0)
                                        {
                                            itemDefinition.Item = item.Item;
                                            itemDefinition.StorageTypeId = storageTypeId;
                                            itemDefinition.Description = item.Description;
                                            itemDefinition.LocationMax = item.LocationMax.ParseInt();
                                            itemDefinition.LocationMin = item.LocationMin.ParseInt();
                                            itemDefinition.SystemMax = item.SystemMax.ParseInt();
                                            itemDefinition.SystemMin = item.SystemMin.ParseInt();
                                            itemDefinition.Weight = Convert.ToSingle(item.Weight);
                                            itemDefinition.Scale = Convert.ToBoolean(item.Scale);
                                            db.ItemDefinitions.Add(itemDefinition);
                                            db.SaveChanges();

                                        }
                                    }


                                }
                            }
                        }
                    }
                    logger.Log($"Item Definition End =====>>>  {item.Item}  {item.Description}");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Item Definition Error: {ex.Message} \r\n {ex.InnerException}");
            }
        }

        private int GetStorageTypeId(string storageTypeVar)
        {
            var storageType = db.StorageTypes.FirstOrDefault(s => s.Name.ToLower().Trim() == storageTypeVar.ToLower().Trim());
            if (storageType != null)
            {
                return storageType.Id;
            }
            return -1;
        }

        private int GetHeightCodeId(string heightCodeVar)
        {
            heightCodeVar = heightCodeVar.TrimStart('0');
            var heightCode = db.HeightCodes.FirstOrDefault(s => s.Name.ToLower().Trim() == heightCodeVar.ToLower().Trim());
            if (heightCode != null)
            {
                return heightCode.Id;
            }
            return -1;
        }

        private int GetVelocityCodeId(string velocityCodeVar)
        {
            velocityCodeVar = velocityCodeVar.TrimStart('0');
            var velocityCode = db.VelocityCodes.FirstOrDefault(s => s.Name.ToLower().Trim() == velocityCodeVar.ToLower().Trim());
            if (velocityCode != null)
            {
                return velocityCode.Id;
            }
            return -1;
        }

        private int GetSizeCodeId(string sizeCodeVar)
        {
            var scode = sizeCodeVar.TrimStart('0');
            var sizeCode = db.SizeCodes.FirstOrDefault(s => s.Name.ToLower().Trim() == scode.ToLower().Trim());
            if (sizeCode != null)
            {
                return sizeCode.Id;
            }
            return -1;
        }

        private int GetUnitOfIssueId(string unitOfIssueVar)
        {
            var unitOfIssue = db.UnitOfIssues.FirstOrDefault(s => s.Name.ToLower().Trim() == unitOfIssueVar.ToLower().Trim());
            if (unitOfIssue != null)
            {
                return unitOfIssue.Id;
            }
            return -1;
        }

        private int GetStationId(string stationVar)
        {
            var result = -1;
            //int.TryParse(stationVar, out var id);
            //// if it's a number, look for StationNumber
            //try
            //{
            //    Station station;
            //    if (stationVar.IsNumeric())
            //    {
            //        station = db.Stations.FirstOrDefault(s => s.StationNumber == id);
            //    }
            //    else  // if it's a string, look for Station Name
            //    {
            //        station = db.Stations.FirstOrDefault(s => s.Name.ToLower().Trim() == stationVar.ToLower().Trim());
            //    }
            //    if (station != null)
            //    {
            //        result = station.Id;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Station Error: {ex.Message} \r\n {ex.InnerException} \r\n {ex.InnerException.Message} \r\n {ex.InnerException.InnerException.Message}");

            //}
            return result;
        }
    }
}
