using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NeutronTrayLayout.NeutronTrayUserControl;

namespace NeutronTrayLayout
{
    public class NeutronTrayManager
    {
        private float _fontSize = 10f;
        public Panel TrayLayout;
        private readonly int _levels;
        private readonly int _level;
        private readonly int _columnCount;
        private readonly int _rowCount;
        private readonly Size _size;
        private readonly int _column;
        private readonly int _row;
        private readonly int _columnSpan;
        private readonly int _rowSpan;
        private NeutronTrayUserControl _userControlTree = null;
        private NeutronTrayUserControl _userControlWidth = null;
        private NeutronTrayUserControl _userControlDepth = null;
        private NeutronTrayUserControl _userControlGrid = null;

        /// <summary>
        /// The NeutronTrayManager is responsible for creating all
        /// 4 possible Tables.
        /// Tree - the table that shows the LEVEL during Carousel and Shelf picking
        /// Height - the table that shows the number of locations back
        /// Width - the table that shows the number of location over
        /// Grid - the grid layout that shows the location
        /// </summary>
        /// <param name="trayLayout">The Panel that will hold all 4 Tables</param>
        /// <param name="levels">The number of Levels on a Tree Table</param>
        /// <param name="level">The Level that the item is on</param>
        /// <param name="columnCount">The number of columns in the Grid</param>
        /// <param name="rowCount">The number of rows in the Grid</param>
        /// <param name="size">The Size of the Table to create</param>
        /// <param name="column">The targeted column on the Grid.  Also used on the Width Table</param>
        /// <param name="row">The targeted row on the Grid.  Also used on the Height Table</param>
        /// <param name="columnSpan">The number of Columns that this item uses</param>
        /// <param name="rowSpan">The number of Rows that this item uses</param>
        /// <param name="fontSize">The Font Size of all the Tables</param>
        public NeutronTrayManager(Panel trayLayout, int levels, int level, int columnCount, int rowCount, int column, int row, int columnSpan, int rowSpan)
        {
            //trayLayout will give the Height and Width dimensions that 
            // the tables have to fit in
            TrayLayout = trayLayout;
            _levels = levels;
            _level = level;
            _columnCount = columnCount;
            _rowCount = rowCount;
            _size = TrayLayout.Size;
            _column = column;
            _row = row;
            _columnSpan = columnSpan;
            _rowSpan = rowSpan;
            _fontSize = GetFontSize();
            // Build a Tree using the passed in variables, _levels and _level
            // If either of the variables, _levels or _level = 0 then don't 
            // build a Tree Table.  It is shuttle or lift tray.
            BuildTableTree();
            BuildTableDepth();
            BuildTableWidth();
            BuildTableGrid();
        }
        /// <summary>
        /// Gets the Font Size based on the Panel Height
        /// Anything less that 200 pixels high and the Font Size is 10
        /// Otherwise the default Font Size is 20
        /// </summary>
        /// <returns></returns>
        private float GetFontSize()
        {
            var fontSize = 20f;
            if (TrayLayout.Height <= 200)
            {
                fontSize = 10f;
            }
            return fontSize;
        }

