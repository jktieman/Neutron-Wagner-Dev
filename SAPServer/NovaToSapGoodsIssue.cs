using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AlliedLogger;
using SAP.Middleware.Connector;
using SAPServer.Models;

namespace SAPServer
{
    public class NovaToSapGoodsIssue
    {
        //readonly ISendEmail _sendEmail;
        private readonly IDynamicLogger _logger;

        public NovaToSapGoodsIssue( IDynamicLogger logger)
        {
            //_sendEmail = sendEmail;
            _logger = logger;
        }
        public void Set(RfcDestination destination)
        {

            try
            {
                RfcRepository repo = destination.Repository;
                IRfcFunction novaToSapList = repo.CreateFunction("ZWM_NOVA_TO_SAP_GOODS_ISSUE");

                IRfcTable goodsIssueTable = novaToSapList.GetTable("LT_SAPNOVA_G");

                List<GoodsIssue> goodsIssueList = GetGoodsIssuesFromOutput();



                if (goodsIssueList.Count > 0)
                {
                    foreach (var good in goodsIssueList)
                    {

                        goodsIssueTable.Append();
                        goodsIssueTable.SetValue("TANUM", good.TANUM);
                        goodsIssueTable.SetValue("TAPOS", good.TAPOS);
                        goodsIssueTable.SetValue("MATNR", good.MATNR);
                        goodsIssueTable.SetValue("NSOLM", good.NSOLM);
                        goodsIssueTable.SetValue("MEINS", good.MEINS);
                        goodsIssueTable.SetValue("MATKL", good.MATKL);
                        goodsIssueTable.SetValue("NISTA", good.NISTA);
                        goodsIssueTable.SetValue("VBELN", good.VBELN);
                        goodsIssueTable.SetValue("EAN11", good.EAN11);
                        goodsIssueTable.SetValue("SPART", good.SPART);
                        goodsIssueTable.SetValue("VSBED", good.VSBED);
                        goodsIssueTable.SetValue("KDMAT", good.KDMAT);


                        try
                        {
                            novaToSapList.Invoke(destination);

                            _logger.Log($"Goods Issue Record Sent To SAP: TASK: {good.TANUM} -- SKU: {good.MATNR}");
                            Console.WriteLine($"Goods Issue Record Sent To SAP: TASK: {good.TANUM} -- SKU: {good.MATNR}");

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
                            _logger.Log($"Single Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Issue RfcCommunicationException Error", _logger.LastLogLines());
                        }
                        catch (RfcLogonException e)
                        {
                            _logger.Log($"Single Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Issue RfcLogonException Error", _logger.LastLogLines());
                        }
                        catch (RfcAbapRuntimeException e)
                        {
                            _logger.Log($"Single Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Issue RfcAbapRuntimeException Error", _logger.LastLogLines());
                        }
                        catch (RfcAbapBaseException e)
                        {
                            _logger.Log($"Single Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Issue RfcAbapBaseException Error", _logger.LastLogLines());
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
                            _logger.Log($"Single Goods Issue Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Console.WriteLine($"Single Goods Issue Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                            // _sendEmail.Message("Single Goods Issue General Exception Error", _logger.LastLogLines());
                        }
                    }
                }
            }
            catch (RfcCommunicationException e)
            {
                _logger.Log($"Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Issue RfcCommunicationException Error", _logger.LastLogLines());
            }
            catch (RfcLogonException e)
            {
                _logger.Log($"Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Issue RfcLogonException Error", _logger.LastLogLines());
            }
            catch (RfcAbapRuntimeException e)
            {
                _logger.Log($"Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Issue RfcAbapRuntimeException Error", _logger.LastLogLines());
            }
            catch (RfcAbapBaseException e)
            {
                _logger.Log($"Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Issue RfcAbapBaseException Error", _logger.LastLogLines());
            }
            catch (Exception e)
            {
                _logger.Log($"Goods Issue Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                // _sendEmail.Message("Goods Issue General Exception Error", _logger.LastLogLines());
            }
        }

        private List<GoodsIssue> GetGoodsIssuesFromOutput()
        {
            List<GoodsIssue> goodsIssues = new List<GoodsIssue>();
            try
            {
                using (var db = new WagnerDb())
                {
                    List<NOVA_OUTPUT> recs = db.NOVA_OUTPUT.Where(n => n.PROCESSED == @"N" && n.TRANSTYPE == "P2").OrderByDescending(o => o.TRANSDATE).ToList();

                    foreach (var item in recs)
                    {
                        var g = new GoodsIssue();
                        g.TRANSID = item.TRANSID;
                        g.TANUM = item.TASKNO.ToString(CultureInfo.InvariantCulture);
                        g.TAPOS = item.TOTENO.ToString(CultureInfo.InvariantCulture);
                        g.MATNR = item.SKU;
                        g.NSOLM = Convert.ToDecimal(item.BEGINNINGQTY);
                        g.NISTA = Convert.ToDecimal(item.QTY);
                        g.MATKL = item.SKUDESC;
                        g.SPART = item.ORDERCOMPANY;
                        g.VSBED = item.PRIORITY;
                        g.VBELN = item.ORDERNO.ToString(CultureInfo.InvariantCulture);
                        g.EAN11 = item.BOXID;
                        g.MEINS = item.BP;

                        _logger.Log($"Goods Issue - Output to SAP.  TASKNO: {g.TANUM}  SKU: {g.MATNR} REQ QTY: {g.NSOLM} ISS QTY: {g.NISTA}");
                        Console.WriteLine($"Goods Issue - Output to SAP.  TASKNO: {g.TANUM}  SKU: {g.MATNR} REQ QTY: {g.NSOLM} ISS QTY: {g.NISTA}");

                        goodsIssues.Add(g);
                    }
                }
            }
            catch (Exception e)
            {
                // _sendEmail.Message("Get Goods Issues From Output Communication Error", _logger.LastLogLines());
                _logger.Log($"GetGoodsIssuesFromOutput {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"GetGoodsIssuesFromOutput {e.Message}{Environment.NewLine}{e.InnerException} ");
            }

            return goodsIssues;
        }
    }
}
