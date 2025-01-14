namespace Neutron.Forms
{
    partial class FrmReplenPickViewAdjustment
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.ButtonAccept = new System.Windows.Forms.Button();
            this.ButtonBackorder = new System.Windows.Forms.Button();
            this.ButtonHighlight = new System.Windows.Forms.Button();
            this.LabelOrder = new System.Windows.Forms.Label();
            this.LabelItem = new System.Windows.Forms.Label();
            this.LabelDescription = new System.Windows.Forms.Label();
            this.LabelPosition = new System.Windows.Forms.Label();
            this.TextBoxPosition = new System.Windows.Forms.TextBox();
            this.LabelNewQuantity = new System.Windows.Forms.Label();
            this.TextBoxNewQuantity = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.LabelOrder);
            this.panel1.Controls.Add(this.LabelItem);
            this.panel1.Controls.Add(this.LabelDescription);
            this.panel1.Controls.Add(this.LabelPosition);
            this.panel1.Controls.Add(this.TextBoxPosition);
            this.panel1.Controls.Add(this.LabelNewQuantity);
            this.panel1.Controls.Add(this.TextBoxNewQuantity);
            this.panel1.Location = new System.Drawing.Point(17, 17);
            this.panel1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(708, 703);
            this.panel1.TabIndex = 30;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel2.Controls.Add(this.ButtonAccept);
            this.panel2.Controls.Add(this.ButtonBackorder);
            this.panel2.Controls.Add(this.ButtonHighlight);
            this.panel2.Location = new System.Drawing.Point(16, 554);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(671, 123);
            this.panel2.TabIndex = 36;
            // 
            // ButtonAccept
            // 
            this.ButtonAccept.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonAccept.Location = new System.Drawing.Point(448, 17);
            this.ButtonAccept.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ButtonAccept.Name = "ButtonAccept";
            this.ButtonAccept.Size = new System.Drawing.Size(200, 89);
            this.ButtonAccept.TabIndex = 2;
            this.ButtonAccept.Text = "Accept";
            this.ButtonAccept.UseVisualStyleBackColor = true;
            this.ButtonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // ButtonBackorder
            // 
            this.ButtonBackorder.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonBackorder.Location = new System.Drawing.Point(235, 17);
            this.ButtonBackorder.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ButtonBackorder.Name = "ButtonBackorder";
            this.ButtonBackorder.Size = new System.Drawing.Size(200, 89);
            this.ButtonBackorder.TabIndex = 1;
            this.ButtonBackorder.Text = "Backorder";
            this.ButtonBackorder.UseVisualStyleBackColor = true;
            this.ButtonBackorder.Click += new System.EventHandler(this.ButtonBackorder_Click);
            // 
            // ButtonHighlight
            // 
            this.ButtonHighlight.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonHighlight.Location = new System.Drawing.Point(23, 17);
            this.ButtonHighlight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ButtonHighlight.Name = "ButtonHighlight";
            this.ButtonHighlight.Size = new System.Drawing.Size(200, 89);
            this.ButtonHighlight.TabIndex = 0;
            this.ButtonHighlight.Text = "Skip";
            this.ButtonHighlight.UseVisualStyleBackColor = true;
            this.ButtonHighlight.Click += new System.EventHandler(this.ButtonHighlight_Click);
            // 
            // LabelOrder
            // 
            this.LabelOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOrder.Location = new System.Drawing.Point(44, 17);
            this.LabelOrder.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.LabelOrder.Name = "LabelOrder";
            this.LabelOrder.Size = new System.Drawing.Size(617, 44);
            this.LabelOrder.TabIndex = 33;
            this.LabelOrder.Text = "Order";
            this.LabelOrder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelItem
            // 
            this.LabelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelItem.Location = new System.Drawing.Point(44, 68);
            this.LabelItem.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.LabelItem.Name = "LabelItem";
            this.LabelItem.Size = new System.Drawing.Size(617, 44);
            this.LabelItem.TabIndex = 33;
            this.LabelItem.Text = "Item";
            this.LabelItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelDescription
            // 
            this.LabelDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelDescription.Location = new System.Drawing.Point(44, 119);
            this.LabelDescription.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.LabelDescription.Name = "LabelDescription";
            this.LabelDescription.Size = new System.Drawing.Size(617, 44);
            this.LabelDescription.TabIndex = 33;
            this.LabelDescription.Text = "Description";
            this.LabelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelPosition
            // 
            this.LabelPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelPosition.Location = new System.Drawing.Point(44, 319);
            this.LabelPosition.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.LabelPosition.Name = "LabelPosition";
            this.LabelPosition.Size = new System.Drawing.Size(617, 44);
            this.LabelPosition.TabIndex = 33;
            this.LabelPosition.Text = "Pick Position";
            this.LabelPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxPosition
            // 
            this.TextBoxPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxPosition.Location = new System.Drawing.Point(268, 214);
            this.TextBoxPosition.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.TextBoxPosition.Name = "TextBoxPosition";
            this.TextBoxPosition.ReadOnly = true;
            this.TextBoxPosition.Size = new System.Drawing.Size(168, 98);
            this.TextBoxPosition.TabIndex = 0;
            this.TextBoxPosition.TabStop = false;
            this.TextBoxPosition.Text = "1";
            this.TextBoxPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LabelNewQuantity
            // 
            this.LabelNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNewQuantity.Location = new System.Drawing.Point(44, 506);
            this.LabelNewQuantity.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.LabelNewQuantity.Name = "LabelNewQuantity";
            this.LabelNewQuantity.Size = new System.Drawing.Size(617, 44);
            this.LabelNewQuantity.TabIndex = 29;
            this.LabelNewQuantity.Text = "Quantity";
            this.LabelNewQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxNewQuantity
            // 
            this.TextBoxNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxNewQuantity.Location = new System.Drawing.Point(181, 400);
            this.TextBoxNewQuantity.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.TextBoxNewQuantity.Name = "TextBoxNewQuantity";
            this.TextBoxNewQuantity.Size = new System.Drawing.Size(336, 98);
            this.TextBoxNewQuantity.TabIndex = 0;
            this.TextBoxNewQuantity.Text = "1";
            this.TextBoxNewQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmReplenPickViewAdjustment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(756, 748);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FrmReplenPickViewAdjustment";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Replen PickView Adjustment";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button ButtonAccept;
        private System.Windows.Forms.Button ButtonBackorder;
        private System.Windows.Forms.Button ButtonHighlight;
        private System.Windows.Forms.Label LabelOrder;
        private System.Windows.Forms.Label LabelItem;
        private System.Windows.Forms.Label LabelDescription;
        private System.Windows.Forms.Label LabelPosition;
        private System.Windows.Forms.TextBox TextBoxPosition;
        private System.Windows.Forms.Label LabelNewQuantity;
        private System.Windows.Forms.TextBox TextBoxNewQuantity;
    }
}