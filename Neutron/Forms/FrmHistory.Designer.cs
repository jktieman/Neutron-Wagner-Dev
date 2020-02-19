namespace Neutron.Forms
{
    partial class FrmHistory
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmHistory));
            this.LabelRecordCount = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ButtonClearAll = new System.Windows.Forms.Button();
            this.ButtonCheckAll = new System.Windows.Forms.Button();
            this.GroupBoxActionCodes = new System.Windows.Forms.GroupBox();
            this.CheckedListBoxActionCodes = new System.Windows.Forms.CheckedListBox();
            this.PanelDateRanges = new System.Windows.Forms.Panel();
            this.LabelTo = new System.Windows.Forms.Label();
            this.DateTimePickerTo = new System.Windows.Forms.DateTimePicker();
            this.LabelFrom = new System.Windows.Forms.Label();
            this.DateTimePickerFrom = new System.Windows.Forms.DateTimePicker();
            this.RadioButtonDateRange = new System.Windows.Forms.RadioButton();
            this.RadioButtonMonth = new System.Windows.Forms.RadioButton();
            this.RadioButtonWeek = new System.Windows.Forms.RadioButton();
            this.RadioButtonToday = new System.Windows.Forms.RadioButton();
            this.LabelFindDescription = new System.Windows.Forms.Label();
            this.TextBoxFind = new System.Windows.Forms.TextBox();
            this.MButtonClose = new MetroFramework.Controls.MetroButton();
            this.MBHistoryTransmitSelected = new MetroFramework.Controls.MetroButton();
            this.MBSaveHistory = new MetroFramework.Controls.MetroButton();
            this.MButtonRun = new MetroFramework.Controls.MetroButton();
            this.DataGridView1 = new System.Windows.Forms.DataGridView();
            this.LabelFormTitle = new System.Windows.Forms.Label();
            this.mlUserInfo = new MetroFramework.Controls.MetroLabel();
            this.LabelFormHeaderText = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.GroupBoxActionCodes.SuspendLayout();
            this.PanelDateRanges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelRecordCount
            // 
            this.LabelRecordCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelRecordCount.Location = new System.Drawing.Point(894, 43);
            this.LabelRecordCount.Name = "LabelRecordCount";
            this.LabelRecordCount.Size = new System.Drawing.Size(279, 30);
            this.LabelRecordCount.TabIndex = 24;
            this.LabelRecordCount.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(22, 77);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1155, 796);
            this.tabControl1.TabIndex = 23;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.RoyalBlue;
            this.tabPage1.Controls.Add(this.ButtonClearAll);
            this.tabPage1.Controls.Add(this.ButtonCheckAll);
            this.tabPage1.Controls.Add(this.GroupBoxActionCodes);
            this.tabPage1.Controls.Add(this.PanelDateRanges);
            this.tabPage1.Controls.Add(this.LabelFindDescription);
            this.tabPage1.Controls.Add(this.TextBoxFind);
            this.tabPage1.Controls.Add(this.MButtonClose);
            this.tabPage1.Controls.Add(this.MBHistoryTransmitSelected);
            this.tabPage1.Controls.Add(this.MBSaveHistory);
            this.tabPage1.Controls.Add(this.MButtonRun);
            this.tabPage1.Controls.Add(this.DataGridView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Size = new System.Drawing.Size(1147, 770);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Listing";
            // 
            // ButtonClearAll
            // 
            this.ButtonClearAll.Location = new System.Drawing.Point(112, 99);
            this.ButtonClearAll.Name = "ButtonClearAll";
            this.ButtonClearAll.Size = new System.Drawing.Size(96, 23);
            this.ButtonClearAll.TabIndex = 23;
            this.ButtonClearAll.Text = "Clear All";
            this.ButtonClearAll.UseVisualStyleBackColor = true;
            this.ButtonClearAll.Click += new System.EventHandler(this.ButtonClearAll_Click);
            // 
            // ButtonCheckAll
            // 
            this.ButtonCheckAll.Location = new System.Drawing.Point(10, 99);
            this.ButtonCheckAll.Name = "ButtonCheckAll";
            this.ButtonCheckAll.Size = new System.Drawing.Size(96, 23);
            this.ButtonCheckAll.TabIndex = 23;
            this.ButtonCheckAll.Text = "Check All";
            this.ButtonCheckAll.UseVisualStyleBackColor = true;
            this.ButtonCheckAll.Click += new System.EventHandler(this.ButtonCheckAll_Click);
            // 
            // GroupBoxActionCodes
            // 
            this.GroupBoxActionCodes.Controls.Add(this.CheckedListBoxActionCodes);
            this.GroupBoxActionCodes.Location = new System.Drawing.Point(11, 126);
            this.GroupBoxActionCodes.Name = "GroupBoxActionCodes";
            this.GroupBoxActionCodes.Size = new System.Drawing.Size(194, 624);
            this.GroupBoxActionCodes.TabIndex = 22;
            this.GroupBoxActionCodes.TabStop = false;
            this.GroupBoxActionCodes.Text = "Action Codes";
            // 
            // CheckedListBoxActionCodes
            // 
            this.CheckedListBoxActionCodes.CheckOnClick = true;
            this.CheckedListBoxActionCodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CheckedListBoxActionCodes.FormattingEnabled = true;
            this.CheckedListBoxActionCodes.Location = new System.Drawing.Point(3, 16);
            this.CheckedListBoxActionCodes.Name = "CheckedListBoxActionCodes";
            this.CheckedListBoxActionCodes.Size = new System.Drawing.Size(188, 605);
            this.CheckedListBoxActionCodes.TabIndex = 0;
            this.CheckedListBoxActionCodes.ThreeDCheckBoxes = true;
            // 
            // PanelDateRanges
            // 
            this.PanelDateRanges.Controls.Add(this.LabelTo);
            this.PanelDateRanges.Controls.Add(this.DateTimePickerTo);
            this.PanelDateRanges.Controls.Add(this.LabelFrom);
            this.PanelDateRanges.Controls.Add(this.DateTimePickerFrom);
            this.PanelDateRanges.Controls.Add(this.RadioButtonDateRange);
            this.PanelDateRanges.Controls.Add(this.RadioButtonMonth);
            this.PanelDateRanges.Controls.Add(this.RadioButtonWeek);
            this.PanelDateRanges.Controls.Add(this.RadioButtonToday);
            this.PanelDateRanges.Location = new System.Drawing.Point(6, 8);
            this.PanelDateRanges.Name = "PanelDateRanges";
            this.PanelDateRanges.Size = new System.Drawing.Size(477, 80);
            this.PanelDateRanges.TabIndex = 21;
            // 
            // LabelTo
            // 
            this.LabelTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTo.Location = new System.Drawing.Point(234, 46);
            this.LabelTo.Name = "LabelTo";
            this.LabelTo.Size = new System.Drawing.Size(70, 18);
            this.LabelTo.TabIndex = 24;
            this.LabelTo.Text = "To";
            this.LabelTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DateTimePickerTo
            // 
            this.DateTimePickerTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateTimePickerTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.DateTimePickerTo.Location = new System.Drawing.Point(310, 46);
            this.DateTimePickerTo.Name = "DateTimePickerTo";
            this.DateTimePickerTo.Size = new System.Drawing.Size(147, 21);
            this.DateTimePickerTo.TabIndex = 23;
            this.DateTimePickerTo.Enter += new System.EventHandler(this.DateTimePickerTo_Enter);
            // 
            // LabelFrom
            // 
            this.LabelFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFrom.Location = new System.Drawing.Point(234, 14);
            this.LabelFrom.Name = "LabelFrom";
            this.LabelFrom.Size = new System.Drawing.Size(70, 18);
            this.LabelFrom.TabIndex = 22;
            this.LabelFrom.Text = "From";
            this.LabelFrom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DateTimePickerFrom
            // 
            this.DateTimePickerFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateTimePickerFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.DateTimePickerFrom.Location = new System.Drawing.Point(310, 14);
            this.DateTimePickerFrom.Name = "DateTimePickerFrom";
            this.DateTimePickerFrom.Size = new System.Drawing.Size(147, 21);
            this.DateTimePickerFrom.TabIndex = 21;
            this.DateTimePickerFrom.Value = new System.DateTime(2018, 1, 1, 5, 20, 0, 0);
            this.DateTimePickerFrom.Enter += new System.EventHandler(this.DateTimePickerFrom_Enter);
            // 
            // RadioButtonDateRange
            // 
            this.RadioButtonDateRange.Checked = true;
            this.RadioButtonDateRange.Location = new System.Drawing.Point(122, 47);
            this.RadioButtonDateRange.Name = "RadioButtonDateRange";
            this.RadioButtonDateRange.Size = new System.Drawing.Size(100, 18);
            this.RadioButtonDateRange.TabIndex = 0;
            this.RadioButtonDateRange.TabStop = true;
            this.RadioButtonDateRange.Text = "Date Range";
            this.RadioButtonDateRange.UseVisualStyleBackColor = true;
            this.RadioButtonDateRange.CheckedChanged += new System.EventHandler(this.RadioButtonDateRange_CheckedChanged);
            // 
            // RadioButtonMonth
            // 
            this.RadioButtonMonth.Location = new System.Drawing.Point(122, 14);
            this.RadioButtonMonth.Name = "RadioButtonMonth";
            this.RadioButtonMonth.Size = new System.Drawing.Size(100, 18);
            this.RadioButtonMonth.TabIndex = 0;
            this.RadioButtonMonth.Text = "Month";
            this.RadioButtonMonth.UseVisualStyleBackColor = true;
            this.RadioButtonMonth.CheckedChanged += new System.EventHandler(this.RadioButtonMonth_CheckedChanged);
            // 
            // RadioButtonWeek
            // 
            this.RadioButtonWeek.Location = new System.Drawing.Point(8, 47);
            this.RadioButtonWeek.Name = "RadioButtonWeek";
            this.RadioButtonWeek.Size = new System.Drawing.Size(100, 18);
            this.RadioButtonWeek.TabIndex = 0;
            this.RadioButtonWeek.Text = "Week";
            this.RadioButtonWeek.UseVisualStyleBackColor = true;
            this.RadioButtonWeek.CheckedChanged += new System.EventHandler(this.RadioButtonWeek_CheckedChanged);
            // 
            // RadioButtonToday
            // 
            this.RadioButtonToday.Location = new System.Drawing.Point(8, 14);
            this.RadioButtonToday.Name = "RadioButtonToday";
            this.RadioButtonToday.Size = new System.Drawing.Size(100, 18);
            this.RadioButtonToday.TabIndex = 0;
            this.RadioButtonToday.Text = "Today";
            this.RadioButtonToday.UseVisualStyleBackColor = true;
            this.RadioButtonToday.CheckedChanged += new System.EventHandler(this.RadioButtonToday_CheckedChanged);
            // 
            // LabelFindDescription
            // 
            this.LabelFindDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFindDescription.Location = new System.Drawing.Point(500, 14);
            this.LabelFindDescription.Name = "LabelFindDescription";
            this.LabelFindDescription.Size = new System.Drawing.Size(198, 27);
            this.LabelFindDescription.TabIndex = 18;
            this.LabelFindDescription.Text = "Search For Part of Order or Item";
            this.LabelFindDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxFind
            // 
            this.TextBoxFind.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxFind.Location = new System.Drawing.Point(704, 10);
            this.TextBoxFind.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBoxFind.Name = "TextBoxFind";
            this.TextBoxFind.Size = new System.Drawing.Size(145, 29);
            this.TextBoxFind.TabIndex = 17;
            this.TextBoxFind.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MButtonClose
            // 
            this.MButtonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MButtonClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonClose.Location = new System.Drawing.Point(1004, 8);
            this.MButtonClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonClose.Name = "MButtonClose";
            this.MButtonClose.Size = new System.Drawing.Size(136, 76);
            this.MButtonClose.TabIndex = 12;
            this.MButtonClose.Text = "Close";
            this.MButtonClose.UseSelectable = true;
            // 
            // MBHistoryTransmitSelected
            // 
            this.MBHistoryTransmitSelected.Enabled = false;
            this.MBHistoryTransmitSelected.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBHistoryTransmitSelected.Location = new System.Drawing.Point(489, 47);
            this.MBHistoryTransmitSelected.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBHistoryTransmitSelected.Name = "MBHistoryTransmitSelected";
            this.MBHistoryTransmitSelected.Size = new System.Drawing.Size(206, 37);
            this.MBHistoryTransmitSelected.TabIndex = 12;
            this.MBHistoryTransmitSelected.Text = "Transmit Selected";
            this.MBHistoryTransmitSelected.UseSelectable = true;
            this.MBHistoryTransmitSelected.Visible = false;
            this.MBHistoryTransmitSelected.Click += new System.EventHandler(this.MBSaveHistory_Click);
            // 
            // MBSaveHistory
            // 
            this.MBSaveHistory.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBSaveHistory.Location = new System.Drawing.Point(704, 47);
            this.MBSaveHistory.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBSaveHistory.Name = "MBSaveHistory";
            this.MBSaveHistory.Size = new System.Drawing.Size(145, 37);
            this.MBSaveHistory.TabIndex = 12;
            this.MBSaveHistory.Text = "Save to File";
            this.MBSaveHistory.UseSelectable = true;
            this.MBSaveHistory.Click += new System.EventHandler(this.MBSaveHistory_Click);
            // 
            // MButtonRun
            // 
            this.MButtonRun.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonRun.Location = new System.Drawing.Point(859, 8);
            this.MButtonRun.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonRun.Name = "MButtonRun";
            this.MButtonRun.Size = new System.Drawing.Size(136, 76);
            this.MButtonRun.TabIndex = 12;
            this.MButtonRun.Text = "Run";
            this.MButtonRun.UseSelectable = true;
            this.MButtonRun.Click += new System.EventHandler(this.MButtonRun_Click);
            // 
            // DataGridView1
            // 
            this.DataGridView1.AllowUserToAddRows = false;
            this.DataGridView1.AllowUserToDeleteRows = false;
            this.DataGridView1.AllowUserToOrderColumns = true;
            this.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView1.Location = new System.Drawing.Point(211, 126);
            this.DataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView1.Name = "DataGridView1";
            this.DataGridView1.ReadOnly = true;
            this.DataGridView1.RowTemplate.Height = 24;
            this.DataGridView1.Size = new System.Drawing.Size(930, 624);
            this.DataGridView1.TabIndex = 0;
            // 
            // LabelFormTitle
            // 
            this.LabelFormTitle.BackColor = System.Drawing.Color.RoyalBlue;
            this.LabelFormTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelFormTitle.Font = new System.Drawing.Font("Comic Sans MS", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormTitle.ForeColor = System.Drawing.Color.Black;
            this.LabelFormTitle.Location = new System.Drawing.Point(493, 10);
            this.LabelFormTitle.Name = "LabelFormTitle";
            this.LabelFormTitle.Size = new System.Drawing.Size(338, 62);
            this.LabelFormTitle.TabIndex = 22;
            this.LabelFormTitle.Text = "History";
            this.LabelFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mlUserInfo
            // 
            this.mlUserInfo.Location = new System.Drawing.Point(846, 10);
            this.mlUserInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlUserInfo.Name = "mlUserInfo";
            this.mlUserInfo.Size = new System.Drawing.Size(327, 30);
            this.mlUserInfo.TabIndex = 21;
            this.mlUserInfo.Text = "Login";
            this.mlUserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelFormHeaderText
            // 
            this.LabelFormHeaderText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelFormHeaderText.Font = new System.Drawing.Font("Comic Sans MS", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormHeaderText.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LabelFormHeaderText.Location = new System.Drawing.Point(27, 10);
            this.LabelFormHeaderText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelFormHeaderText.Name = "LabelFormHeaderText";
            this.LabelFormHeaderText.Size = new System.Drawing.Size(452, 62);
            this.LabelFormHeaderText.TabIndex = 20;
            this.LabelFormHeaderText.Text = "Neutron Warehouse Management";
            this.LabelFormHeaderText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FrmHistory
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1200, 860);
            this.Controls.Add(this.LabelRecordCount);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.LabelFormTitle);
            this.Controls.Add(this.mlUserInfo);
            this.Controls.Add(this.LabelFormHeaderText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmHistory";
            this.Text = "History";
            this.Load += new System.EventHandler(this.FrmHistory_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.GroupBoxActionCodes.ResumeLayout(false);
            this.PanelDateRanges.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LabelRecordCount;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label LabelFindDescription;
        private System.Windows.Forms.TextBox TextBoxFind;
        private MetroFramework.Controls.MetroButton MButtonClose;
        private MetroFramework.Controls.MetroButton MButtonRun;
        private System.Windows.Forms.DataGridView DataGridView1;
        private System.Windows.Forms.Label LabelFormTitle;
        private MetroFramework.Controls.MetroLabel mlUserInfo;
        private System.Windows.Forms.Label LabelFormHeaderText;
        private System.Windows.Forms.Panel PanelDateRanges;
        private System.Windows.Forms.RadioButton RadioButtonDateRange;
        private System.Windows.Forms.RadioButton RadioButtonMonth;
        private System.Windows.Forms.RadioButton RadioButtonWeek;
        private System.Windows.Forms.RadioButton RadioButtonToday;
        private System.Windows.Forms.Label LabelTo;
        private System.Windows.Forms.DateTimePicker DateTimePickerTo;
        private System.Windows.Forms.Label LabelFrom;
        private System.Windows.Forms.DateTimePicker DateTimePickerFrom;
        private MetroFramework.Controls.MetroButton MBSaveHistory;
        private System.Windows.Forms.GroupBox GroupBoxActionCodes;
        private System.Windows.Forms.CheckedListBox CheckedListBoxActionCodes;
        private System.Windows.Forms.Button ButtonClearAll;
        private System.Windows.Forms.Button ButtonCheckAll;
        private MetroFramework.Controls.MetroButton MBHistoryTransmitSelected;
    }
}