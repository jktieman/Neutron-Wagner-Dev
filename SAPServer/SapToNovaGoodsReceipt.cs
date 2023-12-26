using System;
using System.Collections.Generic;
using AlliedLogger;
using JsonManager;
using NeutronEvents;
using SAP.Middleware.Connector;
using SAPServer.Models;

namespace SAPServer
{
    public class SapToNovaGoodsReceipt
    {
        private readonly IJsonData _jsonData;
        private readonly IDynamicLogger _logger;

        public SapToNovaGoodsReceipt(IJsonData jsonData, IDynamicLogger logger)
        {
            _jsonData = jsonData;
            _logger = logger;
        }
        /// <summary>
        /// Retrieves the goods receipt records from the SAP system.
        /// </summary>
        /// <param name="destination">The RFC destination which represents the SAP system.</param>
        /// <remarks>
        /// This method fetches the goods receipt records from the SAP system using the provided RFC destination.
        /// The fetched records are then processed and saved as a JSON file for backup purposes.
        /// If any exceptions occur during the process, they are logged and an email is sent with the exception details.
        /// </remarks>
        /// <exception cref="RfcCommunicationException">Thrown when there is a communication error with the SAP system.</exception>
        /// <exception cref="RfcLogonException">Thrown when there is a logon error in the SAP system.</exception>
        /// <exception cref="RfcAbapRuntimeException">Thrown when there is a runtime error in the ABAP program.</exception>
        /// <exception cref="RfcAbapBaseException">Thrown when there is a base exception in the ABAP program.</exception>

        public void Get(RfcDestination destination)
        {
            var unProcessedGoods = new List<GoodsReceipt>();

            try
            {

                // Represents the repository of the RFC destination. This repository is used to create and manage RFC functions.
                RfcRepository repo = destination.Repository;

                // Represents an RFC function named "ZWM_SAP_TO_NOVA" created from the RFC repository.
                // This function is used to interact with the SAP system, specifically to retrieve a list of goods receipts.
                IRfcFunction sapToNovaList = repo.CreateFunction("ZWM_SAP_TO_NOVA");

                // Represents a table of goods receipts retrieved from the
                // SAP system via the "ZWM_SAP_TO_NOVA" RFC function.
                // Each row in the table corresponds to a single goods receipt,
                // with fields for receipt number (TANUM), item number (TAPOS),
                // material number (MATNR), quantity (VERME), unit of measure (MEINS),
                // and material description (MAKTX).
                IRfcTable goodsReceiptTable = sapToNovaList.GetTable("LT_SAPNOVA");

                // Represents a list of GoodsReceipt objects.
                // Each object in the list corresponds to a goods receipt retrieved from the SAP system.
                // This list is populated by invoking the "ZWM_SAP_TO_NOVA" RFC function
                // and iterating over the returned table of goods receipts.
                // The list is then used for further processing, such as logging
                // the number of retrieved records, creating a backup JSON file,
                // and writing the goods receipts to the INPUT table in the Wagner Database.
                List<GoodsReceipt> goodsReceiptList = new List<GoodsReceipt>();

                
                // Load the data
                sapToNovaList.Invoke(destination);

                
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

