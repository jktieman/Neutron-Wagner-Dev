using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NeutronCore;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace PrintRequest
{
    public class DocumentToPrint
    {
        private readonly CultureInfo _cultureInfo;

        public DocumentToPrint()
        {
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
        }

        public void Print(int positionNumber, string order, DocumentPrinterPreferences printer, string invoice = @"9999999999")
        {
            string directory = LoaderSettings.GetDocumentsDirectory();
            string fileName = ($"_PACKX_{order.Trim()}{invoice.Trim()}.LIS");
            string fullpath = Path.Combine(directory, fileName);

            //MessageBox.Show($"Pos {positionNumber}  Order: {order} Printer: {printerName} Invoice: {invoice}");
            // MessageBox.Show($"Full Path: {fullpath}");
            if (File.Exists(fullpath))
            {
                try
                {
                    RawPrinterHelper.SendFileToPrinter(printer.PrinterName, fullpath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Print Error: {ex.Message} \r\n\r\n {ex.InnerException}");
                }
            }
            else
            {
                MessageBox.Show($"Document File Not Found.  {fullpath}");
            }


        }

        public void Print(string order, DocumentPrinterPreferences printer, string invoice = @"9999999999")
        {
            string directory = LoaderSettings.GetDocumentsDirectory();
            string fileName = ($"_PACKX_{order.Trim()}{invoice.Trim()}.LIS");
            string fullpath = Path.Combine(directory, fileName);

            if (File.Exists(fullpath))
            {
                try
                {
                    RawPrinterHelper.SendFileToPrinter(printer.PrinterName, fullpath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Print Error: {ex.Message} \r\n\r\n {ex.InnerException}");
                }
            }
            else
            {
                MessageBox.Show($"Document File Not Found.  {fullpath}");
            }
        }

        public void PrintAnticipatedOuts(List<AnticipatedOut> anticipatedOuts,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            var result = new AnticipatedOutsProcessor().PrintAnticipatedOutsDocument(anticipatedOuts, printer, printPreview);
        }

        public void PrintPackingList(List<PackingList> packingLists,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            switch (_cultureInfo.Name)
            {
                case "en-US":
                    {
                        var result = new PackingListProcessor().PrintPackingListDocument(packingLists, printer, printPreview);
                        break;
                    }
                case "fr-CA":
                {
                    var result = new PackingListProcessor_FR().PrintPackingListDocument(packingLists, printer, printPreview);
                    break;
                }
            }

        }

        public void PrintAvailableLocations(List<Location> locations,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            var result = new AvailableLocationsProcessor().PrintAvailableLocationsDocument(locations, printer, printPreview);
        }

        public void PrintPickList(List<PickList> pickLists,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            switch (_cultureInfo.Name)
            {
                case "en-US":
                {
                    var result = new PickListProcessor().PrintPickListDocument(pickLists, printer, printPreview);
                        break;
                }
                case "fr-CA":
                {
                    var result = new PickListProcessor_FR().PrintPickListDocument(pickLists, printer, printPreview);
                        break;
                }
            }
        }

        public void PrintSummary(List<ProductivitySummary> summaries,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            var result = new ProductivitySummaryProcessor()
                .PrintProductivitySummaryDocument(summaries, printer, printPreview);
        }

        public void PrintDetail(List<ProductivityDetail> details, DocumentPrinterPreferences printer, bool printPreview)
        {
            var result = new ProductivityDetailProcessor()
                .PrintProductivityDetailDocument(details, printer, printPreview);
        }
    }
}
