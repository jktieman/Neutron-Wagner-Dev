using MetroFramework.Forms;

namespace Neutron.Forms
{
    partial class FrmPin : MetroForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPin));
            this.mButtonLogin = new MetroFramework.Controls.MetroButton();
            this.mtbPin = new MetroFramework.Controls.MetroTextBox();
            this.mlPin = new MetroFramework.Controls.MetroLabel();
            this.SuspendLayout();
            // 
            // mButtonLogin
            // 
            this.mButtonLogin.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.mButtonLogin.Location = new System.Drawing.Point(33, 162);
            this.mButtonLogin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mButtonLogin.Name = "mButtonLogin";
            this.mButtonLogin.Size = new System.Drawing.Size(344, 52);
            this.mButtonLogin.TabIndex = 1;
            this.mButtonLogin.Text = "Login";
            this.mButtonLogin.UseSelectable = true;
            this.mButtonLogin.Click += new System.EventHandler(this.mButtonLogin_Click);
            // 
            // mtbPin
            // 
            // 
            // 
            // 
            this.mtbPin.CustomButton.Image = null;
            this.mtbPin.CustomButton.Location = new System.Drawing.Point(112, 2);
            this.mtbPin.CustomButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mtbPin.CustomButton.Name = "";
            this.mtbPin.CustomButton.Size = new System.Drawing.Size(31, 31);
            this.mtbPin.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.mtbPin.CustomButton.TabIndex = 1;
            this.mtbPin.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.mtbPin.CustomButton.UseSelectable = true;
            this.mtbPin.CustomButton.Visible = false;
            this.mtbPin.FontSize = MetroFramework.MetroTextBoxSize.Tall;
            this.mtbPin.Lines = new string[0];
            this.mtbPin.Location = new System.Drawing.Point(132, 116);
            this.mtbPin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mtbPin.MaxLength = 32767;
            this.mtbPin.Name = "mtbPin";
            this.mtbPin.PasswordChar = '*';
            this.mtbPin.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.mtbPin.SelectedText = "";
            this.mtbPin.SelectionLength = 0;
            this.mtbPin.SelectionStart = 0;
            this.mtbPin.ShortcutsEnabled = true;
            this.mtbPin.Size = new System.Drawing.Size(146, 36);
            this.mtbPin.TabIndex = 0;
            this.mtbPin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mtbPin.UseSelectable = true;
            this.mtbPin.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.mtbPin.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // mlPin
            // 
            this.mlPin.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.mlPin.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.mlPin.Location = new System.Drawing.Point(34, 69);
            this.mlPin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlPin.Name = "mlPin";
            this.mlPin.Size = new System.Drawing.Size(342, 37);
            this.mlPin.TabIndex = 1;
            this.mlPin.Text = "Employee PIN";
            this.mlPin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrmPin
            // 
            this.AcceptButton = this.mButtonLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = MetroFramework.Forms.MetroFormBorderStyle.FixedSingle;
            this.ClientSize = new System.Drawing.Size(410, 224);
            this.Controls.Add(this.mlPin);
            this.Controls.Add(this.mtbPin);
            this.Controls.Add(this.mButtonLogin);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmPin";
            this.Padding = new System.Windows.Forms.Padding(30, 94, 30, 31);
            this.Resizable = false;
            this.Text = "Neutron Login";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmPin_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroButton mButtonLogin;
        private MetroFramework.Controls.MetroTextBox mtbPin;
        private MetroFramework.Controls.MetroLabel mlPin;
    }
}