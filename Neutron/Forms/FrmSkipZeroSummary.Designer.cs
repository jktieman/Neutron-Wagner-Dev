namespace Neutron.Forms
{
    partial class FrmSkipZeroSummary
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
            this.DataGridViewSkipZeroSummary = new System.Windows.Forms.DataGridView();
            this.ButtonClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewSkipZeroSummary)).BeginInit();
            this.SuspendLayout();
            // 
            // DataGridViewSkipZeroSummary
            // 
            this.DataGridViewSkipZeroSummary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridViewSkipZeroSummary.Location = new System.Drawing.Point(13, 13);
            this.DataGridViewSkipZeroSummary.Name = "DataGridViewSkipZeroSummary";
            this.DataGridViewSkipZeroSummary.Size = new System.Drawing.Size(743, 381);
            this.DataGridViewSkipZeroSummary.TabIndex = 0;
            this.DataGridViewSkipZeroSummary.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DataGridViewSkipZeroSummary_DataBindingComplete);
            // 
            // ButtonClose
            // 
            this.ButtonClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonClose.Location = new System.Drawing.Point(637, 410);
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.Size = new System.Drawing.Size(119, 39);
            this.ButtonClose.TabIndex = 1;
            this.ButtonClose.Text = "Close";
            this.ButtonClose.UseVisualStyleBackColor = true;
            this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // FrmSkipZeroSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 461);
            this.Controls.Add(this.ButtonClose);
            this.Controls.Add(this.DataGridViewSkipZeroSummary);
            this.Name = "FrmSkipZeroSummary";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Summary";
            this.Load += new System.EventHandler(this.FrmSkipZeroSummary_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewSkipZeroSummary)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView DataGridViewSkipZeroSummary;
        private System.Windows.Forms.Button ButtonClose;
    }
}