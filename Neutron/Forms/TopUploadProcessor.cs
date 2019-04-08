using Neutron.Global;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public class TopUploadProcessor
    {
        private readonly NeutronVariables neutronVariables;
        private readonly NeutronLicense neutronLicense;

        public TopUploadProcessor(NeutronVariables neutronVariables, NeutronLicense neutronLicense)
        {
            this.neutronVariables = neutronVariables;
            this.neutronLicense = neutronLicense;
        }

        public void CreateHostFile(BindingSource bindingSourcePickStops)
        {
            foreach (PickStop stop in bindingSourcePickStops)
            {
                foreach (PickView pickView in stop.PickViews)
                {
                    ProcessUsedItem(pickView);
                    ProcessPrimeBin(pickView);
                }
            }
        }

        public void CreateReplenHostFile(BindingSource bindingSourcePickStops)
        {
            foreach (ReplenPickStop stop in bindingSourcePickStops)
            {
                foreach (ReplenPickView pickView in stop.PickViews)
                {
                   // ProcessPrimeBin(pickView);
                }
            }
        }

        private void ProcessPrimeBin(PickView pickView)
        {
            // Check to see if NOT picked from PrimeBin and it IS a new item
            foreach (PickLocation pickLocation in pickView.PickLocations)
            {
                if (pickLocation != null)
                {
                    if (pickLocation.Inventory.Location.Slot != pickView.OrderDetail.PrimeBin && pickView.Item.StartsWith($"9"))
                    {
                        string empId = GlobalVar.User.EmpId == null ? "" : GlobalVar.User.EmpId;
                        string newBin = pickLocation.Inventory.Location.Slot;
                        //Wasn't picked out of Prime Bin so Set Trouble Bit and Create Error Record back to Epicor
                        var ord = new HostOrder()
                        {
                            TypeCode = pickView.OrderDetail.TypeCode,
                            PartNum = pickView.OrderDetail.PartNum,
                            PartDesc = pickView.OrderDetail.PartDesc,
                            JobNum = pickView.OrderDetail.JobNum,
                            PrimeBin = pickView.OrderDetail.PrimeBin,
                            NewBin = newBin,
                            Qty = pickLocation.Quantity.ToString(),
                            TroubleBit = "1",
                            DateTime = pickLocation.PickDate.ToString($"yyyyMMddHHmmss"),
                            EmpId = ($"EmpId:{empId} Note: Picked From Different Location")
                        };
                        var hostFile = new HostFile(neutronLicense, neutronVariables);
                        hostFile.CreateHostFile(ord);
                    }
                }
            }
        }

        private void ProcessUsedItem(PickView pickView)
        {
            if (pickView.Item.StartsWith($"8"))
            {
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    //Used Item, so Return to Stock
                    string item = ($"9{pickView.OrderDetail.PartNum.Substring(1)}");
                    string empId = GlobalVar.User.EmpId == null ? "" : GlobalVar.User.EmpId;
                    var ord = new HostOrder()
                    {
                        TypeCode = "1",
                        PartNum = item,
                        PartDesc = pickView.OrderDetail.PartDesc,
                        JobNum = pickView.OrderDetail.JobNum,
                        PrimeBin = pickView.OrderDetail.PrimeBin,
                        NewBin = pickView.OrderDetail.NewBin,
                        Qty = pickLocation.Quantity.ToString(),
                        TroubleBit = pickView.OrderDetail.TroubleBit,
                        DateTime = pickLocation.PickDate.ToString($"yyyyMMddHHmmss"),
                        EmpId = ($"EmpId:{empId} Note: Picked Used")
                    };

                    var hostFile = new HostFile(neutronLicense, neutronVariables);
                    hostFile.CreateHostFile(ord);
                }
            }
        }

        private void ProcessPrimeBin(ReplenPickView pickView)
        {
            // Check to see if NOT picked from PrimeBin and it IS a new item
            foreach (PickLocation pickLocation in pickView.PickLocations)
            {
                if (pickLocation != null)
                {
                    if (pickLocation.Inventory.Location.Slot != pickView.OrderDetail.PrimeBin && pickView.Item.StartsWith($"9"))
                    {
                        string empId = GlobalVar.User.EmpId == null ? "" : GlobalVar.User.EmpId;
                        string newBin = pickLocation.Inventory.Location.Slot;
                        //Wasn't picked out of Prime Bin so Return to Stock
                        var ord = new HostOrder()
                        {
                            TypeCode = pickView.OrderDetail.TypeCode,
                            PartNum = pickView.OrderDetail.PartNum,
                            PartDesc = pickView.OrderDetail.PartDesc,
                            JobNum = pickView.OrderDetail.JobNum,
                            PrimeBin = pickView.OrderDetail.PrimeBin,
                            NewBin = newBin,
                            Qty = pickLocation.Quantity.ToString(),
                            TroubleBit = pickView.OrderDetail.TroubleBit,
                            DateTime = pickLocation.PickDate.ToString($"yyyyMMddHHmmss"),
                            EmpId = ($"EmpId:{empId} Note: Stored in Different Location")
                        };
                        var hostFile = new HostFile(neutronLicense, neutronVariables);
                        hostFile.CreateHostFile(ord);
                    }
                }
            }
        }
    }
}
