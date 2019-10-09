using AlliedLogger;
using NeutronCore;
using NeutronCore.Extensions;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JsonManager;
using NeutronCore.Global;

namespace NeutronLoader
{
    public class Pr1FileProcessor
    {
        private readonly GenericRepository<Order> repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Station> repoStation = new GenericRepository<Station>(new NeutronDb());
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly DynamicLogger _logger;
        private readonly IJsonData _jsonData;

        public Pr1FileProcessor(List<FileInfo> files, NeutronVariables neutronVariables, NeutronLicense neutronLicense, DynamicLogger logger, IJsonData jsonData)
        {
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _logger = logger;
            _jsonData = jsonData;
            logger.Log($"Pr1 File Processor - File Count: {files.Count()} ");

            ProcessFiles(files);
            
        }

        private void ProcessFiles(IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            {
                _logger.Log($"File To Process: {file.FullName}");

                try
                {
                    var allLines = File.ReadAllLines(file.FullName);
                    if (allLines.Length > 0)
                    {
                        _logger.Log($"Lines in File: {allLines.Count()}");
                        if (allLines.First().Contains("REPLENOPRP"))
                        {
                            _logger.Log($"First Line Contains: REPLENOPRP");
                            ProcessReplenOrder(allLines);
                        }
                        else
                        {
                            _logger.Log($"Normal Order");
                            ProcessNormalOrder(allLines);
                        }
                    }
                    ArchiveFile.Archive(file);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error Reading All Order Lines.  File Name: {file.FullName} \r\n {ex.Message} \r\n {ex.InnerException}");
                }
            }
            _logger.Log($"Pr1FileProcessor --- Done");
        }

