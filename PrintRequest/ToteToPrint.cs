using NeutronData.Models;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PrintRequest
{
    public static class ToteToPrint
    {
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
    }
}
