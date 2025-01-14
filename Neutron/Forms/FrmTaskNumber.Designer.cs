namespace Neutron.Forms
{
    partial class FrmTaskNumber
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
            this.ButtonOk = new System.Windows.Forms.Button();
            this.LabelSelectTaskNumber = new System.Windows.Forms.Label();
            this.ComboBoxTaskNumbers = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // ButtonOk
            // 
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(239, 287);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(211, 143);
            this.ButtonOk.TabIndex = 6;
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // LabelSelectTaskNumber
            // 
            this.LabelSelectTaskNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSelectTaskNumber.Location = new System.Drawing.Point(88, 34);
            this.LabelSelectTaskNumber.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.LabelSelectTaskNumber.Name = "LabelSelectTaskNumber";
            this.LabelSelectTaskNumber.Size = new System.Drawing.Size(531, 49);
            this.LabelSelectTaskNumber.TabIndex = 8;
            this.LabelSelectTaskNumber.Text = "Select Task Number";
            this.LabelSelectTaskNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ComboBoxTaskNumbers
            // 
            this.ComboBoxTaskNumbers.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboBoxTaskNumbers.FormattingEnabled = true;
            this.ComboBoxTaskNumbers.Location = new System.Drawing.Point(162, 106);
            this.ComboBoxTaskNumbers.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.ComboBoxTaskNumbers.Name = "ComboBoxTaskNumbers";
            this.ComboBoxTaskNumbers.Size = new System.Drawing.Size(368, 46);
            this.ComboBoxTaskNumbers.TabIndex = 9;
            // 
            // FrmTaskNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(19F, 38F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.ClientSize = new System.Drawing.Size(697, 463);
            this.Controls.Add(this.ComboBoxTaskNumbers);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.LabelSelectTaskNumber);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.Name = "FrmTaskNumber";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Task Number";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Label LabelSelectTaskNumber;
        private System.Windows.Forms.ComboBox ComboBoxTaskNumbers;
    }
}