using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NeutronData.PrintModels;
using Zen.Barcode;

namespace NeutronDllu
{
    public class PackingSlipProcessor
    {
        private bool _leftSide = true;
        private int _pageCount = 1;
        private const int SpaceAboveTitle = 10;
        private const int SpaceAboveHeader = 75;
        private const int NumRowsPerPage = 13;
        private int _adjustedNumRowsPerPage = 0;
        private const int HeaderPadding = 5;
        private int _printedRows = 0;
        private string _prevRec = string.Empty;
        private PickSlip _rec;
        private string[] _orderDetailInfo;

        private List<string> _shipInstructions = new List<string>();

        private readonly int _lineLength = 1050;

        private readonly Font _titleFont = new Font("Arial", 36, FontStyle.Bold);
        private readonly Font _dateFont = new Font("Arial", 12, FontStyle.Regular);
        private readonly Font _headerFont = new Font("Arial", 12, FontStyle.Bold);
        private readonly Font _bodyFont = new Font("Arial", 10, FontStyle.Regular);
        private readonly Font _18BoldFont = new Font("Arial", 18, FontStyle.Bold);
        private readonly Font _24BoldFont = new Font("Arial", 24, FontStyle.Bold);
        private readonly Font _24Font = new Font("Arial", 24, FontStyle.Regular);
        private readonly Font _28BoldFont = new Font("Arial", 28, FontStyle.Bold);


        private const int RowHeight = 22;
        private readonly int[] _columnWidths = new int[]
        {
            50,
            160,
            400,
            250
        };
        private readonly int[] _bodyColumnWidths = new int[]
        {
            50,
            200,
            200,
            300,
            100,
            100
        };
        private readonly string[] _headerNames = new string[]
        {
            "Area",
            "Sku",
            "Description",
            "Quantity"
        };



        //------------------------------------------------------------------------------------------

        private IList<PickSlip> _transferRecs = new List<PickSlip>();
        // private PackingList _packingListHeader;
        private PrintDocument _printDoc;

