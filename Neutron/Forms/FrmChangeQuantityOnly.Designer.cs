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
            this.TextBoxOrder = new System.Windows.Forms.TextBox();
            this.LabelOrder = new System.Windows.Forms.Label();
            this.LabelReservation = new System.Windows.Forms.Label();
            this.LabelItem = new System.Windows.Forms.Label();
            this.LabelQuantity = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.TextBoxItem = new System.Windows.Forms.TextBox();
            this.TextBoxQuantity = new System.Windows.Forms.TextBox();
            this.TextBoxReservation = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.TextBoxQuantity);
            this.panel1.Controls.Add(this.TextBoxItem);
            this.panel1.Controls.Add(this.TextBoxReservation);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.TextBoxOrder);
            this.panel1.Controls.Add(this.MBChangeQuantityCancel);
            this.panel1.Controls.Add(this.MBChangeQuantitySave);
            this.panel1.Controls.Add(this.LabelQuantity);
            this.panel1.Controls.Add(this.LabelItem);
            this.panel1.Controls.Add(this.LabelReservation);
            this.panel1.Controls.Add(this.LabelOrder);
            this.panel1.Controls.Add(this.LabelNewQuantity);
            this.panel1.Controls.Add(this.TextBoxNewQuantity);
            this.panel1.Location = new System.Drawing.Point(38, 22);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(381, 457);
            this.panel1.TabIndex = 31;
            // 
            // MBChangeQuantityCancel
            // 
            this.MBChangeQuantityCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MBChangeQuantityCancel.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBChangeQuantityCancel.Location = new System.Drawing.Point(194, 353);
            this.MBChangeQuantityCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBChangeQuantityCancel.Name = "MBChangeQuantityCancel";
            this.MBChangeQuantityCancel.Size = new System.Drawing.Size(162, 76);
            this.MBChangeQuantityCancel.TabIndex = 2;
            this.MBChangeQuantityCancel.Text = "Cancel";
            this.MBChangeQuantityCancel.UseSelectable = true;
            this.MBChangeQuantityCancel.Click += new System.EventHandler(this.MBChangeQuantityCancel_Click);
            // 
            // MBChangeQuantitySave
            // 
            this.MBChangeQuantitySave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBChangeQuantitySave.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBChangeQuantitySave.Location = new System.Drawing.Point(26, 353);
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
            this.LabelNewQuantity.Location = new System.Drawing.Point(79, 287);
            this.LabelNewQuantity.Name = "LabelNewQuantity";
            this.LabelNewQuantity.Size = new System.Drawing.Size(222, 43);
            this.LabelNewQuantity.TabIndex = 29;
            this.LabelNewQuantity.Text = "New Quantity";
            this.LabelNewQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxNewQuantity
            // 
            this.TextBoxNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxNewQuantity.Location = new System.Drawing.Point(106, 194);
            this.TextBoxNewQuantity.Name = "TextBoxNewQuantity";
            this.TextBoxNewQuantity.Size = new System.Drawing.Size(170, 80);
            this.TextBoxNewQuantity.TabIndex = 0;
            this.TextBoxNewQuantity.Text = "1";
            this.TextBoxNewQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxOrder
            // 
            this.TextBoxOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxOrder.Location = new System.Drawing.Point(175, 27);
            this.TextBoxOrder.Name = "TextBoxOrder";
            this.TextBoxOrder.ReadOnly = true;
            this.TextBoxOrder.Size = new System.Drawing.Size(153, 26);
            this.TextBoxOrder.TabIndex = 31;
            this.TextBoxOrder.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LabelOrder
            // 
            this.LabelOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOrder.Location = new System.Drawing.Point(49, 26);
            this.LabelOrder.Name = "LabelOrder";
            this.LabelOrder.Size = new System.Drawing.Size(120, 29);
            this.LabelOrder.TabIndex = 29;
            this.LabelOrder.Text = "Order";
            this.LabelOrder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelReservation
            // 
            this.LabelReservation.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelReservation.Location = new System.Drawing.Point(49, 58);
            this.LabelReservation.Name = "LabelReservation";
            this.LabelReservation.Size = new System.Drawing.Size(120, 29);
            this.LabelReservation.TabIndex = 29;
            this.LabelReservation.Text = "Reservation";
            this.LabelReservation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelItem
            // 
            this.LabelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelItem.Location = new System.Drawing.Point(49, 90);
            this.LabelItem.Name = "LabelItem";
            this.LabelItem.Size = new System.Drawing.Size(120, 29);
            this.LabelItem.TabIndex = 29;
            this.LabelItem.Text = "Item";
            this.LabelItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelQuantity
            // 
            this.LabelQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelQuantity.Location = new System.Drawing.Point(49, 122);
            this.LabelQuantity.Name = "LabelQuantity";
            this.LabelQuantity.Size = new System.Drawing.Size(120, 29);
            this.LabelQuantity.TabIndex = 29;
            this.LabelQuantity.Text = "Quantity";
            this.LabelQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(175, 58);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(153, 26);
            this.textBox1.TabIndex = 31;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxItem
            // 
            this.TextBoxItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxItem.Location = new System.Drawing.Point(175, 91);
            this.TextBoxItem.Name = "TextBoxItem";
            this.TextBoxItem.ReadOnly = true;
            this.TextBoxItem.Size = new System.Drawing.Size(153, 26);
            this.TextBoxItem.TabIndex = 31;
            this.TextBoxItem.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxQuantity
            // 
            this.TextBoxQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxQuantity.Location = new System.Drawing.Point(175, 123);
            this.TextBoxQuantity.Name = "TextBoxQuantity";
            this.TextBoxQuantity.ReadOnly = true;
            this.TextBoxQuantity.Size = new System.Drawing.Size(153, 26);
            this.TextBoxQuantity.TabIndex = 31;
            this.TextBoxQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxReservation
            // 
            this.TextBoxReservation.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxReservation.Location = new System.Drawing.Point(175, 59);
            this.TextBoxReservation.Name = "TextBoxReservation";
            this.TextBoxReservation.ReadOnly = true;
            this.TextBoxReservation.Size = new System.Drawing.Size(153, 26);
            this.TextBoxReservation.TabIndex = 31;
            this.TextBoxReservation.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmChangeQuantityOnly
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(460, 491);
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
        private System.Windows.Forms.TextBox TextBoxQuantity;
        private System.Windows.Forms.TextBox TextBoxItem;
        private System.Windows.Forms.TextBox TextBoxReservation;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox TextBoxOrder;
        private System.Windows.Forms.Label LabelQuantity;
        private System.Windows.Forms.Label LabelItem;
        private System.Windows.Forms.Label LabelReservation;
        private System.Windows.Forms.Label LabelOrder;
    }
}