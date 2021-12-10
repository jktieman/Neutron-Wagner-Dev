namespace Neutron.UserControls
{
    partial class InductionPosition
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PanelInductionPosition = new System.Windows.Forms.Panel();
            this.LabelPos = new System.Windows.Forms.Label();
            this.TextBoxPos = new System.Windows.Forms.TextBox();
            this.AvailablePosDisplay = new System.Windows.Forms.Panel();
            this.PanelInductionPosition.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelInductionPosition
            // 
            this.PanelInductionPosition.BackColor = System.Drawing.Color.Transparent;
            this.PanelInductionPosition.Controls.Add(this.LabelPos);
            this.PanelInductionPosition.Controls.Add(this.TextBoxPos);
            this.PanelInductionPosition.Controls.Add(this.AvailablePosDisplay);
            this.PanelInductionPosition.Location = new System.Drawing.Point(0, 0);
            this.PanelInductionPosition.Name = "PanelInductionPosition";
            this.PanelInductionPosition.Size = new System.Drawing.Size(135, 90);
            this.PanelInductionPosition.TabIndex = 165;
            // 
            // LabelPos
            // 
            this.LabelPos.BackColor = System.Drawing.Color.Green;
            this.LabelPos.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelPos.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LabelPos.Location = new System.Drawing.Point(50, 3);
            this.LabelPos.Name = "LabelPos";
            this.LabelPos.Size = new System.Drawing.Size(36, 26);
            this.LabelPos.TabIndex = 166;
            this.LabelPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxPos
            // 
            this.TextBoxPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxPos.Location = new System.Drawing.Point(11, 34);
            this.TextBoxPos.Name = "TextBoxPos";
            this.TextBoxPos.Size = new System.Drawing.Size(112, 22);
            this.TextBoxPos.TabIndex = 165;
            this.TextBoxPos.Tag = "0";
            this.TextBoxPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // AvailablePosDisplay
            // 
            this.AvailablePosDisplay.BackColor = System.Drawing.Color.Transparent;
            this.AvailablePosDisplay.Location = new System.Drawing.Point(3, 34);
            this.AvailablePosDisplay.Name = "AvailablePosDisplay";
            this.AvailablePosDisplay.Size = new System.Drawing.Size(128, 53);
            this.AvailablePosDisplay.TabIndex = 167;
            // 
            // InductionPosition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelInductionPosition);
            this.Name = "InductionPosition";
            this.Size = new System.Drawing.Size(134, 93);
            this.PanelInductionPosition.ResumeLayout(false);
            this.PanelInductionPosition.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelInductionPosition;
        private System.Windows.Forms.Label LabelPos;
        private System.Windows.Forms.TextBox TextBoxPos;
        private System.Windows.Forms.Panel AvailablePosDisplay;
    }
}
