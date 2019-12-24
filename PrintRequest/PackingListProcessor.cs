using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore;

namespace PrintRequest
{
    public class PackingListProcessor
    {
        private const int SpaceAboveTitle = 10;
        private const int SpaceAboveHeader = 75;
        private const int NumRowsPerPage = 25;
        private const int RowHeight = 22;
        private const int HeaderPadding = 5;
        private int _printedRows = 0;
        private int lineLength = 795;
        private string _batchPosition = "";

        private readonly int[] _columnWidths = new int[]
        {
            50,
            175,
            320,
            100,
            100
        };
        private readonly string[] _headerNames = new string[]
        {
            "Sys",
            "Sku",
            "Description",
            "Ordered",
            "Shipped"
        };

        private readonly Font _titleFont = new Font("Arial", 36, FontStyle.Bold);
        private readonly Font _dateFont = new Font("Arial", 12, FontStyle.Regular);
        private readonly Font _headerFont = new Font("Arial", 12, FontStyle.Bold);
        private readonly Font _bodyFont = new Font("Arial", 10, FontStyle.Regular);

        private IList<PackingList> _transferRecs = new List<PackingList>();
        private PackingList _packingListHeader;
        private PrintDocument _printDoc;

        public OperationResult PrintPackingListDocument(IList<PackingList> recs, DocumentPrinterPreferences printer, bool printPreview = false)
        {
            var operationResult = new OperationResult();
            _transferRecs = recs;
            if (recs.Count > 0)
            {
                _packingListHeader = recs.First();
                if (!string.IsNullOrEmpty(_packingListHeader.BatchPosition))
                {
                    _batchPosition = $" - {_packingListHeader.BatchPosition}";
                }
            }
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

            return operationResult;
        }

        private void PrintDocPrintPage(object sender, PrintPageEventArgs e)
        {
            e.PageSettings.Margins.Left = 50;
            var rowPosition = SpaceAboveTitle;
            DrawTitle(e.Graphics, ref rowPosition);
            if (_packingListHeader != null)
            {
                DrawTitleDetail(e.Graphics, ref rowPosition);
            }

            rowPosition += 2;  
            DrawHeader(e.Graphics, ref rowPosition);
            rowPosition += 5;
            if (_packingListHeader != null)
            {
                DrawBody(e.Graphics, rowPosition);
                if (MoreRowToPrint())
                {
                    e.HasMorePages = true;
                }
                else
                {
                    _printedRows = 0;
                }
            }
        }

        private void DrawTitle(Graphics g, ref int yValue)
        {
            var xValue = 0F;
            var format = new StringFormat() { Alignment = StringAlignment.Center };
            var measureString = $"Packing List{_batchPosition}";
            var stringSize = g.MeasureString(measureString, _titleFont);
            var rect = new RectangleF(xValue, yValue, 820, stringSize.Height);
            g.DrawString(measureString, _titleFont, Brushes.Black, rect, format);
            yValue += (int)stringSize.Height;
            measureString = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
            stringSize = g.MeasureString(measureString, _dateFont);
            rect = new RectangleF(xValue, yValue, 820, stringSize.Height);
            g.DrawString(measureString, _dateFont, Brushes.Black, rect, format);
            yValue += (int)stringSize.Height;
        }

        private void DrawTitleDetail(Graphics g, ref int yValue)
        {
 yValue = 140;
            var xValue = 25;
            g.DrawString("Time", _headerFont, Brushes.Black, xValue, yValue);
            g.DrawString(": " + _packingListHeader.Time, _headerFont, Brushes.Black, (float)xValue + 115, yValue);
            g.DrawString("Date", _headerFont, Brushes.Black, (float)xValue + 590, yValue);
            g.DrawString(": " + _packingListHeader.Date, _headerFont, Brushes.Black, (float)xValue + 680, yValue);
            yValue = 160;
            g.DrawString("Cost Center", _headerFont, Brushes.Black, xValue, yValue);
            g.DrawString(": " + _packingListHeader.CostCenter, _headerFont, Brushes.Black, (float)xValue + 115, yValue);
            g.DrawString("Order", _headerFont, Brushes.Black, (float)xValue + 590, yValue);
            g.DrawString(": " + _packingListHeader.Order, _headerFont, Brushes.Black, (float)xValue + 680, yValue);
            yValue = 180;
            g.DrawString("Recipient", _headerFont, Brushes.Black, xValue, yValue);
            g.DrawString(": " + _packingListHeader.Recipient, _headerFont, Brushes.Black, (float)xValue + 115, yValue);
            g.DrawString("Invoice", _headerFont, Brushes.Black, (float)xValue + 590, yValue);
            g.DrawString(": " + _packingListHeader.Invoice, _headerFont, Brushes.Black, (float)xValue + 680, yValue);
            yValue = 200;
        }

        private void DrawHeader(Graphics g, ref int yValue)
        {
            var format = new StringFormat() { Alignment = StringAlignment.Far };

            var xValue = 25;  
            g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(lineLength, yValue));

            yValue += 5;
            for (var i = 0; i < _headerNames.Length; i++)
            {
                var rect = new RectangleF(xValue, yValue, _columnWidths[i], RowHeight);
                g.DrawString(_headerNames[i], _headerFont, Brushes.Black, rect, format);

                xValue += _columnWidths[i] + HeaderPadding;
            }

            yValue += RowHeight;

            xValue = 25;
            g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(lineLength, yValue));
        }

        private void DrawBody(Graphics g, int yValue)
        {
            var format = new StringFormat() { Alignment = StringAlignment.Far };


            for (var i = 0; (i < NumRowsPerPage) && ((i + _printedRows) < _transferRecs.Count); i++)
            {
                var rec = _transferRecs[i + _printedRows];
                var xValue = 25;
                
                //Station
                var rect = new RectangleF(xValue, yValue, _columnWidths[0], RowHeight);

                g.DrawString(rec.Station.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[0] + HeaderPadding;
                //SKU
                rect = new RectangleF(xValue, yValue, _columnWidths[1], RowHeight);

                g.DrawString(rec.Item.Trim(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[1] + HeaderPadding;
                //DESCRIPTION
                rect = new RectangleF(xValue, yValue, _columnWidths[2], RowHeight);

                g.DrawString(rec.Description.Trim(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[2] + HeaderPadding;
                //ORDERED
                rect = new RectangleF(xValue, yValue, _columnWidths[3], RowHeight);

                g.DrawString(rec.Ordered.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[3] + HeaderPadding;
                //SHIPPED
                rect = new RectangleF(xValue, yValue, _columnWidths[4], RowHeight);

                g.DrawString(rec.Shipped.ToString(), _bodyFont, Brushes.Black, rect, format);

                xValue = 25;
                yValue += RowHeight;
                g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(lineLength, yValue));
                yValue += 3;
            }

            _printedRows += NumRowsPerPage;
        }

        private bool MoreRowToPrint()
        {
            return _transferRecs.Count > _printedRows;
        }
    }
}