        /// <summary>
        /// Creates the Tree Table showing the level of the item selected
        /// when you're picking from a Carousel or Shelf.
        /// If either of the variables, _levels or _level = 0 then a
        /// Tree Table won't be built because it is shuttle or lift tray.
        /// </summary>
        public void BuildTableTree()
        {
            // Check the values of _levels and _level, Return if either is 0
            if (_levels == 0 || _level == 0) return;

            // if there is already a Tree control, Remove it before creating a new one.
            // it would be in the Panel that was passed in.
            // find the control using its Name property
            var control = TrayLayout.Controls.Find("Tree", false);
            // control is an Array, so check the length 
            if (control.Length > 0)
            {
                // if a control is found that is Named "Tree", it would
                // be the only one in the array, so Remove the first record [0] 
                TrayLayout.Controls.Remove(control[0]);
            }

            var rowCount = _levels;
            var row = _level;

            // From database and Item Information
            // In a Tree, the default is 1 for both.
            var columnCount = 1; 
            var column = 1; 
            
            // The span variables are used in tables to show when an item
            // takes more that one location on a tray or shelf
            // In a Tree, the default is 1 for both.
            var rowSpan = 1; 
            var columnSpan = 1; 

            // Control location and size
            // Comes from Settings in Neutron

            // The Tree Table is on the right edge of the trayLayout
            // It is 50 pixels shorter than the Panel control
            // and Location is upper left corner
            var x = 0; 
            var y = 0;            
            var tableHeight = TrayLayout.Height - 50;
            var tableWidth = 100;
            // If the Panel is vertically oriented then narrow the Tree Table
            if (TrayLayout.Height > TrayLayout.Width)
            {
                tableWidth = 40; 
            }
            // The actual Size of the Table
            var size = new Size(tableWidth, tableHeight);
            // Font size is determined ahead of time and passed in
            var fontSize = _fontSize; 

            // Build the Tree UserControl
            _userControlTree = new NeutronTrayUserControl(TableLayoutPanelType.Tree, _levels, _level, columnCount, rowCount, size, column, row, columnSpan, rowSpan, fontSize);
            TrayLayout.SuspendLayout();
            // Name of the Tree UserControl
            _userControlTree.Name = "Tree";
            // Set the Location of the Tree UserControl on the Panel
            _userControlTree.Location = new Point(x, y);
            // Add the Tree UserControl to the Panels Controls
            TrayLayout.Controls.Add(_userControlTree);
            TrayLayout.ResumeLayout(false);

        }
        /// <summary>
        /// The front to back indicator that is located on the left
        /// side of the Grid
        /// </summary>
        public void BuildTableDepth()
        {
            // Check the values of _rowCount and _row, Return if either is 0
            if (_rowCount == 0 || _row == 0) return;

            // if there is already a Depth control, Remove it before creating a new one.
            // it would be in the Panel that was passed in.
            // find the control using its Name property
            var control = TrayLayout.Controls.Find("Depth", false);
            // control is an Array, so check the length 
            if (control.Length > 0)
            {
                // if a control is found that is Named "Depth", it would
                // be the only one in the array, so Remove the first record [0] 
                TrayLayout.Controls.Remove(control[0]);
            }

            // From database and Item Information
            // In a Depth Table, the default is 1 for both.
            var columnCount = 1;
            var column = 1;

            // The span variables are used in tables to show when an item
            // takes more that one location on a tray or shelf
            // In a Depth Table, the default is 1 for both.
            var rowSpan = 1;
            var columnSpan = 1;

            // Control location and size
            // Comes from Settings in Neutron
            var x = 0;
            var y = 0;
            // The Depth Table is 10 pixels to the right of the Tree Table
            // so X = UserControlTree.Width + 10, if UserControlTree exists.
            // If it is a Tray without a Tree, the Depth Table is on the Left Edge
            // Location 0,0
            if (_userControlTree != null)
            {
                x = _userControlTree.Width + 10;
            }

            var tableHeight = TrayLayout.Height - 50;
            var tableWidth = 30;

            // The actual Size of the Table
            var size = new Size(tableWidth, tableHeight);
            // Font size is determined ahead of time and passed in
            var fontSize = _fontSize;

            // Build the Depth UserControl
            _userControlDepth = new NeutronTrayUserControl(TableLayoutPanelType.Depth, _levels, _level, columnCount, _rowCount, size, column, _row, columnSpan, rowSpan, fontSize);
            TrayLayout.SuspendLayout();
            // Name of the Depth UserControl
            _userControlDepth.Name = "Depth";
            // Set the Location of the Depth UserControl on the Panel
            _userControlDepth.Location = new Point(x, y);
            // Add the Depth UserControl to the Panels Controls
            TrayLayout.Controls.Add(_userControlDepth);
            TrayLayout.ResumeLayout(false);
        }

