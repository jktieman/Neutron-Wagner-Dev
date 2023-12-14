using System;
using System.Collections.Generic;
using AlliedLogger;
using JsonManager;
using NeutronEvents;
using SAP.Middleware.Connector;
using SAPServer.Models;

namespace SAPServer
{
    public class SapToNeutronGoodsReceipt
    {
        private readonly IJsonData _jsonData;
        private readonly IDynamicLogger _logger;

        public SapToNeutronGoodsReceipt(IJsonData jsonData, IDynamicLogger logger)
        {
            _jsonData = jsonData;
            _logger = logger;
        }

        public void Get(RfcDestination destination)
        {
            var unProcessedGoods = new List<GoodsReceipt>();

            try
            {
                RfcRepository repo = destination.Repository;
                IRfcFunction sapToNeutronList = repo.CreateFunction("ZWM_SAP_TO_NOVA");

                IRfcTable goodsReceiptTable = sapToNeutronList.GetTable("LT_SAPNOVA");

                List<GoodsReceipt> goodsReceiptList = new List<GoodsReceipt>();

                sapToNeutronList.Invoke(destination);

                for (int cuIndex = 0; cuIndex < goodsReceiptTable.RowCount; cuIndex++)
                {

                    goodsReceiptTable.CurrentIndex = cuIndex;
                    var goodsReceipt = new GoodsReceipt();
                    goodsReceipt.TANUM = goodsReceiptTable.GetString("TANUM");
                    goodsReceipt.TAPOS = goodsReceiptTable.GetString("TAPOS");
                    goodsReceipt.MATNR = goodsReceiptTable.GetString("MATNR");
                    goodsReceipt.VERME = goodsReceiptTable.GetDecimal("VERME");
                    goodsReceipt.MEINS = goodsReceiptTable.GetString("MEINS");
                    goodsReceipt.MAKTX = goodsReceiptTable.GetString("MAKTX");

                    goodsReceiptList.Add(goodsReceipt);
                }

                _ = _logger.LogDetailAsync($"Goods Receipt Records Retrieved From SAP: {goodsReceiptList.Count}");

                //------------------
                // Create backup Json file in case the Wagner Database is unavailable
                var recs = _jsonData.LoadFile<List<GoodsReceipt>>();
                if (recs.Count > 0)
                {
                    _ = _logger.LogDetailAsync($"Goods Receipts Records Retrieved From Backup: {recs.Count}");
                    foreach (var rec in recs)
                    {
                        goodsReceiptList.Add(rec);
                    }
                }

                _jsonData.SaveFile(goodsReceiptList);

                //------------------





                try
                {
                    using (var context = new WagnerDb())
                    {
                        foreach (var row in goodsReceiptList)
                        {
                            var input = new NOVA_INPUT();
                            input.PROCESSED = "N";
                            input.TRANSTYPE = "02";
                            input.SKU = row.MATNR;
                            input.QTY = row.VERME;
                            input.TASKNO = Convert.ToDecimal(row.TANUM);
                            input.SKUDESC = ValidSkuDesc(row.MAKTX);
                            input.TOTENO = Convert.ToDecimal(row.TAPOS);
                            input.BP = row.MEINS;
                            input.TRANSDATE = DateTime.Now;
                            try
                            {
                                context.NOVA_INPUT.Add(input); 
                                context.SaveChanges();
                                row.Processed = true;
                                _ = _logger.LogDetailAsync($"Saving TaskNo: {input.TASKNO} SKU: {input.SKU} DESC: {input.SKUDESC} to INPUT");
                            }
                            catch (Exception e)
                            {
                                _ = _logger.LogDetailAsync($"Error writing Goods Receipt TaskNo: {input.TASKNO} SKU: {input.SKU} DESC: {input.SKUDESC} to INPUT Table. {Environment.NewLine}  {e.Message} {Environment.NewLine} {e.InnerException}");
                                Mediator.GetInstance().OnSendEmailMessage(this, 
                                    $"Error writing Goods Receipt TaskNo: {input.TASKNO} SKU: {input.SKU} DESC: {input.SKUDESC} to INPUT Table. {Environment.NewLine}  {e.Message} {Environment.NewLine} {e.InnerException}");


                            }
                        }
                    }

                    foreach (var goodsReceipt in goodsReceiptList)
                    {
                        if (goodsReceipt.Processed == false)
                        {
                            unProcessedGoods.Add(goodsReceipt);
                        }
                    }
                    _jsonData.SaveFile(unProcessedGoods);

                    _ = _logger.LogDetailAsync($"UnProcessed Goods Receipt Record Count: {unProcessedGoods.Count}");
                }
                catch (Exception e)
                {
                    _ = _logger.LogDetailAsync($"Error writing Receipt to INPUT Table.  {e.Message} {Environment.NewLine} {e.InnerException}");
                    Mediator.GetInstance().OnSendEmailMessage(this, 
                        $"Error writing Receipt to INPUT Table.  {e.Message} {Environment.NewLine} {e.InnerException}");
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

            }
            catch (RfcCommunicationException e)
            {
                _ = _logger.LogDetailAsync($"Goods Receipt RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Mediator.GetInstance().OnSendEmailMessage(this, 
                    $"Goods Receipt RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            catch (RfcLogonException e)
            {
                _ = _logger.LogDetailAsync($"Goods Receipt RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Mediator.GetInstance().OnSendEmailMessage(this, 
                    $"Goods Receipt RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            catch (RfcAbapRuntimeException e)
            {
                _ = _logger.LogDetailAsync($"Goods Receipt RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Mediator.GetInstance().OnSendEmailMessage(this, 
                    $"Goods Receipt RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            catch (RfcAbapBaseException e)
            {
                _ = _logger.LogDetailAsync($"Goods Receipt RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Mediator.GetInstance().OnSendEmailMessage(this, 
                    $"Goods Receipt RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
        }
        private string ValidSkuDesc(string s)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(s))
                {
                    if (s.Length > 30)
                    {
                        result = s.Substring(0, 30);
                    }
                    else
                    {
                        result = s.Trim().PadRight(30, ' ');
                    }
                }
            }
            catch (Exception e)
            {
                _ = _logger.LogDetailAsync($"Invalid Receipt SkuDesc/MATKL: [ {s} ] {Environment.NewLine} {e.Message} {Environment.NewLine} {e.InnerException}");
                Mediator.GetInstance().OnSendEmailMessage(this, 
                    $"Invalid Receipt SkuDesc/MATKL: [ {s} ] {Environment.NewLine} {e.Message} {Environment.NewLine} {e.InnerException}");
            }
            return result;
        }
    }
}

