using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AlliedLogger;
using SAP.Middleware.Connector;
using SAPServer.Models;

namespace SAPServer
{
    public class NovaToSapGoodsReceipt
    {
        private readonly IDynamicLogger _logger;
       // private readonly ISendEmail _sendEmail;

        public NovaToSapGoodsReceipt( IDynamicLogger logger)
        {
           // _sendEmail = sendEmail;
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

                            _logger.Log($"Goods Receipt Record Sent To SAP: TASK: {good.TANUM} -- SKU: {good.MATNR}");
                            Console.WriteLine($"Goods Receipt Record Sent To SAP:  TASK: {good.TANUM} -- SKU: {good.MATNR}");

                            using (var db = new WagnerDb())
                            {
                                var rec = db.NOVA_OUTPUT.Find(good.TRANSID);
                                if (rec == null) continue;
                                rec.SKUDESC = string.Empty;
                                rec.PROCESSED = "Y";
                                rec.EXPLANATION = string.Empty;
                                db.SaveChanges();

                                _logger.Log($"NOVA_OUTPUT Set to Processed: TASK: {good.TANUM} -- SKU: {good.MATNR}");
                                Console.WriteLine($"NOVA_OUTPUT Set to Processed:  TASK: {good.TANUM} -- SKU: {good.MATNR}");
                            }

                        }
                        catch (RfcCommunicationException e)
                        {
                            _logger.Log($"Single Goods Receipt RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Receipt RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Receipt RfcCommunicationException Error", _logger.LastLogLines());
                        }
                        catch (RfcLogonException e)
                        {
                            _logger.Log($"Single Goods Receipt RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Receipt RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Receipt RfcLogonException Error", _logger.LastLogLines());
                        }
                        catch (RfcAbapRuntimeException e)
                        {
                            _logger.Log($"Single Goods Receipt RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Receipt RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Receipt RfcAbapRuntimeException Error", _logger.LastLogLines());
                        }
                        catch (RfcAbapBaseException e)
                        {
                            _logger.Log($"Single Goods Receipt RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Receipt RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Receipt RfcAbapBaseException Error", _logger.LastLogLines());

                            using (var db = new WagnerDb())
                            {
                                var rec = db.NOVA_OUTPUT.Find(good.TRANSID);
                                if (rec == null) continue;
                                rec.SKUDESC = string.Empty;
                                rec.PROCESSED = "Y";
                                rec.EXPLANATION = string.Empty;
                                db.SaveChanges();

                                _logger.Log($"BLOCKED NOVA_OUTPUT Set to Processed: TASK: {good.TANUM} -- SKU: {good.MATNR}");
                                Console.WriteLine($"BLOCKED NOVA_OUTPUT Set to Processed:  TASK: {good.TANUM} -- SKU: {good.MATNR}");
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Log($"Single Goods Receipt Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Receipt Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Receipt General Exception Error", _logger.LastLogLines());
                        }
                    }
                }
            }
            catch (RfcCommunicationException e)
            {
                _logger.Log($"Goods Receipt RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Receipt RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Receipt RfcCommunicationException Error", _logger.LastLogLines());
            }
            catch (RfcLogonException e)
            {
                _logger.Log($"Goods Receipt RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Receipt RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Receipt RfcLogonException Error", _logger.LastLogLines());
            }
            catch (RfcAbapRuntimeException e)
            {
                _logger.Log($"Goods Receipt RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Receipt RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Receipt RfcAbapRuntimeException Error", _logger.LastLogLines());
            }
            catch (RfcAbapBaseException e)
            {
                _logger.Log($"Goods Receipt RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Receipt RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Receipt RfcAbapBaseException Error", _logger.LastLogLines());
            }
            catch (Exception e)
            {
                _logger.Log($"Goods Receipt Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Receipt Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Receipt General Exception Error", _logger.LastLogLines());
            }
        }

        private List<GoodsReceipt> GetGoodsReceiptsFromOutput()
        {
            var goodsReceipts = new List<GoodsReceipt>();

            try
            {
                using (var db = new WagnerDb())
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
                        _logger.Log($"Goods Receipt - Output to SAP.  TASKNO: {gr.TANUM}  SKU: {gr.MATNR} REQ QTY: {gr.VERME} ISS QTY: {gr.NISTA}");
                        Console.WriteLine($"Goods Receipt - Output to SAP.  TASKNO: {gr.TANUM}  SKU: {gr.MATNR} REQ QTY: {gr.VERME} ISS QTY: {gr.NISTA}");
                        goodsReceipts.Add(gr);

                    }
                }
            }
            catch (Exception e)
            {
                // _sendEmail.Message("Get Goods Receipts From Output Communication Error", _logger.LastLogLines());
                _logger.Log($"GetGoodsReceiptsFromOutput {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"GetGoodsReceiptsFromOutput {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            return goodsReceipts;
        }
    }
}
