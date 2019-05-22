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
using NeutronData.ModelViews;

namespace PrintRequest
{
    public class ProductivityDetailProcessor
    {
        private const int SpaceAboveTitle = 10;
        private const int SpaceAboveHeader = 75;
        private const int NumRowsPerPage = 35;
        private const int RowHeight = 18;
        private const int HeaderPadding = 3;
        private int _printedRows = 0;
        private const int LineLength = 795;
        private readonly StringFormat _formatRight = new StringFormat() { Alignment = StringAlignment.Far };
        private readonly StringFormat _formatLeft = new StringFormat() { Alignment = StringAlignment.Near };
        private readonly StringFormat _formatCenter = new StringFormat() { Alignment = StringAlignment.Center };


        private readonly int[] _columnWidths = new int[]
        {
            60,
            80,
            110,
            75,
            60,
            60,
            195,
            35,
            35,
            30
        };
        private readonly string[] _headerNames = new string[]
        {
            "Date",
            "Employee",
            "Action",
            "Order",
            "Res",
            "Item",
            "Description",
            "Req",
            "Iss",
            "Sys"
        };

        private readonly int[] _columnWidthsTotals = new[]
        {
            150,
            80,
            150,
            80,
            150,
            80
        };

        private readonly Font _titleFont = new Font("Arial", 36, FontStyle.Bold);
        private readonly Font _dateFont = new Font("Arial", 12, FontStyle.Regular);
        private readonly Font _headerFont = new Font("Arial", 7, FontStyle.Bold);
        private readonly Font _bodyFont = new Font("Arial", 7, FontStyle.Regular);
        private readonly Font _totalsFont = new Font("Arial", 10, FontStyle.Bold);

        private IList<ProductivityDetail> _transferRecs = new List<ProductivityDetail>();
        private PrintDocument _printDoc;

        public OperationResult PrintProductivityDetailDocument(IList<ProductivityDetail> recs, DocumentPrinterPreferences printer, bool printPreview = false)
        {
            var operationResult = new OperationResult();
            _transferRecs = recs;
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
            rowPosition += 2;  //SpaceAboveHeader;
            DrawHeader(e.Graphics, ref rowPosition);
            rowPosition += 5;
            DrawBody(e.Graphics, ref rowPosition);

            if (MoreRowToPrint())
            {
                e.HasMorePages = true;
            }
            else
            {
                DrawTotals(e.Graphics, rowPosition);
                _printedRows = 0;
            }
        }

        private void DrawTitle(Graphics g, ref int yValue)
        {
            var xValue = 0F;
            var measureString = "Productivity Detail";
            var stringSize = g.MeasureString(measureString, _titleFont);
            var rect = new RectangleF(xValue, yValue, 820, stringSize.Height);
            g.DrawString(measureString, _titleFont, Brushes.Black, rect, _formatCenter);
            yValue += (int)stringSize.Height;
            measureString = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
            stringSize = g.MeasureString(measureString, _dateFont);
            rect = new RectangleF(xValue, yValue, 820, stringSize.Height);
            g.DrawString(measureString, _dateFont, Brushes.Black, rect, _formatCenter);
            yValue += (int)stringSize.Height;
        }

        private void DrawHeader(Graphics g, ref int yValue)
        {
            var xValue = 25;
            g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(LineLength, yValue));

            yValue += 5;
            for (var i = 0; i < _headerNames.Length; i++)
            {
                var rect = new RectangleF(xValue, yValue, _columnWidths[i], RowHeight);
                g.DrawString(_headerNames[i], _headerFont, Brushes.Black, rect, _formatCenter);

                xValue += _columnWidths[i] + HeaderPadding;
            }

            yValue += RowHeight;

