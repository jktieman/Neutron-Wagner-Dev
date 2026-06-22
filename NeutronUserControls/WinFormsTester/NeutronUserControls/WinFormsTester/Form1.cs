using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronTrayLayout;
using static NeutronTrayLayout.NeutronTrayUserControl;

namespace WinFormsTester
{
    public partial class Form1 : Form
    {
        private List<NeutronTrayUserControl> _userControls = new List<NeutronTrayUserControl>();
        private NeutronTrayUserControl _userControlTree = null;
        private NeutronTrayUserControl _userControlWidth = null;
        private NeutronTrayUserControl _userControlDepth = null;
        private NeutronTrayUserControl _userControlGrid = null;
        private int _levels = 4;
        private int _level = 2;
        private readonly int _columnCount;
        private readonly int _rowCount;
        private readonly Size _size;
        private readonly int _column;
        private readonly int _row;
        private readonly int _columnSpan;
        private readonly int _rowSpan;
        private readonly float _fontSize;
        private readonly float _columnWidth;
        private readonly float _rowHeight;
        private Dictionary<string, Point> _points;

        private NeutronTrayManager _manager = null;

        public Form1()
        {
            InitializeComponent();
            var levels = 3;
            var level = 2;
            var columnCount = 4;
            var rowCount = 1;
            var column = 2;
            var row = 1;
            var columnSpan = 1;
            var rowSpan = 1;
            


            //_manager = new NeutronTrayManager(PanelTableLayout, levels, level, columnCount
            //, rowCount: rowCount, column, row, columnSpan, rowSpan );
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            BuildTableTree();
        }

        private void BuildTableTree()
        {
            var control = PanelTableLayout.Controls.Find("Tree", false);
            if (control.Length > 0)
            {
                PanelTableLayout.Controls.Remove(control[0]);
            }

            // From database information and Item Information
            var rows = int.Parse(TextBoxLevels.Text);
            var row = int.Parse(TextBoxLevel.Text);
            var columns = 1; //int.Parse(TextBoxColumns.Text);
            var column = 1; //int.Parse(TextBoxColumn.Text);
            var rowSpan = 1; //int.Parse(TextBoxRowSpan.Text);
            var columnSpan = 1; //int.Parse(TextBoxColumnSpan.Text);

            // Control location and size
            // Comes from Settings in Neutron
            var x = 0; //  int.Parse(TextBoxLocationX.Text);
            var y = 0; //  int.Parse(TextBoxLocationY.Text);
            var tableHeight = PanelTableLayout.Height - 50; // int.Parse(TextBoxTableHeight.Text);
            var tableWidth = 100; // int.Parse(TextBoxTableWidth.Text);
            var size = new Size(tableWidth, tableHeight);
            var fontSize = (float)NumericUpDownFont.Value;

            _userControlTree = new NeutronTrayUserControl(TableLayoutPanelType.Tree, _levels, _levels, columns, rows, size, column, row,
                columnSpan, rowSpan, fontSize);
            this.SuspendLayout();
            _userControlTree.Name = "Tree";
            _userControlTree.Location = new Point(x, y);

            //this.Controls.Add(_userControlTree);

            // _userControls.Add(_userControlTree);
            PanelTableLayout.Controls.Add(_userControlTree);
            this.ResumeLayout(false);
            //CreateTableLayoutPanel(_userControl1, rows, row, columns, column, rowSpan, columnSpan, x, y, tableHeight, tableWidth);
        }

        private void CreateTableLayoutPanel(NeutronTrayUserControl userControl, int rows, int row, int columns, int column,
             int columnSpan, int rowSpan, int x, int y, int tableHeight, int tableWidth)
        {
            var size = new Size(tableWidth, tableHeight);

            _userControls.Add(userControl);
            //_userControls[0] = new NeutronTrayUserControl(columns, rows, size, column, row, rowSpan, columnSpan);
            userControl.Location = new Point(x, y);

            this.SuspendLayout();
            this.Controls.Add(userControl);
            this.ResumeLayout(false);
            //SetSpan(userControl, columnSpan, rowSpan);
        }

        private void SetSpan(NeutronTrayUserControl userControl, int columnSpan, int rowSpan)
        {
            userControl.SetSpan(columnSpan, rowSpan);
        }

        /// <summary>
        /// Clear button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (_userControls != null)
            {
                foreach (var neutronTrayUserControl in _userControls)
                {
                    this.Controls.Remove(neutronTrayUserControl);
                }
            }
        }

