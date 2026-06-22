using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NeutronData.Models;
using NeutronData.PrintModels;


namespace NeutronDllu
{
    public static class ToteToPrint
    {
        private static readonly object LockerFile = new object();
        public static void Print(int positionNumber, Order order, LabelPrinterPreferences printer)
        {
            string type3 = " " + order.OrderInfo;
            string t16_10 = order.Ord1.Trim();
            string t324_9 = type3.Substring(27, 9);
            string t30_1 = type3.Substring(3, 1);
            string t37_1 = type3.Substring(10, 1);
            string t36_1 = type3.Substring(9, 1);
            string t34_2 = type3.Substring(7, 2);
            string t38_2 = type3.Substring(11, 2);
            string t310_2 = type3.Substring(13, 2);
            string t312_2 = type3.Substring(15, 2);
            string t314_2 = type3.Substring(17, 2);
            string t322_2 = type3.Substring(25, 2);
            string t316_2 = type3.Substring(19, 2);
            string t318_4 = type3.Substring(21, 4);
            string t333_25 = type3.Substring(36, 25);

            string batchPos = positionNumber.ToString();
            string xPos = printer.HomeX.ToString();
            string yPos = printer.HomeY.ToString();

            var sb = new StringBuilder();
            sb.AppendLine($"^XA^LH{xPos},{yPos}^PR5,5,5^MD8^MMT");
            sb.AppendLine($"^BY5,3.0^FO88,308^B3R,N,176,Y,N^FR^FD{t16_10}^FS");
            sb.AppendLine($"^BY3,3.0^FO130,54^BCI,175,Y,N,A^FR^FD{t324_9}{t37_1}{t36_1}{t34_2}^FS");
            sb.AppendLine($"^FO695,240^AER^CI0^FDOrder#:^FS");
            sb.AppendLine($"^FO571,240^AER^CI0^FDStops:^FS");
            sb.AppendLine($"^FO590,380^A0R,190,170^CI0^FD{t16_10}^FS");
            sb.AppendLine($"^FO480,380^A0R,130,145^CI0^FD{t38_2} {t310_2} {t312_2} {t314_2}^FS");
            sb.AppendLine($"^FO350,380^A0R,130,145^CI0^FD{t322_2} {t316_2}^FS");
            sb.AppendLine($"^FO290,780^A0R,190,160^CI0^FD{t318_4}^FS");
            sb.AppendLine($"^FO365,275^A0R,100,75^CI0^FD{batchPos}^FS");
            sb.AppendLine($"^FO260,380^A0R,60,60^CI0^FD{t333_25}^FS");
            sb.AppendLine($"^PQ1,0,0,0,N");
            sb.AppendLine($"^XZ");

            try
            {
                RawPrinterHelper.SendStringToPrinter(printer.PrinterName, sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print Label Error: {ex.Message} \r\n\r\n {ex.InnerException}");
            }
        }

        public static void Print(int positionNumber, ReplenOrder order, LabelPrinterPreferences printer)
        {
            string type3 = " " + order.OrderInfo;
            string t16_10 = order.Ord1.Trim();
            string t324_9 = type3.Substring(27, 9);
            string t30_1 = type3.Substring(3, 1);
            string t37_1 = type3.Substring(10, 1);
            string t36_1 = type3.Substring(9, 1);
            string t34_2 = type3.Substring(7, 2);
            string t38_2 = type3.Substring(11, 2);
            string t310_2 = type3.Substring(13, 2);
            string t312_2 = type3.Substring(15, 2);
            string t314_2 = type3.Substring(17, 2);
            string t322_2 = type3.Substring(25, 2);
            string t316_2 = type3.Substring(19, 2);
            string t318_4 = type3.Substring(21, 4);
            string t333_25 = type3.Substring(36, 25);

            string batchPos = positionNumber.ToString();
            string xPos = printer.HomeX.ToString();
            string yPos = printer.HomeY.ToString();

            var sb = new StringBuilder();
            sb.AppendLine($"^XA^LH{xPos},{yPos}^PR5,5,5^MD8^MMT");
            sb.AppendLine($"^BY5,3.0^FO88,308^B3R,N,176,Y,N^FR^FD{t16_10}^FS");
            sb.AppendLine($"^BY3,3.0^FO130,54^BCI,175,Y,N,A^FR^FD{t324_9}{t37_1}{t36_1}{t34_2}^FS");
            sb.AppendLine($"^FO695,240^AER^CI0^FDOrder#:^FS");
            sb.AppendLine($"^FO571,240^AER^CI0^FDStops:^FS");
            sb.AppendLine($"^FO590,380^A0R,190,170^CI0^FD{t16_10}^FS");
            sb.AppendLine($"^FO480,380^A0R,130,145^CI0^FD{t38_2} {t310_2} {t312_2} {t314_2}^FS");
            sb.AppendLine($"^FO350,380^A0R,130,145^CI0^FD{t322_2} {t316_2}^FS");
            sb.AppendLine($"^FO290,780^A0R,190,160^CI0^FD{t318_4}^FS");
            sb.AppendLine($"^FO365,275^A0R,100,75^CI0^FD{batchPos}^FS");
            sb.AppendLine($"^FO260,380^A0R,60,60^CI0^FD{t333_25}^FS");
            sb.AppendLine($"^PQ1,0,0,0,N");
            sb.AppendLine($"^XZ");

            try
            {
                RawPrinterHelper.SendStringToPrinter(printer.PrinterName, sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print Label Error: {ex.Message} \r\n\r\n {ex.InnerException}");
            }
        }

        public static void Print(Order order, LabelPrinterPreferences printer)
        {
            string type3 = " " + order.OrderInfo;
            string t16_10 = order.Ord1.Trim();
            string t324_9 = type3.Substring(27, 9);
            string t30_1 = type3.Substring(3, 1);
            string t37_1 = type3.Substring(10, 1);
            string t36_1 = type3.Substring(9, 1);
            string t34_2 = type3.Substring(7, 2);
            string t38_2 = type3.Substring(11, 2);
            string t310_2 = type3.Substring(13, 2);
            string t312_2 = type3.Substring(15, 2);
            string t314_2 = type3.Substring(17, 2);
            string t322_2 = type3.Substring(25, 2);
            string t316_2 = type3.Substring(19, 2);
            string t318_4 = type3.Substring(21, 4);
            string t333_25 = type3.Substring(36, 25);

            string batchPos = "1";
            string xPos = printer.HomeX.ToString();
            string yPos = printer.HomeY.ToString();

            var sb = new StringBuilder();
            sb.AppendLine($"^XA^LH{xPos},{yPos}^PR5,5,5^MD8^MMT");
            sb.AppendLine($"^BY5,3.0^FO88,308^B3R,N,176,Y,N^FR^FD{t16_10}^FS");
            sb.AppendLine($"^BY3,3.0^FO130,54^BCI,175,Y,N,A^FR^FD{t324_9}{t37_1}{t36_1}{t34_2}^FS");
            sb.AppendLine($"^FO695,240^AER^CI0^FDOrder#:^FS");
            sb.AppendLine($"^FO571,240^AER^CI0^FDStops:^FS");
            sb.AppendLine($"^FO590,380^A0R,190,170^CI0^FD{t16_10}^FS");
            sb.AppendLine($"^FO480,380^A0R,130,145^CI0^FD{t38_2} {t310_2} {t312_2} {t314_2}^FS");
            sb.AppendLine($"^FO350,380^A0R,130,145^CI0^FD{t322_2} {t316_2}^FS");
            sb.AppendLine($"^FO290,780^A0R,190,160^CI0^FD{t318_4}^FS");
            sb.AppendLine($"^FO365,275^A0R,100,75^CI0^FD{batchPos}^FS");
            sb.AppendLine($"^FO260,380^A0R,60,60^CI0^FD{t333_25}^FS");
            sb.AppendLine($"^PQ1,0,0,0,N");
            sb.AppendLine($"^XZ");

            try
            {
                RawPrinterHelper.SendStringToPrinter(printer.PrinterName, sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print Label Error: {ex.Message} \r\n\r\n {ex.InnerException}");
            }
        }

        public static void Print(int reqFunction, int pickPos, LabelDetail labelDetail, string upc,
            LabelPrinterPreferences printer)
        {
            var msg = string.Empty;
            var item = labelDetail.Item;
            var desc = labelDetail.Description;

            // Get the UPC-A number if it has one
            var printUpc = false;
            //recFound = False
            //key = RIGHT$(SPACE$(35) + TRIM$(TDetail.sku) ,35)
            //var key = labelDetail.ItemDefinition.Item;
            // var upc = 
            // AKAAcc 1, key, 0, RecFound,0,AKA
            // IF recFound THEN
            //  UPC = TRIM$(AKA.AKASKU)
            if (!string.IsNullOrEmpty(upc))
            {
                if (upc.Length >= 11)
                {
                    upc = upc.Substring(0, 11);
                }

                printUpc = true;
            }

            // label print is not for "48" parts unless reprint, 2 is a reprint
            //if (item.StartsWith("48") && reqFunction != 2) return;

            // label print is not for "49" parts unless reprint, 2 is a reprint
            //if (item.StartsWith("49") && reqFunction != 2) return;

            //Change to make adjusted qty's show up on label
            // Jan 18, 2017
            //IF TDetail.DbOvrFlag(1) = TRUE THEN
            //    QTY = FORMAT$(TDetail.DbOvrQty(1))
            //ELSE
            //    QTY = FORMAT$(TDetail.Qty)
            //END IF
            var qty = labelDetail.Quantity;
            var empId = labelDetail.EmpId;
            var order = labelDetail.Order;
            var invoice = $"          "; //labelDetail.Invoice
            var date = labelDetail.LoadDate;
            var loadDate =
                $"{date.Month.ToString().PadLeft(2, '0')}-{date.Day.ToString().PadLeft(2, '0')}-{date.Year.ToString()}";
            var origin = labelDetail.Origin;

            var sb = new StringBuilder();
            sb.AppendLine($"^XA");

            sb.AppendLine(printUpc ? $"^FO465,010^BY3^BUN,150^FD{upc}^FS" : $"^FO25,010^B3N,N,150,N,N^FD{item}^FS");

            sb.AppendLine($"^FO025,175^A0N,100,105^FD{item}^FS");
            sb.AppendLine($"^FO25,260^ABN,25,14^FD{desc}^FS");
            sb.AppendLine($"^FO25,290^ABN,25,14^FDQTY: {qty}        *{empId}*^FS");
            sb.AppendLine($"^FO25,325^ABN,25,14^FDM{order}  {invoice} * {loadDate}^FS");
            sb.AppendLine($"^FO25,360^ABN,25,14^FDMADE IN {origin}^FS");
            sb.AppendLine($"^XZ");

            try
            {
                RawPrinterHelper.SendStringToPrinter(printer.PrinterName, sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Print Label Error: {ex.Message} {ex.InnerException}");
            }

        }

        public static void PrintLoftwareLabel(string filePath, string printerNumber
            , string upc
            , string aItem
            , string cItem
            , string quantity
            , string desc
            , string division)
        {
            // verify that filePath ends with a backslash
            if (!filePath.EndsWith("\\"))
            {
                filePath += "\\";
            }
            var tempFilePath = $"{filePath}Temp" + "\\";
            //make sure tempFilePath exists
            Directory.CreateDirectory(tempFilePath);
            
            
            upc = string.IsNullOrEmpty(upc) ? string.Empty : upc.Trim();
            aItem = aItem.Trim();
            cItem = cItem.Trim();
            quantity = quantity.Trim();
            desc = desc.Trim();
            division = string.IsNullOrEmpty(division) ? string.Empty : division.Trim();

            var divisionFormat = GetDivisionFormat(filePath, division);
            var tmpExtension = ".tmp";
            var pasExtension = ".pas";
            var fileName = GetSpecialFileName(aItem);
            
            // loftWareFilePath does not have an extension
            // the extension will be added later
            var loftwareFilePath = $"{filePath}{fileName}";
            var tmpLoftwareFilePath = $"{tempFilePath}{fileName}{tmpExtension}";
            var pasLoftwareFilePath = $"{loftwareFilePath}{pasExtension}";
            
            LogLoftwareLabel($"Loftware File Path: {tmpLoftwareFilePath}");

            while (File.Exists(tmpLoftwareFilePath))
            {
                LogLoftwareLabel("File already EXISTS!!!");
                System.Threading.Thread.Sleep(100);
                tmpLoftwareFilePath = $"{filePath}{GetSpecialFileName(aItem)}{tmpExtension}";
                LogLoftwareLabel(tmpLoftwareFilePath);
            }

            var sb = new StringBuilder(200);
            sb.AppendLine($"*FORMAT,{divisionFormat}");
            sb.AppendLine($"UPC,{upc}");
            sb.AppendLine($"ITEM,{aItem}");
            sb.AppendLine($"CITEM,{cItem}");
            sb.AppendLine($"BQTY,{quantity}");
            sb.AppendLine($"DESC,{desc}");
            sb.AppendLine($"COMP,{division}");
            sb.AppendLine($"*QUANTITY,{1}");
            sb.AppendLine($"*PRINTERNUMBER,{printerNumber}");
            sb.AppendLine($"*PRINTLABEL");

            LogLoftwareLabel($"Label Data {Environment.NewLine} {sb}");

            lock (LockerFile)
            {
                try
                {
                    File.WriteAllText(tmpLoftwareFilePath, sb.ToString());
                    
                    // rename the file with a .pas extension
                    File.Move(tmpLoftwareFilePath, pasLoftwareFilePath);
                    
                }
                catch (Exception ex)
                {
                    LogLoftwareLabel($"Print Label Error: {ex.Message}{Environment.NewLine}{ex.InnerException}{Environment.NewLine}{ex.StackTrace}");
                }
            }
        }

        private static string GetDivisionFormat(string filePath, string division)
        {
            //            'DEFAULT FORMAT
            var divisionFormat = @"PARTZTPT.LWL";
            var divisionFormatFilename = $"{filePath}DIVISIONFORMAT.TXT";

            if (!File.Exists(divisionFormatFilename))
            {
                LogLoftwareLabel($"Division Format Filename does NOT exist.");
                return divisionFormat;
            }

            // read the division format file
            // open the file
            var divisionFormatFile = File.OpenText(divisionFormatFilename);
            // read the file
            var divisionFormatFileContents = divisionFormatFile.ReadToEnd();
            // close the file
            divisionFormatFile.Close();

            // create an array from divisionFormatFileContents
            var lines = divisionFormatFileContents.Split('\n');
            foreach (var line in lines)
            {
                var div = line.Split(',');
                var d = div[0].Replace('"', ' ').Trim();
                if (div.Length != 2) continue;
                if (d == division)
                {
                    divisionFormat = div[1].Replace('"', ' ').Trim();
                    LogLoftwareLabel($"Division Format: {divisionFormat}");
                    break;
                }
            }
            return divisionFormat;
        }

        private static string GetSpecialFileName(string sku)
        {
            LogLoftwareLabel($"Get Special FileName for this sku: {sku}");
            var r = new Random();
            var random = r.Next(1, 10000);
            // filename without extension
            var fileName = $"{sku}_{random}";
            LogLoftwareLabel(fileName);
            return fileName;
        }

        private static void LogLoftwareLabel(string message)
        {
            string logFilePath = @"C:\Loftware\Loftware.txt";
            try
            {
                if (!File.Exists(logFilePath))
                {
                    Directory.CreateDirectory(@"C:\Loftware");
                }

                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Log Error: {ex.Message} \r\n\r\n {ex.InnerException}");
            }
        }
    }
}
