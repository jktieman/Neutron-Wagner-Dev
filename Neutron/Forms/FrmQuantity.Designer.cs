namespace Neutron.Forms
{
    partial class FrmQuantity
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
            this.PanelQuantity = new System.Windows.Forms.Panel();
            this.LabelQuantity = new System.Windows.Forms.Label();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.TextBoxQuantity = new System.Windows.Forms.TextBox();
            this.PanelQuantity.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelQuantity
            // 
            this.PanelQuantity.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PanelQuantity.Controls.Add(this.LabelQuantity);
            this.PanelQuantity.Controls.Add(this.ButtonOk);
            this.PanelQuantity.Controls.Add(this.TextBoxQuantity);
            this.PanelQuantity.Location = new System.Drawing.Point(26, 31);
            this.PanelQuantity.Name = "PanelQuantity";
            this.PanelQuantity.Size = new System.Drawing.Size(327, 276);
            this.PanelQuantity.TabIndex = 10;
            // 
            // LabelQuantity
            // 
            this.LabelQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelQuantity.Location = new System.Drawing.Point(87, 126);
            this.LabelQuantity.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelQuantity.Name = "LabelQuantity";
            this.LabelQuantity.Size = new System.Drawing.Size(148, 28);
            this.LabelQuantity.TabIndex = 12;
            this.LabelQuantity.Text = "Quantity";
            this.LabelQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonOk
            // 
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(91, 177);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(143, 78);
            this.ButtonOk.TabIndex = 11;
            this.ButtonOk.Text = "&Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // TextBoxQuantity
            // 
            this.TextBoxQuantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxQuantity.Location = new System.Drawing.Point(72, 17);
            this.TextBoxQuantity.Margin = new System.Windows.Forms.Padding(2);
            this.TextBoxQuantity.Name = "TextBoxQuantity";
            this.TextBoxQuantity.Size = new System.Drawing.Size(179, 80);
            this.TextBoxQuantity.TabIndex = 10;
            this.TextBoxQuantity.Text = "1";
            this.TextBoxQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmQuantity
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.ClientSize = new System.Drawing.Size(377, 339);
            this.ControlBox = false;
            this.Controls.Add(this.PanelQuantity);
            this.Name = "FrmQuantity";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter Quantity";
            this.PanelQuantity.ResumeLayout(false);
            this.PanelQuantity.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelQuantity;
        private System.Windows.Forms.Label LabelQuantity;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.TextBox TextBoxQuantity;
    }
}