        private void RemoveUserControl(NeutronTrayUserControl control)
        {
            if (control != null)
            {
                this.Controls.Remove(control);
                _userControls.Remove(control);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BuildTableDepth();
        }

        private void BuildTableDepth()
        {
            var control = PanelTableLayout.Controls.Find("Depth", false);
            if (control.Length > 0)
            {
                PanelTableLayout.Controls.Remove(control[0]);
            }

            var rows = int.Parse(TextBoxRows.Text);
            var row = int.Parse(TextBoxRow.Text);
            var columns = 1; //int.Parse(TextBoxColumns.Text);
            var column = 1; //int.Parse(TextBoxColumn.Text);
            var rowSpan = 1; //int.Parse(TextBoxRowSpan.Text);
            var columnSpan = 1; //int.Parse(TextBoxColumnSpan.Text);

            // Control location and size
            var x = 110; // int.Parse(TextBoxLocationX.Text);
            var y = 0; // int.Parse(TextBoxLocationY.Text);
            var tableHeight = PanelTableLayout.Height - 50; // int.Parse(TextBoxTableHeight.Text);
            var tableWidth = 30; // int.Parse(TextBoxTableWidth.Text);
            var size = new Size(tableWidth, tableHeight);
            var fontSize = (float)NumericUpDownFont.Value;

            _userControlDepth = new NeutronTrayUserControl(TableLayoutPanelType.Depth, _levels, _level, columns, rows, size, column, row,
                columnSpan, rowSpan, fontSize);
            this.SuspendLayout();
            _userControlDepth.Name = "Depth";
            _userControlDepth.Location = new Point(x, y);

            //this.Controls.Add(_userControlDepth);
            //this.ResumeLayout(false);
            //_userControls.Add(_userControlDepth);
            PanelTableLayout.Controls.Add(_userControlDepth);
            this.ResumeLayout(false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            BuildTableWidth();
        }

        private void BuildTableWidth()
        {
            var control = PanelTableLayout.Controls.Find("Width", false);
            if (control.Length > 0)
            {
                PanelTableLayout.Controls.Remove(control[0]);
            }

            var rows = 1; // int.Parse(TextBoxRows.Text);
            var row = 1; // int.Parse(TextBoxRow.Text);
            var columns = int.Parse(TextBoxColumns.Text);
            var column = int.Parse(TextBoxColumn.Text);
            var rowSpan = int.Parse(TextBoxRowSpan.Text);
            var columnSpan = int.Parse(TextBoxColumnSpan.Text);

            // Control location and size
            var x = 140; // int.Parse(TextBoxLocationX.Text);
            var y = PanelTableLayout.Height - 50; // int.Parse(TextBoxLocationY.Text);
            var tableHeight = 50; // int.Parse(TextBoxTableHeight.Text);
            var tableWidth = PanelTableLayout.Width - 140; // int.Parse(TextBoxTableWidth.Text);
            var size = new Size(tableWidth, tableHeight);
            var fontSize = (float)NumericUpDownFont.Value;
            _userControlWidth = new NeutronTrayUserControl(TableLayoutPanelType.Width, _levels, _level, columns, rows, size, column, row,
                columnSpan, rowSpan, fontSize);
            this.SuspendLayout();
            _userControlWidth.Name = "Width";
            _userControlWidth.Location = new Point(x, y);

            PanelTableLayout.Controls.Add(_userControlWidth);
            this.ResumeLayout(false);
        }

        private void ButtonTurnOn_Click(object sender, EventArgs e)
        {
            var row = int.Parse(TextBoxTurnOnRow.Text);
            var column = int.Parse(TextBoxTurnOnColumn.Text);
            _userControls[0].TurnOn(column, row);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            BuildTableGrid();
        }

        private void BuildTableGrid()
        {
            var control = PanelTableLayout.Controls.Find("Grid", false);
            if (control.Length > 0)
            {
                PanelTableLayout.Controls.Remove(control[0]);
            }

            var rows = int.Parse(TextBoxRows.Text);
            var row = int.Parse(TextBoxRow.Text);
            var columns = int.Parse(TextBoxColumns.Text);
            var column = int.Parse(TextBoxColumn.Text);
            var rowSpan = int.Parse(TextBoxRowSpan.Text);
            var columnSpan = int.Parse(TextBoxColumnSpan.Text);

            // Control location and size
            // Grid Location is Height.LocationX + Height.width + 1
            var x = 140; // int.Parse(TextBoxLocationX.Text);
            var y = 0; // int.Parse(TextBoxLocationY.Text);
            var tableHeight = PanelTableLayout.Height - 50; //  int.Parse(TextBoxTableHeight.Text);
            var tableWidth = PanelTableLayout.Width - 140; // int.Parse(TextBoxTableWidth.Text);
            var size = new Size(tableWidth, tableHeight);
            var fontSize = (float)NumericUpDownFont.Value;
            _userControlGrid = new NeutronTrayUserControl(TableLayoutPanelType.Grid, _levels, _level, columns, rows, size, column, row,
                columnSpan, rowSpan, fontSize);
            this.SuspendLayout();
            _userControlGrid.Name = "Grid";
            _userControlGrid.Location = new Point(x, y);

            PanelTableLayout.Controls.Add(_userControlGrid);
            this.ResumeLayout(false);
        }

        private void Button_SizeChanged(object sender, EventArgs e)
        {
            var button = button6;
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

        private void ButtonDecreaseSize_Click(object sender, EventArgs e)
        {
            panel1.Height -= 10;
            panel1.Width -= 10;
        }

        private void ButtonIncreaseSize_Click(object sender, EventArgs e)
        {
            panel1.Height += 10;
            panel1.Width += 10;
        }

        private void ButtonResizeFont_Click(object sender, EventArgs e)
        {
            var button = button6;
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

        private void ButtonAll_Click(object sender, EventArgs e)
        {
            PanelTableLayout.SuspendLayout();
           // BuildTableTree();
            BuildTableDepth();
            BuildTableWidth();
            BuildTableGrid();
            PanelTableLayout.ResumeLayout();
        }
    }
}
