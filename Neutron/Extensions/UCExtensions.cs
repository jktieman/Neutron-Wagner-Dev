using System;
using System.Drawing;
using Neutron.UserControls;
using ContentAlignment = System.Drawing.ContentAlignment;
using Label = System.Windows.Forms.Label;
using System.Windows.Forms;
using Neutron.Classes;

namespace Neutron.Extensions
{
    public static class UcExtensions
    {
        public static UserControlPickPosition AddLabel(this UserControlPickPosition ucPickPosition, Size size, Point point, PanelType panelType)
        {
            var label = new Label();

            label = GetLabelDetails(label, ucPickPosition.Text, panelType);
            label.AutoSize = false;
            label.Font = new Font("Tahoma", 50F);
            label.Size = size;
            label.SizeLabelFont();
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Location = point;
            label.BorderStyle = BorderStyle.None;
            label.BackColor = Color.Transparent;

            ucPickPosition.Controls.Add(label);
            return ucPickPosition;
        }

        public static UserControlPickPosition AddPanel(this UserControlPickPosition ucPickPosition, Size size, Point point, PanelType panelType)
        {
            var textBoxPickPosHeightPercent = .8F;
            var textBoxPickPosWidthPercent = .95F;

            var panel = new Panel();
            panel = GetPanelDetails(panel, ucPickPosition.Text, panelType);
            panel.Size = size;
            panel.Location = point;
            panel.BorderStyle = BorderStyle.None;
            panel.BackColor = Color.Transparent;
            panel.Tag = ucPickPosition.Text;

            var textBoxPickPosSize = new Size(Convert.ToInt32(size.Width * textBoxPickPosWidthPercent)
                , Convert.ToInt32(size.Height * textBoxPickPosHeightPercent));

            var x = (panel.Size.Width - textBoxPickPosSize.Width) / 2;
            var y = 0; 

            var textBoxPickPosPoint = new Point(x, y);

            panel.AddTextBox(textBoxPickPosSize, textBoxPickPosPoint, panelType);

            ucPickPosition.Controls.Add(panel);
            return ucPickPosition;
        }

        private static Label GetLabelDetails(Label label, string pickPosition, PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.OrderInduction:
                {
                    label.Text = pickPosition;
                    label.Name = $@"LabelPos{pickPosition}";
                        break;
                }
                case PanelType.OrderSelection:
                {
                    label.Text = pickPosition;
                    label.Name = $@"LabelPickPos{pickPosition}";
                    break;
                }
                case PanelType.ReplenOrderInduction:
                {
                    label.Text = pickPosition;
                    label.Name = $@"LabelPos{pickPosition}";
                        break;
                }
                case PanelType.ReplenOrderSelection:
                {
                    label.Text = pickPosition;
                    label.Name = $@"LabelPickPos{pickPosition}";
                        break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(panelType), panelType, null);
            }

            return label;
        }

        private static Panel GetPanelDetails(Panel panel, string pickPosition, PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.OrderInduction:
                {
                    panel.Text = pickPosition;
                    panel.Name = $@"AvailablePos{pickPosition}Display";
                    break;
                }
                case PanelType.OrderSelection:
                {
                    panel.Text = pickPosition;
                    panel.Name = $@"Pos{pickPosition}Display";
                    break;
                }
                case PanelType.ReplenOrderInduction:
                {
                    panel.Text = pickPosition;
                    panel.Name = $@"AvailablePos{pickPosition}Display";
                        break;
                }
                case PanelType.ReplenOrderSelection:
                {
                    panel.Text = pickPosition;
                    panel.Name = $@"Pos{pickPosition}Display";
                    break;
                }
            }

