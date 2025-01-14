namespace Neutron.Forms
{
    partial class FrmGetLocationDevice
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
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.PanelLocationInfo = new System.Windows.Forms.Panel();
            this.LabelBack = new System.Windows.Forms.Label();
            this.LabelOver = new System.Windows.Forms.Label();
            this.LabelTray = new System.Windows.Forms.Label();
            this.LabelDevice = new System.Windows.Forms.Label();
            this.TextBoxBack = new System.Windows.Forms.TextBox();
            this.TextBoxOver = new System.Windows.Forms.TextBox();
            this.TextBoxTray = new System.Windows.Forms.TextBox();
            this.TextBoxDevice = new System.Windows.Forms.TextBox();
            this.PanelLocationInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonOk
            // 
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(413, 279);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(135, 42);
            this.ButtonOk.TabIndex = 4;
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(564, 279);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(135, 42);
            this.ButtonCancel.TabIndex = 5;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // PanelLocationInfo
            // 
            this.PanelLocationInfo.Controls.Add(this.LabelBack);
            this.PanelLocationInfo.Controls.Add(this.LabelOver);
            this.PanelLocationInfo.Controls.Add(this.LabelTray);
            this.PanelLocationInfo.Controls.Add(this.LabelDevice);
            this.PanelLocationInfo.Controls.Add(this.TextBoxBack);
            this.PanelLocationInfo.Controls.Add(this.TextBoxOver);
            this.PanelLocationInfo.Controls.Add(this.TextBoxTray);
            this.PanelLocationInfo.Controls.Add(this.TextBoxDevice);
            this.PanelLocationInfo.Location = new System.Drawing.Point(20, 21);
            this.PanelLocationInfo.Name = "PanelLocationInfo";
            this.PanelLocationInfo.Size = new System.Drawing.Size(679, 218);
            this.PanelLocationInfo.TabIndex = 4;
            // 
            // LabelBack
            // 
            this.LabelBack.AutoSize = true;
            this.LabelBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelBack.Location = new System.Drawing.Point(549, 17);
            this.LabelBack.Name = "LabelBack";
            this.LabelBack.Size = new System.Drawing.Size(81, 32);
            this.LabelBack.TabIndex = 5;
            this.LabelBack.Text = "Back";
            // 
            // LabelOver
            // 
            this.LabelOver.AutoSize = true;
            this.LabelOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOver.Location = new System.Drawing.Point(387, 17);
            this.LabelOver.Name = "LabelOver";
            this.LabelOver.Size = new System.Drawing.Size(79, 32);
            this.LabelOver.TabIndex = 5;
            this.LabelOver.Text = "Over";
            // 
            // LabelTray
            // 
            this.LabelTray.AutoSize = true;
            this.LabelTray.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTray.Location = new System.Drawing.Point(219, 17);
            this.LabelTray.Name = "LabelTray";
            this.LabelTray.Size = new System.Drawing.Size(74, 32);
            this.LabelTray.TabIndex = 5;
            this.LabelTray.Text = "Tray";
            // 
            // LabelDevice
            // 
            this.LabelDevice.AutoSize = true;
            this.LabelDevice.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelDevice.Location = new System.Drawing.Point(38, 17);
            this.LabelDevice.Name = "LabelDevice";
            this.LabelDevice.Size = new System.Drawing.Size(107, 32);
            this.LabelDevice.TabIndex = 5;
            this.LabelDevice.Text = "Device";
            // 
            // TextBoxBack
            // 
            this.TextBoxBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxBack.Location = new System.Drawing.Point(514, 52);
            this.TextBoxBack.Name = "TextBoxBack";
            this.TextBoxBack.Size = new System.Drawing.Size(145, 143);
            this.TextBoxBack.TabIndex = 3;
            this.TextBoxBack.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxOver
            // 
            this.TextBoxOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxOver.Location = new System.Drawing.Point(349, 52);
            this.TextBoxOver.Name = "TextBoxOver";
            this.TextBoxOver.Size = new System.Drawing.Size(145, 143);
            this.TextBoxOver.TabIndex = 2;
            this.TextBoxOver.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxTray
            // 
            this.TextBoxTray.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxTray.Location = new System.Drawing.Point(182, 52);
            this.TextBoxTray.Name = "TextBoxTray";
            this.TextBoxTray.Size = new System.Drawing.Size(145, 143);
            this.TextBoxTray.TabIndex = 1;
            this.TextBoxTray.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxDevice
            // 
            this.TextBoxDevice.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxDevice.Location = new System.Drawing.Point(18, 52);
            this.TextBoxDevice.Name = "TextBoxDevice";
            this.TextBoxDevice.Size = new System.Drawing.Size(145, 143);
            this.TextBoxDevice.TabIndex = 0;
            this.TextBoxDevice.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmGetLocationDevice
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(743, 397);
            this.Controls.Add(this.PanelLocationInfo);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FrmGetLocationDevice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Device Location Entry Form";
            this.PanelLocationInfo.ResumeLayout(false);
            this.PanelLocationInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Panel PanelLocationInfo;
        private System.Windows.Forms.Label LabelBack;
        private System.Windows.Forms.Label LabelOver;
        private System.Windows.Forms.Label LabelTray;
        private System.Windows.Forms.Label LabelDevice;
        private System.Windows.Forms.TextBox TextBoxBack;
        private System.Windows.Forms.TextBox TextBoxOver;
        private System.Windows.Forms.TextBox TextBoxTray;
        private System.Windows.Forms.TextBox TextBoxDevice;
    }
}