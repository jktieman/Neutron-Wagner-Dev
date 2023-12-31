using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Extensions
{
    public static class FormUtilities
    {
        public static void CopyCellToClipBoard(DataGridView dataGridView, int columnIndex, int rowIndex)
        {
            dataGridView.CurrentCell = dataGridView[columnIndex, rowIndex];
            dataGridView.ContextMenuStrip = new ContextMenuStrip();
            dataGridView.ContextMenuStrip.Items.Add("Copy", null, (s, ev) =>
            {
                Clipboard.SetText(dataGridView.CurrentCell.Value.ToString());
            });
        }
        
        public static void PasteFromClipboard(TextBox textBox)
        {
            textBox.ContextMenuStrip = new ContextMenuStrip();
            textBox.ContextMenuStrip.Items.Add("Paste", null, (s, ev) =>
            {
                textBox.Text = Clipboard.GetText();
            });
        }
    }
}
