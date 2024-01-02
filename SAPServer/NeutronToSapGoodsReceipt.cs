using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AlliedLogger;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronEvents;
using SAP.Middleware.Connector;
using SAPServer.Models;

namespace SAPServer
{
    public class NovaToSapGoodsReceipt
    {
        private readonly IDynamicLogger _logger;

        public NovaToSapGoodsReceipt( IDynamicLogger logger)
        {
            _logger = logger;
        }

        public void Set(RfcDestination destination)
        {

            try
            {
                RfcRepository repo = destination.Repository;
                IRfcFunction novaToSapList = repo.CreateFunction("ZWM_NOVA_TO_SAP");

                IRfcTable goodsReceiptTable = novaToSapList.GetTable("LT_NOVASAP");

                List<GoodsReceipt> goodsReceiptList = GetGoodsReceiptsFromOutput();


                if (goodsReceiptList.Count > 0)
                {
                    foreach (var good in goodsReceiptList)
                    {
                        goodsReceiptTable.Append();
                        goodsReceiptTable.SetValue("TANUM", good.TANUM);
                        goodsReceiptTable.SetValue("TAPOS", good.TAPOS);
                        goodsReceiptTable.SetValue("MATNR", good.MATNR);
                        goodsReceiptTable.SetValue("VERME", good.VERME);
                        goodsReceiptTable.SetValue("MEINS", good.MEINS);
                        goodsReceiptTable.SetValue("MAKTX", good.MAKTX);
                        goodsReceiptTable.SetValue("NISTA", good.NISTA);


                        try
                        {
                            novaToSapList.Invoke(destination);

                         _ = _logger.LogDetailAsync($"Goods Receipt Record Sent To SAP: TASK: {good.TANUM} -- SKU: {good.MATNR}");

                            using (var db = new NeutronDb())
                            {
                                var rec = db.NOVA_OUTPUT.Find(good.TRANSID);
                                if (rec == null) continue;
                                rec.SKUDESC = string.Empty;
                                rec.PROCESSED = "Y";
                                rec.EXPLANATION = string.Empty;
                                db.SaveChanges();

                             _ = _logger.LogDetailAsync($"NOVA_OUTPUT Set to Processed: TASK: {good.TANUM} -- SKU: {good.MATNR}");
                            }

                        }
                        catch (RfcCommunicationException e)
                        {
                         _ = _logger.LogDetailAsync($"Single Goods Receipt RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                         Mediator.GetInstance().OnSendEmailMessage(this, 
                             $"Single Goods Receipt RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                        }
                        catch (RfcLogonException e)
                        {
                         _ = _logger.LogDetailAsync($"Single Goods Receipt RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                         Mediator.GetInstance().OnSendEmailMessage(this, 
                             $"Single Goods Receipt RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                        }
                        catch (RfcAbapRuntimeException e)
                        {
                         _ = _logger.LogDetailAsync($"Single Goods Receipt RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                         Mediator.GetInstance().OnSendEmailMessage(this, 
                             $"Single Goods Receipt RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                        }
                        catch (RfcAbapBaseException e)
                        {
                         _ = _logger.LogDetailAsync($"Single Goods Receipt RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                         Mediator.GetInstance().OnSendEmailMessage(this, 
                             $"Single Goods Receipt RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");

                           using (var db = new NeutronDb())
                            {
                                var rec = db.NOVA_OUTPUT.Find(good.TRANSID);
                                if (rec == null) continue;
                                rec.SKUDESC = string.Empty;
                                rec.PROCESSED = "Y";
                                rec.EXPLANATION = string.Empty;
                                db.SaveChanges();

                             _ = _logger.LogDetailAsync($"BLOCKED NOVA_OUTPUT Set to Processed: TASK: {good.TANUM} -- SKU: {good.MATNR}");
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                         _ = _logger.LogDetailAsync($"Single Goods Receipt Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                         Mediator.GetInstance().OnSendEmailMessage(this, 
                             $"Single Goods Receipt Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                        }
                    }
                }
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
            catch (Exception e)
            {
             _ = _logger.LogDetailAsync($"Goods Receipt Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
             Mediator.GetInstance().OnSendEmailMessage(this, 
                 $"Goods Receipt Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
        }

        private List<GoodsReceipt> GetGoodsReceiptsFromOutput()
        {
            var goodsReceipts = new List<GoodsReceipt>();

            try
            {
                using (var db = new NeutronDb())
                {
                    List<NOVA_OUTPUT> recs = db.NOVA_OUTPUT.Where(n => n.PROCESSED == @"N" && n.TRANSTYPE == "02").OrderByDescending(o => o.TRANSDATE).ToList();

                    foreach (var item in recs)
                    {
                        var gr = new GoodsReceipt();
                        gr.TRANSID = item.TRANSID;
                        gr.TANUM = item.TASKNO.ToString(CultureInfo.InvariantCulture);
                        gr.TAPOS = item.TOTENO.ToString(CultureInfo.InvariantCulture);
                        gr.MATNR = item.SKU;
                        gr.VERME = Convert.ToDecimal(item.BEGINNINGQTY);
                        gr.NISTA = Convert.ToDecimal(item.QTY);
                     _ = _logger.LogDetailAsync($"Goods Receipt - Output to SAP.  TASKNO: {gr.TANUM}  SKU: {gr.MATNR} REQ QTY: {gr.VERME} ISS QTY: {gr.NISTA}");
                        goodsReceipts.Add(gr);

                    }
                }
            }
            catch (Exception e)
            {
             _ = _logger.LogDetailAsync($"GetGoodsReceiptsFromOutput {e.Message}{Environment.NewLine}{e.InnerException} ");
             Mediator.GetInstance().OnSendEmailMessage(this, 
                 $"GetGoodsReceiptsFromOutput {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            return goodsReceipts;
        }
    }
}
