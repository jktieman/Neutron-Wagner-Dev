using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Neutron.Models;
using NeutronCore;
using NeutronData.Models;

namespace PrintRequest
{
    public static class DocumentToPrint
    {

        public static void Print(int positionNumber, string order, DocumentPrinterPreferences printer, string invoice = @"9999999999")
        {
            string directory = LoaderSettings.GetDocumentsDirectory();
            string fileName = ($"_PACKX_{order.Trim()}{invoice.Trim()}.LIS");
            string fullpath = Path.Combine(directory, fileName );

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

        public static void Print(string order, DocumentPrinterPreferences printer, string invoice = @"9999999999")
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

        public static void PrintAnticipatedOuts(List<AnticipatedOut> anticipatedOuts,
            DocumentPrinterPreferences printer)
        {
            var result = new AnticipatedOutsProcessor().PrintAnticipatedOutsDocument(anticipatedOuts, printer);
        }

        public static void PrintPackingList(List<PackingList> packingLists,
            DocumentPrinterPreferences printer)
        {
            var result = new PackingListProcessor().PrintPackingListDocument(packingLists, printer);
        }

        public static void PrintAvailableLocations(List<Location> locations,
            DocumentPrinterPreferences printer)
        {
            var result = new AvailableLocationsProcessor().PrintAvailableLocationsDocument(locations, printer);
        }

        public static void PrintPickList(List<PickList> pickLists,
            DocumentPrinterPreferences printer)
        {
            var result = new PickListProcessor().PrintPickListDocument(pickLists, printer);
        }
    }
}