        private void ProcessNormalOrder(string[] allLines)
        {
            _logger.Log($"In ProcessNormalOrder - Line Count: {allLines.Length} ");
            Order order = null;
            var orderId = 0;
            OrderDetail detail = null;

            for (var i = 0; i < allLines.Count(); i++)
            {
                var line = allLines[i];
                var lineType = line.Substring(0, 1);
                if (lineType == "2")
                {
                    if (order != null)
                    {
                        //time to write the Order record and get Order Id.
                        try
                        {
                            repoOrder.Insert(order);
                            orderId = order.Id;
                            order = null;
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Error Inserting Order Line. \r\n  {ex.Message} \r\n {ex.InnerException}");
                        }
                    }
                }

                switch (lineType)
                {
                    case "1":
                        var odr = line.Substring(8, 10).Trim();
                        odr = string.IsNullOrEmpty(odr) ? "CUSUNKNOWN" : line.Substring(8, 10).Trim();

                        order = new Order();
                        order.Ord1 = odr;
                        order.Ord2 = line.Substring(19, 10).Trim();
                        order.Priority = line.Substring(31, 2).ParseInt();
                        order.LoadDate = DateTime.Now;
                        order.OrderStatusId = 1;
                        order.ShipMethodId = 1;
                        order.ShipperId = 1;
                        break;
                    case "2":
                        if (detail != null)
                        {
                            try
                            {
                                repoOrderDetail.Insert(detail);
                                detail = null;
                            }
                            catch (Exception ex)
                            {
                                _logger.Log($"Error Inserting Order Detail Line.  \r\n  {ex.Message} \r\n {ex.InnerException}");
                            }
                        }

                        ItemDefinition itemDef;

                        var partNum = line.Substring(2, 35).Trim();
                        var description = line.Substring(74, 30).Trim();

                        if (line.Substring(48, 1) == "O")
                        {

                            itemDef = GetItemDefinition(partNum, description, "O");

                            if (_neutronVariables.UpdateItemDefinitionDescription)
                            {
                                itemDef = UpdateItemDefinitionDescription(itemDef, description);
                            }

                             var primeBin = "OC";
                            detail = new OrderDetail
                            {
                                OrderId = orderId,
                                ItemDefinitionId = itemDef.Id,
                                PartNum = partNum.Trim(),
                                Quantity = line.Substring(38, 9).ParseInt(),
                                PrimeBin = primeBin,
                                PartDesc = description.Trim(),
                                OrderDetailInfo = line.Substring(105, 100).Trim(),
                                StationNumber = 8,
                                LineStatusId = 1,
                                PickedQuantity = 0
                            };
                        }
                        else
                        {
                            itemDef = GetItemDefinition(partNum, description);

                            if (_neutronVariables.UpdateItemDefinitionDescription)
                            {
                                itemDef = UpdateItemDefinitionDescription(itemDef, description);
                            }

                            _logger.Log($"Station Id: {itemDef.StationId}");
                            var stationNumber = repoStation.FindByKey(itemDef.StationId).StationNumber;
                            var primeBin = GetPrimeBin(stationNumber, line.Substring(55, 11));
                            detail = new OrderDetail
                            {
                                OrderId = orderId,
                                ItemDefinitionId = itemDef.Id,
                                PartNum = partNum.Trim(),
                                Quantity = line.Substring(38, 9).ParseInt(),
                                PrimeBin = primeBin,
                                PartDesc = description.Trim(),
                                OrderDetailInfo = line.Substring(105).Trim(),
                                StationNumber = stationNumber,
                                LineStatusId = 1,
                                PickedQuantity = 0
                            };
                        }
                        break;
                    case "3":
                        if (order != null) order.OrderInfo = line.Trim();
                        break;
                    case "4":
                        if (order != null) order.OrderInfo += line.Trim();
                        break;
                    case "5":
                        if (order != null) order.OrderInfo += line.Trim();
                        break;
                    case "6":
                        if (order != null) order.OrderInfo += line.Trim();
                        break;
                    case "7":
                        if (order != null) order.OrderInfo += line.Trim();
                        break;
                    case "8":
                        if (detail != null) detail.OrderDetailInfo += line.Trim();
                        break;
                    case "9":
                        if (detail != null) detail.OrderDetailInfo += line.Trim();
                        break;
                    default:
                        break;

                }

                if (i == allLines.Length - 1)
                {
                    //last record in file.  Write the last OrderDetail to database.
                    try
                    {
                        if (detail != null)
                        {
                            repoOrderDetail.Insert(detail);
                            detail = null;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Error Saving Order Detail Line.  \r\n  {ex.Message} \r\n {ex.InnerException}");
                    }
                }
            }
        }

        private void ProcessReplenOrder(string[] allLines)
        {
            _logger.Log($"Process Replen Order Line Count: {allLines.Length} ");
            ReplenOrder order = null;
            int orderId = 0;
            ReplenOrderDetail replenDetail = null;
            string line = "";

            for (int i = 0; i < allLines.Length; i++)
            {
                line = allLines[i];
               
                string lineType = line.Substring(0, 1);
                if (lineType == "2")
                {
                    if (order != null)
                    {
                        //time to write the Order record and get Order Id.
                        try
                        {
                            repoReplenOrder.Insert(order);
                            orderId = order.Id;
                            order = null;
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Error Inserting Order Line: {line} \r\n  {ex.Message} \r\n {ex.InnerException}");
                        }
                    }
                    //string oc = line.Substring(48, 1);
                    //if (oc == "O")
                    //{
                    //    continue;
                    //}
                }


                switch (lineType)
                {
                    case "1":
                        order = new ReplenOrder();
                        order.Ord1 = line.Substring(8, 10) == "REPLENOPRP" ? string.Format(@"R{0}", line.Substring(20, 9)) : line.Substring(8, 10);
                        order.Ord2 = line.Substring(19, 10);
                        order.Priority = line.Substring(31, 2).ParseInt();
                        order.LoadDate = DateTime.Now;
                        order.OrderStatusId = 1;
                        order.ShipMethodId = 1;
                        order.ShipperId = 1;
                        break;
                    case "2":
                        if (replenDetail != null)
                        {
                            try
                            {
                                repoReplenOrderDetail.Insert(replenDetail);
                                replenDetail = null;
                            }
                            catch (Exception ex)
                            {
                                _logger.Log($"Error Inserting Order replenDetail Line. {line} \r\n  {ex.Message} \r\n {ex.InnerException}");
                            }
                        }
                        if (line.Substring(48, 1) == "O")
                        {
                            // Off Carousel not used to pick only for tote label logic routing
                            continue;
                        }

                        ItemDefinition itemDef = null;

                        string partNum = line.Substring(2, 35);
                        string description = line.Substring(74, 30);

                        itemDef = GetItemDefinition(partNum, description);

                        if (_neutronVariables.UpdateItemDefinitionDescription)
                        {
                            itemDef = UpdateItemDefinitionDescription(itemDef, description);
                        }

                        int stationNumber = GetStationNumber(itemDef.StationId);
                        string primeBin = GetPrimeBin(stationNumber, line.Substring(55, 11));
                        replenDetail = new ReplenOrderDetail();
                        replenDetail.ReplenOrderId = orderId;
                        replenDetail.ItemDefinitionId = itemDef.Id;
                        replenDetail.PartNum = partNum;
                        replenDetail.Quantity = line.Substring(38, 9).ParseInt();
                        replenDetail.PrimeBin = primeBin;
                        replenDetail.PartDesc = description;
                        replenDetail.OrderDetailInfo = line.Substring(105, 100);
                        replenDetail.StationNumber = stationNumber;
                        replenDetail.LineStatusId = 1;
                        replenDetail.PickedQuantity = 0;
                        break;
                    case "3":
                        order.OrderInfo = line;
                        break;
                    case "4":
                        order.OrderInfo += line;
                        break;
                    case "5":
                        order.OrderInfo += line;
                        break;
                    case "6":
                        order.OrderInfo += line;
                        break;
                    case "7":
                        order.OrderInfo += line;
                        break;
                    case "8":
                        replenDetail.OrderDetailInfo += line;
                        break;
                    case "9":
                        replenDetail.OrderDetailInfo += line;
                        break;
                    default:
                        break;

                }

                if (i == allLines.Count() - 1)
                {
                    //last record in file.  Write the last OrderDetail to database.
                    try
                    {
                        repoReplenOrderDetail.Insert(replenDetail);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Error Saving Order replenDetail Line. {line} \r\n   {ex.Message} \r\n {ex.InnerException}");
                    }
                }
            }
        }

        private ItemDefinition GetItemDefinition(string partNum, string description, string oc = "C")
        {
            ItemDefinition item = null;
            if (oc == "O")  //force to Off Carousel Station 8 at Dexter
            {
                _logger.Log(msg: "GetItemDefinition oc = 0  ");
                try
                {
                    int stationId = GetStationId(stationNumber: 8);
                    //try to find it anywhere first
                    item = repoItemDefinition.FindBy(r => r.Item.ToLower().Trim() == partNum.ToLower().Trim()).FirstOrDefault();
                    if (item == null)
                    {
                        //didn't find it anywhere so create it in Station 8, OC
                        item = new ItemDefinitionProcessor(_jsonData).GetOrCreate(partNum, description, stationId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error Finding Item Definition 1. {ex.Message} \r\n {ex.InnerException}");
                }
            }
            else
            {
                try
                {
                    item = repoItemDefinition.FindBy(r => r.Item.ToLower().Trim() == partNum.ToLower().Trim()).FirstOrDefault();
                    if (item == null)
                    {
                        item = new ItemDefinitionProcessor(_jsonData).GetOrCreate(partNum, description);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Finding Item Definition 1. {ex.Message} \r\n {ex.InnerException}");
                }
            }


            return item;
        }

        private ItemDefinition GetItemDefinition(string partNum, string description)
        {
            var sku = partNum.ToLower().Trim();
            var des = description;

            ItemDefinition item = null;
            {
                try
                {
                    item = repoItemDefinition.FindBy(r => r.Item.ToLower().Trim() == sku).FirstOrDefault() ?? new ItemDefinitionProcessor(_jsonData).GetOrCreate(sku, des);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Finding Item Definition 1. {ex.Message}  {ex.InnerException}");
                }
            }
            return item;
        }

        private ItemDefinition UpdateItemDefinitionDescription(ItemDefinition itemDefinition, string description)
        {
            var itemDef = repoItemDefinition.FindByKey(itemDefinition.Id);
            if (itemDef != null)
            {
                itemDef.Description = description;
                repoItemDefinition.Update(itemDef);
            }
            return itemDef;
        }

        private int GetStationNumber(int stationId)
        {
            int stationNum = 0;

            try
            {
                Station station = repoStation.FindBy(r => r.Id == stationId).FirstOrDefault();
               
                if (station != null)
                {
                    _logger.Log($"Get Station Number: {station.StationNumber}  StationId: {stationId} ");
                    stationNum = station.StationNumber;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Returning Station Number. {ex.Message} \r\n {ex.InnerException}");
            }
            _logger.Log($"Get Station Number Return: {stationNum}  StationId: {stationId} ");
            return stationNum;
        }

        private int GetStationId(int stationNumber)
        {
            int stationId = 0;

            try
            {
                Station station = repoStation.FindBy(r => r.StationNumber == stationNumber).FirstOrDefault();
                if (station != null)
                {
                    stationId = station.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Returning Station Id. {ex.Message} \r\n {ex.InnerException}");
            }
            return stationId;
        }

        private string GetPrimeBin(int stationNum, string v)
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(v.Trim()))
            {
                if (v.Length >= 11)
                {
                    result.Append(stationNum.ToString());
                    result.Append($"{v.Substring(0, 2)}");
                    result.Append($"{v.Substring(2, 3)}");
                    result.Append($"{v.Substring(6, 2)}");
                    result.Append($"{v.Substring(9, 2)}");
                }
            }
            return result.ToString();
        }
    }
}
