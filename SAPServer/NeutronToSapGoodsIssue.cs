using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AlliedLogger;
using NeutronEvents;
using SAP.Middleware.Connector;
using SAPServer.Models;


namespace SAPServer
{
    public class NeutronToSapGoodsIssue
    {
        private readonly IDynamicLogger _logger;

        public NeutronToSapGoodsIssue( IDynamicLogger logger)
        {
            _logger = logger;
        }
        public void Set(RfcDestination destination)
        {

            try
            {
                RfcRepository repo = destination.Repository;
                IRfcFunction neutronToSapList = repo.CreateFunction("ZWM_NOVA_TO_SAP_GOODS_ISSUE");

                IRfcTable goodsIssueTable = neutronToSapList.GetTable("LT_SAPNOVA_G");

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
                            neutronToSapList.Invoke(destination);

                            _ = _logger.LogDetailAsync(
                                $"Goods Issue Record Sent To SAP: TASK: {good.TANUM} -- SKU: {good.MATNR}");

                            using (var db = new WagnerDb())
                            {
                                var rec = db.NOVA_OUTPUT.Find(good.TRANSID);
                                if (rec == null) continue;
                                //rec.SKUDESC = string.Empty;
                                rec.PROCESSED = "Y";
                                //rec.EXPLANATION = string.Empty;
                                db.SaveChanges();

                                _ = _logger.LogDetailAsync(
                                    $"NOVA_OUTPUT Set to Processed: TASK: {good.TANUM} -- SKU: {good.MATNR}");
                            }
                        }
                        catch (RfcCommunicationException e)
                        {
                            _ = _logger.LogDetailAsync($"Single Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Mediator.GetInstance().OnSendEmailMessage(this,
                                $"Single Goods Issue RfcCommunicationException Error {e.Message}{Environment.NewLine}{e.InnerException} ");
                        }
                        catch (RfcLogonException e)
                        {
                            _ = _logger.LogDetailAsync($"Single Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Mediator.GetInstance().OnSendEmailMessage(this, 
                                $"Single Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                        }
                        catch (RfcAbapRuntimeException e)
                        {
                            _ = _logger.LogDetailAsync($"Single Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Mediator.GetInstance().OnSendEmailMessage(this, 
                                $"Single Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                        }
                        catch (RfcAbapBaseException e)
                        {
                            _ = _logger.LogDetailAsync($"Single Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Mediator.GetInstance().OnSendEmailMessage(this, 
                                $"Single Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");

                            using (var db = new WagnerDb())
                            {
                                var rec = db.NOVA_OUTPUT.Find(good.TRANSID);
                                if (rec == null) continue;
                                rec.SKUDESC = string.Empty;
                                rec.PROCESSED = "Y";
                                rec.EXPLANATION = string.Empty;
                                db.SaveChanges();

                                _ = _logger.LogDetailAsync(
                                    $"BLOCKED NOVA_OUTPUT Set to Processed: TASK: {good.TANUM} -- SKU: {good.MATNR}");
                                break;
                            }

                        }
                        catch (Exception e)
                        {
                            _ = _logger.LogDetailAsync(
                                $"Single Goods Issue Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                            Mediator.GetInstance().OnSendEmailMessage(this, 
                                $"Single Goods Issue Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
                        }
                    }
                }
            }
            catch (RfcCommunicationException e)
            {
             _ = _logger.LogDetailAsync($"Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
             Mediator.GetInstance().OnSendEmailMessage(this, 
                 $"Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            catch (RfcLogonException e)
            {
             _ = _logger.LogDetailAsync($"Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
             Mediator.GetInstance().OnSendEmailMessage(this, 
                 $"Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            catch (RfcAbapRuntimeException e)
            {
             _ = _logger.LogDetailAsync($"Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
             Mediator.GetInstance().OnSendEmailMessage(this, 
                 $"Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            catch (RfcAbapBaseException e)
            {
             _ = _logger.LogDetailAsync($"Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
             Mediator.GetInstance().OnSendEmailMessage(this, 
                 $"Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
            catch (Exception e)
            {
             _ = _logger.LogDetailAsync($"Goods Issue Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
             Mediator.GetInstance().OnSendEmailMessage(this, 
                 $"Goods Issue Exception {e.Message}{Environment.NewLine}{e.InnerException} ");
            }
        }

        private List<GoodsIssue> GetGoodsIssuesFromOutput()
        {
            List<GoodsIssue> goodsIssues = new List<GoodsIssue>();
            try
            {
                using (var db = new WagnerDb())
                {
                    List<NOVA_OUTPUT> recs = db.NOVA_OUTPUT.Where(n => n.PROCESSED == "N" && n.TRANSTYPE == "22").OrderByDescending(o => o.TRANSDATE).ToList();

                    foreach (var item in recs)
                    {
                        var g = new GoodsIssue();
                        g.TRANSID = Convert.ToDecimal(item.TRANSID);
                        g.TANUM = item.TASKNO.ToString(CultureInfo.InvariantCulture);
                        g.TAPOS = item.TOTENO.ToString(CultureInfo.InvariantCulture);
                        g.MATNR = item.SKU;
                        g.NSOLM = Convert.ToDecimal(item.BEGINNINGQTY);
                        g.NISTA = Convert.ToDecimal(item.QTY);
                        g.MATKL = item.SKUDESC;
                        g.SPART = item.DIVISION;
                        g.VSBED = item.PRIORITY;
                        g.VBELN = item.ORDERNO.ToString(CultureInfo.InvariantCulture);
                        g.EAN11 = item.UPC;
                        g.MEINS = item.BP;

                        _ = _logger.LogDetailAsync($"Goods Issue - Output to SAP.  TASKNO: {g.TANUM}  SKU: {g.MATNR} REQ QTY: {g.NSOLM} ISS QTY: {g.NISTA}");

                        goodsIssues.Add(g);
                    }
                }
            }
            catch (Exception e)
            {
             _ = _logger.LogDetailAsync($"GetGoodsIssuesFromOutput {e.Message}{Environment.NewLine}{e.InnerException} ");
             Mediator.GetInstance().OnSendEmailMessage(this, 
                 $"GetGoodsIssuesFromOutput {e.Message}{Environment.NewLine}{e.InnerException} ");
            }

            return goodsIssues;
        }
    }
}
