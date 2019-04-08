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
                        int stationId = GetStationId(item.Station);
                        if (stationId > 0)
                        {
                            itemDefinition.StationId = stationId;
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
                                        if (string.IsNullOrEmpty(item.LocationCode))
                                        {
                                            itemDefinition.LocationCodeId = db.LocationCodes.FirstOrDefault().Id;
                                        }
                                        else
                                        {
                                            int locationCodeId = GetLocationCodeId(item.LocationCode);
                                            if (locationCodeId > 0)
                                            {
                                                itemDefinition.LocationCodeId = locationCodeId;
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
            StorageType storageType;
            storageType = db.StorageTypes.Where(s => s.Name.ToLower().Trim() == storageTypeVar.ToLower().Trim()).FirstOrDefault();
            if (storageType != null)
            {
                return storageType.Id;
            }
            else
            {
                return -1;
            }
        }

        private int GetLocationCodeId(string locationCodeVar)
        {
            LocationCode locationCode;
            locationCodeVar = locationCodeVar.TrimStart('0');
            locationCode = db.LocationCodes.Where(s => s.Name.ToLower().Trim() == locationCodeVar.ToLower().Trim()).FirstOrDefault();
            if (locationCode != null)
            {
                return locationCode.Id;
            }
            else
            {
                return -1;
            }
        }

        private int GetHeightCodeId(string heightCodeVar)
        {
            HeightCode heightCode;
            heightCodeVar = heightCodeVar.TrimStart('0');
            heightCode = db.HeightCodes.Where(s => s.Name.ToLower().Trim() == heightCodeVar.ToLower().Trim()).FirstOrDefault();
            if (heightCode != null)
            {
                return heightCode.Id;
            }
            else
            {
                return -1;
            }
        }

        private int GetVelocityCodeId(string velocityCodeVar)
        {
            VelocityCode velocityCode;
            velocityCodeVar = velocityCodeVar.TrimStart('0');
            velocityCode = db.VelocityCodes.Where(s => s.Name.ToLower().Trim() == velocityCodeVar.ToLower().Trim()).FirstOrDefault();
            if (velocityCode != null)
            {
                return velocityCode.Id;
            }
            else
            {
                return -1;
            }
        }

        private int GetSizeCodeId(string sizeCodeVar)
        {
            string scode = sizeCodeVar.TrimStart('0');
            SizeCode sizeCode;
            sizeCode = db.SizeCodes.Where(s => s.Name.ToLower().Trim() == scode.ToLower().Trim()).FirstOrDefault();
            if (sizeCode != null)
            {
                return sizeCode.Id;
            }
            else
            {
                return -1;
            }
        }

        private int GetUnitOfIssueId(string unitOfIssueVar)
        {
            UnitOfIssue unitOfIssue;
            unitOfIssue = db.UnitOfIssues.Where(s => s.Name.ToLower().Trim() == unitOfIssueVar.ToLower().Trim()).FirstOrDefault();
            if (unitOfIssue != null)
            {
                return unitOfIssue.Id;
            }
            else
            {
                return -1;
            }
        }

        private int GetStationId(string stationVar)
        {
            Station station;
            int result = -1;
            int id;
            int.TryParse(stationVar, out id);
            // if it's a number, look for StationNumber
            try
            {
                if (stationVar.IsNumeric())
                {
                    station = db.Stations.Where(s => s.StationNumber == id).FirstOrDefault();
                }
                else  // if it's a string, look for Station Name
                {
                    station = db.Stations.Where(s => s.Name.ToLower().Trim() == stationVar.ToLower().Trim()).FirstOrDefault();
                }
                if (station != null)
                {
                    result = station.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Station Error: {ex.Message} \r\n {ex.InnerException} \r\n {ex.InnerException.Message} \r\n {ex.InnerException.InnerException.Message}");

            }
            return result;
        }
    }
}
