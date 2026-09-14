using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.Models;
using NeutronData.PrintModels;
using Zen.Barcode;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace NeutronDllu
{
    public class ReplenDocProcessor
    {
        private ReplenOrderDetail _orderDetail;
        private int _lineLength = 750;
        private int _pageWidth = 850;
        private int _pageHeight = 1100;

        private readonly Font _18BFont = new Font("Arial", 20, FontStyle.Bold);
        private readonly Font _30BFont = new Font("Arial", 30, FontStyle.Bold);
        private readonly Font _dateFont = new Font("Arial", 12, FontStyle.Regular);
        private readonly Font _16Font = new Font("Arial", 16, FontStyle.Regular);
        private readonly Font _areaFont = new Font("Arial", 96, FontStyle.Bold);
        private PrintDocument _printDoc;

        public void PrintReplenDoc(ReplenOrderDetail orderDetail, DocumentPrinterPreferences printer, bool printPreview = false)
        {
            if (orderDetail == null) return;
            if (printer == null) return;

            _orderDetail = orderDetail;

            try
            {
                _printDoc = new PrintDocument();
                _printDoc.PrintPage += PrintDocPrintPage;
                _printDoc.PrinterSettings.PrinterName = printer.PrinterName;
                _printDoc.DefaultPageSettings.Landscape = false;
                var margins = new Margins(50, 50, 50, 50);
                _printDoc.DefaultPageSettings.Margins = margins;

                if (printPreview)
                {
                    var preview = new PrintPreviewDialog { Document = _printDoc };
                    preview.ShowDialog();
                }
                else
                {
                    _printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"An error occured while printing. " + ex.Message);
            }
        }
        void PrintDocPrintPage(object sender, PrintPageEventArgs e)
        {
            var yValue = 50;
            _pageWidth = e.PageBounds.Width;
            _pageHeight = e.PageBounds.Height;
            DrawBody(e.Graphics, ref yValue);
        }

        private void DrawBarcode(Graphics g, ref int xValue, ref int yValue, string text)
        {
            var barcodeImage = GetBarcodeImage128(text, 75);
            g.DrawImage(barcodeImage, xValue, yValue);
        }
        private void DrawBody(Graphics g, ref int yValue)
        {
            var xValue = 50;
            var formatRight = new StringFormat() { Alignment = StringAlignment.Far };
            var formatLeft = new StringFormat() { Alignment = StringAlignment.Near };
            var formatCenter = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            var p = new Pen(Brushes.Black, 1);

            var measureString = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
            var stringSize = g.MeasureString(measureString, _dateFont);
            var rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _16Font, Brushes.Black, rect, formatCenter);

            yValue += 20;

            //Area Numbers in  Upper Corners    

            measureString = $"{_orderDetail.AreaId}";
            stringSize = g.MeasureString(measureString, _areaFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _areaFont, Brushes.Black, rect, formatLeft);

            measureString = $"{_orderDetail.AreaId}";
            stringSize = g.MeasureString(measureString, _areaFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _areaFont, Brushes.Black, rect, formatRight);

            //g.DrawString(rec.Area, _bodyFont, Brushes.Black, rect, formatCenter);
            //    xValue += _bodyColumnWidths[0] + HeaderPadding;


            //    SKU
            //    
            yValue += 120;
            
            measureString = $"SKU";
            stringSize = g.MeasureString(measureString, _18BFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _18BFont, Brushes.Black, rect, formatCenter);
            
            yValue += 30;
            
            var barcodeImage = GetBarcodeImage(_orderDetail.PartNum.Trim(), 100);
            var x = (_pageWidth - barcodeImage.Width) / 2;
            var y = yValue;
            var point = new Point(x, y);
            g.DrawImage(barcodeImage, point);

            yValue += 115;
            measureString = _orderDetail.PartNum;
            stringSize = g.MeasureString(measureString, _30BFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _30BFont, Brushes.Black, rect, formatCenter);

            // QUANTITY
            
            yValue += 100;
            measureString = $"QUANTITY";
            stringSize = g.MeasureString(measureString, _18BFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _18BFont, Brushes.Black, rect, formatCenter);

            yValue += 30;

            barcodeImage = GetBarcodeImage($"{_orderDetail.Quantity}", 100);
            x = (_pageWidth - barcodeImage.Width) / 2;
            y = yValue;
            point = new Point(x, y);
            g.DrawImage(barcodeImage, point);

            yValue += 115;
            measureString = $"{_orderDetail.Quantity}";
            stringSize = g.MeasureString(measureString, _30BFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _30BFont, Brushes.Black, rect, formatCenter);

            //  TRANSACTION ID

            yValue += 80;
            measureString = $"TRANSACTION ID";
            stringSize = g.MeasureString(measureString, _18BFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _18BFont, Brushes.Black, rect, formatCenter);

            yValue += 30;

            barcodeImage = GetBarcodeImage($"{_orderDetail.OrderDetailInfo}", 100);
            x = (_pageWidth - barcodeImage.Width) / 2;
            y = yValue;
            point = new Point(x, y);
            g.DrawImage(barcodeImage, point);

            yValue += 115;

            measureString = $"{_orderDetail.OrderDetailInfo}";
            stringSize = g.MeasureString(measureString, _30BFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _30BFont, Brushes.Black, rect, formatCenter);

            // DESCRIPTION

            yValue += 80;
            measureString = $"{_orderDetail.PartDesc.Trim()}";
            stringSize = g.MeasureString(measureString, _30BFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _30BFont, Brushes.Black, rect, formatCenter);


            yValue = 930;

            //Area Numbers in  Lower Corners    

            measureString = $"{_orderDetail.AreaId}";
            stringSize = g.MeasureString(measureString, _areaFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _areaFont, Brushes.Black, rect, formatLeft);

            measureString = $"{_orderDetail.AreaId}";
            stringSize = g.MeasureString(measureString, _areaFont);
            rect = new RectangleF(xValue, yValue, _lineLength, stringSize.Height);
            g.DrawString(measureString, _areaFont, Brushes.Black, rect, formatRight);

        }

        private Image GetBarcodeImage(string text, int height)
        {
            Code39BarcodeDraw barcode = BarcodeDrawFactory.Code39WithoutChecksum;
            Image barcodeImage = barcode.Draw(text, height,2);
            return barcodeImage;
            
            //BarcodeDraw bdraw = BarcodeDrawFactory.GetSymbology(BarcodeSymbology.Code39C);

            //Image barcodeImage = bdraw.Draw(text, height,2);
            //return barcodeImage;
        }
        private Image GetBarcodeImage128(string text, int height)
        {
            BarcodeDraw bdraw = BarcodeDrawFactory.GetSymbology(BarcodeSymbology.Code128);
            Image barcodeImage = bdraw.Draw(text, height);
            return barcodeImage;
        }
    }
}
