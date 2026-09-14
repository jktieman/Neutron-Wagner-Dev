
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using NeutronData.Models;
using NeutronData.PrintModels;


namespace NeutronDllu
{
    public class AvailableLocationsProcessor
    {
        private const int SpaceAboveTitle = 10;
        private const int SpaceAboveHeader = 75;
        private const int NumRowsPerPage = 35;
        private const int RowHeight = 22;
        private const int HeaderPadding = 5;
        private int _printedRows = 0;
        private int lineLength = 795;

        private readonly int[] _columnWidths = new int[]
        {
            50,
            90,
            90,
            90,
            90,
            90,
            90,
            90
        };
        private readonly string[] _headerNames = new string[]
        {
            "Area",
            "Carousel",
            "Bin",
            "Level",
            "Partition",
            "Size",
            "Velocity",
            "Height"
        };

        private readonly Font _titleFont = new Font("Arial", 36, FontStyle.Bold);
        private readonly Font _dateFont = new Font("Arial", 12, FontStyle.Regular);
        private readonly Font _headerFont = new Font("Arial", 12, FontStyle.Bold);
        private readonly Font _bodyFont = new Font("Arial", 10, FontStyle.Regular);

        private IList<Location> _transferRecs = new List<Location>();
        private PrintDocument _printDoc;

        public void PrintAvailableLocationsDocument(IList<Location> recs, DocumentPrinterPreferences printer, bool printPreview = false)
        {
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
        }

        private void PrintDocPrintPage(object sender, PrintPageEventArgs e)
        {
            e.PageSettings.Margins.Left = 50;
            var rowPosition = SpaceAboveTitle;
            DrawTitle(e.Graphics, ref rowPosition);
            rowPosition += 2;  //SpaceAboveHeader;
            DrawHeader(e.Graphics, ref rowPosition);
            rowPosition += 5;
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

        private void DrawTitle(Graphics g, ref int yValue)
        {
            var xValue = 0F;
            var format = new StringFormat() { Alignment = StringAlignment.Center };
            var measureString = "Available Locations";
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

        private void DrawHeader(Graphics g, ref int yValue)
        {
            var format = new StringFormat() { Alignment = StringAlignment.Far };

            var xValue = 25;  //50;
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

                //Area
                var rect = new RectangleF(xValue, yValue, _columnWidths[0], RowHeight);

                g.DrawString(rec.Area.AreaNumber.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[0] + HeaderPadding;
                //LOC1
                rect = new RectangleF(xValue, yValue, _columnWidths[1], RowHeight);

                g.DrawString(rec.Loc1.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[1] + HeaderPadding;
                //LOC2
                rect = new RectangleF(xValue, yValue, _columnWidths[2], RowHeight);

                g.DrawString(rec.Loc2.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[2] + HeaderPadding;
                //LOC3
                rect = new RectangleF(xValue, yValue, _columnWidths[3], RowHeight);

                g.DrawString(rec.Loc3.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[3] + HeaderPadding;
                //LOC4
                rect = new RectangleF(xValue, yValue, _columnWidths[4], RowHeight);

                g.DrawString(rec.Loc4.ToString(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[4] + HeaderPadding;
                //SIZECODE
                rect = new RectangleF(xValue, yValue, _columnWidths[5], RowHeight);

                g.DrawString(rec.SizeCode.Name.Trim(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[5] + HeaderPadding;
                //VELOCITYCODE
                rect = new RectangleF(xValue, yValue, _columnWidths[6], RowHeight);

                g.DrawString(rec.VelocityCode.Name.Trim(), _bodyFont, Brushes.Black, rect, format);
                xValue += _columnWidths[6] + HeaderPadding;
                //HEIGHTCODE
                rect = new RectangleF(xValue, yValue, _columnWidths[7], RowHeight);

                g.DrawString(rec.HeightCode.Name.Trim(), _bodyFont, Brushes.Black, rect, format);

                //LINE
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
