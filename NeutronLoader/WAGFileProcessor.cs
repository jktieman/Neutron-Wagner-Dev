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
using NeutronCore.Enums;
using NeutronCore.Global;
using NeutronData.ModelViews;

namespace NeutronLoader
{
    // ReSharper disable once InconsistentNaming
    public class WAGFileProcessor : IFileProcessor
    {
        private readonly GenericRepository<Order> _repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IDynamicLogger _logger;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;

        public WAGFileProcessor(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView)
        {
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            _logger = NeutronCore.Global.Logger.SetupLogger("FileProcessor");
        }

        public void LoadFiles(List<FileInfo> files)
        {
            foreach (var file in files)
            {
                _logger.LogDetailAsync($"File To Process: {file.FullName}");

                try
                {
                    var allLines = File.ReadAllLines(file.FullName);
                    if (allLines.Length > 0)
                    {
                        _logger.LogDetailAsync($"Lines in File: {allLines.Count()}");
                        if (allLines.First().Contains("REPLENOPRP"))
                        {
                            _logger.LogDetailAsync($"First Line Contains: REPLENOPRP");
                            ProcessReplenOrder(allLines);
                        }
                        else
                        {
                            _logger.LogDetailAsync($"Normal Order");
                            ProcessNormalOrder(allLines);
                        }
                    }
                    ArchiveFile.Archive(file, _logger);
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync($"Error Reading All Order Lines.  File Name: {file.FullName} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            _logger.LogDetailAsync($"Pr1FileProcessor --- Done");
        }

        public void ProcessNormalOrder(string[] allLines)
        {
            _logger.LogDetailAsync($"In ProcessNormalOrder - Line Count: {allLines.Length} ");
            Order order = null;
            var orderId = 0;
            OrderDetail detail = null;

            for (var i = 0; i < allLines.Count(); i++)
            {
                var line = allLines[i].PadRight(1000,' ');
                var lineType = line.Substring(0, 1);
                if (lineType == "2")
                {
                    if (order != null)
                    {
                        //time to write the Order record and get Order Id.
                        try
                        {
                            _repoOrder.Insert(order);
                            orderId = order.Id;
                            order = null;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDetailAsync($"Error Inserting Order Line. {Environment.NewLine}  {ex.Message} {Environment.NewLine} {ex.InnerException}");
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
                        order.OrderStatusId = (int)OrderStatus.Available;
                        order.ShipMethodId = 1;
                        order.ShipperId = 1;
                        break;
                    case "2":
                        if (detail != null)
                        {
                            try
                            {
                                _repoOrderDetail.Insert(detail);
                                detail = null;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDetailAsync($"Error Inserting Order Detail Line.  {Environment.NewLine}  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                            }
                        }

                        ItemDefinition itemDef;

                        var partNum = line.Substring(2, 35).Trim();
                        var description = line.Substring(74, 30).Trim();

                        if (line.Substring(48, 1) == "O")  //Force to Off Carousel
                        {

                            itemDef = GetItemDefinition(partNum, description, "O");

                            if (_neutronVariables.UpdateItemDefinitionDescription)
                            {
                                itemDef = UpdateItemDefinitionDescription(itemDef, description);
                            }
                            else
                            {
                                description = itemDef.Description;
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
                                OrderDetailInfo = line.Substring(131).Trim(),
                                AreaId = itemDef.AreaId,
                                LineStatusId = (int)LineStatus.Available,
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
                            else
                            {
                                description = itemDef.Description;
                            }

                            _logger.LogDetailAsync($"Station Id: {itemDef.AreaId}");

                            var areaId = itemDef.AreaId;
                            var primeBin = GetPrimeBin(areaId, line.Substring(55, 11));
                            detail = new OrderDetail
                            {
                                OrderId = orderId,
                                ItemDefinitionId = itemDef.Id,
                                PartNum = partNum.Trim(),
                                Quantity = line.Substring(38, 9).ParseInt(),
                                PrimeBin = primeBin,
                                PartDesc = description.Trim(),
                                OrderDetailInfo = line.Substring(125).Trim(),
                                AreaId = areaId,
                                LineStatusId = (int)LineStatus.Available,
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
                            _repoOrderDetail.Insert(detail);
                            detail = null;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogDetailAsync($"Error Saving Order Detail Line.  {Environment.NewLine}  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    }
                }
            }
        }

        public void ProcessReplenOrder(string[] allLines)
        {
            _logger.LogDetailAsync($"Process Replen Order Line Count: {allLines.Length} ");
            ReplenOrder order = null;
            int orderId = 0;
            ReplenOrderDetail replenDetail = null;
            string line = "";

            for (int i = 0; i < allLines.Length; i++)
            {
                line = allLines[i].PadRight(1000,' ');

                string lineType = line.Substring(0, 1);
                if (lineType == "2")
                {
                    if (order != null)
                    {
                        //time to write the Order record and get Order Id.
                        try
                        {
                            _repoReplenOrder.Insert(order);
                            orderId = order.Id;
                            order = null;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDetailAsync($"Error Inserting Order Line: {line} {Environment.NewLine}  {ex.Message} {Environment.NewLine} {ex.InnerException}");
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
                        order.OrderStatusId = (int)LineStatus.Available;
                        order.ShipMethodId = 1;
                        order.ShipperId = 1;
                        break;
                    case "2":
                        if (replenDetail != null)
                        {
                            try
                            {
                                _repoReplenOrderDetail.Insert(replenDetail);
                                replenDetail = null;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDetailAsync($"Error Inserting Order replenDetail Line. {line} {Environment.NewLine}  {ex.Message} {Environment.NewLine} {ex.InnerException}");
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

                        int areaId = itemDef.AreaId;
                        string primeBin = GetPrimeBin(areaId, line.Substring(55, 11));
                        replenDetail = new ReplenOrderDetail();
                        replenDetail.ReplenOrderId = orderId;
                        replenDetail.ItemDefinitionId = itemDef.Id;
                        replenDetail.PartNum = itemDef.Item;
                        replenDetail.Quantity = line.Substring(38, 9).ParseInt();
                        replenDetail.PrimeBin = primeBin;
                        replenDetail.PartDesc = itemDef.Description;
                        replenDetail.OrderDetailInfo =
                            line.Length >= 205 ? line.Substring(105, 100) : line.Substring(105);
                        replenDetail.AreaId = areaId;
                        replenDetail.LineStatusId = (int)LineStatus.Available;
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
                        _repoReplenOrderDetail.Insert(replenDetail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDetailAsync($"Error Saving Order replenDetail Line. {line} {Environment.NewLine}   {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    }
                }
            }
        }

        private void ProcessReplenOrderNewNotUsed(string[] allLines)
        {
            _logger.LogDetailAsync($"Process Replen Order Line Count: {allLines.Length} ");
            var orders = new List<ReplenOrder>();
            var order = new ReplenOrder();
            var orderDetail = new ReplenOrderDetail();
            var orderInfo = new StringBuilder();
            var orderDetailInfo = new StringBuilder();
            var orderId = 0;
            var replenOrderDetails = new List<ReplenOrderDetail>();
            var orderWorking = false;
            var lineWorking = false;

            for (var i = 0; i < allLines.Length; i++)
            {
                var line = allLines[i];
                var lineType = line.Substring(0, 1);

                if (lineType == "1")
                {
                    if (orderWorking)
                    {
                        //Build the order
                        order.OrderInfo = orderInfo.ToString();
                        _repoReplenOrder.Insert(order);
                        orderId = order.Id;
                        foreach (var detail in replenOrderDetails)
                        {
                            detail.ReplenOrderId = orderId;
                            _repoReplenOrderDetail.Insert(detail);
                        }
                        orderWorking = false;
                    }

                    if (!orderWorking)
                    {
                        replenOrderDetails = new List<ReplenOrderDetail>();
                        orderInfo = new StringBuilder();
                        orderDetailInfo = new StringBuilder();
                        order = CreateOrder(line);
                        orderWorking = true;
                    }

                }

                if (lineType == "3"
                    || lineType == "4"
                    || lineType == "5"
                    || lineType == "6"
                    || lineType == "7")
                {
                    orderInfo.AppendLine(line);
                }


                if (lineType == "2")
                {
                    // build the line
                    if (lineWorking)
                    {
                        orderDetail.OrderDetailInfo = orderDetailInfo.ToString();
                        replenOrderDetails.Add(orderDetail);
                        lineWorking = false;
                    }

                    if (!lineWorking)
                    {
                        orderDetail = CreateOrderDetail(line);
                        lineWorking = true;
                    }

                }

                if (lineType == "8"
                    || lineType == "9")
                {
                    orderDetailInfo.AppendLine(line);
                }
            }

            if (order != null)
            {
                // add the last detail record
                if (orderDetail != null)
                {
                    orderDetail.OrderDetailInfo = orderDetailInfo.ToString();
                    replenOrderDetails.Add(orderDetail); 
                }

                //Build the last order
                order.OrderInfo = orderInfo.ToString();
                _repoReplenOrder.Insert(order);
                orderId = order.Id;
                foreach (var detail in replenOrderDetails)
                {
                    detail.ReplenOrderId = orderId;
                    _repoReplenOrderDetail.Insert(detail);
                }
            }
        }

        private ReplenOrderDetail CreateOrderDetail(string line)
        {
            ItemDefinition itemDef = null;

            string partNum = line.Substring(2, 35);
            string description = line.Substring(74, 30);

            itemDef = GetItemDefinition(partNum, description);

            if (_neutronVariables.UpdateItemDefinitionDescription)
            {
                itemDef = UpdateItemDefinitionDescription(itemDef, description);
            }

            var areaId = itemDef.AreaId;
            string primeBin = GetPrimeBin(areaId, line.Substring(55, 11));
            var replenDetail = new ReplenOrderDetail();
            //replenDetail.ReplenOrderId = order.Id;
            replenDetail.ItemDefinitionId = itemDef.Id;
            replenDetail.PartNum = itemDef.Item;
            replenDetail.Quantity = line.Substring(38, 9).ParseInt();
            replenDetail.PrimeBin = primeBin;
            replenDetail.PartDesc = itemDef.Description;
            replenDetail.OrderDetailInfo =
                line.Length >= 205 ? line.Substring(105, 100) : line.Substring(105);
            replenDetail.AreaId = areaId;
            replenDetail.LineStatusId = (int)LineStatus.Available;
            replenDetail.PickedQuantity = 0;

            return replenDetail;
        }

        private ReplenOrder CreateOrder(string line)
        {
            var order = new ReplenOrder();
            order.Ord1 = line.Substring(8, 10) == "REPLENOPRP" ? $@"R{line.Substring(20, 9)}" : line.Substring(8, 10);
            order.Ord2 = line.Substring(19, 10);
            order.Priority = line.Substring(31, 2).ParseInt();
            order.LoadDate = DateTime.Now;
            order.OrderStatusId = (int)OrderStatus.Available;
            order.ShipMethodId = 1;
            order.ShipperId = 1;
            order.OrderInfo = string.Empty;

            //_repoReplenOrder.Insert(order);
            return order;
        }

        private ItemDefinition GetItemDefinition(string partNum, string description, string oc = "C")
        {
            ItemDefinition item = null;
            if (oc == "O")  //force to Off Carousel Station 8 at Dexter
            {
                _logger.LogDetailAsync(msg: "GetItemDefinition oc = 0  ");
                try
                {
                    int areaId = _workstationView.AreaId;
                    //try to find it anywhere first
                    item = _repoItemDefinition.FindBy(r => r.Item.ToLower().Trim() == partNum.ToLower().Trim()).FirstOrDefault() ??
                           new ItemDefinitionProcessor(_jsonData).GetOrCreate(partNum, description, areaId);
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync($"Error Finding Item Definition 1. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else
            {
                try
                {
                    item = _repoItemDefinition.FindBy(r => r.Item.ToLower().Trim() == partNum.ToLower().Trim()).FirstOrDefault() ??
                           new ItemDefinitionProcessor(_jsonData).GetOrCreate(partNum, description);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Finding Item Definition 1. {ex.Message} {Environment.NewLine} {ex.InnerException}");
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
                    item = _repoItemDefinition.FindBy(r => r.Item.ToLower().Trim() == sku).FirstOrDefault() ?? new ItemDefinitionProcessor(_jsonData).GetOrCreate(sku, des);
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
            var itemDef = _repoItemDefinition.FindByKey(itemDefinition.Id);
            if (itemDef != null)
            {
                itemDef.Description = description;
                _repoItemDefinition.Update(itemDef);
            }
            return itemDef;
        }

        //private int GetStationNumber(int workstationId)
        //{
        //    int stationNum = 0;

        //    try
        //    {
        //        Station station = _repoStation.FindBy(r => r.Id == workstationId).FirstOrDefault();

        //        if (station != null)
        //        {
        //            _logger.LogDetailAsync($"Get Station Number: {station.StationNumber}  WorkstationId: {workstationId} ");
        //            stationNum = station.StationNumber;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error Returning Station Number. {ex.Message} {Environment.NewLine} {ex.InnerException}");
        //    }
        //    _logger.LogDetailAsync($"Get Station Number Return: {stationNum}  WorkstationId: {workstationId} ");
        //    return stationNum;
        //}

        //private int GetStationId(int stationNumber)
        //{
        //    int workstationId = 0;

        //    try
        //    {
        //        Station station = _repoStation.FindBy(r => r.StationNumber == stationNumber).FirstOrDefault();
        //        if (station != null)
        //        {
        //            workstationId = station.Id;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error Returning Station Id. {ex.Message} {Environment.NewLine} {ex.InnerException}");
        //    }
        //    return workstationId;
        //}

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
