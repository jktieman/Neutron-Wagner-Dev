namespace Neutron.Forms
{
    partial class FrmLogin : MetroFramework.Forms.MetroForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLogin));
            this.mButtonLogin = new MetroFramework.Controls.MetroButton();
            this.mlOr = new MetroFramework.Controls.MetroLabel();
            this.mtbUsername = new MetroFramework.Controls.MetroTextBox();
            this.pictureBoxLogin = new System.Windows.Forms.PictureBox();
            this.mlUsername = new MetroFramework.Controls.MetroLabel();
            this.mtbPassword = new MetroFramework.Controls.MetroTextBox();
            this.mlPassword = new MetroFramework.Controls.MetroLabel();
            this.mtbPin = new MetroFramework.Controls.MetroTextBox();
            this.mlPin = new MetroFramework.Controls.MetroLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogin)).BeginInit();
            this.SuspendLayout();
            // 
            // mButtonLogin
            // 
            this.mButtonLogin.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.mButtonLogin.Location = new System.Drawing.Point(369, 261);
            this.mButtonLogin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mButtonLogin.Name = "mButtonLogin";
            this.mButtonLogin.Size = new System.Drawing.Size(237, 52);
            this.mButtonLogin.TabIndex = 3;
            this.mButtonLogin.Text = "Login";
            this.mButtonLogin.UseSelectable = true;
            this.mButtonLogin.Click += new System.EventHandler(this.mButtonLogin_Click);
            // 
            // mlOr
            // 
            this.mlOr.AutoSize = true;
            this.mlOr.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.mlOr.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.mlOr.Location = new System.Drawing.Point(293, 44);
            this.mlOr.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlOr.Name = "mlOr";
            this.mlOr.Size = new System.Drawing.Size(33, 25);
            this.mlOr.TabIndex = 1;
            this.mlOr.Text = "Or";
            this.mlOr.Visible = false;
            // 
            // mtbUsername
            // 
            // 
            // 
            // 
            this.mtbUsername.CustomButton.Image = null;
            this.mtbUsername.CustomButton.Location = new System.Drawing.Point(203, 2);
            this.mtbUsername.CustomButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mtbUsername.CustomButton.Name = "";
            this.mtbUsername.CustomButton.Size = new System.Drawing.Size(31, 31);
            this.mtbUsername.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.mtbUsername.CustomButton.TabIndex = 1;
            this.mtbUsername.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.mtbUsername.CustomButton.UseSelectable = true;
            this.mtbUsername.CustomButton.Visible = false;
            this.mtbUsername.FontSize = MetroFramework.MetroTextBoxSize.Tall;
            this.mtbUsername.Lines = new string[0];
            this.mtbUsername.Location = new System.Drawing.Point(369, 126);
            this.mtbUsername.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mtbUsername.MaxLength = 32767;
            this.mtbUsername.Name = "mtbUsername";
            this.mtbUsername.PasswordChar = '\0';
            this.mtbUsername.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.mtbUsername.SelectedText = "";
            this.mtbUsername.SelectionLength = 0;
            this.mtbUsername.SelectionStart = 0;
            this.mtbUsername.ShortcutsEnabled = true;
            this.mtbUsername.Size = new System.Drawing.Size(237, 36);
            this.mtbUsername.TabIndex = 1;
            this.mtbUsername.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mtbUsername.UseSelectable = true;
            this.mtbUsername.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.mtbUsername.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // pictureBoxLogin
            // 
            this.pictureBoxLogin.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxLogin.Image")));
            this.pictureBoxLogin.Location = new System.Drawing.Point(34, 81);
            this.pictureBoxLogin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBoxLogin.Name = "pictureBoxLogin";
            this.pictureBoxLogin.Size = new System.Drawing.Size(184, 242);
            this.pictureBoxLogin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxLogin.TabIndex = 3;
            this.pictureBoxLogin.TabStop = false;
            // 
            // mlUsername
            // 
            this.mlUsername.AutoSize = true;
            this.mlUsername.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.mlUsername.Location = new System.Drawing.Point(250, 132);
            this.mlUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlUsername.Name = "mlUsername";
            this.mlUsername.Size = new System.Drawing.Size(98, 25);
            this.mlUsername.TabIndex = 1;
            this.mlUsername.Text = "User name:";
            // 
            // mtbPassword
            // 
            // 
            // 
            // 
            this.mtbPassword.CustomButton.Image = null;
            this.mtbPassword.CustomButton.Location = new System.Drawing.Point(203, 2);
            this.mtbPassword.CustomButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mtbPassword.CustomButton.Name = "";
            this.mtbPassword.CustomButton.Size = new System.Drawing.Size(31, 31);
            this.mtbPassword.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.mtbPassword.CustomButton.TabIndex = 1;
            this.mtbPassword.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.mtbPassword.CustomButton.UseSelectable = true;
            this.mtbPassword.CustomButton.Visible = false;
            this.mtbPassword.FontSize = MetroFramework.MetroTextBoxSize.Tall;
            this.mtbPassword.Lines = new string[0];
            this.mtbPassword.Location = new System.Drawing.Point(369, 186);
            this.mtbPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mtbPassword.MaxLength = 32767;
            this.mtbPassword.Name = "mtbPassword";
            this.mtbPassword.PasswordChar = '*';
            this.mtbPassword.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.mtbPassword.SelectedText = "";
            this.mtbPassword.SelectionLength = 0;
            this.mtbPassword.SelectionStart = 0;
            this.mtbPassword.ShortcutsEnabled = true;
            this.mtbPassword.Size = new System.Drawing.Size(237, 36);
            this.mtbPassword.TabIndex = 2;
            this.mtbPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mtbPassword.UseSelectable = true;
            this.mtbPassword.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.mtbPassword.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // mlPassword
            // 
            this.mlPassword.AutoSize = true;
            this.mlPassword.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.mlPassword.Location = new System.Drawing.Point(262, 192);
            this.mlPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlPassword.Name = "mlPassword";
            this.mlPassword.Size = new System.Drawing.Size(86, 25);
            this.mlPassword.TabIndex = 1;
            this.mlPassword.Text = "Password:";
            // 
            // mtbPin
            // 
            // 
            // 
            // 
            this.mtbPin.CustomButton.Image = null;
            this.mtbPin.CustomButton.Location = new System.Drawing.Point(203, 2);
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
            this.mtbPin.Location = new System.Drawing.Point(369, 13);
            this.mtbPin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mtbPin.MaxLength = 32767;
            this.mtbPin.Name = "mtbPin";
            this.mtbPin.PasswordChar = '*';
            this.mtbPin.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.mtbPin.SelectedText = "";
            this.mtbPin.SelectionLength = 0;
            this.mtbPin.SelectionStart = 0;
            this.mtbPin.ShortcutsEnabled = true;
            this.mtbPin.Size = new System.Drawing.Size(237, 36);
            this.mtbPin.TabIndex = 0;
            this.mtbPin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mtbPin.UseSelectable = true;
            this.mtbPin.Visible = false;
            this.mtbPin.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.mtbPin.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // mlPin
            // 
            this.mlPin.AutoSize = true;
            this.mlPin.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.mlPin.Location = new System.Drawing.Point(239, 19);
            this.mlPin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlPin.Name = "mlPin";
            this.mlPin.Size = new System.Drawing.Size(109, 25);
            this.mlPin.TabIndex = 1;
            this.mlPin.Text = "Employee Id:";
            this.mlPin.Visible = false;
            // 
            // FrmLogin
            // 
            this.AcceptButton = this.mButtonLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 354);
            this.Controls.Add(this.pictureBoxLogin);
            this.Controls.Add(this.mtbPassword);
            this.Controls.Add(this.mlPassword);
            this.Controls.Add(this.mlPin);
            this.Controls.Add(this.mlUsername);
            this.Controls.Add(this.mtbPin);
            this.Controls.Add(this.mtbUsername);
            this.Controls.Add(this.mlOr);
            this.Controls.Add(this.mButtonLogin);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FrmLogin";
            this.Padding = new System.Windows.Forms.Padding(30, 94, 30, 31);
            this.Resizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Login";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmLogin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogin)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroButton mButtonLogin;
        private MetroFramework.Controls.MetroLabel mlOr;
        private MetroFramework.Controls.MetroTextBox mtbUsername;
        private System.Windows.Forms.PictureBox pictureBoxLogin;
        private MetroFramework.Controls.MetroLabel mlUsername;
        private MetroFramework.Controls.MetroTextBox mtbPassword;
        private MetroFramework.Controls.MetroLabel mlPassword;
        private MetroFramework.Controls.MetroTextBox mtbPin;
        private MetroFramework.Controls.MetroLabel mlPin;
    }
}