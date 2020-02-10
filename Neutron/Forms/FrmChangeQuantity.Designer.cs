namespace Neutron.Forms
{
    partial class FrmChangeQuantity
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmChangeQuantity));
            this.panel1 = new System.Windows.Forms.Panel();
            this.LabelChangeQuantityPosition = new System.Windows.Forms.Label();
            this.TextBoxChangeQuantityPosition = new System.Windows.Forms.TextBox();
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
            this.panel1.Controls.Add(this.LabelChangeQuantityPosition);
            this.panel1.Controls.Add(this.TextBoxChangeQuantityPosition);
            this.panel1.Controls.Add(this.MBChangeQuantityCancel);
            this.panel1.Controls.Add(this.MBChangeQuantitySave);
            this.panel1.Controls.Add(this.LabelNewQuantity);
            this.panel1.Controls.Add(this.TextBoxNewQuantity);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(381, 462);
            this.panel1.TabIndex = 28;
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
            // TextBoxChangeQuantityPosition
            // 
            this.TextBoxChangeQuantityPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxChangeQuantityPosition.Location = new System.Drawing.Point(126, 26);
            this.TextBoxChangeQuantityPosition.Name = "TextBoxChangeQuantityPosition";
            this.TextBoxChangeQuantityPosition.Size = new System.Drawing.Size(124, 80);
            this.TextBoxChangeQuantityPosition.TabIndex = 2;
            this.TextBoxChangeQuantityPosition.Text = "1";
            this.TextBoxChangeQuantityPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxChangeQuantityPosition.Leave += new System.EventHandler(this.TextBoxChangeQuantityPosition_TextChanged);
            // 
            // MBChangeQuantityCancel
            // 
            this.MBChangeQuantityCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MBChangeQuantityCancel.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBChangeQuantityCancel.Location = new System.Drawing.Point(191, 337);
            this.MBChangeQuantityCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBChangeQuantityCancel.Name = "MBChangeQuantityCancel";
            this.MBChangeQuantityCancel.Size = new System.Drawing.Size(162, 76);
            this.MBChangeQuantityCancel.TabIndex = 3;
            this.MBChangeQuantityCancel.Text = "Cancel";
            this.MBChangeQuantityCancel.UseSelectable = true;
            this.MBChangeQuantityCancel.Click += new System.EventHandler(this.MBChangeQuantityCancel_Click);
            // 
            // MBChangeQuantitySave
            // 
            this.MBChangeQuantitySave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBChangeQuantitySave.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBChangeQuantitySave.Location = new System.Drawing.Point(23, 337);
            this.MBChangeQuantitySave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBChangeQuantitySave.Name = "MBChangeQuantitySave";
            this.MBChangeQuantitySave.Size = new System.Drawing.Size(162, 76);
            this.MBChangeQuantitySave.TabIndex = 1;
            this.MBChangeQuantitySave.Text = "Save";
            this.MBChangeQuantitySave.UseSelectable = true;
            this.MBChangeQuantitySave.Click += new System.EventHandler(this.MBChangeQuantitySave_Click);
            // 
            // LabelNewQuantity
            // 
            this.LabelNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNewQuantity.Location = new System.Drawing.Point(77, 238);
            this.LabelNewQuantity.Name = "LabelNewQuantity";
            this.LabelNewQuantity.Size = new System.Drawing.Size(222, 43);
            this.LabelNewQuantity.TabIndex = 29;
            this.LabelNewQuantity.Text = "New Quantity";
            this.LabelNewQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxNewQuantity
            // 
            this.TextBoxNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxNewQuantity.Location = new System.Drawing.Point(103, 155);
            this.TextBoxNewQuantity.Name = "TextBoxNewQuantity";
            this.TextBoxNewQuantity.Size = new System.Drawing.Size(170, 80);
            this.TextBoxNewQuantity.TabIndex = 0;
            this.TextBoxNewQuantity.Text = "1";
            this.TextBoxNewQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxNewQuantity.Leave += new System.EventHandler(this.TextBoxNewQuantity_TextChanged);
            // 
            // FrmChangeQuantity
            // 
            this.AcceptButton = this.MBChangeQuantitySave;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.RoyalBlue;
            this.CancelButton = this.MBChangeQuantityCancel;
            this.ClientSize = new System.Drawing.Size(405, 486);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmChangeQuantity";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Change Quantity";
            this.TopMost = true;
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LabelChangeQuantityPosition;
        private System.Windows.Forms.TextBox TextBoxChangeQuantityPosition;
        private MetroFramework.Controls.MetroButton MBChangeQuantityCancel;
        private MetroFramework.Controls.MetroButton MBChangeQuantitySave;
        private System.Windows.Forms.Label LabelNewQuantity;
        private System.Windows.Forms.TextBox TextBoxNewQuantity;
    }
}