        public void PrintPackingSlipDocument(IList<PickSlip> recs, DocumentPrinterPreferences printer, bool printPreview = false)
        {

            _transferRecs = recs;

            _rec = _transferRecs[0];
            //var order = _transferRecs[0].Order;
            //var ord = _repoOrders.FindBy(r => r.Ord1 == order).FirstOrDefault();
            //if (ord == null)
            //{
            //    MessageBox.Show($"Order {order} not found.");
            //    return;
            //}

            //_transferRecs = GetPickList(ord.Id);

            //var o = _repoOrders.FindBy(r => r.Ord1 == _rec.Order).FirstOrDefault();
            //// split into list of strings
            //if (o != null)
            //{
            //    _orderDetailInfo = o.OrderInfo.Split('|');
            //    var shippingInstructions = _orderDetailInfo[9];
            //}
            if (string.IsNullOrEmpty(_rec.OrderInfo))
            {
                _orderDetailInfo = new string[10];
            }
            else
            {
                _orderDetailInfo = _rec.OrderInfo.Split('|');
                if (_orderDetailInfo.Length != 10)
                {
                    _orderDetailInfo = new string[10];
                }
            }


            try
            {
                _printDoc = new PrintDocument();
                _printDoc.PrintPage += PrintDocPrintPage;
                _printDoc.PrinterSettings.PrinterName = printer.PrinterName;
                _printDoc.DefaultPageSettings.Landscape = true;
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

            DrawBarcode(e.Graphics, ref yValue);
            DrawPriRouteBox(e.Graphics, ref yValue);
            DrawShipTo(e.Graphics, ref yValue);
            DrawTaskNoSection(e.Graphics, ref yValue);
            if (_pageCount == 1)
            {
                DrawShippingInstructions(e.Graphics, ref yValue);
            }

            DrawHeader(e.Graphics, ref yValue);
            DrawBody(e.Graphics, ref yValue);



            //e.PageSettings.Margins.Left = 50;
            //var yValue = SpaceAboveTitle;
            //DrawTitle(e.Graphics, ref yValue);
            //if (_pickListHeader != null)
            //{
            //    DrawTitleDetail(e.Graphics, ref yValue);
            //}

            //yValue += 2;
            //DrawHeader(e.Graphics, ref yValue);
            //yValue += 5;
            //if (_pickListHeader != null)
            //{
            //    DrawBody(e.Graphics, yValue);
            if (MoreRowToPrint())
            {
                _pageCount++;
                e.HasMorePages = true;
            }
            else
            {
                _printedRows = 0;
            }
            //}
        }

        private void DrawBarcode(Graphics g, ref int yValue)
        {
            var xValue = 50;
            var barcodeImage = GetBarcodeImage(_rec.Order, 75);
            g.DrawImage(barcodeImage, xValue, yValue);
            g.DrawString(_rec.Order, _24Font, new SolidBrush(Color.Black), 70, 135);

        }
        private void DrawPriRouteBox(Graphics g, ref int yValue)
        {
            var input = _transferRecs.Select(r => r.Area).Distinct();

            // loop through a string and insert a comma between the letters
            var route = string.Join("-", input);


            //var format = new StringFormat() { Alignment = StringAlignment.Center  };
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            var xValue = 400;
            var p = new Pen(Brushes.Black, 5);
            g.DrawRectangle(p, new Rectangle(new Point(xValue, yValue), new Size(300, 75)));
            g.DrawRectangle(p, new Rectangle(new Point(xValue, yValue), new Size(100, 75)));

            var measureString = _rec.Priority;
            var stringSize = g.MeasureString(measureString, _dateFont);
            var rect = new RectangleF(xValue, yValue, 100, 75);
            g.DrawString(measureString, _28BoldFont, Brushes.Black, rect, format);

            measureString = route;
            stringSize = g.MeasureString(measureString, _dateFont);
            rect = new RectangleF(xValue + 100, yValue, 200, 75);
            g.DrawString(measureString, _28BoldFont, Brushes.Black, rect, format);


            yValue += 85;
            //p = new Pen(Brushes.Black, 1);
            //g.DrawRectangle(p, new Rectangle(new Point(xValue, yValue), new Size(100, 40)));            
            //g.DrawRectangle(p, new Rectangle(new Point(xValue + 100, yValue), new Size(200, 40)));

            measureString = $"PRI";
            stringSize = g.MeasureString(measureString, _dateFont);
            rect = new RectangleF(xValue, yValue, 100, 40);
            g.DrawString(measureString, _24Font, Brushes.Black, rect, format);

            measureString = $"ROUTE";
            stringSize = g.MeasureString(measureString, _dateFont);
            rect = new RectangleF(xValue + 100, yValue, 200, 40);
            g.DrawString(measureString, _24Font, Brushes.Black, rect, format);

            // g.DrawString("PRI", new Font("arial", 18), new SolidBrush(Color.Black), 420, 140);
            //g.DrawString("ROUTE", _18BoldFont, new SolidBrush(Color.Black), 550, 140);
            // g.DrawString("7", _24BoldFont, new SolidBrush(Color.Black), 430, 70);
            // g.DrawString("ROUTE", _24BoldFont, new SolidBrush(Color.Black), 520, 70);
            yValue = 50;
        }
        private void DrawShipTo(Graphics g, ref int yValue)
        {
            var xValue = 50;
            g.DrawString("Ship-To:", new Font("arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), 740, yValue);
            yValue += RowHeight;
            g.DrawString(_orderDetailInfo[3], new Font("arial", 10), new SolidBrush(Color.Black), 760, yValue);
            yValue += RowHeight;
            g.DrawString(_orderDetailInfo[4], new Font("arial", 10), new SolidBrush(Color.Black), 760, yValue);
            yValue += RowHeight;
            g.DrawString($"{_orderDetailInfo[5]}, {_orderDetailInfo[6]}  {_orderDetailInfo[7]}", new Font("arial", 10), new SolidBrush(Color.Black), 760, yValue);
            yValue += RowHeight;
            g.DrawString(_orderDetailInfo[8], new Font("arial", 10), new SolidBrush(Color.Black), 760, yValue);
            yValue += RowHeight;
            g.DrawLine(Pens.Black, new Point(xValue, 175), new Point(1050, 175));
            yValue += RowHeight;
        }
        private void DrawTaskNoSection(Graphics g, ref int yValue)
        {
            var xValue = 50;
            g.DrawLine(Pens.Black, new Point(50, 175), new Point(1050, 175));
            g.DrawString("TO: ", new Font("arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), 50, 185);
            g.DrawString(_rec.Invoice, new Font("arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), 150, 185);
            g.DrawString("Page:", new Font("arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), 950, 185);
            g.DrawString(_pageCount.ToString(), new Font("arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), 1010, 185);

            var format = new StringFormat() { Alignment = StringAlignment.Center };
            var measureString = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
            var stringSize = g.MeasureString(measureString, _dateFont);
            var rect = new RectangleF(xValue, yValue, 1050, stringSize.Height);
            g.DrawString(measureString, _dateFont, Brushes.Black, rect, format);
            yValue += (int)stringSize.Height;
            g.DrawLine(Pens.Black, new Point(50, 210), new Point(1050, 210));
            yValue += 10;
        }
        private void DrawShippingInstructions(Graphics g, ref int yValue)
        {
            var xValue = 50;
            var instructions = GetShippingInstructions(_orderDetailInfo[9]);
            if (instructions.Any())
            {
                g.DrawString("Shipping Instructions:", new Font("arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), xValue, yValue);

                foreach (var instruction in instructions)
                {
                    yValue += RowHeight;
                    g.DrawString(instruction, new Font("arial", 8), new SolidBrush(Color.Black), xValue, yValue);
                }
            }
            else
            {
                g.DrawString("Shipping Instructions: No Instructions", new Font("arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), xValue, yValue);
            }
            yValue += RowHeight;
            g.DrawLine(Pens.Black, new Point(50, yValue), new Point(_lineLength, yValue));

            yValue += 5;
        }
        private void DrawHeader(Graphics g, ref int yValue)
        {
            var format = new StringFormat() { Alignment = StringAlignment.Far };

            var xValue = 50;
            g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(_lineLength, yValue));

            yValue += 5;
            for (var i = 0; i < _headerNames.Length; i++)
            {
                var rect = new RectangleF(xValue, yValue, _columnWidths[i], RowHeight);
                g.DrawString(_headerNames[i], _headerFont, Brushes.Black, rect, format);

                xValue += _columnWidths[i] + HeaderPadding;
            }

            yValue += RowHeight;

            xValue = 50;
            g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(_lineLength, yValue));
            yValue += 5;
        }
        private void DrawBody(Graphics g, ref int yValue)
        {
            var xValue = 50;
            var format = new StringFormat() { Alignment = StringAlignment.Far };
            var formatLeft = new StringFormat() { Alignment = StringAlignment.Near };
            var formatCenter = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            // adjust the number of rows to print based on the number of lines in the shipping instructions
            _adjustedNumRowsPerPage = NumRowsPerPage;
            if (_pageCount == 1)
            {
                _adjustedNumRowsPerPage = NumRowsPerPage - _shipInstructions.Count;
            }



            var p = new Pen(Brushes.Black, 1);

            for (var i = 0; (i < _adjustedNumRowsPerPage) && ((i + _printedRows) < _transferRecs.Count); i++)
            {
                var RowHeight = 35;
                var rec = _transferRecs[i + _printedRows];

                //Area    
                var rect = new RectangleF(xValue, yValue, _bodyColumnWidths[0], RowHeight);

                //var rectangle = new Rectangle(xValue, yValue, _bodyColumnWidths[0], RowHeight);
                //g.DrawRectangle(p, rectangle);

                g.DrawString(rec.Area, _bodyFont, Brushes.Black, rect, formatCenter);
                xValue += _bodyColumnWidths[0] + HeaderPadding;


                //SKU
                rect = new RectangleF(xValue, yValue, _bodyColumnWidths[1], RowHeight);

                //rectangle = new Rectangle(xValue, yValue, _bodyColumnWidths[1], RowHeight);
                //g.DrawRectangle(p, rectangle);


                if (_leftSide)
                {
                    //g.FillRectangle(Brushes.Aqua, rectangle);
                    g.DrawString(rec.Item.Trim(), _bodyFont, Brushes.Black, rect, formatCenter);
                    xValue += _bodyColumnWidths[1] + HeaderPadding;
                }
                else
                {

                    var barcodeImage = GetBarcodeImage128(rec.Item.Trim(), RowHeight - 5);
                    g.DrawImage(barcodeImage, xValue, yValue);
                    xValue += _bodyColumnWidths[1] + HeaderPadding;
                }

                //SKU -2
                rect = new RectangleF(xValue, yValue, _bodyColumnWidths[2], RowHeight);

                //rectangle = new Rectangle(xValue, yValue, _bodyColumnWidths[2], RowHeight);
                //g.DrawRectangle(p, rectangle);

                if (_leftSide)
                {
                    var barcodeImage = GetBarcodeImage128(rec.Item.Trim(), RowHeight - 5);
                    g.DrawImage(barcodeImage, xValue, yValue);
                    xValue += _bodyColumnWidths[2] + HeaderPadding;
                }
                else
                {
                    // g.FillRectangle(Brushes.Chartreuse, rectangle);
                    g.DrawString(rec.Item.Trim(), _bodyFont, Brushes.Black, rect, formatCenter);
                    xValue += _bodyColumnWidths[2] + HeaderPadding;
                }


                ////DESCRIPTION
                rect = new RectangleF(xValue, yValue, _bodyColumnWidths[3], RowHeight);

                //rectangle = new Rectangle(xValue, yValue, _bodyColumnWidths[3], RowHeight);
                //g.DrawRectangle(p, rectangle);

                g.DrawString(rec.Description.Trim(), _bodyFont, Brushes.Black, rect, formatLeft);
                xValue += _bodyColumnWidths[3] + HeaderPadding;

                //ORDERED
                rect = new RectangleF(xValue, yValue, _bodyColumnWidths[4], RowHeight);

                //rectangle = new Rectangle(xValue, yValue, _bodyColumnWidths[4], RowHeight);
                //g.DrawRectangle(p, rectangle);

                if (_leftSide)
                {

                    g.DrawString(rec.Quantity, _bodyFont, Brushes.Black, rect, formatCenter);
                    xValue += _bodyColumnWidths[4] + HeaderPadding;
                }
                else
                {
                    var barcodeImage = GetBarcodeImage128(rec.Quantity, RowHeight - 5);
                    g.DrawImage(barcodeImage, xValue, yValue);
                    xValue += _bodyColumnWidths[4] + HeaderPadding;
                }

                //ORDERED - 2
                rect = new RectangleF(xValue, yValue, _bodyColumnWidths[5], RowHeight);

                //rectangle = new Rectangle(xValue, yValue, _bodyColumnWidths[5], RowHeight);
                //g.DrawRectangle(p, rectangle);

                if (_leftSide)
                {
                    var barcodeImage = GetBarcodeImage128(rec.Quantity, RowHeight - 5);
                    g.DrawImage(barcodeImage, xValue, yValue);
                    xValue += _bodyColumnWidths[5] + HeaderPadding;
                }
                else
                {
                    g.DrawString(rec.Quantity, _bodyFont, Brushes.Black, rect, formatCenter);
                    xValue += _bodyColumnWidths[5] + HeaderPadding;

                }



                LeftRightToggle();
                xValue = 50;
                yValue += RowHeight;
                g.DrawLine(Pens.Black, new Point(xValue, yValue), new Point(_lineLength, yValue));
                yValue += 3;
            }

            _printedRows += _adjustedNumRowsPerPage;

            //var pageNumberFormat = new StringFormat { Alignment = StringAlignment.Center };
            //var pageNumberString = $"Page {_pageCount}";
            //var stringSize = g.MeasureString(pageNumberString, _bodyFont);

            //var pageNumberRect = new RectangleF(0F, 775F, 1050F, stringSize.Height);
            //g.DrawString(pageNumberString, _bodyFont, Brushes.Black, pageNumberRect, pageNumberFormat);
        }
        private bool MoreRowToPrint()
        {
            return _transferRecs.Count > _printedRows;
        }

        private Image GetBarcodeImage(string text, int height)
        {
            var barcode = BarcodeDrawFactory.Code39WithoutChecksum;
            var barcodeImage = barcode.Draw(text, height, scale:1);
            return barcodeImage;
        }



        //private Image GetBarcodeImage(string text, int height)
        //{
        //    BarcodeDraw bdraw = BarcodeDrawFactory.GetSymbology(BarcodeSymbology.Code39C);
        //    Image barcodeImage = bdraw.Draw(text, height);
        //    return barcodeImage;
        //}
        private Image GetBarcodeImage128(string text, int height)
        {
            var bdraw = BarcodeDrawFactory.GetSymbology(BarcodeSymbology.Code128);
            var barcodeImage = bdraw.Draw(text, height);
            return barcodeImage;
        }
        private List<string> GetShippingInstructions(string input)
        {
            if (input == null) return new List<string>();

            _shipInstructions = new List<string>();
            var maxLength = 160;


            var words = input.Split(' ');
            var output = new StringBuilder();
            var lineLen = 0;

            foreach (var word in words)
            {
                if (lineLen + word.Length + 1 > maxLength)
                {
                    output.Append(Environment.NewLine);
                    lineLen = 0;
                }
                output.Append(word + " ");
                lineLen += word.Length + 1;
            }
            var wrappedText = output.ToString();

            var lines = wrappedText.Split(new string[] { "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                _shipInstructions.Add(line);
            }
            return _shipInstructions;
        }
        private void LeftRightToggle()
        {
            _leftSide = !_leftSide;
        }
    }
}
