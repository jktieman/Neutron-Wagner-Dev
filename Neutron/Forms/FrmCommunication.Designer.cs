namespace Neutron.Forms
{
    partial class FrmCommunication
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
            this.ListBoxRequests = new System.Windows.Forms.ListBox();
            this.ButtonClear = new System.Windows.Forms.Button();
            this.ButtonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ListBoxRequests
            // 
            this.ListBoxRequests.FormattingEnabled = true;
            this.ListBoxRequests.Location = new System.Drawing.Point(5, 37);
            this.ListBoxRequests.Name = "ListBoxRequests";
            this.ListBoxRequests.Size = new System.Drawing.Size(306, 719);
            this.ListBoxRequests.TabIndex = 120;
            // 
            // ButtonClear
            // 
            this.ButtonClear.Location = new System.Drawing.Point(12, 8);
            this.ButtonClear.Name = "ButtonClear";
            this.ButtonClear.Size = new System.Drawing.Size(113, 23);
            this.ButtonClear.TabIndex = 121;
            this.ButtonClear.Text = "Clear";
            this.ButtonClear.UseVisualStyleBackColor = true;
            this.ButtonClear.Click += new System.EventHandler(this.ButtonClear_Click);
            // 
            // ButtonClose
            // 
            this.ButtonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonClose.Location = new System.Drawing.Point(156, 8);
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.Size = new System.Drawing.Size(113, 23);
            this.ButtonClose.TabIndex = 121;
            this.ButtonClose.Text = "Close";
            this.ButtonClose.UseVisualStyleBackColor = true;
            this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // FrmCommunication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonClose;
            this.ClientSize = new System.Drawing.Size(316, 769);
            this.Controls.Add(this.ButtonClose);
            this.Controls.Add(this.ButtonClear);
            this.Controls.Add(this.ListBoxRequests);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmCommunication";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Serial Communication";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ListBoxRequests;
        private System.Windows.Forms.Button ButtonClear;
        private System.Windows.Forms.Button ButtonClose;
    }
}