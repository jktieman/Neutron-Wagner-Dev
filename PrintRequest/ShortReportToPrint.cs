using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintRequest
{
    public static class ShortReportToPrint
    {
        public static void Print(List<ShortReport> shortReports, LabelPrinterPreferences printer)
        {

            float cy = 0; //     ' This variable keeps track of the Y position on the page.

            var c = new float[5];  // c(1 TO 4) AS SINGLE  ' This array will contain column positions

            //float lh = .1667F; //  6 lines per inch // This is the line-height or space between lines
            c[1] = .25F;
            c[2] = 1.5F;
            c[3] = 3F;
            c[4] = 4.5F;
            //StringBuilder displayBuffer = new StringBuilder();
            //string work;
            // string msg;
            int cardNum = 1;
            bool linePrinted = false;

            //int I;
            //string position = "1";
            //string order = "25468545";
            //string invoice = "6598641";
            //string sku = "10532165";
            //string reqQty = "100";
            //string shippedQty = "50";

            ShortReport firstShortReport = shortReports[0];
            int x = printer.HomeX;
            int y = printer.HomeY;
            var labelBuffer = new StringBuilder();

            StartNewDoc(ref labelBuffer, ref cy, ref x, ref y);

            PrintHeading(firstShortReport, ref labelBuffer, ref cy, ref linePrinted, ref cardNum, ref x, ref y, ref printer);

            PrintOneLine(firstShortReport, ref labelBuffer, ref cy, ref linePrinted, ref cardNum, ref x, ref y, ref printer);

            for (int i = 1; i < shortReports.Count(); i++)
            {
                PrintOneLine(shortReports[i], ref labelBuffer, ref cy, ref linePrinted, ref cardNum, ref x, ref y, ref printer);
            }

            //MessageBox.Show(displayBuffer.ToString());

            //labelBuffer.AppendLine($"^XA^LH0,100");
            //labelBuffer.AppendLine($"^FO100,100^BY3");
            //labelBuffer.AppendLine($"^B1R,N,150,Y,N");
            //labelBuffer.AppendLine($"^FD123456^FS");
            //labelBuffer.AppendLine($"^XZ");


            labelBuffer.AppendLine($"^XZ");


            RawPrinterHelper.SendStringToPrinter(printer.PrinterName, labelBuffer.ToString());
        }

        private static void StartNewDoc(ref StringBuilder labelBuffer, ref float cy, ref int x, ref int y)
        {
            //string x = printer.TextBoxX.Text;
            //string y = TextBoxY.Text;
            //int X = int.Parse(x);
            //int Y = int.Parse(y);
            //string y2 = (Y + 600).ToString();
            //string x2 = (X + 600).ToString();

            cy = 420;
            //labelBuffer.AppendLine($"^XA^LH0,100^PR5,5,5^MMT");
            labelBuffer.AppendLine($"^XA^LH{x},{y}^PR5,5,5^MD8^MMT");
        }

        private static void PrintOneLine(ShortReport shortReport, ref StringBuilder labelBuffer, ref float cy, ref bool linePrinted, ref int cardNum, ref int x, ref int y, ref LabelPrinterPreferences printer)
        {
            linePrinted = true;
            if (cy < 100)
            {
                cardNum += 1;
                labelBuffer.AppendLine($"^XZ");
                RawPrinterHelper.SendStringToPrinter(printer.PrinterName, labelBuffer.ToString());
                labelBuffer = new StringBuilder();

                StartNewDoc(ref labelBuffer, ref cy, ref x, ref y);
                PrintHeading(shortReport, ref labelBuffer, ref cy, ref linePrinted, ref cardNum, ref x, ref y, ref printer);
            }
            //Print the first detail line
            //I = shortReport.Item.Length;
            //displayBuffer.Append($"{sku}{ new string(' ', 35 - I)}{new string(' ', 8 - I)}");
            //I = reqQty.Length;
            //displayBuffer.Append($"{new string(' ', 5 - I)}{reqQty}{new string(' ', 10 - I)}");
            //I = shippedQty.Length;
            //displayBuffer.Append($"{new string(' ', 5 - I)}{shippedQty}{new string(' ', 10 - I)}");
            //displayBuffer.AppendLine();

            labelBuffer.AppendLine($"^FO{cy.ToString()},80^A0R,50,50^CI0^FD{shortReport.Item}^FS");
            labelBuffer.AppendLine($"^FO{cy.ToString()},640^A0R,50,50^CI0^FD{shortReport.ReqQty}^FS");
            labelBuffer.AppendLine($"^FO{cy.ToString()},900^A0R,50,50^CI0^FD{shortReport.IssQty}^FS");

            cy = cy - 80;
        }

        private static void PrintHeading(ShortReport shortReport, ref StringBuilder labelBuffer, ref float cy, ref bool linePrinted, ref int cardNum, ref int x, ref int y, ref LabelPrinterPreferences printer)
        {

            //if (cardNum == 1)
            //{
            //    displayBuffer.AppendLine($"Pos: {position}    Order: {order}    Invoice: {invoice}");
            //    displayBuffer.AppendLine($"SKU                                Ordered   Shipped");
            //}
            //string x = TextBoxX.Text;
            //string y = TextBoxY.Text;
            //int X = int.Parse(x);
            //int Y = int.Parse(y);
            //string y2 = (Y + 600).ToString();
            //string x2 = (X + 600).ToString();
            //labelBuffer.AppendLine($"^FO50,{y}^A0R,80,80^CI0^FD{x},{y}^FS");
            //labelBuffer.AppendLine($"^FO{x},{y2}^A0R,80,80^CI0^FD{x},{y2}^FS");
            //labelBuffer.AppendLine($"^FO{x2},{y}^A0R,80,80^CI0^FD{x2},{y}^FS");
            //labelBuffer.AppendLine($"^FO{x2},{y2}^A0R,80,80^CI0^FD{x2},{y2}^FS");

            labelBuffer.AppendLine($"^FO680,350^A0R,80,80^CI0^FDSHORT REPORT^FS");
            labelBuffer.AppendLine($"^FO630,430^A0R,60,60^CI0^FDTABLE POS: {shortReport.Position}^FS");
            labelBuffer.AppendLine($"^FO570,80^A0R,50,50^CI0^FDORDER: ^FS");
            labelBuffer.AppendLine($"^FO570,325^A0R,50,50^CI0^FD{shortReport.Order}^FS");
            labelBuffer.AppendLine($"^FO570,640^A0R,50,50^CI0^FDINVOICE: ^FS");
            labelBuffer.AppendLine($"^FO570,900^A0R,50,50^CI0^FD{shortReport.Invoice}^FS");
            labelBuffer.AppendLine($"^FO500,80^A0R,50,50^CI0^FDSKU^FS");
            labelBuffer.AppendLine($"^FO500,640^A0R,50,50^CI0^FDORDERED^FS");
            labelBuffer.AppendLine($"^FO500,900^A0R,50,50^CI0^FDSHIPPED^FS");

        }
    }
}
