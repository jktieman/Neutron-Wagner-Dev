using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore;
using NeutronData.Models;

namespace PrintRequest
{
    public class AnticipatedOutsProcessor
    {
        private const int SpaceAboveTitle = 10;
        private const int SpaceAboveHeader = 75;
        private const int NumRowsPerPage = 25;
        private const int RowHeight = 22;
        private const int HeaderPadding = 5;
        private int _printedRows = 0;
        private int lineLength = 1050;
        private int _pageCount = 1;

        private readonly int[] _columnWidths = new int[]
        {
            50,
            100,
            325,
            100,
            100,
            100,
            100,
            100
        };
        private readonly string[] _headerNames = new string[]
        {
            "Sys",
            "Sku",
            "Description",
            "Required",
            "Ordered",
            "OnHand",
            "Loc Max",
            "Cap Minus"
        };

        private readonly Font _titleFont = new Font("Arial", 36, FontStyle.Bold);
        private readonly Font _dateFont = new Font("Arial", 12, FontStyle.Regular);
        private readonly Font _headerFont = new Font("Arial", 12, FontStyle.Bold);
        private readonly Font _bodyFont = new Font("Arial", 10, FontStyle.Regular);

        private IList<AnticipatedOut> _transferRecs = new List<AnticipatedOut>();

        private PrintDocument _printDoc;

        public OperationResult PrintAnticipatedOutsDocument(IList<AnticipatedOut> recs, DocumentPrinterPreferences printer, bool printPreview = false)
        {
            var operationResult = new OperationResult();
            _transferRecs = recs;
            try
            {
                _printDoc = new PrintDocument();
                _printDoc.PrintPage += PrintDocPrintPage;
                _printDoc.PrinterSettings.PrinterName = printer.PrinterName;
                _printDoc.DefaultPageSettings.Landscape = true;
                var margins = new Margins(50,50,50,50);
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

        void PrintDocPrintPage(object sender, PrintPageEventArgs e)
        {
            e.PageSettings.Margins.Left = 50;
            var rowPosition = SpaceAboveTitle;
            DrawTitle(e.Graphics, ref rowPosition);
            rowPosition += 2;
            DrawHeader(e.Graphics, ref rowPosition);
            rowPosition += 5;
            DrawBody(e.Graphics, rowPosition);
            if (MoreRowToPrint())
            {
                _pageCount++;
                e.HasMorePages = true;
            }
            else
            {
                _printedRows = 0;
            }
        }

        private void DrawTitle(Graphics g, ref int yValue)
        {
            var xValue = 0F;
            var format = new StringFormat() { Alignment = StringAlignment.Center };
            var measureString = "Anticipated Outs";
            var stringSize = g.MeasureString(measureString, _titleFont);
            var rect = new RectangleF(xValue, yValue, 1050, stringSize.Height);
            g.DrawString(measureString, _titleFont, Brushes.Black, rect, format);
            yValue += (int)stringSize.Height;
            measureString = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
            stringSize = g.MeasureString(measureString, _dateFont);
            rect = new RectangleF(xValue, yValue, 1050, stringSize.Height);
            g.DrawString(measureString, _dateFont, Brushes.Black, rect, format);
            yValue += (int)stringSize.Height;
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
                int xValue = 25;
               
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
                //REQUIRED
                rect = new RectangleF(xValue, yValue, _columnWidths[3], RowHeight);

                g.DrawString(rec.Required.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[3] + HeaderPadding;
                //ORDERED
                rect = new RectangleF(xValue, yValue, _columnWidths[4], RowHeight);

                g.DrawString(rec.Ordered.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[4] + HeaderPadding;
                //ONHAND
                rect = new RectangleF(xValue, yValue, _columnWidths[5], RowHeight);

                g.DrawString(rec.OnHand.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[5] + HeaderPadding;
                //LOCATIONMAX
                rect = new RectangleF(xValue, yValue, _columnWidths[6], RowHeight);

                g.DrawString(rec.LocationMax.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[6] + HeaderPadding;
                //CAPACITYMINUS
                rect = new RectangleF(xValue, yValue, _columnWidths[7], RowHeight);

                g.DrawString(rec.CapacityMinus.ToString(), _bodyFont, Brushes.Black, rect, format);

                xValue = 25;
                yValue += RowHeight;
                g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(lineLength, yValue));
                yValue += 3;
            }

            _printedRows += NumRowsPerPage;

            var pageNumberFormat = new StringFormat() { Alignment = StringAlignment.Center };
            var pageNumberString = $"Page {_pageCount}";
            var stringSize = g.MeasureString(pageNumberString, _bodyFont);

            var pageNumberRect = new RectangleF(0F, 775F, 1050F, stringSize.Height);
            g.DrawString(pageNumberString, _bodyFont, Brushes.Black, pageNumberRect, pageNumberFormat);
        }

        private bool MoreRowToPrint()
        {
            return _transferRecs.Count > _printedRows;
        }
    }
}
