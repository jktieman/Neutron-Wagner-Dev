namespace Neutron.Forms
{
    partial class FrmChangePriority
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmChangePriority));
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.LabelNewPriority = new System.Windows.Forms.Label();
            this.TextBoxNewPriority = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(187, 196);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(117, 86);
            this.ButtonCancel.TabIndex = 5;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonOk
            // 
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.Location = new System.Drawing.Point(40, 196);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(125, 86);
            this.ButtonOk.TabIndex = 6;
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // LabelNewPriority
            // 
            this.LabelNewPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNewPriority.Location = new System.Drawing.Point(98, 150);
            this.LabelNewPriority.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelNewPriority.Name = "LabelNewPriority";
            this.LabelNewPriority.Size = new System.Drawing.Size(148, 28);
            this.LabelNewPriority.TabIndex = 4;
            this.LabelNewPriority.Text = "New Priority";
            this.LabelNewPriority.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxNewPriority
            // 
            this.TextBoxNewPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxNewPriority.Location = new System.Drawing.Point(40, 54);
            this.TextBoxNewPriority.Margin = new System.Windows.Forms.Padding(2);
            this.TextBoxNewPriority.Name = "TextBoxNewPriority";
            this.TextBoxNewPriority.Size = new System.Drawing.Size(266, 80);
            this.TextBoxNewPriority.TabIndex = 3;
            this.TextBoxNewPriority.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmChangePriority
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(345, 305);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.LabelNewPriority);
            this.Controls.Add(this.TextBoxNewPriority);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmChangePriority";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Change Priority";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Label LabelNewPriority;
        private System.Windows.Forms.TextBox TextBoxNewPriority;
    }
}