namespace Neutron.Forms
{
    partial class FrmReprint
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmReprint));
            this.panel1 = new System.Windows.Forms.Panel();
            this.CheckBoxToteLabel = new System.Windows.Forms.CheckBox();
            this.CheckBoxDocument = new System.Windows.Forms.CheckBox();
            this.LabelChangeQuantityPosition = new System.Windows.Forms.Label();
            this.TextBoxReprintPosition = new System.Windows.Forms.TextBox();
            this.MBReprintCancel = new MetroFramework.Controls.MetroButton();
            this.MBReprintPrint = new MetroFramework.Controls.MetroButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.CheckBoxToteLabel);
            this.panel1.Controls.Add(this.CheckBoxDocument);
            this.panel1.Controls.Add(this.LabelChangeQuantityPosition);
            this.panel1.Controls.Add(this.TextBoxReprintPosition);
            this.panel1.Controls.Add(this.MBReprintCancel);
            this.panel1.Controls.Add(this.MBReprintPrint);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(381, 372);
            this.panel1.TabIndex = 29;
            // 
            // CheckBoxToteLabel
            // 
            this.CheckBoxToteLabel.AutoSize = true;
            this.CheckBoxToteLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckBoxToteLabel.Location = new System.Drawing.Point(126, 212);
            this.CheckBoxToteLabel.Name = "CheckBoxToteLabel";
            this.CheckBoxToteLabel.Size = new System.Drawing.Size(113, 24);
            this.CheckBoxToteLabel.TabIndex = 2;
            this.CheckBoxToteLabel.Text = "Tote Label";
            this.CheckBoxToteLabel.UseVisualStyleBackColor = true;
            // 
            // CheckBoxDocument
            // 
            this.CheckBoxDocument.AutoSize = true;
            this.CheckBoxDocument.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckBoxDocument.Location = new System.Drawing.Point(126, 168);
            this.CheckBoxDocument.Name = "CheckBoxDocument";
            this.CheckBoxDocument.Size = new System.Drawing.Size(125, 24);
            this.CheckBoxDocument.TabIndex = 1;
            this.CheckBoxDocument.Text = "Packing List";
            this.CheckBoxDocument.UseVisualStyleBackColor = true;
            // 
            // LabelChangeQuantityPosition
            // 
            this.LabelChangeQuantityPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelChangeQuantityPosition.Location = new System.Drawing.Point(104, 109);
            this.LabelChangeQuantityPosition.Name = "LabelChangeQuantityPosition";
            this.LabelChangeQuantityPosition.Size = new System.Drawing.Size(169, 43);
            this.LabelChangeQuantityPosition.TabIndex = 33;
            this.LabelChangeQuantityPosition.Text = "Pick Position";
            this.LabelChangeQuantityPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxReprintPosition
            // 
            this.TextBoxReprintPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxReprintPosition.Location = new System.Drawing.Point(126, 26);
            this.TextBoxReprintPosition.Name = "TextBoxReprintPosition";
            this.TextBoxReprintPosition.Size = new System.Drawing.Size(124, 80);
            this.TextBoxReprintPosition.TabIndex = 0;
            this.TextBoxReprintPosition.Text = "1";
            this.TextBoxReprintPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MBReprintCancel
            // 
            this.MBReprintCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MBReprintCancel.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBReprintCancel.Location = new System.Drawing.Point(191, 267);
            this.MBReprintCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBReprintCancel.Name = "MBReprintCancel";
            this.MBReprintCancel.Size = new System.Drawing.Size(162, 76);
            this.MBReprintCancel.TabIndex = 4;
            this.MBReprintCancel.Text = "Cancel";
            this.MBReprintCancel.UseSelectable = true;
            this.MBReprintCancel.Click += new System.EventHandler(this.MBReprintCancel_Click);
            // 
            // MBReprintPrint
            // 
            this.MBReprintPrint.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBReprintPrint.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBReprintPrint.Location = new System.Drawing.Point(23, 267);
            this.MBReprintPrint.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBReprintPrint.Name = "MBReprintPrint";
            this.MBReprintPrint.Size = new System.Drawing.Size(162, 76);
            this.MBReprintPrint.TabIndex = 3;
            this.MBReprintPrint.Text = "Print";
            this.MBReprintPrint.UseSelectable = true;
            this.MBReprintPrint.Click += new System.EventHandler(this.MBReprintPrint_Click);
            // 
            // FrmReprint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.RoyalBlue;
            this.ClientSize = new System.Drawing.Size(405, 402);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmReprint";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmReprint";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox CheckBoxToteLabel;
        private System.Windows.Forms.CheckBox CheckBoxDocument;
        private System.Windows.Forms.Label LabelChangeQuantityPosition;
        private System.Windows.Forms.TextBox TextBoxReprintPosition;
        private MetroFramework.Controls.MetroButton MBReprintCancel;
        private MetroFramework.Controls.MetroButton MBReprintPrint;
    }
}