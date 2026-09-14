using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NeutronTrayLayout
{
    public partial class NeutronTrayUserControl : UserControl
    {
        public enum TableLayoutPanelType
        {
            Tree,
            Width,
            Depth,
            Grid
        }

        private TableLayoutPanel _tableLayoutPanel;
       // private readonly int _levels;
       // private readonly int _level;
        private readonly int _columnCount;
        private readonly int _rowCount;
        private readonly Size _size;
        private readonly int _column;
        private readonly int _row;
        private readonly int _columnSpan;
        private readonly int _rowSpan;
        private readonly float _fontSize;
        private readonly float _columnWidth;
        private readonly float _columnW;
        private readonly float _rowHeight;
        private readonly float _rowH;
       // private readonly Font _columnFont;
       // private readonly Font _rowFont;

        private Dictionary<string, Point> _points;
        /// <summary>
        /// This UserControl creates a dynamic table based on the parameters
        /// passed in
        /// </summary>
        /// <param name="tableLayoutPanelType">An enum that indicates the Type of <see cref="TableLayoutPanel"/> </param>
        /// <param name="levels">The number of Levels on a Tree Table</param>
        /// <param name="level">The Level to light up on the Tree Table</param>
        /// <param name="columnCount">The number of columns in the Grid</param>
        /// <param name="rowCount">The number of rows in the Grid</param>
        /// <param name="size">The Size of the Table to create</param>
        /// <param name="column">The targeted column on the Grid.  Also used on the Width Table</param>
        /// <param name="row">The targeted row on the Grid.  Also used on the Height Table</param>
        /// <param name="columnSpan">The number of Columns that this item uses</param>
        /// <param name="rowSpan">The number of Rows that this item uses</param>
        /// <param name="fontSize">The Font Size of all the Tables</param>
        public NeutronTrayUserControl(TableLayoutPanelType tableLayoutPanelType, int levels, int level, int columnCount, int rowCount, Size size, int column, int row, int columnSpan, int rowSpan, float fontSize)
        {
            InitializeComponent();

            _columnCount = columnCount;
            // In a Tree Table the rowCount is the same as the levels
            _rowCount = rowCount;
            CreatePoints(columnCount, rowCount);

            _size = size;

            // primary location to light
            _column = column;
            // In a Tree Table the row is the same as the level
            _row = row;

            _rowSpan = rowSpan;
            _columnSpan = columnSpan;
            
            _fontSize = fontSize;
            _columnWidth = (float)(100f / _columnCount);

            _columnW = (float)size.Width / columnCount;

            _rowHeight = (float)(100f / _rowCount);

            _rowH = (float)size.Height / rowCount;



            this.Size = new Size(_size.Width, _size.Height);


            if (_tableLayoutPanel != null)
            {
                this.Controls.Remove(_tableLayoutPanel);
            }
            _tableLayoutPanel = new TableLayoutPanel();

            _tableLayoutPanel.ColumnStyles.Clear();
            _tableLayoutPanel.RowStyles.Clear();
            _tableLayoutPanel.Controls.Clear();
            _tableLayoutPanel.AutoSize = false;
            _tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            _tableLayoutPanel.Dock = DockStyle.Fill;
            _tableLayoutPanel.RowCount = _rowCount;
            _tableLayoutPanel.ColumnCount = _columnCount;
            _tableLayoutPanel.SuspendLayout();

            var firstTime = true;
            var font = new Font(new FontFamily("Tahoma"), _fontSize);

            // Try to reverse the entry to the grid
            for (var i = _rowCount - 1; i >= 0; i--)
            {
                _tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, _rowHeight));
                for (var j = _columnCount - 1; j >= 0; j--)
                {
                    _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, _columnWidth));

                    if (firstTime)
                    {
                        font = GetFont(_columnW, _rowH, i.ToString(), _fontSize);
                        firstTime = false;
                    }
                    _tableLayoutPanel.Controls
                        .Add(NewButton(tableLayoutPanelType, font, $"{j + 1}", $"{_rowCount - i}"), j, i);
                }
            }
            _tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            _tableLayoutPanel.BorderStyle = BorderStyle.None;
            _tableLayoutPanel.BackColor = Color.White;
            if (tableLayoutPanelType == TableLayoutPanelType.Grid)
            {
                SetSpan(columnSpan, rowSpan);
            }
            else if (tableLayoutPanelType == TableLayoutPanelType.Depth)
            {
                _tableLayoutPanel.BackColor = Color.DarkSlateGray;
                TurnOn(1, row);
            }
            else if (tableLayoutPanelType == TableLayoutPanelType.Width)
            {
                _tableLayoutPanel.BackColor = Color.DarkSlateGray;
                TurnOn(column, 1);
            }
            else if (tableLayoutPanelType == TableLayoutPanelType.Tree)
            {
                _tableLayoutPanel.BackColor = Color.DarkSlateGray;
                TurnOn(1, row);
            }

            _tableLayoutPanel.ResumeLayout();
            this.Controls.Add(_tableLayoutPanel);
        }

        private Font GetFont(float columnW, float rowH, string text, float fontSize)
        {
            text = "88";
            fontSize = 1;
            SizeF size;
            columnW -= 5;
            rowH -= 5;
            bool fit = false;
            float curSize = 1f;
            float sizeStep = 1f;
            if (fontSize == 1 && columnW > 5)
            {
                while (fit != true)
                {
                    curSize += sizeStep;
                    Font font = new Font(new FontFamily("Tahoma"), curSize);

                    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        size = g.MeasureString(text, font);
                    }
                    Console.WriteLine($"FontSize={curSize}  -  Graphics- Width:{size.Width}  Height:{size.Height}");
                    
                    Size textSize = TextRenderer.MeasureText(text, font);
                    Console.WriteLine($"FontSize={curSize}  -  TextRenderer- Width:{textSize.Width}  Height:{textSize.Height}");


                    if (size.Height >= rowH || size.Width >= columnW || rowH == 0 || columnW == 0)
                    {
                        fit = true;

                        curSize -= 3;

                        //if (textSize.Width >= columnW)
                        //{
                        //    curSize -= sizeStep * 3;
                        //}

                        //if (textSize.Height >= rowH)
                        //{
                        //    curSize -= sizeStep * 3;
                        //}
                    }
                }
            }
            else
            {
                curSize = fontSize;
            }
            Console.WriteLine($"Font Size: {curSize}");
            return new Font(new FontFamily("Tahoma"), curSize);
        }

        private Font GetFontByRowHeight(float rowH)
        {
            var fontSize = 4;
            if (rowH > 20f && rowH < 40f)
            {
                fontSize = 10;
            }

            if (rowH >= 40f)
            {
                fontSize = 20;
            }

            return new Font(new FontFamily("Tahoma"), fontSize);
        }
        private Font GetFontByColumnWidth(float columnW)
        {
            var fontSize = 6;
            //var f = GetFont(columnW, 100f, "88", 1);
            //var fontSize = f.Size;
            
            if (columnW > 25f && columnW < 40f)
            {
                fontSize = 10;
            }

            if (columnW >= 40f)
            {
                fontSize = 20;
            }

            return new Font(new FontFamily("Tahoma"), fontSize);
        }


        private Button NewButton(TableLayoutPanelType tableLayoutPanelType, Font font, string column, string row)
        {
            var text = string.Empty;

            switch (tableLayoutPanelType)
            {
                case TableLayoutPanelType.Depth:
                    text = row;
                    break;
                case TableLayoutPanelType.Width:
                    text = column;
                    break;
                case TableLayoutPanelType.Tree:
                    text = row;
                    break;
                case TableLayoutPanelType.Grid:
                    {
                        text = string.Empty;
                        break;
                    }
            }

            var button = new Button
            {
                Name = $"Button{column}-{row}"
                ,
                Text = $"{text}"
                ,
                Margin = new Padding(0)
                ,
                Dock = DockStyle.Fill
                ,
                AutoSize = true
                ,
                FlatStyle = FlatStyle.Flat
                ,
                ForeColor = Color.White
                ,
                BackColor = Color.DarkSlateGray
                ,
                TextAlign = ContentAlignment.MiddleCenter
                ,
                Font = font
                ,
                Padding = new Padding(0)
                //, Font = new System.Drawing.Font("Microsoft Sans Serif", 28F, System.Drawing.FontStyle.Regular,
                //System.Drawing.GraphicsUnit.Point, ((byte)(0))),
            };
            if (tableLayoutPanelType == TableLayoutPanelType.Grid)
            {
                BackColor = Color.White;
                ForeColor = Color.Black;
            }

            button.Click += ButtonClick;
            // button.SizeChanged += Button_SizeChanged;
            // button.SizeChanged += AdjustText;
            return button;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            MessageBox.Show($"Button is: {button.Name}");

        }

        private void Button_SizeChanged(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button == null) return;
            if (button.Height < 10) return;
            if (button.Text == "") return;
            SizeF stringSize;

            // create a graphics object for this form
            using (Graphics gfx = this.CreateGraphics())
            {
                // Get the size given the string and the font
                stringSize = gfx.MeasureString(button.Text, button.Font);
                ////test how many rows
                //int rows = (int)((double)button.Height / (stringSize.Height));
                //if (rows == 0)
                //    return;
                //double areaAvailable = rows * stringSize.Height * button.Width;
                double areaAvailable = stringSize.Height * button.Width;
                double areaRequired = stringSize.Width * stringSize.Height * 1.1;

                if (areaAvailable / areaRequired > 1.3)
                {
                    while (areaAvailable / areaRequired > 1.3)
                    {
                        button.Font = new Font(button.Font.FontFamily, button.Font.Size * 1.1F);
                        stringSize = gfx.MeasureString(button.Text, button.Font);
                        areaRequired = stringSize.Width * stringSize.Height * 1.1;
                    }
                }
                else
                {
                    while (areaRequired * 1.3 > areaAvailable)
                    {
                        button.Font = new Font(button.Font.FontFamily, button.Font.Size / 1.1F);
                        stringSize = gfx.MeasureString(button.Text, button.Font);
                        areaRequired = stringSize.Width * stringSize.Height * 1.1;
                    }
                }
            }
        }

        private void AdjustText(object sender, EventArgs e)
        {
            var button = (Button)sender;
            bool fit = false;
            float curSize = 0;
            float sizeStep = 1;
            while (fit != true)
            {
                curSize += sizeStep;
                Font font = new Font(button.Font.Name, curSize);
                Size textSize = TextRenderer.MeasureText(button.Text, font);
                if (textSize.Height >= button.Height || textSize.Width >= button.Width || button.Height == 0 || button.Width == 0)
                {
                    fit = true;
                    if (textSize.Width > button.Width)
                    {
                        curSize -= sizeStep;
                    }

                    if (textSize.Height > button.Height)
                    {
                        curSize -= sizeStep;
                    }
                }
            }

            if (curSize >= 0)
            {
                button.Font = new Font(button.Font.Name, curSize);

            }
        }

        private static Panel NewPanel(TableLayoutPanelType tableLayoutPanelType, string column, string row)
        {
            var text = string.Empty;

            switch (tableLayoutPanelType)
            {
                case TableLayoutPanelType.Depth:
                    text = row;
                    break;
                case TableLayoutPanelType.Width:
                    text = column;
                    break;
                case TableLayoutPanelType.Tree:
                    text = row;
                    break;
            }

            var label = new Label()
            {
                Text = $"{text}"
                ,
                Dock = DockStyle.Fill
                ,
                AutoSize = true
                ,
                BackColor = Color.Yellow
                ,
                ForeColor = Color.Green
                ,
                TextAlign = ContentAlignment.MiddleCenter
            };
            var panel = new Panel
            {
                Name = $"Panel{column}-{row}"
                ,
                BorderStyle = BorderStyle.None
                ,
                Dock = DockStyle.Fill
                ,
                AutoSize = true
                ,
                ForeColor = Color.FromArgb(85, 85, 85)
                //, Font = new System.Drawing.Font("Microsoft Sans Serif", 28F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
            };
            panel.Controls.Add(label);
            panel.Click += PanelClick;
            return panel;
        }

        private static void PanelClick(object sender, EventArgs e)
        {
            var panel = (Panel)sender;
            MessageBox.Show($"Panel is: {panel.Name}");

        }

        public void SetSpan(int columnSpan, int rowSpan)
        {
            var partNum = $"ST6657923-45";
            if (columnSpan <= 0 || rowSpan <= 0) return;
            var firstLocation = true;
            for (int i = 0; i < rowSpan; i++)
            {
                var row = _row + i;
                for (int j = 0; j < columnSpan; j++)
                {
                    var column = _column + j;
                    KeyValuePair<string, Point> point;
                    point = _points.FirstOrDefault(r => r.Key == $"{column}-{row}");
                    var cell = _tableLayoutPanel.GetControlFromPosition(point.Value.X, point.Value.Y);
                    var button = (Button)cell;
                    if (firstLocation)
                    {
                        // first control is a Label
                        button.Text = string.Empty;  // $"{partNum}";
                        firstLocation = false;
                    }
                    else
                    {
                        button.Text = string.Empty;
                    }

                    //        cell.AutoSize = true;
                    //        //cell.Font = new System.Drawing.Font("Microsoft Sans Serif", 28F, System.Drawing.FontStyle.Bold,
                    //        //    System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    button.BackColor = Color.Yellow;
                    button.ForeColor = Color.DarkSlateGray;
                }
            }

        }

        public void TurnOn(int column, int row)
        {
            KeyValuePair<string, Point> point;
            point = _points.FirstOrDefault(r => r.Key == $"{column}-{row}");
            if (point.Key != null)
            {
                var cell = _tableLayoutPanel.GetControlFromPosition(point.Value.X, point.Value.Y);
                cell.BackColor = Color.Yellow;
                cell.ForeColor = Color.DarkSlateGray; 
            }
        }

        private void CreatePoints(int columnCount, int rowCount)
        {
            _points = new Dictionary<string, Point>();

            for (var i = 0; i <= rowCount - 1; i++)
            {
                var rCount = rowCount - i;
                for (var j = 0; j < columnCount; j++)
                {
                    var cCount = j + 1;
                    var key = $"{cCount}-{rCount}";
                    _points.Add(key, new Point(j, i));
                }
            }
        }
    }
}
