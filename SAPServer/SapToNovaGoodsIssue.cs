using System;
using System.Collections.Generic;
using System.Linq;
using AlliedLogger;
//using AlliedPostOffice;
using JsonManager;
using NeutronData.DataContexts;
using SAP.Middleware.Connector;
using SAPServer.Models;

namespace SAPServer
{
    public class SapToNeutronGoodsIssue
    {
       // private readonly ISendEmail _sendEmail;
        private readonly IJsonData _jsonData;
        private readonly IDynamicLogger _logger;

        public SapToNeutronGoodsIssue( IJsonData jsonData, IDynamicLogger logger)
        {
           // _sendEmail = sendEmail;
            _jsonData = jsonData;
            _logger = logger;
        }

        public void Get(RfcDestination destination)
        {
            var unProcessedGoods = new List<GoodsIssue>();

            try
            {

                RfcRepository repo = destination.Repository;

                IRfcFunction sapToNeutronList = repo.CreateFunction("ZWM_SAP_TO_NOVA_GOODS_ISSUE");

                var goodsIssueList = new List<GoodsIssue>();

                IRfcTable goodsIssueTable = sapToNeutronList.GetTable("LT_SAPNOVA_G");

                sapToNeutronList.Invoke(destination);

                for (int cuIndex = 0; cuIndex < goodsIssueTable.RowCount; cuIndex++)
                {

                    goodsIssueTable.CurrentIndex = cuIndex;
                    var goodsIssue = new GoodsIssue();
                    goodsIssue.TANUM = goodsIssueTable.GetString("TANUM");
                    goodsIssue.VBELN = goodsIssueTable.GetString("VBELN");
                    goodsIssue.TAPOS = goodsIssueTable.GetString("TAPOS");
                    goodsIssue.MATNR = goodsIssueTable.GetString("MATNR");
                    goodsIssue.NSOLM = goodsIssueTable.GetDecimal("NSOLM");
                    goodsIssue.MEINS = goodsIssueTable.GetString("MEINS");
                    goodsIssue.MATKL = goodsIssueTable.GetString("MATKL");
                    goodsIssue.NISTA = goodsIssueTable.GetDecimal("NSOLM");
                    goodsIssue.EAN11 = goodsIssueTable.GetString("EAN11");
                    goodsIssue.SPART = goodsIssueTable.GetString("SPART");
                    goodsIssue.VSBED = goodsIssueTable.GetString("VSBED");
                    goodsIssue.KDMAT = goodsIssueTable.GetString("KDMAT");
                    goodsIssue.TEXT = goodsIssueTable.GetString("TEXT");
                    
                    if (string.IsNullOrEmpty(goodsIssue.VBELN))
                    {
                        goodsIssue.VBELN = goodsIssue.TANUM;
                    }

                    goodsIssueList.Add(goodsIssue);
                }

                Console.WriteLine($"Goods Issue Records Retrieved From SAP: {goodsIssueList.Count}");
                _ = _logger.LogDetailAsync($"Goods Issue Records Retrieved From SAP: {goodsIssueList.Count}");
                //------------------
                // Create backup Json file in case the Wagner Database is unavailable
                var recs = _jsonData.LoadFile<List<GoodsIssue>>();
                if (recs.Count > 0)
                {
                    _ = _logger.LogDetailAsync($"Goods Issue Records Retrieved From Backup: {recs.Count}");
                    Console.WriteLine($"Goods Issue Records Retrieved From Backup: {recs.Count}");
                    foreach (var rec in recs)
                    {
                        goodsIssueList.Add(rec);
                    }
                }

                _jsonData.SaveFile(goodsIssueList);

                //------------------


                try
                {
                    using (var context = new WagnerDb())
                    //using (var context = new NeutronDb())
                    {

                        foreach (var row in goodsIssueList)
                        {
                            var input = new NOVA_INPUT();
                            input.PROCESSED = @"N";
                            input.TRANSTYPE = @"22";
                            input.SKU = row.MATNR;
                            input.QTY = Convert.ToInt32(row.NSOLM);
                            input.TASKNO = Convert.ToDecimal(row.TANUM);
                            input.SKUDESC = ValidSkuDesc(row.MATKL);   
                            input.PRIORITY = GetPriority(row.SPART, row.VSBED);  // PRIORITY
                            input.DIVISION = row.SPART;  //DIVISION
                            input.ORDERNO = Convert.ToDecimal(row.VBELN);
                            input.TOTENO = Convert.ToDecimal(row.TAPOS);
                            input.UPC = row.EAN11;
                            input.BP = row.MEINS;
                            input.TRANSDATE = DateTime.Now;
                            input.TEXT = row.TEXT;


                            try
                            {
                                context.NOVA_INPUT.Add(input);
                                context.SaveChanges();
                                row.Processed = true;
                                Console.WriteLine($"Saving TaskNo: {input.TASKNO} SKU: {input.SKU} DESC: {input.SKUDESC} to INPUT");
                                _ = _logger.LogDetailAsync($"Saving TaskNo: {input.TASKNO} SKU: {input.SKU} DESC: {input.SKUDESC} to INPUT");
                            }
                            catch (Exception e)
                            {
                                _ = _logger.LogDetailAsync($"Error writing Goods Issue TaskNo: {input.TASKNO} SKU: {input.SKU} DESC: {input.SKUDESC} to INPUT Table. {Environment.NewLine}  {e.Message} {Environment.NewLine} {e.InnerException}");
                                Console.WriteLine($"Error writing Goods Issue TaskNo: {input.TASKNO} SKU: {input.SKU} DESC: {input.SKUDESC} to INPUT Table. {Environment.NewLine}  {e.Message} {Environment.NewLine} {e.InnerException}");
                               // _sendEmail.Message($"Error writing Single Goods Issue to INPUT Table", _logger.LastLogLines());

                            }
                        }
                    }

                    foreach (var goodsIssue in goodsIssueList)
                    {
                        if (goodsIssue.Processed == false)
                        {
                            unProcessedGoods.Add(goodsIssue);
                        }
                    }
                    _jsonData.SaveFile(unProcessedGoods);
                    _ = _logger.LogDetailAsync($"UnProcessed Goods Issue Record Count: {unProcessedGoods.Count}");
                    Console.WriteLine($"UnProcessed Goods Issue Record Count: {unProcessedGoods.Count}");

                }
                catch (Exception e)
                {
                    _ = _logger.LogDetailAsync($"Error writing Goods Issue to INPUT Table.  {e.Message} {Environment.NewLine} {e.InnerException}");
                    Console.WriteLine($"Error writing Goods Issue to INPUT Table.  {e.Message} {Environment.NewLine} {e.InnerException}");
                    //_sendEmail.Message($"Error writing Goods Issue to INPUT Table", _logger.LastLogLines());
                }

            }
            catch (RfcCommunicationException e)
            {
                _ = _logger.LogDetailAsync($"Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue RfcCommunicationException {e.Message}{Environment.NewLine}{e.InnerException} ");
                //_sendEmail.Message("SAP to Nova Goods Issue Communication Error", _logger.LastLogLines());
            }
            catch (RfcLogonException e)
            {
                _ = _logger.LogDetailAsync($"Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue RfcLogonException {e.Message}{Environment.NewLine}{e.InnerException} ");
                //_sendEmail.Message("SAP to Nova Goods Issue Communication Error", _logger.LastLogLines());
            }
            catch (RfcAbapRuntimeException e)
            {
                _ = _logger.LogDetailAsync($"Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue RfcAbapRuntimeException {e.Message}{Environment.NewLine}{e.InnerException} ");
                //_sendEmail.Message("SAP to Nova Goods Issue Communication Error", _logger.LastLogLines());
            }
            catch (RfcAbapBaseException e)
            {
                _ = _logger.LogDetailAsync($"Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                Console.WriteLine($"Goods Issue RfcAbapBaseException {e.Message}{Environment.NewLine}{e.InnerException} ");
                //_sendEmail.Message("SAP to Nova Goods Issue Communication Error", _logger.LastLogLines());
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
                    result = result.Replace("'", "");
                }
            }
            catch (Exception e)
            {
                _ = _logger.LogDetailAsync($"Invalid Issue SkuDesc/MATKL: [ {s} ] {Environment.NewLine} {e.Message} {Environment.NewLine} {e.InnerException}");
                Console.WriteLine($"Invalid Issue SkuDesc/MATKL: [ {s} ] {Environment.NewLine} {e.Message} {Environment.NewLine} {e.InnerException}");
            }
            return result;
        }

        private string GetPriority(string spart, string vsbeds)
        {
            var result = string.Empty;
            
                using (var db = new WagnerDb())
                //using (var db = new NeutronDb())
                {
                    var pri = db.PriorityRecords.FirstOrDefault(r => r.Spart == spart && r.Vsbed == vsbeds);
                    if (pri != null)
                    {
                        result = pri.Priority;
                    }
                }
            return result;
        }

    }
}
