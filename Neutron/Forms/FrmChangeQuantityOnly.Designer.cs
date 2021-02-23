namespace Neutron.Forms
{
    partial class FrmChangeQuantityOnly
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
            this.MBChangeQuantityCancel = new MetroFramework.Controls.MetroButton();
            this.MBChangeQuantitySave = new MetroFramework.Controls.MetroButton();
            this.LabelNewQuantity = new System.Windows.Forms.Label();
            this.TextBoxNewQuantity = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.MBChangeQuantityCancel);
            this.panel1.Controls.Add(this.MBChangeQuantitySave);
            this.panel1.Controls.Add(this.LabelNewQuantity);
            this.panel1.Controls.Add(this.TextBoxNewQuantity);
            this.panel1.Location = new System.Drawing.Point(38, 22);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(381, 317);
            this.panel1.TabIndex = 31;
            // 
            // MBChangeQuantityCancel
            // 
            this.MBChangeQuantityCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MBChangeQuantityCancel.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBChangeQuantityCancel.Location = new System.Drawing.Point(194, 208);
            this.MBChangeQuantityCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBChangeQuantityCancel.Name = "MBChangeQuantityCancel";
            this.MBChangeQuantityCancel.Size = new System.Drawing.Size(162, 76);
            this.MBChangeQuantityCancel.TabIndex = 2;
            this.MBChangeQuantityCancel.Text = "Cancel";
            this.MBChangeQuantityCancel.UseSelectable = true;
            // 
            // MBChangeQuantitySave
            // 
            this.MBChangeQuantitySave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBChangeQuantitySave.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBChangeQuantitySave.Location = new System.Drawing.Point(26, 208);
            this.MBChangeQuantitySave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBChangeQuantitySave.Name = "MBChangeQuantitySave";
            this.MBChangeQuantitySave.Size = new System.Drawing.Size(162, 76);
            this.MBChangeQuantitySave.TabIndex = 1;
            this.MBChangeQuantitySave.Text = "Save";
            this.MBChangeQuantitySave.UseSelectable = true;
            // 
            // LabelNewQuantity
            // 
            this.LabelNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNewQuantity.Location = new System.Drawing.Point(79, 142);
            this.LabelNewQuantity.Name = "LabelNewQuantity";
            this.LabelNewQuantity.Size = new System.Drawing.Size(222, 43);
            this.LabelNewQuantity.TabIndex = 29;
            this.LabelNewQuantity.Text = "New Quantity";
            this.LabelNewQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxNewQuantity
            // 
            this.TextBoxNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxNewQuantity.Location = new System.Drawing.Point(106, 49);
            this.TextBoxNewQuantity.Name = "TextBoxNewQuantity";
            this.TextBoxNewQuantity.Size = new System.Drawing.Size(170, 80);
            this.TextBoxNewQuantity.TabIndex = 0;
            this.TextBoxNewQuantity.Text = "1";
            this.TextBoxNewQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmChangeQuantityOnly
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(460, 375);
            this.Controls.Add(this.panel1);
            this.Name = "FrmChangeQuantityOnly";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Change Quantity";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private MetroFramework.Controls.MetroButton MBChangeQuantityCancel;
        private MetroFramework.Controls.MetroButton MBChangeQuantitySave;
        private System.Windows.Forms.Label LabelNewQuantity;
        private System.Windows.Forms.TextBox TextBoxNewQuantity;
    }
}