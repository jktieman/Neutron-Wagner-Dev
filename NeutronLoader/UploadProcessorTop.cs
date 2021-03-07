using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Threading.Timer;
using static System.Int32;
using NeutronData.DataContexts;

namespace NeutronLoader
{
    public class UploadProcessorTop : IUploadProcessor
    {
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly DynamicLogger _logger;
        private readonly Station _rackStation;
        private Timer _timer;
        private bool _uploadBusy;

        public UploadProcessorTop(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            DynamicLogger logger, Station rackStation)
        {
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _logger = logger;
            _rackStation = rackStation;
        }


        public void RunUploadOnce()
        {
            CreateHostFile();
        }

        public void StartProcessingUploadFiles()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.UploadDelay);
            _timer = new Timer(t => { CreateHostFile(); }, null, startTimeSpan, periodTimeSpan);
        }

        public void StopProcessingUploadFiles()
        {
            _timer.Dispose();
        }

        public void CreateHostFile()
        {
            var counter = 0;
            while (_uploadBusy)
            {
                Task.Delay(200);
                ++counter;
                if (counter >= 20) return;
            }

            _uploadBusy = true;
            var actionCodes = _neutronVariables.ActionCodes.Split(',').Select(Parse).ToList();
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.History.Where(h => !h.TransmitDateTime.HasValue && actionCodes.Contains(h.ActionCode)).ToList();
                    if (recs.Count <= 0) return;
                    var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
                    var result = hostFile.CreateHostFile(recs);
                    if (result == true)
                    {
                        foreach (var rec in recs)
                        {
                            rec.TransmitDateTime = DateTime.Now;
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show(@"Upload Process Failed, see Log file in HostFile.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Upload Process Failed, see Log file in HostFile.");
                _logger.Log($"Create Host File Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            _uploadBusy = false;
        }

        public void ReturnToStock(OrderDetail detail)
        {
            var ord = new HostOrder()
            {
                TypeCode = "1",
                PartNum = detail.PartNum,
                PartDesc = detail.PartDesc,
                JobNum = detail.JobNum,
                PrimeBin = detail.PrimeBin,
                NewBin = detail.NewBin,
                Qty = detail.Qty,
                TroubleBit = detail.TroubleBit,
                DateTime = detail.DateTime,
                EmpId = detail.EmpId
            };
            var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
            hostFile.CreateHostFile(ord);
        }

        public void ReturnOrderToStock(Order order)
        {
            foreach (var detail in order.OrderDetails)
            {
                var ord = new HostOrder()
                {
                    TypeCode = "1",
                    PartNum = detail.PartNum,
                    PartDesc = detail.PartDesc,
                    JobNum = detail.JobNum,
                    PrimeBin = detail.PrimeBin,
                    NewBin = detail.NewBin,
                    Qty = detail.Qty,
                    TroubleBit = detail.TroubleBit,
                    DateTime = detail.DateTime,
                    EmpId = ($"EmpId:{detail.EmpId} Note: Returned to Stock")
                };
                var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
                hostFile.CreateHostFile(ord);
            }
        }

        public void ReturnOrderToStock(ReplenOrder order)
        {
            foreach (var detail in order.ReplenOrderDetails)
            {
                var ord = new HostOrder()
                {
                    TypeCode = "1",
                    PartNum = detail.PartNum,
                    PartDesc = detail.PartDesc,
                    JobNum = detail.JobNum,
                    PrimeBin = detail.PrimeBin,
                    NewBin = detail.NewBin,
                    Qty = detail.Qty,
                    TroubleBit = detail.TroubleBit,
                    DateTime = detail.DateTime,
                    EmpId = ($"EmpId:{detail.EmpId} Note: Returned to Stock")
                };
                var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
                hostFile.CreateHostFile(ord);
            }
        }

        public void CreateHostFile(BindingSource bindingSourcePickStops)
        {
            _logger.Log("CreateHostFile BindingSource bindingSourcePickStops");

            var date = DateTime.MinValue;
            var hostOrders = new List<HostOrder>();
            foreach (PickStop stop in bindingSourcePickStops)
            {
                if (stop.Skipped) continue;
                foreach (var pickView in stop.PickViews)
                {
                    var pickLocation = pickView.PickLocations.FirstOrDefault();
                    if (pickLocation != null)
                    {
                        date = pickLocation.PickDate;
                    }

                    var ord = new HostOrder()
                    {
                        TypeCode = "1",
                        PartNum = pickView.OrderDetail.PartNum,
                        PartDesc = pickView.OrderDetail.PartDesc,
                        JobNum = pickView.Ord1,
                        PrimeBin = pickView.OrderDetail.PrimeBin,
                        NewBin = pickView.OrderDetail.NewBin,
                        Qty = pickView.PickedQty.ToString(),
                        TroubleBit = pickView.OrderDetail.TroubleBit,
                        DateTime = date.ToString(format: "yyyyMMdd"),
                        EmpId = pickView.OrderDetail.EmpId,
                        OrderDetail = pickView.OrderDetail
                    };
                    hostOrders.Add(ord);
                }
            }

            if (!hostOrders.Any()) return;
            _logger.Log("Calling HostFile");
            var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
            hostFile.CreateHostFile(hostOrders);
        }

        public void CreateHostFileRack(List<Order> orders)
        {
            _logger.Log("CreateHostFile Rack orders");

            var date = DateTime.Now;
            var hostOrders = new List<HostOrder>();
            foreach (var order in orders)
            {
                foreach (var orderDetail in order.OrderDetails)
                {
                    var ord = new HostOrder()
                    {
                        TypeCode = "1",
                        PartNum = orderDetail.PartNum,
                        PartDesc = orderDetail.PartDesc,
                        JobNum = order.Ord1,
                        PrimeBin = orderDetail.PrimeBin,
                        NewBin = orderDetail.NewBin,
                        Qty = orderDetail.Quantity.ToString(),
                        TroubleBit = orderDetail.TroubleBit,
                        DateTime = date.ToString(format: "yyyyMMdd"),
                        EmpId = orderDetail.EmpId,
                        OrderDetail = orderDetail
                    };

                    hostOrders.Add(ord);
                }
            }

            if (!hostOrders.Any()) return;
            _logger.Log("Calling HostFile");
            var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
            hostFile.CreateHostFile(hostOrders);
        }

        public void CreateHostFileRack(List<ReplenOrder> orders)
        {
            var date = DateTime.Now;
            var hostOrders = new List<ReplenHostOrder>();
            foreach (var order in orders)
            {
                foreach (var orderDetail in order.ReplenOrderDetails)
                {
                    var ord = new ReplenHostOrder()
                    {
                        TypeCode = "1",
                        PartNum = orderDetail.PartNum,
                        PartDesc = orderDetail.PartDesc,
                        JobNum = order.Ord1,
                        PrimeBin = orderDetail.PrimeBin,
                        NewBin = orderDetail.NewBin,
                        Qty = orderDetail.Quantity.ToString(),
                        TroubleBit = orderDetail.TroubleBit,
                        DateTime = date.ToString(format: "yyyyMMdd"),
                        EmpId = orderDetail.EmpId,
                        OrderDetail = orderDetail
                    };

                    hostOrders.Add(ord);
                }
            }

            if (!hostOrders.Any()) return;
            _logger.Log("Calling HostFile");
            var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
            hostFile.CreateHostFile(hostOrders);
        }


        public void CreateHostFile(PickStop pickStop)
        {
            //if (Variables.SendAllPicksToHost)
            //{

            //}
            //// Picked Used
            _logger.Log("CreateHostFile(PickStop pickStop)");
            foreach (var pickView in pickStop.PickViews)
            {
                // var ord = new HostOrder();
                //if (pickView.Item.Substring(0, 1) == "8")
                //{
                //Wasn't picked out of Prime Bin so Return to Stock
                var ord = new HostOrder()
                {
                    TypeCode = "1",
                    PartNum = pickView.OrderDetail.PartNum,
                    PartDesc = pickView.OrderDetail.PartDesc,
                    JobNum = pickView.OrderDetail.JobNum,
                    PrimeBin = pickView.OrderDetail.PrimeBin,
                    NewBin = pickView.OrderDetail.NewBin,
                    Qty = pickView.PickedQty.ToString(),
                    TroubleBit = pickView.OrderDetail.TroubleBit,
                    DateTime = pickView.OrderDetail.DateTime,
                    EmpId = ($"EmpId:{pickView.OrderDetail.EmpId} Note: Picked Used"),
                    OrderDetail = pickView.OrderDetail
                };

                _logger.Log("CreateHostFile(PickStop pickStop) Call HostFile");
                var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
                hostFile.CreateHostFile(ord);
                // Didn't pick from PrimeBin and it IS a new item
                //if (pickView.Slot != pickView.OrderDetail.PrimeBin && pickView.Item.Substring(0, 1) == "9")
                //{
                //    PickLocation pickLocation = pickView.PickLocations.FirstOrDefault();
                //    if (pickLocation != null)
                //    {
                //        string newBin = pickLocation.Inventory.Location.Slot;
                //        //Wasn't picked out of Prime Bin so Return to Stock
                //        ord = new HostOrder()
                //        {
                //            TypeCode = "1",
                //            PartNum = pickView.OrderDetail.PartNum,
                //            PartDesc = pickView.OrderDetail.PartDesc,
                //            JobNum = pickView.OrderDetail.JobNum,
                //            PrimeBin = pickView.OrderDetail.PrimeBin,
                //            NewBin = newBin,
                //            Qty = pickLocation.Quantity.ToString(),
                //            TroubleBit = pickView.OrderDetail.TroubleBit,
                //            DateTime = pickView.OrderDetail.DateTime,
                //            EmpId = ($"EmpId:{pickView.OrderDetail.EmpId} Note: Picked From Different Location")
                //        };
                //    }
                //    var hostFile = new HostFile(ord);
                //}
            }

        }

        public void CreateReplenHostFile(BindingSource bindingSourcePickStops)
        {


            var date = DateTime.MinValue;
            var hostOrders = new List<ReplenHostOrder>();
            foreach (ReplenPickStop stop in bindingSourcePickStops)
            {
                foreach (var pickView in stop.PickViews)
                {
                    foreach (var pickLocation in pickView.PickLocations)
                    {
                        if (pickLocation != null)
                        {
                            date = pickLocation.PickDate;
                        }

                        var ord = new ReplenHostOrder()
                        {
                            TypeCode = "1",
                            PartNum = pickView.OrderDetail.PartNum,
                            PartDesc = pickView.OrderDetail.PartDesc,
                            JobNum = pickView.Ord1,
                            PrimeBin = pickView.OrderDetail.PrimeBin,
                            NewBin = pickView.OrderDetail.NewBin,
                            Qty = pickLocation.Quantity.ToString(),
                            TroubleBit = pickView.OrderDetail.TroubleBit,
                            DateTime = date.ToString(format: "yyyyMMdd"),
                            EmpId = pickLocation.User.EmpId,
                            OrderDetail = pickView.OrderDetail
                        };

                        hostOrders.Add(ord);
                    }
                }
            }

            if (!hostOrders.Any()) return;
            _logger.Log("Calling HostFile");
            var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
            hostFile.CreateHostFile(hostOrders);
        }

    }
}
