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
            this.PanelPriority = new System.Windows.Forms.Panel();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.LabelNewPriority = new System.Windows.Forms.Label();
            this.TextBoxNewPriority = new System.Windows.Forms.TextBox();
            this.PanelPriority.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelPriority
            // 
            this.PanelPriority.Controls.Add(this.ButtonCancel);
            this.PanelPriority.Controls.Add(this.ButtonOk);
            this.PanelPriority.Controls.Add(this.LabelNewPriority);
            this.PanelPriority.Controls.Add(this.TextBoxNewPriority);
            this.PanelPriority.Location = new System.Drawing.Point(25, 23);
            this.PanelPriority.Name = "PanelPriority";
            this.PanelPriority.Size = new System.Drawing.Size(303, 264);
            this.PanelPriority.TabIndex = 7;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(166, 159);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(117, 86);
            this.ButtonCancel.TabIndex = 9;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonOk
            // 
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(19, 159);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(125, 86);
            this.ButtonOk.TabIndex = 10;
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // LabelNewPriority
            // 
            this.LabelNewPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNewPriority.Location = new System.Drawing.Point(77, 113);
            this.LabelNewPriority.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelNewPriority.Name = "LabelNewPriority";
            this.LabelNewPriority.Size = new System.Drawing.Size(148, 28);
            this.LabelNewPriority.TabIndex = 8;
            this.LabelNewPriority.Text = "New Priority";
            this.LabelNewPriority.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxNewPriority
            // 
            this.TextBoxNewPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxNewPriority.Location = new System.Drawing.Point(64, 19);
            this.TextBoxNewPriority.Margin = new System.Windows.Forms.Padding(2);
            this.TextBoxNewPriority.Name = "TextBoxNewPriority";
            this.TextBoxNewPriority.Size = new System.Drawing.Size(175, 80);
            this.TextBoxNewPriority.TabIndex = 7;
            this.TextBoxNewPriority.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FrmChangePriority
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.ClientSize = new System.Drawing.Size(358, 323);
            this.ControlBox = false;
            this.Controls.Add(this.PanelPriority);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmChangePriority";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Change Priority";
            this.PanelPriority.ResumeLayout(false);
            this.PanelPriority.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelPriority;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Label LabelNewPriority;
        private System.Windows.Forms.TextBox TextBoxNewPriority;
    }
}