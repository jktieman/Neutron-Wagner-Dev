namespace Neutron.Forms
{
    partial class FrmScanTransId
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.TextBoxOrder = new System.Windows.Forms.TextBox();
            this.MBCancel = new MetroFramework.Controls.MetroButton();
            this.MBSave = new MetroFramework.Controls.MetroButton();
            this.LabelOrder = new System.Windows.Forms.Label();
            this.LabelScanTransid = new System.Windows.Forms.Label();
            this.TextBoxTransId = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.TextBoxOrder);
            this.panel1.Controls.Add(this.MBCancel);
            this.panel1.Controls.Add(this.MBSave);
            this.panel1.Controls.Add(this.LabelOrder);
            this.panel1.Controls.Add(this.LabelScanTransid);
            this.panel1.Controls.Add(this.TextBoxTransId);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(381, 347);
            this.panel1.TabIndex = 32;
            // 
            // TextBoxOrder
            // 
            this.TextBoxOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxOrder.Location = new System.Drawing.Point(149, 40);
            this.TextBoxOrder.Name = "TextBoxOrder";
            this.TextBoxOrder.ReadOnly = true;
            this.TextBoxOrder.Size = new System.Drawing.Size(176, 26);
            this.TextBoxOrder.TabIndex = 31;
            this.TextBoxOrder.TabStop = false;
            this.TextBoxOrder.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MBCancel
            // 
            this.MBCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MBCancel.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBCancel.Location = new System.Drawing.Point(191, 248);
            this.MBCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBCancel.Name = "MBCancel";
            this.MBCancel.Size = new System.Drawing.Size(162, 76);
            this.MBCancel.TabIndex = 2;
            this.MBCancel.Text = "Cancel";
            this.MBCancel.UseSelectable = true;
            this.MBCancel.Click += new System.EventHandler(this.MBCancel_Click);
            // 
            // MBSave
            // 
            this.MBSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBSave.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBSave.Location = new System.Drawing.Point(23, 248);
            this.MBSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBSave.Name = "MBSave";
            this.MBSave.Size = new System.Drawing.Size(162, 76);
            this.MBSave.TabIndex = 1;
            this.MBSave.Text = "Save";
            this.MBSave.UseSelectable = true;
            this.MBSave.Click += new System.EventHandler(this.MBSave_Click);
            // 
            // LabelOrder
            // 
            this.LabelOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOrder.Location = new System.Drawing.Point(46, 39);
            this.LabelOrder.Name = "LabelOrder";
            this.LabelOrder.Size = new System.Drawing.Size(97, 29);
            this.LabelOrder.TabIndex = 29;
            this.LabelOrder.Text = "Delivery";
            this.LabelOrder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelScanTransid
            // 
            this.LabelScanTransid.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelScanTransid.Location = new System.Drawing.Point(76, 201);
            this.LabelScanTransid.Name = "LabelScanTransid";
            this.LabelScanTransid.Size = new System.Drawing.Size(222, 43);
            this.LabelScanTransid.TabIndex = 29;
            this.LabelScanTransid.Text = "Scan TransId";
            this.LabelScanTransid.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxTransId
            // 
            this.TextBoxTransId.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxTransId.Location = new System.Drawing.Point(50, 102);
            this.TextBoxTransId.Name = "TextBoxTransId";
            this.TextBoxTransId.Size = new System.Drawing.Size(275, 80);
            this.TextBoxTransId.TabIndex = 0;
            this.TextBoxTransId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmScanTransId
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 374);
            this.Controls.Add(this.panel1);
            this.Name = "FrmScanTransId";
            this.Text = "Scan TransId";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox TextBoxOrder;
        private MetroFramework.Controls.MetroButton MBCancel;
        private MetroFramework.Controls.MetroButton MBSave;
        private System.Windows.Forms.Label LabelOrder;
        private System.Windows.Forms.Label LabelScanTransid;
        private System.Windows.Forms.TextBox TextBoxTransId;
    }
}