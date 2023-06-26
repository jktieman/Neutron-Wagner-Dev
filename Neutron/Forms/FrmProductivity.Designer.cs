namespace Neutron.Forms
{
    partial class FrmProductivity
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
            this.LabelRecordCount = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.ButtonConfigureUsers = new System.Windows.Forms.Button();
            this.ButtonClearAllUsers = new System.Windows.Forms.Button();
            this.ButtonCheckAllUsers = new System.Windows.Forms.Button();
            this.CheckedListBoxGroups = new System.Windows.Forms.CheckedListBox();
            this.CheckedListBoxUsers = new System.Windows.Forms.CheckedListBox();
            this.ButtonConfigureActions = new System.Windows.Forms.Button();
            this.ButtonClearAllActions = new System.Windows.Forms.Button();
            this.ButtonCheckAllActions = new System.Windows.Forms.Button();
            this.CheckedListBoxActionCodes = new System.Windows.Forms.CheckedListBox();
            this.SplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.TextBoxTotalOrdersSummary = new System.Windows.Forms.TextBox();
            this.TextBoxTotalPiecesSummary = new System.Windows.Forms.TextBox();
            this.TextBoxTotalLinesSummary = new System.Windows.Forms.TextBox();
            this.ButtonPrintSummary = new System.Windows.Forms.Button();
            this.LabelTotalOrders = new System.Windows.Forms.Label();
            this.LabelTotalPieces = new System.Windows.Forms.Label();
            this.LabelTotalLines = new System.Windows.Forms.Label();
            this.DataGridView1 = new System.Windows.Forms.DataGridView();
            this.TextBoxTotalOrdersDetail = new System.Windows.Forms.TextBox();
            this.TextBoxTotalPiecesDetail = new System.Windows.Forms.TextBox();
            this.TextBoxTotalLinesDetail = new System.Windows.Forms.TextBox();
            this.ButtonPrintDetail = new System.Windows.Forms.Button();
            this.LabelTotalOrdersDetail = new System.Windows.Forms.Label();
            this.LabelTotalPiecesDetail = new System.Windows.Forms.Label();
            this.LabelTotalLinesDetail = new System.Windows.Forms.Label();
            this.DataGridView2 = new System.Windows.Forms.DataGridView();
            this.PanelDateRanges = new System.Windows.Forms.Panel();
            this.LabelTo = new System.Windows.Forms.Label();
            this.DateTimePickerTo = new System.Windows.Forms.DateTimePicker();
            this.LabelFrom = new System.Windows.Forms.Label();
            this.DateTimePickerFrom = new System.Windows.Forms.DateTimePicker();
            this.RadioButtonDateRange = new System.Windows.Forms.RadioButton();
            this.RadioButtonMonth = new System.Windows.Forms.RadioButton();
            this.RadioButtonWeek = new System.Windows.Forms.RadioButton();
            this.RadioButtonToday = new System.Windows.Forms.RadioButton();
            this.MButtonClose = new MetroFramework.Controls.MetroButton();
            this.MBSaveDetail = new MetroFramework.Controls.MetroButton();
            this.MBSaveSummary = new MetroFramework.Controls.MetroButton();
            this.MButtonRun = new MetroFramework.Controls.MetroButton();
            this.LabelFormTitle = new System.Windows.Forms.Label();
            this.mlUserInfo = new MetroFramework.Controls.MetroLabel();
            this.LabelFormHeaderText = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView2)).BeginInit();
            this.PanelDateRanges.SuspendLayout();
            this.SuspendLayout();
            // 
            // LabelRecordCount
            // 
            this.LabelRecordCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelRecordCount.Location = new System.Drawing.Point(986, 40);
            this.LabelRecordCount.Name = "LabelRecordCount";
            this.LabelRecordCount.Size = new System.Drawing.Size(279, 30);
            this.LabelRecordCount.TabIndex = 29;
            this.LabelRecordCount.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(22, 77);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1247, 665);
            this.tabControl1.TabIndex = 28;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.RoyalBlue;
            this.tabPage1.Controls.Add(this.splitContainer2);
            this.tabPage1.Controls.Add(this.SplitContainer1);
            this.tabPage1.Controls.Add(this.PanelDateRanges);
            this.tabPage1.Controls.Add(this.MButtonClose);
            this.tabPage1.Controls.Add(this.MBSaveDetail);
            this.tabPage1.Controls.Add(this.MBSaveSummary);
            this.tabPage1.Controls.Add(this.MButtonRun);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Size = new System.Drawing.Size(1239, 639);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Listing";
            // 
            // splitContainer2
            // 
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(3, 99);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.splitContainer2.Panel1.Controls.Add(this.ButtonConfigureUsers);
            this.splitContainer2.Panel1.Controls.Add(this.ButtonClearAllUsers);
            this.splitContainer2.Panel1.Controls.Add(this.ButtonCheckAllUsers);
            this.splitContainer2.Panel1.Controls.Add(this.CheckedListBoxGroups);
            this.splitContainer2.Panel1.Controls.Add(this.CheckedListBoxUsers);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.Color.AliceBlue;
            this.splitContainer2.Panel2.Controls.Add(this.ButtonConfigureActions);
            this.splitContainer2.Panel2.Controls.Add(this.ButtonClearAllActions);
            this.splitContainer2.Panel2.Controls.Add(this.ButtonCheckAllActions);
            this.splitContainer2.Panel2.Controls.Add(this.CheckedListBoxActionCodes);
            this.splitContainer2.Size = new System.Drawing.Size(173, 540);
            this.splitContainer2.SplitterDistance = 306;
            this.splitContainer2.TabIndex = 27;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer2_SplitterMoved);
            // 
            // ButtonConfigureUsers
            // 
            this.ButtonConfigureUsers.Location = new System.Drawing.Point(6, 275);
            this.ButtonConfigureUsers.Name = "ButtonConfigureUsers";
            this.ButtonConfigureUsers.Size = new System.Drawing.Size(160, 23);
            this.ButtonConfigureUsers.TabIndex = 0;
            this.ButtonConfigureUsers.Text = "Configure Users";
            this.ButtonConfigureUsers.UseVisualStyleBackColor = true;
            this.ButtonConfigureUsers.Click += new System.EventHandler(this.ButtonConfigureUsers_Click);
            // 
            // ButtonClearAllUsers
            // 
            this.ButtonClearAllUsers.Location = new System.Drawing.Point(90, 247);
            this.ButtonClearAllUsers.Name = "ButtonClearAllUsers";
            this.ButtonClearAllUsers.Size = new System.Drawing.Size(80, 23);
            this.ButtonClearAllUsers.TabIndex = 4;
            this.ButtonClearAllUsers.Text = "Clear All";
            this.ButtonClearAllUsers.UseVisualStyleBackColor = true;
            this.ButtonClearAllUsers.Click += new System.EventHandler(this.ButtonClearAllUsers_Click);
            // 
            // ButtonCheckAllUsers
            // 
            this.ButtonCheckAllUsers.Location = new System.Drawing.Point(3, 247);
            this.ButtonCheckAllUsers.Name = "ButtonCheckAllUsers";
            this.ButtonCheckAllUsers.Size = new System.Drawing.Size(80, 23);
            this.ButtonCheckAllUsers.TabIndex = 3;
            this.ButtonCheckAllUsers.Text = "Check All";
            this.ButtonCheckAllUsers.UseVisualStyleBackColor = true;
            this.ButtonCheckAllUsers.Click += new System.EventHandler(this.ButtonCheckAllUsers_Click);
            // 
            // CheckedListBoxGroups
            // 
            this.CheckedListBoxGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckedListBoxGroups.CheckOnClick = true;
            this.CheckedListBoxGroups.FormattingEnabled = true;
            this.CheckedListBoxGroups.Location = new System.Drawing.Point(0, 3);
            this.CheckedListBoxGroups.Name = "CheckedListBoxGroups";
            this.CheckedListBoxGroups.Size = new System.Drawing.Size(173, 109);
            this.CheckedListBoxGroups.TabIndex = 2;
            this.CheckedListBoxGroups.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxGroups_ItemCheck);
            // 
            // CheckedListBoxUsers
            // 
            this.CheckedListBoxUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckedListBoxUsers.CheckOnClick = true;
            this.CheckedListBoxUsers.FormattingEnabled = true;
            this.CheckedListBoxUsers.Location = new System.Drawing.Point(0, 117);
            this.CheckedListBoxUsers.Name = "CheckedListBoxUsers";
            this.CheckedListBoxUsers.Size = new System.Drawing.Size(173, 124);
            this.CheckedListBoxUsers.TabIndex = 1;
            this.CheckedListBoxUsers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxUsers_ItemCheck);
            // 
            // ButtonConfigureActions
            // 
            this.ButtonConfigureActions.Location = new System.Drawing.Point(6, 196);
            this.ButtonConfigureActions.Name = "ButtonConfigureActions";
            this.ButtonConfigureActions.Size = new System.Drawing.Size(160, 23);
            this.ButtonConfigureActions.TabIndex = 0;
            this.ButtonConfigureActions.Text = "Configure Actions";
            this.ButtonConfigureActions.UseVisualStyleBackColor = true;
            this.ButtonConfigureActions.Click += new System.EventHandler(this.ButtonConfigureActions_Click);
            // 
            // ButtonClearAllActions
            // 
            this.ButtonClearAllActions.Location = new System.Drawing.Point(90, 168);
            this.ButtonClearAllActions.Name = "ButtonClearAllActions";
            this.ButtonClearAllActions.Size = new System.Drawing.Size(80, 23);
            this.ButtonClearAllActions.TabIndex = 3;
            this.ButtonClearAllActions.Text = "Clear All";
            this.ButtonClearAllActions.UseVisualStyleBackColor = true;
            this.ButtonClearAllActions.Click += new System.EventHandler(this.ButtonClearAllActions_Click);
            // 
            // ButtonCheckAllActions
            // 
            this.ButtonCheckAllActions.Location = new System.Drawing.Point(3, 168);
            this.ButtonCheckAllActions.Name = "ButtonCheckAllActions";
            this.ButtonCheckAllActions.Size = new System.Drawing.Size(80, 23);
            this.ButtonCheckAllActions.TabIndex = 2;
            this.ButtonCheckAllActions.Text = "Check All";
            this.ButtonCheckAllActions.UseVisualStyleBackColor = true;
            this.ButtonCheckAllActions.Click += new System.EventHandler(this.ButtonCheckAllActions_Click);
            // 
            // CheckedListBoxActionCodes
            // 
            this.CheckedListBoxActionCodes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckedListBoxActionCodes.CheckOnClick = true;
            this.CheckedListBoxActionCodes.FormattingEnabled = true;
            this.CheckedListBoxActionCodes.Location = new System.Drawing.Point(0, 3);
            this.CheckedListBoxActionCodes.Name = "CheckedListBoxActionCodes";
            this.CheckedListBoxActionCodes.Size = new System.Drawing.Size(173, 154);
            this.CheckedListBoxActionCodes.TabIndex = 1;
            this.CheckedListBoxActionCodes.ThreeDCheckBoxes = true;
            this.CheckedListBoxActionCodes.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxActionCodes_ItemCheck);
            // 
            // SplitContainer1
            // 
            this.SplitContainer1.Location = new System.Drawing.Point(179, 99);
            this.SplitContainer1.Name = "SplitContainer1";
            this.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer1.Panel1
            // 
            this.SplitContainer1.Panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.SplitContainer1.Panel1.Controls.Add(this.TextBoxTotalOrdersSummary);
            this.SplitContainer1.Panel1.Controls.Add(this.TextBoxTotalPiecesSummary);
            this.SplitContainer1.Panel1.Controls.Add(this.TextBoxTotalLinesSummary);
            this.SplitContainer1.Panel1.Controls.Add(this.ButtonPrintSummary);
            this.SplitContainer1.Panel1.Controls.Add(this.LabelTotalOrders);
            this.SplitContainer1.Panel1.Controls.Add(this.LabelTotalPieces);
            this.SplitContainer1.Panel1.Controls.Add(this.LabelTotalLines);
            this.SplitContainer1.Panel1.Controls.Add(this.DataGridView1);
            // 
            // SplitContainer1.Panel2
            // 
            this.SplitContainer1.Panel2.BackColor = System.Drawing.Color.White;
            this.SplitContainer1.Panel2.Controls.Add(this.TextBoxTotalOrdersDetail);
            this.SplitContainer1.Panel2.Controls.Add(this.TextBoxTotalPiecesDetail);
            this.SplitContainer1.Panel2.Controls.Add(this.TextBoxTotalLinesDetail);
            this.SplitContainer1.Panel2.Controls.Add(this.ButtonPrintDetail);
            this.SplitContainer1.Panel2.Controls.Add(this.LabelTotalOrdersDetail);
            this.SplitContainer1.Panel2.Controls.Add(this.LabelTotalPiecesDetail);
            this.SplitContainer1.Panel2.Controls.Add(this.LabelTotalLinesDetail);
            this.SplitContainer1.Panel2.Controls.Add(this.DataGridView2);
            this.SplitContainer1.Size = new System.Drawing.Size(1062, 540);
            this.SplitContainer1.SplitterDistance = 238;
            this.SplitContainer1.TabIndex = 26;
            this.SplitContainer1.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.SplitContainer1_SplitterMoving);
            this.SplitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer1_SplitterMoved);
            // 
            // TextBoxTotalOrdersSummary
            // 
            this.TextBoxTotalOrdersSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxTotalOrdersSummary.Location = new System.Drawing.Point(744, 204);
            this.TextBoxTotalOrdersSummary.Name = "TextBoxTotalOrdersSummary";
            this.TextBoxTotalOrdersSummary.ReadOnly = true;
            this.TextBoxTotalOrdersSummary.Size = new System.Drawing.Size(100, 22);
            this.TextBoxTotalOrdersSummary.TabIndex = 4;
            this.TextBoxTotalOrdersSummary.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxTotalPiecesSummary
            // 
            this.TextBoxTotalPiecesSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxTotalPiecesSummary.Location = new System.Drawing.Point(477, 204);
            this.TextBoxTotalPiecesSummary.Name = "TextBoxTotalPiecesSummary";
            this.TextBoxTotalPiecesSummary.ReadOnly = true;
            this.TextBoxTotalPiecesSummary.Size = new System.Drawing.Size(100, 22);
            this.TextBoxTotalPiecesSummary.TabIndex = 4;
            this.TextBoxTotalPiecesSummary.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxTotalLinesSummary
            // 
            this.TextBoxTotalLinesSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxTotalLinesSummary.Location = new System.Drawing.Point(210, 205);
            this.TextBoxTotalLinesSummary.Name = "TextBoxTotalLinesSummary";
            this.TextBoxTotalLinesSummary.ReadOnly = true;
            this.TextBoxTotalLinesSummary.Size = new System.Drawing.Size(100, 22);
            this.TextBoxTotalLinesSummary.TabIndex = 4;
            this.TextBoxTotalLinesSummary.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ButtonPrintSummary
            // 
            this.ButtonPrintSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonPrintSummary.Location = new System.Drawing.Point(859, 202);
            this.ButtonPrintSummary.Name = "ButtonPrintSummary";
            this.ButtonPrintSummary.Size = new System.Drawing.Size(142, 23);
            this.ButtonPrintSummary.TabIndex = 1;
            this.ButtonPrintSummary.Text = "Print Summary";
            this.ButtonPrintSummary.UseVisualStyleBackColor = true;
            this.ButtonPrintSummary.Click += new System.EventHandler(this.ButtonPrintSummary_Click);
            // 
            // LabelTotalOrders
            // 
            this.LabelTotalOrders.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTotalOrders.Location = new System.Drawing.Point(593, 207);
            this.LabelTotalOrders.Name = "LabelTotalOrders";
            this.LabelTotalOrders.Size = new System.Drawing.Size(135, 16);
            this.LabelTotalOrders.TabIndex = 2;
            this.LabelTotalOrders.Text = "Total Orders";
            this.LabelTotalOrders.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelTotalPieces
            // 
            this.LabelTotalPieces.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTotalPieces.Location = new System.Drawing.Point(326, 207);
            this.LabelTotalPieces.Name = "LabelTotalPieces";
            this.LabelTotalPieces.Size = new System.Drawing.Size(135, 16);
            this.LabelTotalPieces.TabIndex = 2;
            this.LabelTotalPieces.Text = "Total Pieces";
            this.LabelTotalPieces.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelTotalLines
            // 
            this.LabelTotalLines.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTotalLines.Location = new System.Drawing.Point(59, 207);
            this.LabelTotalLines.Name = "LabelTotalLines";
            this.LabelTotalLines.Size = new System.Drawing.Size(135, 16);
            this.LabelTotalLines.TabIndex = 2;
            this.LabelTotalLines.Text = "Total Lines";
            this.LabelTotalLines.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DataGridView1
            // 
            this.DataGridView1.AllowUserToAddRows = false;
            this.DataGridView1.AllowUserToDeleteRows = false;
            this.DataGridView1.AllowUserToOrderColumns = true;
            this.DataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView1.Location = new System.Drawing.Point(0, 0);
            this.DataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView1.Name = "DataGridView1";
            this.DataGridView1.ReadOnly = true;
            this.DataGridView1.RowTemplate.Height = 24;
            this.DataGridView1.Size = new System.Drawing.Size(1052, 195);
            this.DataGridView1.TabIndex = 0;
            this.DataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellContentClick);
            this.DataGridView1.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_RowEnter);
            // 
            // TextBoxTotalOrdersDetail
            // 
            this.TextBoxTotalOrdersDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxTotalOrdersDetail.Location = new System.Drawing.Point(744, 252);
            this.TextBoxTotalOrdersDetail.Name = "TextBoxTotalOrdersDetail";
            this.TextBoxTotalOrdersDetail.ReadOnly = true;
            this.TextBoxTotalOrdersDetail.Size = new System.Drawing.Size(100, 22);
            this.TextBoxTotalOrdersDetail.TabIndex = 30;
            this.TextBoxTotalOrdersDetail.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxTotalPiecesDetail
            // 
            this.TextBoxTotalPiecesDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxTotalPiecesDetail.Location = new System.Drawing.Point(477, 253);
            this.TextBoxTotalPiecesDetail.Name = "TextBoxTotalPiecesDetail";
            this.TextBoxTotalPiecesDetail.ReadOnly = true;
            this.TextBoxTotalPiecesDetail.Size = new System.Drawing.Size(100, 22);
            this.TextBoxTotalPiecesDetail.TabIndex = 31;
            this.TextBoxTotalPiecesDetail.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxTotalLinesDetail
            // 
            this.TextBoxTotalLinesDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxTotalLinesDetail.Location = new System.Drawing.Point(210, 253);
            this.TextBoxTotalLinesDetail.Name = "TextBoxTotalLinesDetail";
            this.TextBoxTotalLinesDetail.ReadOnly = true;
            this.TextBoxTotalLinesDetail.Size = new System.Drawing.Size(100, 22);
            this.TextBoxTotalLinesDetail.TabIndex = 32;
            this.TextBoxTotalLinesDetail.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ButtonPrintDetail
            // 
            this.ButtonPrintDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonPrintDetail.Location = new System.Drawing.Point(859, 251);
            this.ButtonPrintDetail.Name = "ButtonPrintDetail";
            this.ButtonPrintDetail.Size = new System.Drawing.Size(142, 23);
            this.ButtonPrintDetail.TabIndex = 1;
            this.ButtonPrintDetail.Text = "Print Detail";
            this.ButtonPrintDetail.UseVisualStyleBackColor = true;
            this.ButtonPrintDetail.Click += new System.EventHandler(this.ButtonPrintDetail_Click);
            // 
            // LabelTotalOrdersDetail
            // 
            this.LabelTotalOrdersDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTotalOrdersDetail.Location = new System.Drawing.Point(593, 254);
            this.LabelTotalOrdersDetail.Name = "LabelTotalOrdersDetail";
            this.LabelTotalOrdersDetail.Size = new System.Drawing.Size(135, 16);
            this.LabelTotalOrdersDetail.TabIndex = 26;
            this.LabelTotalOrdersDetail.Text = "Total Orders";
            this.LabelTotalOrdersDetail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelTotalPiecesDetail
            // 
            this.LabelTotalPiecesDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTotalPiecesDetail.Location = new System.Drawing.Point(326, 254);
            this.LabelTotalPiecesDetail.Name = "LabelTotalPiecesDetail";
            this.LabelTotalPiecesDetail.Size = new System.Drawing.Size(135, 16);
            this.LabelTotalPiecesDetail.TabIndex = 27;
            this.LabelTotalPiecesDetail.Text = "Total Pieces";
            this.LabelTotalPiecesDetail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelTotalLinesDetail
            // 
            this.LabelTotalLinesDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTotalLinesDetail.Location = new System.Drawing.Point(59, 256);
            this.LabelTotalLinesDetail.Name = "LabelTotalLinesDetail";
            this.LabelTotalLinesDetail.Size = new System.Drawing.Size(135, 16);
            this.LabelTotalLinesDetail.TabIndex = 28;
            this.LabelTotalLinesDetail.Text = "Total Lines";
            this.LabelTotalLinesDetail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DataGridView2
            // 
            this.DataGridView2.AllowUserToAddRows = false;
            this.DataGridView2.AllowUserToDeleteRows = false;
            this.DataGridView2.AllowUserToResizeColumns = false;
            this.DataGridView2.AllowUserToResizeRows = false;
            this.DataGridView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView2.Location = new System.Drawing.Point(0, 5);
            this.DataGridView2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView2.MultiSelect = false;
            this.DataGridView2.Name = "DataGridView2";
            this.DataGridView2.ReadOnly = true;
            this.DataGridView2.RowTemplate.Height = 24;
            this.DataGridView2.Size = new System.Drawing.Size(1052, 229);
            this.DataGridView2.TabIndex = 0;
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
            this.DateTimePickerTo.TabIndex = 5;
            this.DateTimePickerTo.ValueChanged += new System.EventHandler(this.DateTimePickerTo_ValueChanged);
            this.DateTimePickerTo.Enter += new System.EventHandler(this.DateTimePicker_Enter);
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
            this.DateTimePickerFrom.TabIndex = 4;
            this.DateTimePickerFrom.Value = new System.DateTime(2020, 1, 1, 0, 0, 0, 0);
            this.DateTimePickerFrom.ValueChanged += new System.EventHandler(this.DateTimePickerFrom_ValueChanged);
            this.DateTimePickerFrom.Enter += new System.EventHandler(this.DateTimePicker_Enter);
            // 
            // RadioButtonDateRange
            // 
            this.RadioButtonDateRange.Checked = true;
            this.RadioButtonDateRange.Location = new System.Drawing.Point(122, 47);
            this.RadioButtonDateRange.Name = "RadioButtonDateRange";
            this.RadioButtonDateRange.Size = new System.Drawing.Size(100, 18);
            this.RadioButtonDateRange.TabIndex = 3;
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
            this.RadioButtonMonth.TabIndex = 2;
            this.RadioButtonMonth.Text = "Month";
            this.RadioButtonMonth.UseVisualStyleBackColor = true;
            this.RadioButtonMonth.CheckedChanged += new System.EventHandler(this.RadioButtonMonth_CheckedChanged);
            // 
            // RadioButtonWeek
            // 
            this.RadioButtonWeek.Location = new System.Drawing.Point(8, 47);
            this.RadioButtonWeek.Name = "RadioButtonWeek";
            this.RadioButtonWeek.Size = new System.Drawing.Size(100, 18);
            this.RadioButtonWeek.TabIndex = 1;
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
            // MButtonClose
            // 
            this.MButtonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MButtonClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonClose.Location = new System.Drawing.Point(1095, 11);
            this.MButtonClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonClose.Name = "MButtonClose";
            this.MButtonClose.Size = new System.Drawing.Size(136, 80);
            this.MButtonClose.TabIndex = 3;
            this.MButtonClose.Text = "Close";
            this.MButtonClose.UseSelectable = true;
            // 
            // MBSaveDetail
            // 
            this.MBSaveDetail.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBSaveDetail.Location = new System.Drawing.Point(511, 56);
            this.MBSaveDetail.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBSaveDetail.Name = "MBSaveDetail";
            this.MBSaveDetail.Size = new System.Drawing.Size(245, 32);
            this.MBSaveDetail.TabIndex = 1;
            this.MBSaveDetail.Text = "Save Detail to File";
            this.MBSaveDetail.UseSelectable = true;
            this.MBSaveDetail.Click += new System.EventHandler(this.MBSaveDetail_Click);
            // 
            // MBSaveSummary
            // 
            this.MBSaveSummary.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBSaveSummary.Location = new System.Drawing.Point(511, 8);
            this.MBSaveSummary.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBSaveSummary.Name = "MBSaveSummary";
            this.MBSaveSummary.Size = new System.Drawing.Size(245, 32);
            this.MBSaveSummary.TabIndex = 0;
            this.MBSaveSummary.Text = "Save Summary to File";
            this.MBSaveSummary.UseSelectable = true;
            this.MBSaveSummary.Click += new System.EventHandler(this.MBSaveSummary_Click);
            // 
            // MButtonRun
            // 
            this.MButtonRun.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonRun.Location = new System.Drawing.Point(944, 11);
            this.MButtonRun.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonRun.Name = "MButtonRun";
            this.MButtonRun.Size = new System.Drawing.Size(136, 80);
            this.MButtonRun.TabIndex = 2;
            this.MButtonRun.Text = "Refresh";
            this.MButtonRun.UseSelectable = true;
            this.MButtonRun.Click += new System.EventHandler(this.MButtonRun_Click);
            // 
            // LabelFormTitle
            // 
            this.LabelFormTitle.BackColor = System.Drawing.Color.RoyalBlue;
            this.LabelFormTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelFormTitle.Font = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormTitle.ForeColor = System.Drawing.Color.Black;
            this.LabelFormTitle.Location = new System.Drawing.Point(493, 10);
            this.LabelFormTitle.Name = "LabelFormTitle";
            this.LabelFormTitle.Size = new System.Drawing.Size(350, 30);
            this.LabelFormTitle.TabIndex = 27;
            this.LabelFormTitle.Text = "Productivity";
            this.LabelFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mlUserInfo
            // 
            this.mlUserInfo.Location = new System.Drawing.Point(945, 10);
            this.mlUserInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlUserInfo.Name = "mlUserInfo";
            this.mlUserInfo.Size = new System.Drawing.Size(312, 30);
            this.mlUserInfo.TabIndex = 26;
            this.mlUserInfo.Text = "Login ?";
            this.mlUserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelFormHeaderText
            // 
            this.LabelFormHeaderText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelFormHeaderText.Font = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormHeaderText.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LabelFormHeaderText.Location = new System.Drawing.Point(27, 10);
            this.LabelFormHeaderText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelFormHeaderText.Name = "LabelFormHeaderText";
            this.LabelFormHeaderText.Size = new System.Drawing.Size(350, 30);
            this.LabelFormHeaderText.TabIndex = 25;
            this.LabelFormHeaderText.Text = "Neutron Warehouse Management";
            this.LabelFormHeaderText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FrmProductivity
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 750);
            this.Controls.Add(this.LabelRecordCount);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.LabelFormTitle);
            this.Controls.Add(this.mlUserInfo);
            this.Controls.Add(this.LabelFormHeaderText);
            this.Name = "FrmProductivity";
            this.Load += new System.EventHandler(this.FrmProductivity_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel1.PerformLayout();
            this.SplitContainer1.Panel2.ResumeLayout(false);
            this.SplitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
            this.SplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView2)).EndInit();
            this.PanelDateRanges.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LabelRecordCount;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel PanelDateRanges;
        private System.Windows.Forms.Label LabelTo;
        private System.Windows.Forms.DateTimePicker DateTimePickerTo;
        private System.Windows.Forms.Label LabelFrom;
        private System.Windows.Forms.DateTimePicker DateTimePickerFrom;
        private System.Windows.Forms.RadioButton RadioButtonDateRange;
        private System.Windows.Forms.RadioButton RadioButtonMonth;
        private System.Windows.Forms.RadioButton RadioButtonWeek;
        private System.Windows.Forms.RadioButton RadioButtonToday;
        private MetroFramework.Controls.MetroButton MButtonClose;
        private MetroFramework.Controls.MetroButton MBSaveSummary;
        private MetroFramework.Controls.MetroButton MButtonRun;
        private System.Windows.Forms.Label LabelFormTitle;
        private MetroFramework.Controls.MetroLabel mlUserInfo;
        private System.Windows.Forms.Label LabelFormHeaderText;
        private System.Windows.Forms.SplitContainer SplitContainer1;
        private System.Windows.Forms.DataGridView DataGridView1;
        private System.Windows.Forms.DataGridView DataGridView2;
        private System.Windows.Forms.TextBox TextBoxTotalOrdersSummary;
        private System.Windows.Forms.TextBox TextBoxTotalPiecesSummary;
        private System.Windows.Forms.TextBox TextBoxTotalLinesSummary;
        private System.Windows.Forms.Button ButtonPrintSummary;
        private System.Windows.Forms.Label LabelTotalOrders;
        private System.Windows.Forms.Label LabelTotalPieces;
        private System.Windows.Forms.Label LabelTotalLines;
        private System.Windows.Forms.TextBox TextBoxTotalOrdersDetail;
        private System.Windows.Forms.TextBox TextBoxTotalPiecesDetail;
        private System.Windows.Forms.TextBox TextBoxTotalLinesDetail;
        private System.Windows.Forms.Button ButtonPrintDetail;
        private System.Windows.Forms.Label LabelTotalOrdersDetail;
        private System.Windows.Forms.Label LabelTotalPiecesDetail;
        private System.Windows.Forms.Label LabelTotalLinesDetail;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button ButtonClearAllUsers;
        private System.Windows.Forms.Button ButtonCheckAllUsers;
        private System.Windows.Forms.CheckedListBox CheckedListBoxUsers;
        private System.Windows.Forms.Button ButtonClearAllActions;
        private System.Windows.Forms.Button ButtonCheckAllActions;
        private System.Windows.Forms.CheckedListBox CheckedListBoxActionCodes;
        private System.Windows.Forms.Button ButtonConfigureUsers;
        private System.Windows.Forms.Button ButtonConfigureActions;
        private MetroFramework.Controls.MetroButton MBSaveDetail;
        private System.Windows.Forms.CheckedListBox CheckedListBoxGroups;
    }
}