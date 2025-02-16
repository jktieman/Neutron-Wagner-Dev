using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neutron.Interfaces;

namespace Neutron.Interfaces
{
    public interface IDialogService
    {
        Task<bool> ShowAsync(string title, string message, string okButtonText, string cancelButtonText);
        bool Show2(string title, string message, string okButtonText, string cancelButtonText);
    }
}

namespace Neutron.Models
{
    public class DialogService : IDialogService
    {
        public async Task<bool> ShowAsync(string title, string message, string okButtonText, string cancelButtonText)
        {
            return await Task.Run(() =>
            {
                using (Form form = new Form())
                {
                    form.Text = title;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.Font = new Font(form.Font.FontFamily, 40); // Set the font size here
                    Label label = new Label() { Text = message, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(form.Font.FontFamily, 16) };
                    Button okButton = new Button() { AutoSize = true, Text = okButtonText, Margin = new Padding(30), DialogResult = DialogResult.Yes, Dock = DockStyle.Bottom };
                    Button cancelButton = new Button() { AutoSize = true, Text = cancelButtonText, Margin = new Padding(30), DialogResult = DialogResult.No, Dock = DockStyle.Bottom };
                    form.Controls.Add(label);
                    form.Controls.Add(okButton);
                    form.Controls.Add(cancelButton);
                    form.AcceptButton = okButton;
                    form.CancelButton = cancelButton;
                    var result = form.ShowDialog();
                    return result == DialogResult.Yes;
                }
            });
        }

        public bool Show2(string title, string message, string okButtonText, string cancelButtonText)
        {
            bool result = false;
            using (Form form = new Form())
            using (Font formFont = new Font(form.Font.FontFamily, 30))
            using (Font labelFont = new Font(form.Font.FontFamily, 16))
            {
                form.Text = title;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.Font = formFont;
                form.Size = new Size(500, 450);
                form.BackColor = Color.Orange;
                Label label = new Label
                {
                    Text = message,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = labelFont
                };
                FlowLayoutPanel buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    FlowDirection = FlowDirection.RightToLeft,
                    AutoSize = true
                };
                Button okButton = new Button
                {
                    AutoSize = true,
                    Text = okButtonText,
                    Margin = new Padding(30),
                    DialogResult = DialogResult.Yes
                };
                Button cancelButton = new Button
                {
                    AutoSize = true,
                    Text = cancelButtonText,
                    Margin = new Padding(30),
                    DialogResult = DialogResult.No
                };
                buttonPanel.Controls.Add(cancelButton);
                buttonPanel.Controls.Add(okButton);
                form.Controls.Add(label);
                form.Controls.Add(buttonPanel);
                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;
                result = form.ShowDialog() == DialogResult.Yes;
            }
            return result;

        }
    }
}
