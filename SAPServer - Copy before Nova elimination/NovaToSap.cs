using SAP.Middleware.Connector;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using JsonManager;
using SAPServer.Models;
using AlliedLogger;
using AlliedPostOffice.Concrete;
using System.Text;
using System.Net.Mail;
using SAPServer.Extensions;

namespace SAPServer
{
    public class NovaToSap
    {
        protected string TANUM;
        protected string TAPOS;
        protected string MATNR;
        protected decimal VERME;
        protected string MEINS;
        protected string MAKTX;


        public void SetNovaToSapDetails(RfcDestination destination)
        {
            try
            {
                RfcRepository repo = destination.Repository;
                IRfcFunction novaToSapList = repo.CreateFunction("ZWM_NOVA_TO_SAP");

                //IRfcTable idRange = sapToNovaList.GetTable("IdRange");
                //idRange.SetValue("SIGN", "I");
                //idRange.SetValue("OPTION", "BT");
                //idRange.SetValue("LOW", "");
                //idRange.SetValue("HIGH", "999999");
                ////add selection range to customerList function to search for all customers
                //sapToNovaList.SetValue("idrange", idRange);


                IRfcTable NovaData = novaToSapList.GetTable("LT_NOVASAP");
                foreach (var good in goodsReceiptList)
                {
                    NovaData.Append();
                    NovaData.SetValue("TANUM", good.TANUM);
                    NovaData.SetValue("TAPOS", good.TAPOS);
                    NovaData.SetValue("MATNR", good.MATNR);
                    NovaData.SetValue("VERME", good.VERME);
                    NovaData.SetValue("MEINS", good.MEINS);
                    NovaData.SetValue("MAKTX", good.MAKTX);
                    NovaData.SetValue("NISTA", good.VERME);
                }


                novaToSapList.Invoke(destination);


                //Console.WriteLine(NovaData.GetValue("TANUM"));


                //var fieldsTable = novaToSapList.GetTable(0);



                //for (int cuIndex = 0; cuIndex < NovaData.RowCount; cuIndex++)
                //{

                //    NovaData.CurrentIndex = cuIndex;
                //    //IRfcFunction customerHierachy = repo.CreateFunction("BAPI_CUSTOMER_GETSALESAREAS");
                //    //IRfcFunction customerDetail1 = repo.CreateFunction("BAPI_CUSTOMER_GETDETAIL1");
                //    //IRfcFunction customerDetail2 = repo.CreateFunction("BAPI_CUSTOMER_GETDETAIL2");

                //    this.TANUM = NovaData.GetString("TANUM");
                //    this.TAPOS = NovaData.GetString("TAPOS");
                //    this.MATNR = NovaData.GetString("MATNR");
                //    this.VERME = NovaData.GetDecimal("VERME");
                //    this.MEINS = NovaData.GetString("MEINS");
                //    this.MAKTX = NovaData.GetString("MAKTX");


                //    Console.WriteLine($"TANUM: {TANUM}");

                //    //customerDetail2.SetValue("CustomerNo", this.CustomerNo);
                //    //customerDetail2.Invoke(destination);
                //    //IRfcStructure generalDetail = customerDetail2.GetStructure("CustomerGeneralDetail");
                //    //this.Region = generalDetail.GetString("Reg_Market");
                //    //this.Industry = generalDetail.GetString("Industry");
                //    //customerDetail1.Invoke(destination);
                //    //IRfcStructure detail1 = customerDetail1.GetStructure("PE_CompanyData");
                //    //this.District = detail1.GetString("District");

                //    //customerHierachy.Invoke(destination);
                //    //customerHierachy.SetValue("CustomerNo", this.CustomerNo);
                //    //customerHierachy.Invoke(destination);
                //    //IRfcTable otherDetail = customerHierachy.GetTable("SalesAreas");
                //    //if (otherDetail.RowCount > 0)
                //    //{
                //    //    this.SalesOrg = otherDetail.GetString("SalesOrg");
                //    //    this.DistributionChannel = otherDetail.GetString("DistrChn");
                //    //    this.Division = otherDetail.GetString("Division");
                //    //}
                //    //customerHierachy = null;
                //    //customerDetail1 = null;
                //    //customerDetail2 = null;
                //    GC.Collect();
                //    GC.WaitForPendingFinalizers();
                //}
            }
            catch (RfcCommunicationException e)
            {
                Console.WriteLine($"RfcCommunicationException {e.Message}\r\n{e.InnerException} ");
            }
            catch (RfcLogonException e)
            {
                Console.WriteLine($"RfcLogonException {e.Message}\r\n{e.InnerException} ");
            }
            catch (RfcAbapRuntimeException e)
            {
                Console.WriteLine($"RfcAbapRuntimeException {e.Message}\r\n{e.InnerException} ");
            }
            catch (RfcAbapBaseException e)
            {
                Console.WriteLine($"RfcAbapBaseException {e.Message}\r\n{e.InnerException} ");
            }
        }


    }
}
