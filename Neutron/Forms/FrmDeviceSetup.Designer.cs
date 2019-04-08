namespace Neutron.Forms
{
    partial class FrmDeviceSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDeviceSetup));
            this.label1 = new System.Windows.Forms.Label();
            this.ComboBoxDeviceType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TextBoxNumberOfCarriers = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TextBoxCarrierWidth = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.TextBoxCarrierDepth = new System.Windows.Forms.TextBox();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.TextBoxId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ComboBoxStation = new System.Windows.Forms.ComboBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(73, 109);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Device Type";
            // 
            // ComboBoxDeviceType
            // 
            this.ComboBoxDeviceType.FormattingEnabled = true;
            this.ComboBoxDeviceType.Items.AddRange(new object[] {
            "Kardex C3000",
            "Remstar RCC-1",
            "Remstar RCC-2",
            "Rack Shelving"});
            this.ComboBoxDeviceType.Location = new System.Drawing.Point(150, 104);
            this.ComboBoxDeviceType.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ComboBoxDeviceType.Name = "ComboBoxDeviceType";
            this.ComboBoxDeviceType.Size = new System.Drawing.Size(115, 21);
            this.ComboBoxDeviceType.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(39, 136);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Number Of Carriers";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxNumberOfCarriers
            // 
            this.TextBoxNumberOfCarriers.Location = new System.Drawing.Point(150, 136);
            this.TextBoxNumberOfCarriers.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextBoxNumberOfCarriers.Name = "TextBoxNumberOfCarriers";
            this.TextBoxNumberOfCarriers.Size = new System.Drawing.Size(68, 20);
            this.TextBoxNumberOfCarriers.TabIndex = 3;
            this.TextBoxNumberOfCarriers.Text = "44";
            this.TextBoxNumberOfCarriers.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(68, 167);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Carrier Width";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxCarrierWidth
            // 
            this.TextBoxCarrierWidth.Location = new System.Drawing.Point(151, 167);
            this.TextBoxCarrierWidth.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextBoxCarrierWidth.Name = "TextBoxCarrierWidth";
            this.TextBoxCarrierWidth.Size = new System.Drawing.Size(68, 20);
            this.TextBoxCarrierWidth.TabIndex = 4;
            this.TextBoxCarrierWidth.Text = "120";
            this.TextBoxCarrierWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(64, 198);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Carrier Depth ";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxCarrierDepth
            // 
            this.TextBoxCarrierDepth.Location = new System.Drawing.Point(150, 198);
            this.TextBoxCarrierDepth.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextBoxCarrierDepth.Name = "TextBoxCarrierDepth";
            this.TextBoxCarrierDepth.Size = new System.Drawing.Size(68, 20);
            this.TextBoxCarrierDepth.TabIndex = 5;
            this.TextBoxCarrierDepth.Text = "30";
            this.TextBoxCarrierDepth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(265, 246);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(56, 20);
            this.ButtonCancel.TabIndex = 7;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonSave
            // 
            this.ButtonSave.Location = new System.Drawing.Point(195, 246);
            this.ButtonSave.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(56, 20);
            this.ButtonSave.TabIndex = 6;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // TextBoxId
            // 
            this.TextBoxId.Location = new System.Drawing.Point(8, 8);
            this.TextBoxId.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextBoxId.Name = "TextBoxId";
            this.TextBoxId.Size = new System.Drawing.Size(68, 20);
            this.TextBoxId.TabIndex = 2;
            this.TextBoxId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxId.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(95, 77);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Station";
            // 
            // ComboBoxStation
            // 
            this.ComboBoxStation.FormattingEnabled = true;
            this.ComboBoxStation.Items.AddRange(new object[] {
            "Kardex C3000",
            "Remstar RCC-1",
            "Remstar RCC-2",
            "Rack Shelving"});
            this.ComboBoxStation.Location = new System.Drawing.Point(151, 75);
            this.ComboBoxStation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ComboBoxStation.Name = "ComboBoxStation";
            this.ComboBoxStation.Size = new System.Drawing.Size(115, 21);
            this.ComboBoxStation.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(150, 43);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(172, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.Tag = "";
            this.textBox1.Text = "Shuttle - 1";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(102, 45);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Name";
            // 
            // FrmDeviceSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 295);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.TextBoxId);
            this.Controls.Add(this.TextBoxCarrierDepth);
            this.Controls.Add(this.TextBoxCarrierWidth);
            this.Controls.Add(this.TextBoxNumberOfCarriers);
            this.Controls.Add(this.ComboBoxStation);
            this.Controls.Add(this.ComboBoxDeviceType);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FrmDeviceSetup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hardware Device Setup";
            this.Load += new System.EventHandler(this.FrmDeviceSetup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ComboBoxDeviceType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TextBoxNumberOfCarriers;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TextBoxCarrierWidth;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox TextBoxCarrierDepth;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.TextBox TextBoxId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ComboBoxStation;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
    }
}