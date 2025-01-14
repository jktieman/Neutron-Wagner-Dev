namespace Neutron.Forms
{
    partial class FrmGetLocationBlast
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
            this.PanelLocationInfo = new System.Windows.Forms.Panel();
            this.LabelBack = new System.Windows.Forms.Label();
            this.LabelOver = new System.Windows.Forms.Label();
            this.TextBoxBack = new System.Windows.Forms.TextBox();
            this.TextBoxOver = new System.Windows.Forms.TextBox();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.PanelLocationInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelLocationInfo
            // 
            this.PanelLocationInfo.Controls.Add(this.LabelBack);
            this.PanelLocationInfo.Controls.Add(this.LabelOver);
            this.PanelLocationInfo.Controls.Add(this.TextBoxBack);
            this.PanelLocationInfo.Controls.Add(this.TextBoxOver);
            this.PanelLocationInfo.Location = new System.Drawing.Point(41, 75);
            this.PanelLocationInfo.Name = "PanelLocationInfo";
            this.PanelLocationInfo.Size = new System.Drawing.Size(402, 218);
            this.PanelLocationInfo.TabIndex = 6;
            // 
            // LabelBack
            // 
            this.LabelBack.AutoSize = true;
            this.LabelBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelBack.Location = new System.Drawing.Point(235, 17);
            this.LabelBack.Name = "LabelBack";
            this.LabelBack.Size = new System.Drawing.Size(115, 32);
            this.LabelBack.TabIndex = 5;
            this.LabelBack.Text = "Display";
            // 
            // LabelOver
            // 
            this.LabelOver.AutoSize = true;
            this.LabelOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOver.Location = new System.Drawing.Point(57, 17);
            this.LabelOver.Name = "LabelOver";
            this.LabelOver.Size = new System.Drawing.Size(117, 32);
            this.LabelOver.TabIndex = 5;
            this.LabelOver.Text = "Section";
            // 
            // TextBoxBack
            // 
            this.TextBoxBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxBack.Location = new System.Drawing.Point(211, 52);
            this.TextBoxBack.Name = "TextBoxBack";
            this.TextBoxBack.Size = new System.Drawing.Size(164, 143);
            this.TextBoxBack.TabIndex = 2;
            this.TextBoxBack.Tag = "2";
            this.TextBoxBack.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxBack.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // TextBoxOver
            // 
            this.TextBoxOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxOver.Location = new System.Drawing.Point(27, 52);
            this.TextBoxOver.Name = "TextBoxOver";
            this.TextBoxOver.Size = new System.Drawing.Size(164, 143);
            this.TextBoxOver.TabIndex = 0;
            this.TextBoxOver.Tag = "1";
            this.TextBoxOver.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxOver.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(246, 319);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(135, 42);
            this.ButtonCancel.TabIndex = 4;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonOk
            // 
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(95, 319);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(135, 42);
            this.ButtonOk.TabIndex = 3;
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // FrmGetLocationBlast
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 450);
            this.Controls.Add(this.PanelLocationInfo);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Name = "FrmGetLocationBlast";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Get Blastzone Location";
            this.PanelLocationInfo.ResumeLayout(false);
            this.PanelLocationInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelLocationInfo;
        private System.Windows.Forms.Label LabelBack;
        private System.Windows.Forms.Label LabelOver;
        private System.Windows.Forms.TextBox TextBoxBack;
        private System.Windows.Forms.TextBox TextBoxOver;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
    }
}