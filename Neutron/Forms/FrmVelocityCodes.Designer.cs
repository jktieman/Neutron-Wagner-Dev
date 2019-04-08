namespace Neutron.Forms
{
    partial class FrmVelocityCodes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmVelocityCodes));
            this.mlUserInfo = new MetroFramework.Controls.MetroLabel();
            this.LabelFormHeaderText = new System.Windows.Forms.Label();
            this.LabelFormTitle = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.LabelFindDescription = new System.Windows.Forms.Label();
            this.TextBoxFind = new System.Windows.Forms.TextBox();
            this.MButtonNew = new MetroFramework.Controls.MetroButton();
            this.MButtonViewEdit = new MetroFramework.Controls.MetroButton();
            this.ButtonClear = new System.Windows.Forms.Button();
            this.MButtonClose = new MetroFramework.Controls.MetroButton();
            this.MButtonSearch = new MetroFramework.Controls.MetroButton();
            this.DataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.LabelAction = new System.Windows.Forms.Label();
            this.MbViewEditListing = new MetroFramework.Controls.MetroButton();
            this.MbViewEditDelete = new MetroFramework.Controls.MetroButton();
            this.MbViewEditClose = new MetroFramework.Controls.MetroButton();
            this.MbViewEditSave = new MetroFramework.Controls.MetroButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TextBoxViewEditId = new System.Windows.Forms.TextBox();
            this.TextBoxViewEditSequence = new System.Windows.Forms.TextBox();
            this.TextBoxViewEditName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.LabelActionNew = new System.Windows.Forms.Label();
            this.MbNewListing = new MetroFramework.Controls.MetroButton();
            this.MbNewViewEdit = new MetroFramework.Controls.MetroButton();
            this.MbNewClose = new MetroFramework.Controls.MetroButton();
            this.MbNewSave = new MetroFramework.Controls.MetroButton();
            this.PanelNew = new System.Windows.Forms.Panel();
            this.TextBoxNewSequence = new System.Windows.Forms.TextBox();
            this.TextBoxNewName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.LabelRecordCount = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView1)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.PanelNew.SuspendLayout();
            this.SuspendLayout();
            // 
            // mlUserInfo
            // 
            this.mlUserInfo.Location = new System.Drawing.Point(793, 35);
            this.mlUserInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlUserInfo.Name = "mlUserInfo";
            this.mlUserInfo.Size = new System.Drawing.Size(380, 30);
            this.mlUserInfo.TabIndex = 10;
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
            this.LabelFormHeaderText.TabIndex = 9;
            this.LabelFormHeaderText.Text = "Neutron Warehouse Management";
            this.LabelFormHeaderText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelFormTitle
            // 
            this.LabelFormTitle.BackColor = System.Drawing.Color.RoyalBlue;
            this.LabelFormTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelFormTitle.Font = new System.Drawing.Font("Comic Sans MS", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormTitle.ForeColor = System.Drawing.Color.Black;
            this.LabelFormTitle.Location = new System.Drawing.Point(367, 98);
            this.LabelFormTitle.Name = "LabelFormTitle";
            this.LabelFormTitle.Size = new System.Drawing.Size(418, 66);
            this.LabelFormTitle.TabIndex = 17;
            this.LabelFormTitle.Text = "Velocity Codes";
            this.LabelFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(22, 173);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1155, 670);
            this.tabControl1.TabIndex = 18;
            this.tabControl1.Enter += new System.EventHandler(this.tabControl1_Enter);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.RoyalBlue;
            this.tabPage1.Controls.Add(this.LabelFindDescription);
            this.tabPage1.Controls.Add(this.TextBoxFind);
            this.tabPage1.Controls.Add(this.MButtonNew);
            this.tabPage1.Controls.Add(this.MButtonViewEdit);
            this.tabPage1.Controls.Add(this.ButtonClear);
            this.tabPage1.Controls.Add(this.MButtonClose);
            this.tabPage1.Controls.Add(this.MButtonSearch);
            this.tabPage1.Controls.Add(this.DataGridView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Size = new System.Drawing.Size(1147, 644);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Listing";
            // 
            // LabelFindDescription
            // 
            this.LabelFindDescription.Location = new System.Drawing.Point(370, 72);
            this.LabelFindDescription.Name = "LabelFindDescription";
            this.LabelFindDescription.Size = new System.Drawing.Size(343, 25);
            this.LabelFindDescription.TabIndex = 18;
            this.LabelFindDescription.Text = "Search for:";
            this.LabelFindDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxFind
            // 
            this.TextBoxFind.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxFind.Location = new System.Drawing.Point(364, 10);
            this.TextBoxFind.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBoxFind.Name = "TextBoxFind";
            this.TextBoxFind.Size = new System.Drawing.Size(347, 37);
            this.TextBoxFind.TabIndex = 17;
            this.TextBoxFind.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxFind_KeyDown);
            // 
            // MButtonNew
            // 
            this.MButtonNew.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonNew.Location = new System.Drawing.Point(6, 10);
            this.MButtonNew.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonNew.Name = "MButtonNew";
            this.MButtonNew.Size = new System.Drawing.Size(162, 76);
            this.MButtonNew.TabIndex = 16;
            this.MButtonNew.Text = "New";
            this.MButtonNew.UseSelectable = true;
            this.MButtonNew.Click += new System.EventHandler(this.MButtonNew_Click);
            // 
            // MButtonViewEdit
            // 
            this.MButtonViewEdit.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonViewEdit.Location = new System.Drawing.Point(186, 10);
            this.MButtonViewEdit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonViewEdit.Name = "MButtonViewEdit";
            this.MButtonViewEdit.Size = new System.Drawing.Size(162, 76);
            this.MButtonViewEdit.TabIndex = 15;
            this.MButtonViewEdit.Text = "View/Edit";
            this.MButtonViewEdit.UseSelectable = true;
            this.MButtonViewEdit.Click += new System.EventHandler(this.MButtonViewEdit_Click);
            // 
            // ButtonClear
            // 
            this.ButtonClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonClear.Location = new System.Drawing.Point(726, 20);
            this.ButtonClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ButtonClear.Name = "ButtonClear";
            this.ButtonClear.Size = new System.Drawing.Size(34, 38);
            this.ButtonClear.TabIndex = 14;
            this.ButtonClear.Text = "X";
            this.ButtonClear.UseVisualStyleBackColor = true;
            this.ButtonClear.Click += new System.EventHandler(this.ButtonClear_Click);
            // 
            // MButtonClose
            // 
            this.MButtonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MButtonClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonClose.Location = new System.Drawing.Point(964, 10);
            this.MButtonClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonClose.Name = "MButtonClose";
            this.MButtonClose.Size = new System.Drawing.Size(177, 76);
            this.MButtonClose.TabIndex = 12;
            this.MButtonClose.Text = "Close";
            this.MButtonClose.UseSelectable = true;
            this.MButtonClose.Click += new System.EventHandler(this.MButtonClose_Click);
            // 
            // MButtonSearch
            // 
            this.MButtonSearch.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MButtonSearch.Location = new System.Drawing.Point(771, 10);
            this.MButtonSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MButtonSearch.Name = "MButtonSearch";
            this.MButtonSearch.Size = new System.Drawing.Size(176, 76);
            this.MButtonSearch.TabIndex = 12;
            this.MButtonSearch.Text = "Search";
            this.MButtonSearch.UseSelectable = true;
            this.MButtonSearch.Click += new System.EventHandler(this.MButtonFind_Click);
            // 
            // DataGridView1
            // 
            this.DataGridView1.AllowUserToAddRows = false;
            this.DataGridView1.AllowUserToDeleteRows = false;
            this.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView1.Location = new System.Drawing.Point(3, 97);
            this.DataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView1.Name = "DataGridView1";
            this.DataGridView1.ReadOnly = true;
            this.DataGridView1.RowTemplate.Height = 24;
            this.DataGridView1.Size = new System.Drawing.Size(1141, 536);
            this.DataGridView1.TabIndex = 0;
            this.DataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellClick);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.RoyalBlue;
            this.tabPage2.Controls.Add(this.LabelAction);
            this.tabPage2.Controls.Add(this.MbViewEditListing);
            this.tabPage2.Controls.Add(this.MbViewEditDelete);
            this.tabPage2.Controls.Add(this.MbViewEditClose);
            this.tabPage2.Controls.Add(this.MbViewEditSave);
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage2.Size = new System.Drawing.Size(1147, 644);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "View/Edit";
            // 
            // LabelAction
            // 
            this.LabelAction.Font = new System.Drawing.Font("Comic Sans MS", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelAction.Location = new System.Drawing.Point(376, 18);
            this.LabelAction.Name = "LabelAction";
            this.LabelAction.Size = new System.Drawing.Size(360, 65);
            this.LabelAction.TabIndex = 21;
            this.LabelAction.Text = "View/Edit ";
            this.LabelAction.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MbViewEditListing
            // 
            this.MbViewEditListing.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MbViewEditListing.Location = new System.Drawing.Point(14, 10);
            this.MbViewEditListing.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MbViewEditListing.Name = "MbViewEditListing";
            this.MbViewEditListing.Size = new System.Drawing.Size(155, 76);
            this.MbViewEditListing.TabIndex = 1;
            this.MbViewEditListing.Text = "Listing";
            this.MbViewEditListing.UseSelectable = true;
            this.MbViewEditListing.Click += new System.EventHandler(this.MbViewEditListing_Click);
            // 
            // MbViewEditDelete
            // 
            this.MbViewEditDelete.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MbViewEditDelete.Location = new System.Drawing.Point(177, 10);
            this.MbViewEditDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MbViewEditDelete.Name = "MbViewEditDelete";
            this.MbViewEditDelete.Size = new System.Drawing.Size(155, 76);
            this.MbViewEditDelete.TabIndex = 2;
            this.MbViewEditDelete.Text = "Delete";
            this.MbViewEditDelete.UseSelectable = true;
            this.MbViewEditDelete.Click += new System.EventHandler(this.MbViewEditDelete_Click);
            // 
            // MbViewEditClose
            // 
            this.MbViewEditClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MbViewEditClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MbViewEditClose.Location = new System.Drawing.Point(933, 10);
            this.MbViewEditClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MbViewEditClose.Name = "MbViewEditClose";
            this.MbViewEditClose.Size = new System.Drawing.Size(155, 76);
            this.MbViewEditClose.TabIndex = 3;
            this.MbViewEditClose.Text = "Close";
            this.MbViewEditClose.UseSelectable = true;
            this.MbViewEditClose.Click += new System.EventHandler(this.MbViewEditClose_Click);
            // 
            // MbViewEditSave
            // 
            this.MbViewEditSave.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MbViewEditSave.Location = new System.Drawing.Point(771, 10);
            this.MbViewEditSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MbViewEditSave.Name = "MbViewEditSave";
            this.MbViewEditSave.Size = new System.Drawing.Size(155, 76);
            this.MbViewEditSave.TabIndex = 0;
            this.MbViewEditSave.Text = "Save";
            this.MbViewEditSave.UseSelectable = true;
            this.MbViewEditSave.Click += new System.EventHandler(this.MbViewEditSave_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.panel1.Controls.Add(this.TextBoxViewEditId);
            this.panel1.Controls.Add(this.TextBoxViewEditSequence);
            this.panel1.Controls.Add(this.TextBoxViewEditName);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(3, 104);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1091, 619);
            this.panel1.TabIndex = 0;
            // 
            // TextBoxViewEditId
            // 
            this.TextBoxViewEditId.Location = new System.Drawing.Point(10, 18);
            this.TextBoxViewEditId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBoxViewEditId.Name = "TextBoxViewEditId";
            this.TextBoxViewEditId.ReadOnly = true;
            this.TextBoxViewEditId.Size = new System.Drawing.Size(112, 28);
            this.TextBoxViewEditId.TabIndex = 1;
            this.TextBoxViewEditId.Visible = false;
            // 
            // TextBoxViewEditSequence
            // 
            this.TextBoxViewEditSequence.Location = new System.Drawing.Point(549, 238);
            this.TextBoxViewEditSequence.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBoxViewEditSequence.Name = "TextBoxViewEditSequence";
            this.TextBoxViewEditSequence.Size = new System.Drawing.Size(224, 28);
            this.TextBoxViewEditSequence.TabIndex = 1;
            this.TextBoxViewEditSequence.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxViewEditSequence.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxViewEditName_KeyDown);
            // 
            // TextBoxViewEditName
            // 
            this.TextBoxViewEditName.Location = new System.Drawing.Point(549, 188);
            this.TextBoxViewEditName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBoxViewEditName.Name = "TextBoxViewEditName";
            this.TextBoxViewEditName.Size = new System.Drawing.Size(224, 28);
            this.TextBoxViewEditName.TabIndex = 1;
            this.TextBoxViewEditName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxViewEditName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxViewEditName_KeyDown);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(317, 238);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(225, 36);
            this.label3.TabIndex = 0;
            this.label3.Text = "Sequence";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(317, 188);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(225, 36);
            this.label5.TabIndex = 0;
            this.label5.Text = "Height Code";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.RoyalBlue;
            this.tabPage3.Controls.Add(this.LabelActionNew);
            this.tabPage3.Controls.Add(this.MbNewListing);
            this.tabPage3.Controls.Add(this.MbNewViewEdit);
            this.tabPage3.Controls.Add(this.MbNewClose);
            this.tabPage3.Controls.Add(this.MbNewSave);
            this.tabPage3.Controls.Add(this.PanelNew);
            this.tabPage3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage3.Size = new System.Drawing.Size(1147, 644);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "New";
            // 
            // LabelActionNew
            // 
            this.LabelActionNew.Font = new System.Drawing.Font("Comic Sans MS", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelActionNew.Location = new System.Drawing.Point(372, 16);
            this.LabelActionNew.Name = "LabelActionNew";
            this.LabelActionNew.Size = new System.Drawing.Size(360, 65);
            this.LabelActionNew.TabIndex = 26;
            this.LabelActionNew.Text = "New";
            this.LabelActionNew.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MbNewListing
            // 
            this.MbNewListing.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MbNewListing.Location = new System.Drawing.Point(14, 10);
            this.MbNewListing.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MbNewListing.Name = "MbNewListing";
            this.MbNewListing.Size = new System.Drawing.Size(155, 76);
            this.MbNewListing.TabIndex = 1;
            this.MbNewListing.Text = "Listing";
            this.MbNewListing.UseSelectable = true;
            this.MbNewListing.Click += new System.EventHandler(this.MbNewListing_Click);
            // 
            // MbNewViewEdit
            // 
            this.MbNewViewEdit.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MbNewViewEdit.Location = new System.Drawing.Point(177, 10);
            this.MbNewViewEdit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MbNewViewEdit.Name = "MbNewViewEdit";
            this.MbNewViewEdit.Size = new System.Drawing.Size(155, 76);
            this.MbNewViewEdit.TabIndex = 2;
            this.MbNewViewEdit.Text = "View/Edit";
            this.MbNewViewEdit.UseSelectable = true;
            this.MbNewViewEdit.Click += new System.EventHandler(this.MbNewViewEdit_Click);
            // 
            // MbNewClose
            // 
            this.MbNewClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MbNewClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MbNewClose.Location = new System.Drawing.Point(933, 10);
            this.MbNewClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MbNewClose.Name = "MbNewClose";
            this.MbNewClose.Size = new System.Drawing.Size(155, 76);
            this.MbNewClose.TabIndex = 3;
            this.MbNewClose.Text = "Close";
            this.MbNewClose.UseSelectable = true;
            this.MbNewClose.Click += new System.EventHandler(this.MbNewClose_Click);
            // 
            // MbNewSave
            // 
            this.MbNewSave.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MbNewSave.Location = new System.Drawing.Point(771, 10);
            this.MbNewSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MbNewSave.Name = "MbNewSave";
            this.MbNewSave.Size = new System.Drawing.Size(155, 76);
            this.MbNewSave.TabIndex = 0;
            this.MbNewSave.Text = "Save";
            this.MbNewSave.UseSelectable = true;
            this.MbNewSave.Click += new System.EventHandler(this.MbNewSave_Click);
            // 
            // PanelNew
            // 
            this.PanelNew.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.PanelNew.Controls.Add(this.TextBoxNewSequence);
            this.PanelNew.Controls.Add(this.TextBoxNewName);
            this.PanelNew.Controls.Add(this.label2);
            this.PanelNew.Controls.Add(this.label17);
            this.PanelNew.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PanelNew.Location = new System.Drawing.Point(3, 104);
            this.PanelNew.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PanelNew.Name = "PanelNew";
            this.PanelNew.Size = new System.Drawing.Size(1091, 619);
            this.PanelNew.TabIndex = 21;
            // 
            // TextBoxNewSequence
            // 
            this.TextBoxNewSequence.Location = new System.Drawing.Point(549, 238);
            this.TextBoxNewSequence.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBoxNewSequence.Name = "TextBoxNewSequence";
            this.TextBoxNewSequence.Size = new System.Drawing.Size(114, 28);
            this.TextBoxNewSequence.TabIndex = 1;
            this.TextBoxNewSequence.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxNewSequence.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxNewName_KeyDown);
            // 
            // TextBoxNewName
            // 
            this.TextBoxNewName.Location = new System.Drawing.Point(549, 188);
            this.TextBoxNewName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBoxNewName.Name = "TextBoxNewName";
            this.TextBoxNewName.Size = new System.Drawing.Size(224, 28);
            this.TextBoxNewName.TabIndex = 1;
            this.TextBoxNewName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxNewName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxNewName_KeyDown);
            this.TextBoxNewName.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxNewName_Validating);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(317, 241);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(225, 36);
            this.label2.TabIndex = 14;
            this.label2.Text = "Sequence";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(317, 188);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(225, 36);
            this.label17.TabIndex = 14;
            this.label17.Text = "Height Code";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelRecordCount
            // 
            this.LabelRecordCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelRecordCount.Location = new System.Drawing.Point(894, 129);
            this.LabelRecordCount.Name = "LabelRecordCount";
            this.LabelRecordCount.Size = new System.Drawing.Size(231, 35);
            this.LabelRecordCount.TabIndex = 19;
            this.LabelRecordCount.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // FrmVelocityCodes
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1200, 860);
            this.Controls.Add(this.LabelRecordCount);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.LabelFormTitle);
            this.Controls.Add(this.mlUserInfo);
            this.Controls.Add(this.LabelFormHeaderText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmVelocityCodes";
            this.Padding = new System.Windows.Forms.Padding(22, 75, 22, 25);
            this.Resizable = false;
            this.Load += new System.EventHandler(this.FrmVelocityCodes_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.PanelNew.ResumeLayout(false);
            this.PanelNew.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroLabel mlUserInfo;
        private System.Windows.Forms.Label LabelFormHeaderText;
        private System.Windows.Forms.Label LabelFormTitle;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView DataGridView1;
        private System.Windows.Forms.TabPage tabPage2;
        private MetroFramework.Controls.MetroButton MButtonNew;
        private MetroFramework.Controls.MetroButton MButtonViewEdit;
        private System.Windows.Forms.Button ButtonClear;
        private MetroFramework.Controls.MetroButton MButtonSearch;
        private MetroFramework.Controls.MetroButton MButtonClose;
        private System.Windows.Forms.TextBox TextBoxFind;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox TextBoxViewEditName;
        private System.Windows.Forms.Label LabelAction;
        private MetroFramework.Controls.MetroButton MbViewEditListing;
        private MetroFramework.Controls.MetroButton MbViewEditDelete;
        private MetroFramework.Controls.MetroButton MbViewEditClose;
        private MetroFramework.Controls.MetroButton MbViewEditSave;
        private System.Windows.Forms.Label LabelActionNew;
        private MetroFramework.Controls.MetroButton MbNewListing;
        private MetroFramework.Controls.MetroButton MbNewViewEdit;
        private MetroFramework.Controls.MetroButton MbNewClose;
        private MetroFramework.Controls.MetroButton MbNewSave;
        private System.Windows.Forms.Panel PanelNew;
        private System.Windows.Forms.Label LabelRecordCount;
        private System.Windows.Forms.Label LabelFindDescription;
        private System.Windows.Forms.TextBox TextBoxViewEditId;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TextBoxNewName;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox TextBoxNewSequence;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TextBoxViewEditSequence;
        private System.Windows.Forms.Label label3;
    }
}