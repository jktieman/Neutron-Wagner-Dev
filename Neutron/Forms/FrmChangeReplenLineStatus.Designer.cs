namespace Neutron.Forms
{
    partial class FrmChangeReplenLineStatus
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
            this.ButtonSave = new System.Windows.Forms.Button();
            this.LabelInfo = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ComboBoxStatus = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.ButtonSave);
            this.panel1.Controls.Add(this.LabelInfo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.ComboBoxStatus);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(432, 184);
            this.panel1.TabIndex = 6;
            // 
            // ButtonSave
            // 
            this.ButtonSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSave.Location = new System.Drawing.Point(264, 111);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(127, 33);
            this.ButtonSave.TabIndex = 7;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // LabelInfo
            // 
            this.LabelInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelInfo.Location = new System.Drawing.Point(109, 27);
            this.LabelInfo.Name = "LabelInfo";
            this.LabelInfo.Size = new System.Drawing.Size(215, 39);
            this.LabelInfo.TabIndex = 6;
            this.LabelInfo.Text = "12345678";
            this.LabelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(105, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Status";
            // 
            // ComboBoxStatus
            // 
            this.ComboBoxStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboBoxStatus.FormattingEnabled = true;
            this.ComboBoxStatus.Location = new System.Drawing.Point(42, 114);
            this.ComboBoxStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ComboBoxStatus.Name = "ComboBoxStatus";
            this.ComboBoxStatus.Size = new System.Drawing.Size(180, 28);
            this.ComboBoxStatus.TabIndex = 4;
            // 
            // FrmReplenChangeLineStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(459, 222);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "FrmChangeReplenLineStatus";
            this.Text = "Change Line Status";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Label LabelInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ComboBoxStatus;
    }
}