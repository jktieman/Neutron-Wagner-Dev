using System;
using System.Drawing;
using System.Windows.Forms;
using Neutron.Extensions;
using Neutron.UserControls;

namespace Neutron.Classes
{
    public enum PanelType
    {
        OrderSelection,
        OrderInduction,
        ReplenOrderSelection,
        ReplenOrderInduction
    }
    public class PanelManager
    {
        private readonly PanelType _panelType;
        private readonly int _height;
        private readonly int _width;
        private readonly int _rows;
        private readonly int _positions;

        /// <summary>
        /// This constructor builds a NEW Master Panel based on the
        /// parameters passed in.
        /// The PanelManager is used to divide a Master Panel into
        /// smaller panels in order to display the picking positions or
        /// order positions in the induction screen.
        /// </summary>
        /// <param name="height">The height of the Panel"/></param>
        /// <param name="width">The width of the Panel</param>
        /// <param name="rows">The number of Rows to display in the Panel</param>
        /// <param name="positions">The total number of positions or <see cref="UserControlPickPosition"/> </param>
        /// <param name="panelType">Enum indicating which type of panel we're building</param>
        public PanelManager(int height, int width, int rows, int positions, PanelType panelType)
        {
            _height = height;
            _width = width;
            _rows = rows;
            _positions = positions;
            MasterPanel = new Panel();
            MasterPanel.Size = new Size(width, height);
        }
        /// <summary>
        /// This constructor takes an existing Panel and PanelType
        /// and builds on that
        /// </summary>
        /// <param name="panel">An existing Panel that will become the Master Panel</param>
        /// <param name="panelType">Enum indicating which type of panel we're building</param>
        public PanelManager(Panel panel, PanelType panelType)
        {
            _panelType = panelType;
            _height = panel.Size.Height;
            _width = panel.Size.Width;

            MasterPanel = panel;
        }

        public Panel MasterPanel { get; set; }

        public Panel AddPositions(int rows, int positions)
        {
            var labelPickPosWidthPercent = .35F;
            var labelPickPosHeightPercent = .25F;
            var posDisplayHeightPercent = .7F;
            var posDisplayWidthPercent = .8F;

            int widthOffset = 0;

            // The number of positions in each row
            var positionsInRow = positions / rows;
            // The height of each row is determined by dividing
            // the height of the master panel by the number of rows
            // if there is only a single row, in order to center it vertically
            // calculate the rowDivider 
            var rowDivider = rows == 1 ? 2 : rows;
            var rowHeight = _height / rowDivider;
            // The width of each position based on dividing the 
            //  width of the master panel by the number of positions requested. 
            var positionWidth = _width / positionsInRow;
            // the maximum width of a position is 25% of the master panel width
            var maxPositionWidth = Convert.ToInt32(_width * .25F);
            positionWidth = positionWidth > maxPositionWidth ? maxPositionWidth : positionWidth;
            // Clear the Master Panel of any existing Controls
            MasterPanel.Controls.Clear();
            // Set the first pick position to 1
            var pickPos = 1;
            // In order to create the rows from bottom to top
            // start with the total number of rows and reduced by 1
            // on each loop.
            for (var j = rows - 1; j > -1; j--)
            {
                // Loop over each position in the row
                for (var i = 0; i < positionsInRow; i++)
                {
                    // Create a UserControlPickPosition to hold each positions 
                    // LabelPickPos_, Pos_Display and TextBoxPickPos_
                    var ucPickPosition = new UserControlPickPosition();
                    ucPickPosition.Size = new Size(positionWidth, rowHeight);

                    if (positionsInRow < 4)
                    {
                        widthOffset = (_width - (positionsInRow * maxPositionWidth)) / 2;
                    }


                    ucPickPosition.Location = new Point(i * positionWidth + widthOffset, j * rowHeight);



                    ucPickPosition.BorderStyle = BorderStyle.Fixed3D;
                    ucPickPosition.BackColor = Color.Transparent;
                    ucPickPosition.Text = $@"{pickPos}";

                    // Add the label  LabelPickPos_
                    var labelPickPosSize = new Size(Convert.ToInt32(positionWidth * labelPickPosWidthPercent)
                        , Convert.ToInt32(rowHeight * labelPickPosHeightPercent));
                    var x = (ucPickPosition.Size.Width - labelPickPosSize.Width) / 2;
                    var y = 5;

                    var labelPickPosPoint = new Point(x, y);

                    ucPickPosition.AddLabel(labelPickPosSize, labelPickPosPoint, _panelType);

                    // add the Panel   Pos_Display  
                    var posDisplaySize = new Size(Convert.ToInt32(positionWidth * posDisplayWidthPercent)
                        , Convert.ToInt32(rowHeight * posDisplayHeightPercent));

                    x = (ucPickPosition.Size.Width - posDisplaySize.Width) / 2;
                    y = labelPickPosSize.Height + 6;

                    var posDisplayPoint = new Point(x, y);

                    ucPickPosition.AddPanel(posDisplaySize, posDisplayPoint, _panelType);

                    MasterPanel.Controls.Add(ucPickPosition);
                    pickPos++;
                }
            }

            return MasterPanel;
        }
    }
}