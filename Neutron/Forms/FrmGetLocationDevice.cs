using NeutronData.ModelViews;
using SlotNameFactory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore.Extensions;

namespace Neutron.Forms
{
    public partial class FrmGetLocationDevice : Form
    {
        private readonly BindingSource _newLocationBindingSource;
        public string Device = string.Empty;
        public string Tray = string.Empty;
        public string Over = string.Empty;
        public string Back = string.Empty;
        
        
        public LocationView Rec;
        public FrmGetLocationDevice(BindingSource newLocationBindingSource)
        {
            InitializeComponent();
            _newLocationBindingSource = newLocationBindingSource;            
            TextBoxDevice.Focus();
            TextBoxDevice.TextChanged += TextBox_TextChanged;
            TextBoxTray.TextChanged += TextBox_TextChanged;
            TextBoxOver.TextChanged += TextBox_TextChanged;
            TextBoxBack.TextChanged += TextBox_TextChanged;
        }
        


        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Device = TextBoxDevice.Text.Trim();
            Tray = TextBoxTray.Text.Trim();
            Over = TextBoxOver.Text.Trim();
            Back = TextBoxBack.Text.Trim();

            // Convert string inputs to integers
            if (!int.TryParse(Device, out int deviceInt) || !int.TryParse(Tray, out int trayInt) ||
                !int.TryParse(Over, out int overInt) || !int.TryParse(Back, out int backInt))
            {
                MessageBox.Show("Please enter valid numeric values for Device, Tray, Over, and Back.");
                return;
            }

            // get a record from _newLocationBindingSource that matches the slot
            Rec = _newLocationBindingSource.List.Cast<LocationView>()
                .FirstOrDefault(r => r.Loc1 == deviceInt && r.Loc2 == trayInt && r.Loc3 == overInt && r.Loc4 == backInt);
            if (Rec == null)
            {
                MessageBox.Show("No record found for the specified location.");
                TextBoxDevice.Focus();
                TextBoxDevice.SelectAll();                
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            // Cast the sender to a TextBox
            var textBox = sender as TextBox;
            // Check if the TextBox has 2 characters
            if (textBox != null && textBox.Text.Length == 2)
            {
                // Move focus to the next control
                this.SelectNextControl(textBox, forward: true, tabStopOnly: true, nested: true, wrap: true);
            }
        }
    }
}