        public void BuildTableWidth()
        {
            // Check the values of _rowCount and _row, Return if either is 0
            if (_columnCount == 0 || _column == 0) return;

            // if there is already a Width control, Remove it before creating a new one.
            // it would be in the Panel that was passed in.
            // find the control using its Name property
            var control = TrayLayout.Controls.Find("Width", false);
            // control is an Array, so check the length 
            if (control.Length > 0)
            {
                // if a control is found that is Named "Width", it would
                // be the only one in the array, so Remove the first record [0] 
                TrayLayout.Controls.Remove(control[0]);
            }

            // From database and Item Information
            // In a Width Table, the default is 1 for both.
            var rowCount = 1;
            var row = 1;

            // The span variables are used in tables to show when an item
            // takes more that one location on a tray or shelf
            // In a Width Table, the default is 1 for both.
            var rowSpan = 1;
            var columnSpan = 1;

            // Control location and size
            // Comes from Settings in Neutron
            var x = 0;
            var y = TrayLayout.Height - 49;
            // The Width Table is 1 pixel to the right of the Depth Table
            // so X = UserControlTree.Width + 10, if UserControlTree exists.
            // If it is a Tray without a Tree, the Width Table is on the Left Edge
            // Location 0,0
            if (_userControlDepth != null)
            {
                x = _userControlDepth.Location.X + _userControlDepth.Width + 1;
            }

            var tableHeight = 30;
            var tableWidth = TrayLayout.Width - x;

            // The actual Size of the Table
            var size = new Size(tableWidth, tableHeight);
            // Font size is determined ahead of time and passed in
            var fontSize = _fontSize;

            // Build the Depth UserControl
            _userControlWidth = new NeutronTrayUserControl(TableLayoutPanelType.Width, _levels, _level, _columnCount, rowCount, size, _column, row, columnSpan, rowSpan, fontSize);
            TrayLayout.SuspendLayout();
            // Name of the Depth UserControl
            _userControlWidth.Name = "Width";
            // Set the Location of the Width UserControl on the Panel
            _userControlWidth.Location = new Point(x, y);
            // Add the Width UserControl to the Panels Controls
            TrayLayout.Controls.Add(_userControlWidth);
            TrayLayout.ResumeLayout(false);
        }

        public void BuildTableGrid()
        {
            // Check the values of _rowCount and _row, Return if either is 0
            if (_rowCount == 0 || _columnCount == 0) return;

            // if there is already a Grid control, Remove it before creating a new one.
            // it would be in the Panel that was passed in.
            // find the control using its Name property
            var control = TrayLayout.Controls.Find("Grid", false);
            // control is an Array, so check the length 
            if (control.Length > 0)
            {
                // if a control is found that is Named "Grid", it would
                // be the only one in the array, so Remove the first record [0] 
                TrayLayout.Controls.Remove(control[0]);
            }

            // The span variables are used in tables to show when an item
            // takes more that one location on a tray or shelf
            // In a Grid Table, the default is 1 for both.

            // Control location and size
            // Comes from Settings in Neutron
            var x = _userControlDepth.Width + 1;
            var y = 0;
            // The Grid Table is 1 pixels to the right of the Depth Table
            // so X = UserControlTree.Width + UserControlDepth.Width +  11, if UserControlTree exists.
            // If it is a Tray without a Tree, the Grid Table is on the Left Edge
            // Location 0,0
            if (_userControlTree != null)
            {
                x = _userControlTree.Width + _userControlDepth.Width + 11;
            }

            var tableHeight = TrayLayout.Height - 50;
            var tableWidth = TrayLayout.Width - x;

            // The actual Size of the Table
            var size = new Size(tableWidth, tableHeight);
            // Font size is determined ahead of time and passed in
            var fontSize = _fontSize;

            // Build the Grid UserControl
            _userControlGrid = new NeutronTrayUserControl(TableLayoutPanelType.Grid, _levels, _level, _columnCount, _rowCount, size, _column, _row, _columnSpan, _rowSpan, fontSize);
            TrayLayout.SuspendLayout();
            // Name of the Grid UserControl
            _userControlGrid.Name = "Grid";
            // Set the Location of the Grid UserControl on the Panel
            _userControlGrid.Location = new Point(x, y);
            // Add the Grid UserControl to the Panels Controls
            TrayLayout.Controls.Add(_userControlGrid);
            TrayLayout.ResumeLayout(false);
        }
    }
}