            xValue = 25;
            g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(LineLength, yValue));
        }

        private void DrawBody(Graphics g, ref int yValue)
        {
            for (var i = 0; (i < NumRowsPerPage) && ((i + _printedRows) < _transferRecs.Count); i++)
            {
                var rec = _transferRecs[i + _printedRows];
                var xValue = 25;

                //COLUMN 1
                var rect = new RectangleF(xValue, yValue, _columnWidths[0], RowHeight);
                g.DrawString(rec.Date, _bodyFont, Brushes.Black, rect, _formatLeft);
                xValue += _columnWidths[0] + HeaderPadding;

                //COLUMN 2
                rect = new RectangleF(xValue, yValue, _columnWidths[1], RowHeight);
                g.DrawString(rec.Employee, _bodyFont, Brushes.Black, rect, _formatLeft);
                xValue += _columnWidths[1] + HeaderPadding;
                
                //COLUMN 3
                rect = new RectangleF(xValue, yValue, _columnWidths[2], RowHeight);
                g.DrawString(rec.Action, _bodyFont, Brushes.Black, rect, _formatLeft);
                xValue += _columnWidths[2] + HeaderPadding;
                
                //COLUMN 4
                rect = new RectangleF(xValue, yValue, _columnWidths[3], RowHeight);
                g.DrawString(rec.Order, _bodyFont, Brushes.Black, rect, _formatLeft);
                xValue += _columnWidths[3] + HeaderPadding;
                
                //COLUMN 5
                rect = new RectangleF(xValue, yValue, _columnWidths[4], RowHeight);
                g.DrawString(rec.Reservation, _bodyFont, Brushes.Black, rect, _formatLeft);
                xValue += _columnWidths[4] + HeaderPadding;
                
                //COLUMN 6
                rect = new RectangleF(xValue, yValue, _columnWidths[5], RowHeight);
                g.DrawString(rec.Item, _bodyFont, Brushes.Black, rect, _formatLeft);
                xValue += _columnWidths[5] + HeaderPadding;
                
                //COLUMN 7
                rect = new RectangleF(xValue, yValue, _columnWidths[6], RowHeight);
                g.DrawString(rec.Description, _bodyFont, Brushes.Black, rect, _formatLeft);
                xValue += _columnWidths[6] + HeaderPadding;

                //COLUMN 8
                rect = new RectangleF(xValue, yValue, _columnWidths[7], RowHeight);
                g.DrawString(rec.Requested.ToString(), _bodyFont, Brushes.Black, rect, _formatRight);
                xValue += _columnWidths[7] + HeaderPadding;

                //COLUMN 8
                rect = new RectangleF(xValue, yValue, _columnWidths[8], RowHeight);
                g.DrawString(rec.Issued.ToString(), _bodyFont, Brushes.Black, rect, _formatRight);
                xValue += _columnWidths[8] + HeaderPadding;

                //COLUMN 10
                rect = new RectangleF(xValue, yValue, _columnWidths[9], RowHeight);
                g.DrawString(rec.Station.ToString(), _bodyFont, Brushes.Black, rect, _formatRight);

                //LINE
                xValue = 25;
                yValue += RowHeight;
                g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(LineLength, yValue));
                yValue += 3;

            }
            _printedRows += NumRowsPerPage;
        }

        private void DrawTotals(Graphics g, int yValue)
        {
            var rec = _transferRecs[0];
            var xValue = 25;


            g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(LineLength, yValue));
            yValue += 3;

            //COLUMN 1
            var rect = new RectangleF(xValue, yValue, _columnWidthsTotals[0], RowHeight);
            g.DrawString("Total Lines:", _totalsFont, Brushes.Black, rect, _formatRight);
            xValue += _columnWidthsTotals[0] + HeaderPadding;

            //COLUMN 2
            rect = new RectangleF(xValue, yValue, _columnWidthsTotals[1], RowHeight);
            g.DrawString(rec.TotalLines.ToString(), _totalsFont, Brushes.Black, rect, _formatLeft);
            xValue += _columnWidthsTotals[1] + HeaderPadding;

            //COLUMN 3
            rect = new RectangleF(xValue, yValue, _columnWidthsTotals[2], RowHeight);
            g.DrawString("Total Pieces", _totalsFont, Brushes.Black, rect, _formatRight);
            xValue += _columnWidthsTotals[2] + HeaderPadding;

            //COLUMN 4
            rect = new RectangleF(xValue, yValue, _columnWidthsTotals[3], RowHeight);
            g.DrawString(rec.TotalPieces.ToString(), _totalsFont, Brushes.Black, rect, _formatLeft);
            xValue += _columnWidthsTotals[3] + HeaderPadding;

            //COLUMN 5
            rect = new RectangleF(xValue, yValue, _columnWidthsTotals[4], RowHeight);
            g.DrawString("Total Orders", _totalsFont, Brushes.Black, rect, _formatRight);
            xValue += _columnWidthsTotals[4] + HeaderPadding;

            //COLUMN 6
            rect = new RectangleF(xValue, yValue, _columnWidthsTotals[5], RowHeight);
            g.DrawString(rec.TotalOrders.ToString(), _totalsFont, Brushes.Black, rect, _formatLeft);

            xValue = 25;
            yValue += RowHeight;
            g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(LineLength, yValue));
            yValue += 3;

        }

        private bool MoreRowToPrint()
        {
            return _transferRecs.Count > _printedRows;
        }
    }
}
