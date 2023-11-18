namespace NeutronTestForm
{
    partial class FrmNeutronTest
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
            this.ButtonExcelTesting = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonExcelTesting
            // 
            this.ButtonExcelTesting.Location = new System.Drawing.Point(23, 24);
            this.ButtonExcelTesting.Name = "ButtonExcelTesting";
            this.ButtonExcelTesting.Size = new System.Drawing.Size(127, 33);
            this.ButtonExcelTesting.TabIndex = 0;
            this.ButtonExcelTesting.Text = "Excel Testing";
            this.ButtonExcelTesting.UseVisualStyleBackColor = true;
            this.ButtonExcelTesting.Click += new System.EventHandler(this.ButtonExcelTesting_Click);
            // 
            // FrmNeutronTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ButtonExcelTesting);
            this.Name = "FrmNeutronTest";
            this.Text = "Neutron Test Form";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonExcelTesting;
    }
}

