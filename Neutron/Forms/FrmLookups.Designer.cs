namespace Neutron.Forms
{
    partial class FrmLookups
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLookups));
            this.MetroPanelLookup = new MetroFramework.Controls.MetroPanel();
            this.LabelLookupName = new System.Windows.Forms.Label();
            this.MBAddNew = new MetroFramework.Controls.MetroButton();
            this.MButtonSave = new MetroFramework.Controls.MetroButton();
            this.MBPrintLookup = new MetroFramework.Controls.MetroButton();
            this.DataGridViewLookups = new System.Windows.Forms.DataGridView();
            this.ListBoxCodeNames = new System.Windows.Forms.ListBox();
            this.MButtonClose = new MetroFramework.Controls.MetroButton();
            this.LabelRecordCount = new System.Windows.Forms.Label();
            this.LabelFormTitle = new System.Windows.Forms.Label();
            this.mlUserInfo = new MetroFramework.Controls.MetroLabel();
            this.LabelFormHeaderText = new System.Windows.Forms.Label();
            this.MetroPanelLookup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewLookups)).BeginInit();
            this.SuspendLayout();
            // 
            // MetroPanelLookup
            // 
            this.MetroPanelLookup.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.MetroPanelLookup.BackColor = System.Drawing.Color.RoyalBlue;
            this.MetroPanelLookup.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MetroPanelLookup.Controls.Add(this.LabelLookupName);
            this.MetroPanelLookup.Controls.Add(this.MBAddNew);
            this.MetroPanelLookup.Controls.Add(this.MButtonSave);
            this.MetroPanelLookup.Controls.Add(this.MBPrintLookup);
            this.MetroPanelLookup.Controls.Add(this.DataGridViewLookups);
            this.MetroPanelLookup.Controls.Add(this.ListBoxCodeNames);
            this.MetroPanelLookup.Controls.Add(this.MButtonClose);
            this.MetroPanelLookup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetroPanelLookup.HorizontalScrollbarBarColor = true;
            this.MetroPanelLookup.HorizontalScrollbarHighlightOnWheel = false;
            this.MetroPanelLookup.HorizontalScrollbarSize = 10;
            this.MetroPanelLookup.Location = new System.Drawing.Point(22, 173);
            this.MetroPanelLookup.Name = "MetroPanelLookup";
            this.MetroPanelLookup.Size = new System.Drawing.Size(1156, 664);
            this.MetroPanelLookup.Style = MetroFramework.MetroColorStyle.Blue;
            this.MetroPanelLookup.TabIndex = 29;
            this.MetroPanelLookup.VerticalScrollbarBarColor = true;
            this.MetroPanelLookup.VerticalScrollbarHighlightOnWheel = false;
            this.MetroPanelLookup.VerticalScrollbarSize = 9;
            // 
            // LabelLookupName
            // 
            this.LabelLookupName.BackColor = System.Drawing.Color.White;
            this.LabelLookupName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelLookupName.Font = new System.Drawing.Font("Comic Sans MS", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelLookupName.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LabelLookupName.Location = new System.Drawing.Point(332, 13);
            this.LabelLookupName.Name = "LabelLookupName";
            this.LabelLookupName.Size = new System.Drawing.Size(604, 76);
            this.LabelLookupName.TabIndex = 33;
            this.LabelLookupName.Text = "Size Codes";
            this.LabelLookupName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MBAddNew
            // 
            this.MBAddNew.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBAddNew.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBAddNew.Location = new System.Drawing.Point(526, 553);
            this.MBAddNew.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBAddNew.Name = "MBAddNew";
            this.MBAddNew.Size = new System.Drawing.Size(146, 76);
            this.MBAddNew.TabIndex = 17;
            this.MBAddNew.Text = "Add New";
            this.MBAddNew.UseSelectable = true;
            this.MBAddNew.Click += new System.EventHandler(this.MBAddNew_Click);
            // 
            // MButtonSave
            // 
            this.MButtonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MButtonSave.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonSave.Location = new System.Drawing.Point(862, 553);
            this.MButtonSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonSave.Name = "MButtonSave";
            this.MButtonSave.Size = new System.Drawing.Size(146, 76);
            this.MButtonSave.TabIndex = 17;
            this.MButtonSave.Text = "Save";
            this.MButtonSave.UseSelectable = true;
            this.MButtonSave.Click += new System.EventHandler(this.MButtonSave_Click);
            // 
            // MBPrintLookup
            // 
            this.MBPrintLookup.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBPrintLookup.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBPrintLookup.Location = new System.Drawing.Point(367, 553);
            this.MBPrintLookup.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBPrintLookup.Name = "MBPrintLookup";
            this.MBPrintLookup.Size = new System.Drawing.Size(143, 76);
            this.MBPrintLookup.TabIndex = 16;
            this.MBPrintLookup.Text = "Save To File";
            this.MBPrintLookup.UseSelectable = true;
            // 
            // DataGridViewLookups
            // 
            this.DataGridViewLookups.AllowUserToOrderColumns = true;
            this.DataGridViewLookups.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DataGridViewLookups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridViewLookups.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.DataGridViewLookups.Location = new System.Drawing.Point(274, 132);
            this.DataGridViewLookups.Name = "DataGridViewLookups";
            this.DataGridViewLookups.RowTemplate.Height = 28;
            this.DataGridViewLookups.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.DataGridViewLookups.Size = new System.Drawing.Size(833, 412);
            this.DataGridViewLookups.TabIndex = 15;
            // 
            // ListBoxCodeNames
            // 
            this.ListBoxCodeNames.BackColor = System.Drawing.SystemColors.Window;
            this.ListBoxCodeNames.DisplayMember = "Name";
            this.ListBoxCodeNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListBoxCodeNames.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ListBoxCodeNames.FormattingEnabled = true;
            this.ListBoxCodeNames.ItemHeight = 20;
            this.ListBoxCodeNames.Location = new System.Drawing.Point(16, 132);
            this.ListBoxCodeNames.Name = "ListBoxCodeNames";
            this.ListBoxCodeNames.Size = new System.Drawing.Size(211, 484);
            this.ListBoxCodeNames.TabIndex = 14;
            this.ListBoxCodeNames.ValueMember = "TableName";
            this.ListBoxCodeNames.SelectedIndexChanged += new System.EventHandler(this.ListBoxCodeNames_SelectedIndexChanged);
            // 
            // MButtonClose
            // 
            this.MButtonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MButtonClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonClose.Location = new System.Drawing.Point(963, 13);
            this.MButtonClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonClose.Name = "MButtonClose";
            this.MButtonClose.Size = new System.Drawing.Size(177, 76);
            this.MButtonClose.TabIndex = 13;
            this.MButtonClose.Text = "Close";
            this.MButtonClose.UseSelectable = true;
            this.MButtonClose.Click += new System.EventHandler(this.MButtonClose_Click);
            // 
            // LabelRecordCount
            // 
            this.LabelRecordCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelRecordCount.Location = new System.Drawing.Point(905, 126);
            this.LabelRecordCount.Name = "LabelRecordCount";
            this.LabelRecordCount.Size = new System.Drawing.Size(279, 35);
            this.LabelRecordCount.TabIndex = 33;
            this.LabelRecordCount.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // LabelFormTitle
            // 
            this.LabelFormTitle.BackColor = System.Drawing.Color.RoyalBlue;
            this.LabelFormTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelFormTitle.Font = new System.Drawing.Font("Comic Sans MS", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormTitle.ForeColor = System.Drawing.Color.Black;
            this.LabelFormTitle.Location = new System.Drawing.Point(391, 98);
            this.LabelFormTitle.Name = "LabelFormTitle";
            this.LabelFormTitle.Size = new System.Drawing.Size(418, 66);
            this.LabelFormTitle.TabIndex = 32;
            this.LabelFormTitle.Text = "Lookups";
            this.LabelFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mlUserInfo
            // 
            this.mlUserInfo.Location = new System.Drawing.Point(804, 32);
            this.mlUserInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlUserInfo.Name = "mlUserInfo";
            this.mlUserInfo.Size = new System.Drawing.Size(380, 30);
            this.mlUserInfo.TabIndex = 31;
            this.mlUserInfo.Text = "Login ?";
            this.mlUserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelFormHeaderText
            // 
            this.LabelFormHeaderText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelFormHeaderText.Font = new System.Drawing.Font("Comic Sans MS", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormHeaderText.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LabelFormHeaderText.Location = new System.Drawing.Point(27, 16);
            this.LabelFormHeaderText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelFormHeaderText.Name = "LabelFormHeaderText";
            this.LabelFormHeaderText.Size = new System.Drawing.Size(713, 62);
            this.LabelFormHeaderText.TabIndex = 30;
            this.LabelFormHeaderText.Text = "Neutron Warehouse Management";
            this.LabelFormHeaderText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FrmLookups
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = MetroFramework.Forms.MetroFormBorderStyle.FixedSingle;
            this.ClientSize = new System.Drawing.Size(1200, 860);
            this.Controls.Add(this.MetroPanelLookup);
            this.Controls.Add(this.LabelRecordCount);
            this.Controls.Add(this.LabelFormTitle);
            this.Controls.Add(this.mlUserInfo);
            this.Controls.Add(this.LabelFormHeaderText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmLookups";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmLookups_FormClosing);
            this.Load += new System.EventHandler(this.FrmLookups_Load);
            this.MetroPanelLookup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewLookups)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroPanel MetroPanelLookup;
        private System.Windows.Forms.Label LabelRecordCount;
        private System.Windows.Forms.Label LabelFormTitle;
        private MetroFramework.Controls.MetroLabel mlUserInfo;
        private System.Windows.Forms.Label LabelFormHeaderText;
        private MetroFramework.Controls.MetroButton MButtonClose;
        private System.Windows.Forms.DataGridView DataGridViewLookups;
        private System.Windows.Forms.ListBox ListBoxCodeNames;
        private System.Windows.Forms.Label LabelLookupName;
        private MetroFramework.Controls.MetroButton MButtonSave;
        private MetroFramework.Controls.MetroButton MBPrintLookup;
        private MetroFramework.Controls.MetroButton MBAddNew;
    }
}