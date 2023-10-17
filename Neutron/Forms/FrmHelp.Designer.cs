namespace Neutron.Forms
{
    partial class FrmHelp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmHelp));
            this.LabelFormTitle = new System.Windows.Forms.Label();
            this.mlUserInfo = new MetroFramework.Controls.MetroLabel();
            this.LabelFormHeaderText = new System.Windows.Forms.Label();
            this.PanelHelp = new System.Windows.Forms.Panel();
            this.MBMainClose = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // LabelFormTitle
            // 
            this.LabelFormTitle.BackColor = System.Drawing.Color.DodgerBlue;
            this.LabelFormTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelFormTitle.Font = new System.Drawing.Font("Comic Sans MS", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormTitle.ForeColor = System.Drawing.Color.White;
            this.LabelFormTitle.Location = new System.Drawing.Point(367, 98);
            this.LabelFormTitle.Name = "LabelFormTitle";
            this.LabelFormTitle.Size = new System.Drawing.Size(418, 66);
            this.LabelFormTitle.TabIndex = 25;
            this.LabelFormTitle.Text = "Help";
            this.LabelFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mlUserInfo
            // 
            this.mlUserInfo.Location = new System.Drawing.Point(793, 35);
            this.mlUserInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlUserInfo.Name = "mlUserInfo";
            this.mlUserInfo.Size = new System.Drawing.Size(380, 30);
            this.mlUserInfo.TabIndex = 24;
            this.mlUserInfo.Text = "Login";
            this.mlUserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelFormHeaderText
            // 
            this.LabelFormHeaderText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelFormHeaderText.Font = new System.Drawing.Font("Comic Sans MS", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormHeaderText.ForeColor = System.Drawing.Color.DodgerBlue;
            this.LabelFormHeaderText.Location = new System.Drawing.Point(27, 16);
            this.LabelFormHeaderText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelFormHeaderText.Name = "LabelFormHeaderText";
            this.LabelFormHeaderText.Size = new System.Drawing.Size(713, 62);
            this.LabelFormHeaderText.TabIndex = 23;
            this.LabelFormHeaderText.Text = "Neutron Warehouse Management";
            this.LabelFormHeaderText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PanelHelp
            // 
            this.PanelHelp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.PanelHelp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PanelHelp.Location = new System.Drawing.Point(22, 173);
            this.PanelHelp.Name = "PanelHelp";
            this.PanelHelp.Size = new System.Drawing.Size(1235, 554);
            this.PanelHelp.TabIndex = 26;
            // 
            // MBMainClose
            // 
            this.MBMainClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBMainClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBMainClose.Location = new System.Drawing.Point(1080, 98);
            this.MBMainClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBMainClose.Name = "MBMainClose";
            this.MBMainClose.Size = new System.Drawing.Size(177, 68);
            this.MBMainClose.TabIndex = 29;
            this.MBMainClose.Text = " Home";
            this.MBMainClose.UseSelectable = true;
            this.MBMainClose.Click += new System.EventHandler(this.MBMainClose_Click);
            // 
            // FrmHelp
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = MetroFramework.Forms.MetroFormBorderStyle.FixedSingle;
            this.ClientSize = new System.Drawing.Size(1280, 750);
            this.Controls.Add(this.MBMainClose);
            this.Controls.Add(this.PanelHelp);
            this.Controls.Add(this.LabelFormTitle);
            this.Controls.Add(this.mlUserInfo);
            this.Controls.Add(this.LabelFormHeaderText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmHelp";
            this.Text = "Help";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmHelp_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LabelFormTitle;
        private MetroFramework.Controls.MetroLabel mlUserInfo;
        private System.Windows.Forms.Label LabelFormHeaderText;
        private System.Windows.Forms.Panel PanelHelp;
        private MetroFramework.Controls.MetroButton MBMainClose;
    }
}