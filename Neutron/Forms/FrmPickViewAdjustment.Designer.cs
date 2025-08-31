namespace Neutron.Forms
{
    partial class FrmPickViewAdjustment
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
            this.ButtonCancel = new System.Windows.Forms.Button();
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
            this.panel1.Location = new System.Drawing.Point(18, 18);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(532, 572);
            this.panel1.TabIndex = 29;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel2.Controls.Add(this.ButtonCancel);
            this.panel2.Controls.Add(this.ButtonAccept);
            this.panel2.Controls.Add(this.ButtonBackorder);
            this.panel2.Controls.Add(this.ButtonHighlight);
            this.panel2.Location = new System.Drawing.Point(12, 450);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(503, 100);
            this.panel2.TabIndex = 36;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(382, 14);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(113, 72);
            this.ButtonCancel.TabIndex = 2;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonAccept
            // 
            this.ButtonAccept.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonAccept.Location = new System.Drawing.Point(258, 14);
            this.ButtonAccept.Name = "ButtonAccept";
            this.ButtonAccept.Size = new System.Drawing.Size(113, 72);
            this.ButtonAccept.TabIndex = 2;
            this.ButtonAccept.Text = "Accept";
            this.ButtonAccept.UseVisualStyleBackColor = true;
            this.ButtonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // ButtonBackorder
            // 
            this.ButtonBackorder.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonBackorder.Location = new System.Drawing.Point(134, 14);
            this.ButtonBackorder.Name = "ButtonBackorder";
            this.ButtonBackorder.Size = new System.Drawing.Size(113, 72);
            this.ButtonBackorder.TabIndex = 1;
            this.ButtonBackorder.Text = "Backorder";
            this.ButtonBackorder.UseVisualStyleBackColor = true;
            this.ButtonBackorder.Click += new System.EventHandler(this.ButtonBackorder_Click);
            // 
            // ButtonHighlight
            // 
            this.ButtonHighlight.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonHighlight.Location = new System.Drawing.Point(10, 14);
            this.ButtonHighlight.Name = "ButtonHighlight";
            this.ButtonHighlight.Size = new System.Drawing.Size(113, 72);
            this.ButtonHighlight.TabIndex = 0;
            this.ButtonHighlight.Text = "Skip";
            this.ButtonHighlight.UseVisualStyleBackColor = true;
            this.ButtonHighlight.Click += new System.EventHandler(this.ButtonHighlight_Click);
            // 
            // LabelOrder
            // 
            this.LabelOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOrder.Location = new System.Drawing.Point(33, 14);
            this.LabelOrder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelOrder.Name = "LabelOrder";
            this.LabelOrder.Size = new System.Drawing.Size(463, 36);
            this.LabelOrder.TabIndex = 33;
            this.LabelOrder.Text = "Order";
            this.LabelOrder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelItem
            // 
            this.LabelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelItem.Location = new System.Drawing.Point(33, 55);
            this.LabelItem.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelItem.Name = "LabelItem";
            this.LabelItem.Size = new System.Drawing.Size(463, 36);
            this.LabelItem.TabIndex = 33;
            this.LabelItem.Text = "Item";
            this.LabelItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelDescription
            // 
            this.LabelDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelDescription.Location = new System.Drawing.Point(33, 97);
            this.LabelDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelDescription.Name = "LabelDescription";
            this.LabelDescription.Size = new System.Drawing.Size(463, 36);
            this.LabelDescription.TabIndex = 33;
            this.LabelDescription.Text = "Description";
            this.LabelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelPosition
            // 
            this.LabelPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelPosition.Location = new System.Drawing.Point(33, 259);
            this.LabelPosition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelPosition.Name = "LabelPosition";
            this.LabelPosition.Size = new System.Drawing.Size(463, 36);
            this.LabelPosition.TabIndex = 33;
            this.LabelPosition.Text = "Pick Position";
            this.LabelPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxPosition
            // 
            this.TextBoxPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxPosition.Location = new System.Drawing.Point(201, 174);
            this.TextBoxPosition.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxPosition.Name = "TextBoxPosition";
            this.TextBoxPosition.ReadOnly = true;
            this.TextBoxPosition.Size = new System.Drawing.Size(127, 80);
            this.TextBoxPosition.TabIndex = 0;
            this.TextBoxPosition.TabStop = false;
            this.TextBoxPosition.Text = "1";
            this.TextBoxPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LabelNewQuantity
            // 
            this.LabelNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNewQuantity.Location = new System.Drawing.Point(33, 411);
            this.LabelNewQuantity.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelNewQuantity.Name = "LabelNewQuantity";
            this.LabelNewQuantity.Size = new System.Drawing.Size(463, 36);
            this.LabelNewQuantity.TabIndex = 29;
            this.LabelNewQuantity.Text = "Quantity Picked";
            this.LabelNewQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxNewQuantity
            // 
            this.TextBoxNewQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxNewQuantity.Location = new System.Drawing.Point(136, 325);
            this.TextBoxNewQuantity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxNewQuantity.Name = "TextBoxNewQuantity";
            this.TextBoxNewQuantity.Size = new System.Drawing.Size(253, 80);
            this.TextBoxNewQuantity.TabIndex = 0;
            this.TextBoxNewQuantity.Text = "1";
            this.TextBoxNewQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmPickViewAdjustment
            // 
            this.AcceptButton = this.ButtonCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(567, 608);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FrmPickViewAdjustment";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pick Adjustment";
            this.TopMost = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmPickViewAdjustment_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LabelOrder;
        private System.Windows.Forms.Label LabelItem;
        private System.Windows.Forms.Label LabelDescription;
        private System.Windows.Forms.Label LabelPosition;
        private System.Windows.Forms.TextBox TextBoxPosition;
        private System.Windows.Forms.Label LabelNewQuantity;
        private System.Windows.Forms.TextBox TextBoxNewQuantity;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button ButtonAccept;
        private System.Windows.Forms.Button ButtonBackorder;
        private System.Windows.Forms.Button ButtonHighlight;
        private System.Windows.Forms.Button ButtonCancel;
    }
}