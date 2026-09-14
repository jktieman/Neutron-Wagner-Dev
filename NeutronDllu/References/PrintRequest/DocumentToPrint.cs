using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NeutronCore;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronDllu;


namespace PrintRequest
{
    public class DocumentToPrint
    {
        
        public void Print(int positionNumber, string order, DocumentPrinterPreferences printer, string invoice = @"9999999999")
        {
            var directory = LoaderSettings.GetDocumentsDirectory();
            var fileName = ($"_PACKX_{order.Trim()}{invoice.Trim()}.LIS");
            var fullPath = Path.Combine(directory, fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    RawPrinterHelper.SendFileToPrinter(printer.PrinterName, fullPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Print Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else
            {
                MessageBox.Show($"Document File Not Found.  {fullPath}");
            }


        }

        public void Print(string order, DocumentPrinterPreferences printer, string invoice = @"9999999999")
        {
            var directory = LoaderSettings.GetDocumentsDirectory();
            var fileName = ($"_PACKX_{order.Trim()}{invoice.Trim()}.LIS");
            var fullPath = Path.Combine(directory, fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    RawPrinterHelper.SendFileToPrinter(printer.PrinterName, fullPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Print Error: {ex.Message} \r\n\r\n {ex.InnerException}");
                }
            }
            else
            {
                MessageBox.Show($"Document File Not Found.  {fullPath}");
            }
        }

        public void PrintAnticipatedOuts(List<AnticipatedOut> anticipatedOuts,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            new AnticipatedOutsProcessor().PrintAnticipatedOutsDocument(anticipatedOuts, printer, printPreview);
        }

        public void PrintPackingList(List<PackingList> packingLists,
            DocumentPrinterPreferences printer, bool printPreview)
        {

            new PackingListProcessor().PrintPackingListDocument(packingLists, printer, printPreview);
        }

        public void PrintAvailableLocations(List<Location> locations,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            new AvailableLocationsProcessor().PrintAvailableLocationsDocument(locations, printer, printPreview);
        }

        public void PrintPickList(List<PickList> pickLists,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            new PickListProcessor().PrintPickListDocument(pickLists, printer, printPreview);
        }

        public void PrintSummary(List<ProductivitySummary> summaries,
            DocumentPrinterPreferences printer, bool printPreview)
        {
            new ProductivitySummaryProcessor()
                .PrintProductivitySummaryDocument(summaries, printer, printPreview);
        }

        public void PrintDetail(List<ProductivityDetail> details, DocumentPrinterPreferences printer, bool printPreview)
        {
            new ProductivityDetailProcessor()
                .PrintProductivityDetailDocument(details, printer, printPreview);
        }
    }
}