            return panel;
        }

        private static TextBox GetTextBoxDetails(TextBox textBox, string pickPosition, PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.OrderInduction:
                {
                    textBox.Text = "88888888888";
                    textBox.Name = $@"TextBoxPos{pickPosition}";
                    textBox.BackColor = Color.White;
                    textBox.Multiline = true;
                    textBox.SizeTextBoxFont(2);
                        break;
                }
                case PanelType.OrderSelection:
                {
                    textBox.Text = "8888";
                    textBox.Name = $@"TextBoxPickPos{pickPosition}";
                    textBox.BackColor = Color.White;
                    textBox.Multiline = true;
                    textBox.SizeTextBoxFont(1);
                        break;
                }
                case PanelType.ReplenOrderInduction:
                {
                    textBox.Text = "88888888888";
                    textBox.Name = $@"TextBoxPos{pickPosition}";
                    textBox.BackColor = Color.White;
                    textBox.Multiline = true;
                    textBox.SizeTextBoxFont(2);
                        break;
                }
                case PanelType.ReplenOrderSelection:
                {
                    textBox.Text = "8888";
                    textBox.Name = $@"TextBoxPickPos{pickPosition}";
                    textBox.BackColor = Color.White;
                    textBox.Multiline = true;
                    textBox.SizeTextBoxFont(1);
                        break;
                }
            }

            return textBox;
        }
        /// <summary>
        /// Extension Method of a <see cref="Panel"/>  Adds a <see cref="TextBox"/> control to a <see cref="Panel"/>
        /// </summary>
        /// <param name="panel">The panel to add a TextBox to</param>
        /// <param name="size">The <see cref="Size"/> of the TextBox</param>
        /// <param name="point"></param>
        /// <param name="panelType"></param>
        /// <returns></returns>
        public static Panel AddTextBox(this Panel panel, Size size, Point point, PanelType panelType)
        {
            var textBox = new TextBox();
            var topPad = (panel.Size.Height - size.Height - 5) / 2 < 0 ? 0 : (panel.Size.Height - size.Height - 5) / 2;
            textBox.AutoSize = false;
            textBox.Font = new Font("Tahoma", 50F);
            textBox.Size = size;
            textBox = GetTextBoxDetails(textBox, Convert.ToString(panel.Tag), panelType);
            textBox.ReadOnly = false;
            textBox.Padding = new Padding(0, topPad, 0, 0);
            textBox.Location = point;
            textBox.Tag = panel.Tag;



            textBox.TextAlign = HorizontalAlignment.Center;
            //textBox.Padding = new Padding(0, 0, 0, 2);
            //textBox.BorderStyle = BorderStyle.FixedSingle;
            

            

            //textBox.Text = "8888";
            textBox.BringToFront();
            panel.Controls.Add(textBox);
            return panel;
        }

        /// <summary>
        /// Returns the largest font possible based on the size of the Label
        /// and its text.
        /// </summary>
        /// <param name="label">The label to process</param>
        /// <returns></returns>
        public static Label SizeLabelFont(this Label label)
        {
            // Only bother if there's text.
            var txt = label.Text;
            if (txt.Length <= 0) return label;
            var bestSize = 100;

            // See how much room we have,
            // allowing a bit for the Label's internal margin.
            // In this case, allowing 0 to get max size
            var wid = label.DisplayRectangle.Width - 1;
            var hgt = label.DisplayRectangle.Height - 1;

            // Make a Graphics object to measure the text.
            using (var gr = label.CreateGraphics())
            {
                for (var i = 8; i <= 200; i++)
                {
                    using (var testFont = new Font(label.Font.FontFamily.Name, i, FontStyle.Bold))
                    {
                        // See how much space the text would
                        // need, specifying a maximum width.
                        var textSize = gr.MeasureString(txt, testFont);
                        // if either of these are true, return the previous font size
                        if ((textSize.Width > wid) || (textSize.Height > hgt))
                        {
                            bestSize = i - 1;
                            break;
                        }
                    }
                }
            }

            // Use that font size.
            // Set the font of the label and return the label
            label.Font = new Font(label.Font.FontFamily.Name, bestSize, FontStyle.Bold);

            return label;
        }

        /// <summary>
        /// Returns the largest font possible based on the size of the TextBox
        /// and its text.
        /// </summary>
        /// <param name="textBox">The TextBox to process</param>
        /// <returns></returns>
        public static TextBox SizeTextBoxFont(this TextBox textBox, int lines = 1)
        {
            // Only bother if there's text.
            //var txt = textBox.Text;
           var txt = lines == 2 ? @"88888888888" : @"8888";

           var bestSize = 100;

            // See how much room we have, allowing a bit
            // for the Label's internal margin.
            var wid = textBox.DisplayRectangle.Width - 1;
            var hgt = (textBox.DisplayRectangle.Height - 1) / lines;

            // Make a Graphics object to measure the text.
            using (var gr = textBox.CreateGraphics())
            {
                for (var i = 8; i <= 200; i++)
                {
                    using (var testFont = new Font(textBox.Font.FontFamily.Name, i))
                    {
                        // See how much space the text would
                        // need, specifying a maximum width.
                        var textSize = gr.MeasureString(txt, testFont);
                            
                        if ((textSize.Width > wid) || (textSize.Height > hgt))
                        {
                            bestSize = i - 1;
                            break;
                        }
                    }
                }
            }

            // Use that font size.
            textBox.Font = new Font(textBox.Font.FontFamily.Name, bestSize);
            return textBox;
        }
    }
}

