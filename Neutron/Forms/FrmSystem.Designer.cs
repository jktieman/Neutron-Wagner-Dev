namespace Neutron.Forms
{
    partial class FrmSystem
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSystem));
            this.LabelRecordCount = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Main = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.TextBoxStationToPrint = new System.Windows.Forms.TextBox();
            this.TextBoxOrderToPrint = new System.Windows.Forms.TextBox();
            this.MBMainClose = new MetroFramework.Controls.MetroButton();
            this.MBRunUpload = new MetroFramework.Controls.MetroButton();
            this.MBStartUpload = new MetroFramework.Controls.MetroButton();
            this.MBRunLoaderOnce = new MetroFramework.Controls.MetroButton();
            this.MBMainSqlServer = new MetroFramework.Controls.MetroButton();
            this.MBStartLoader = new MetroFramework.Controls.MetroButton();
            this.MBMainInterfaceFile = new MetroFramework.Controls.MetroButton();
            this.SqlServer = new System.Windows.Forms.TabPage();
            this.PanelSql = new System.Windows.Forms.Panel();
            this.CheckBoxSqlServerAuthentication = new System.Windows.Forms.CheckBox();
            this.LabelConnectionString = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TextBoxPassword = new System.Windows.Forms.TextBox();
            this.TextBoxUserId = new System.Windows.Forms.TextBox();
            this.LabelPassword = new System.Windows.Forms.Label();
            this.LabelUserId = new System.Windows.Forms.Label();
            this.ButtonTest = new System.Windows.Forms.Button();
            this.TextBoxInitialCatalog = new System.Windows.Forms.TextBox();
            this.LabelInitialCatalog = new System.Windows.Forms.Label();
            this.LabelServer = new System.Windows.Forms.Label();
            this.ButtonVerifySql = new System.Windows.Forms.Button();
            this.ButtonSaveConnectionString = new System.Windows.Forms.Button();
            this.TextBoxDataSource = new System.Windows.Forms.TextBox();
            this.LabelSqlServerInterface = new System.Windows.Forms.Label();
            this.MBSqlServerBack = new MetroFramework.Controls.MetroButton();
            this.InterfaceFiles = new System.Windows.Forms.TabPage();
            this.PanelFile = new System.Windows.Forms.Panel();
            this.ButtonLanguageDirectory = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.LanguageDirectory = new System.Windows.Forms.TextBox();
            this.ButtonFindCostCenterFile = new System.Windows.Forms.Button();
            this.ButtonCostCenterDirectory = new System.Windows.Forms.Button();
            this.ButtonMaintenanceFileDirectory = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.CostCenterFileName = new System.Windows.Forms.TextBox();
            this.CostCenterDirectory = new System.Windows.Forms.TextBox();
            this.MaintenanceFileDirectory = new System.Windows.Forms.TextBox();
            this.ButtonRootDirectory = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.RootDirectory = new System.Windows.Forms.TextBox();
            this.TextBoxMaintenanceFileFilter = new System.Windows.Forms.TextBox();
            this.TextBoxHostOrderFileFilter = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ButtonFindImagesDirectory = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.ImagesDirectory = new System.Windows.Forms.TextBox();
            this.ButtonDocumentsDirectory = new System.Windows.Forms.Button();
            this.ButtonLogFileDirectory = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.DocumentsDirectory = new System.Windows.Forms.TextBox();
            this.LogFileDirectory = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.EnableLogging = new System.Windows.Forms.CheckBox();
            this.ButtonFindHostUploadDirectory = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.HostUploadDirectory = new System.Windows.Forms.TextBox();
            this.ButtonFindHostUploadFile = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.HostUploadFile = new System.Windows.Forms.TextBox();
            this.ButtonFindHostOrderDirectory = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.HostOrderDirectory = new System.Windows.Forms.TextBox();
            this.ButtonFindHostOrderFile = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.HostOrderFile = new System.Windows.Forms.TextBox();
            this.MBInterfaceFilesBack = new MetroFramework.Controls.MetroButton();
            this.LabelInterfaceFiles = new System.Windows.Forms.Label();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.LabelFormTitle = new System.Windows.Forms.Label();
            this.mlUserInfo = new MetroFramework.Controls.MetroLabel();
            this.LabelFormHeaderText = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.tabControl1.SuspendLayout();
            this.Main.SuspendLayout();
            this.SqlServer.SuspendLayout();
            this.PanelSql.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.InterfaceFiles.SuspendLayout();
            this.PanelFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelRecordCount
            // 
            this.LabelRecordCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelRecordCount.Location = new System.Drawing.Point(894, 129);
            this.LabelRecordCount.Name = "LabelRecordCount";
            this.LabelRecordCount.Size = new System.Drawing.Size(279, 35);
            this.LabelRecordCount.TabIndex = 24;
            this.LabelRecordCount.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Main);
            this.tabControl1.Controls.Add(this.SqlServer);
            this.tabControl1.Controls.Add(this.InterfaceFiles);
            this.tabControl1.Location = new System.Drawing.Point(23, 168);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1155, 670);
            this.tabControl1.TabIndex = 4;
            // 
            // Main
            // 
            this.Main.BackColor = System.Drawing.Color.RoyalBlue;
            this.Main.Controls.Add(this.button1);
            this.Main.Controls.Add(this.TextBoxStationToPrint);
            this.Main.Controls.Add(this.TextBoxOrderToPrint);
            this.Main.Controls.Add(this.MBMainClose);
            this.Main.Controls.Add(this.MBRunUpload);
            this.Main.Controls.Add(this.MBStartUpload);
            this.Main.Controls.Add(this.MBRunLoaderOnce);
            this.Main.Controls.Add(this.MBMainSqlServer);
            this.Main.Controls.Add(this.MBStartLoader);
            this.Main.Controls.Add(this.MBMainInterfaceFile);
            this.Main.Location = new System.Drawing.Point(4, 22);
            this.Main.Name = "Main";
            this.Main.Size = new System.Drawing.Size(1147, 644);
            this.Main.TabIndex = 8;
            this.Main.Text = "Main";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(116, 230);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 30;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TextBoxStationToPrint
            // 
            this.TextBoxStationToPrint.Location = new System.Drawing.Point(900, 282);
            this.TextBoxStationToPrint.Name = "TextBoxStationToPrint";
            this.TextBoxStationToPrint.Size = new System.Drawing.Size(59, 20);
            this.TextBoxStationToPrint.TabIndex = 29;
            this.TextBoxStationToPrint.Visible = false;
            // 
            // TextBoxOrderToPrint
            // 
            this.TextBoxOrderToPrint.Location = new System.Drawing.Point(900, 244);
            this.TextBoxOrderToPrint.Name = "TextBoxOrderToPrint";
            this.TextBoxOrderToPrint.Size = new System.Drawing.Size(159, 20);
            this.TextBoxOrderToPrint.TabIndex = 29;
            this.TextBoxOrderToPrint.Visible = false;
            // 
            // MBMainClose
            // 
            this.MBMainClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBMainClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBMainClose.Location = new System.Drawing.Point(1000, 10);
            this.MBMainClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBMainClose.Name = "MBMainClose";
            this.MBMainClose.Size = new System.Drawing.Size(135, 76);
            this.MBMainClose.TabIndex = 6;
            this.MBMainClose.Text = "Close";
            this.MBMainClose.UseSelectable = true;
            this.MBMainClose.Click += new System.EventHandler(this.MBMainClose_Click);
            // 
            // MBRunUpload
            // 
            this.MBRunUpload.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBRunUpload.Location = new System.Drawing.Point(396, 525);
            this.MBRunUpload.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBRunUpload.Name = "MBRunUpload";
            this.MBRunUpload.Size = new System.Drawing.Size(350, 70);
            this.MBRunUpload.TabIndex = 4;
            this.MBRunUpload.Text = "Run Upload Once";
            this.MBRunUpload.UseSelectable = true;
            this.MBRunUpload.Click += new System.EventHandler(this.MBRunUpload_Click);
            // 
            // MBStartUpload
            // 
            this.MBStartUpload.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBStartUpload.Location = new System.Drawing.Point(396, 430);
            this.MBStartUpload.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBStartUpload.Name = "MBStartUpload";
            this.MBStartUpload.Size = new System.Drawing.Size(350, 70);
            this.MBStartUpload.TabIndex = 5;
            this.MBStartUpload.Text = "Run Upload Continuously";
            this.MBStartUpload.UseSelectable = true;
            this.MBStartUpload.Click += new System.EventHandler(this.MBStartUpload_Click);
            // 
            // MBRunLoaderOnce
            // 
            this.MBRunLoaderOnce.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBRunLoaderOnce.Location = new System.Drawing.Point(396, 335);
            this.MBRunLoaderOnce.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBRunLoaderOnce.Name = "MBRunLoaderOnce";
            this.MBRunLoaderOnce.Size = new System.Drawing.Size(350, 70);
            this.MBRunLoaderOnce.TabIndex = 2;
            this.MBRunLoaderOnce.Text = "Run Loader Once";
            this.MBRunLoaderOnce.UseSelectable = true;
            this.MBRunLoaderOnce.Click += new System.EventHandler(this.MBRunLoaderOnce_Click);
            // 
            // MBMainSqlServer
            // 
            this.MBMainSqlServer.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBMainSqlServer.Location = new System.Drawing.Point(396, 50);
            this.MBMainSqlServer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBMainSqlServer.Name = "MBMainSqlServer";
            this.MBMainSqlServer.Size = new System.Drawing.Size(350, 70);
            this.MBMainSqlServer.TabIndex = 0;
            this.MBMainSqlServer.Text = "SQL Server";
            this.MBMainSqlServer.UseSelectable = true;
            this.MBMainSqlServer.Click += new System.EventHandler(this.MBMainSqlServer_Click);
            // 
            // MBStartLoader
            // 
            this.MBStartLoader.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBStartLoader.Location = new System.Drawing.Point(396, 240);
            this.MBStartLoader.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBStartLoader.Name = "MBStartLoader";
            this.MBStartLoader.Size = new System.Drawing.Size(350, 70);
            this.MBStartLoader.TabIndex = 3;
            this.MBStartLoader.Text = "Run Loader Continuously";
            this.MBStartLoader.UseSelectable = true;
            this.MBStartLoader.Click += new System.EventHandler(this.MBStartLoader_Click);
            // 
            // MBMainInterfaceFile
            // 
            this.MBMainInterfaceFile.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBMainInterfaceFile.Location = new System.Drawing.Point(396, 145);
            this.MBMainInterfaceFile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBMainInterfaceFile.Name = "MBMainInterfaceFile";
            this.MBMainInterfaceFile.Size = new System.Drawing.Size(350, 70);
            this.MBMainInterfaceFile.TabIndex = 1;
            this.MBMainInterfaceFile.Text = "Interface Information";
            this.MBMainInterfaceFile.UseSelectable = true;
            this.MBMainInterfaceFile.Click += new System.EventHandler(this.MBMainInterfaceFiles_Click);
            // 
            // SqlServer
            // 
            this.SqlServer.BackColor = System.Drawing.Color.Turquoise;
            this.SqlServer.Controls.Add(this.PanelSql);
            this.SqlServer.Controls.Add(this.MBSqlServerBack);
            this.SqlServer.Location = new System.Drawing.Point(4, 22);
            this.SqlServer.Name = "SqlServer";
            this.SqlServer.Size = new System.Drawing.Size(1147, 644);
            this.SqlServer.TabIndex = 5;
            this.SqlServer.Text = "Sql Server";
            // 
            // PanelSql
            // 
            this.PanelSql.Controls.Add(this.CheckBoxSqlServerAuthentication);
            this.PanelSql.Controls.Add(this.LabelConnectionString);
            this.PanelSql.Controls.Add(this.groupBox1);
            this.PanelSql.Controls.Add(this.ButtonTest);
            this.PanelSql.Controls.Add(this.TextBoxInitialCatalog);
            this.PanelSql.Controls.Add(this.LabelInitialCatalog);
            this.PanelSql.Controls.Add(this.LabelServer);
            this.PanelSql.Controls.Add(this.ButtonVerifySql);
            this.PanelSql.Controls.Add(this.ButtonSaveConnectionString);
            this.PanelSql.Controls.Add(this.TextBoxDataSource);
            this.PanelSql.Controls.Add(this.LabelSqlServerInterface);
            this.PanelSql.Location = new System.Drawing.Point(171, 105);
            this.PanelSql.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PanelSql.Name = "PanelSql";
            this.PanelSql.Size = new System.Drawing.Size(804, 496);
            this.PanelSql.TabIndex = 40;
            // 
            // CheckBoxSqlServerAuthentication
            // 
            this.CheckBoxSqlServerAuthentication.AutoSize = true;
            this.CheckBoxSqlServerAuthentication.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckBoxSqlServerAuthentication.Location = new System.Drawing.Point(267, 189);
            this.CheckBoxSqlServerAuthentication.Name = "CheckBoxSqlServerAuthentication";
            this.CheckBoxSqlServerAuthentication.Size = new System.Drawing.Size(270, 24);
            this.CheckBoxSqlServerAuthentication.TabIndex = 2;
            this.CheckBoxSqlServerAuthentication.Text = "Use Sql Server Authentication";
            this.CheckBoxSqlServerAuthentication.UseVisualStyleBackColor = true;
            this.CheckBoxSqlServerAuthentication.CheckedChanged += new System.EventHandler(this.CheckBoxSqlServerAuthentication_CheckedChanged);
            // 
            // LabelConnectionString
            // 
            this.LabelConnectionString.Location = new System.Drawing.Point(18, 360);
            this.LabelConnectionString.Name = "LabelConnectionString";
            this.LabelConnectionString.Size = new System.Drawing.Size(770, 35);
            this.LabelConnectionString.TabIndex = 30;
            this.LabelConnectionString.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TextBoxPassword);
            this.groupBox1.Controls.Add(this.TextBoxUserId);
            this.groupBox1.Controls.Add(this.LabelPassword);
            this.groupBox1.Controls.Add(this.LabelUserId);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(123, 233);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(558, 101);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sql Server Authentication";
            // 
            // TextBoxPassword
            // 
            this.TextBoxPassword.Enabled = false;
            this.TextBoxPassword.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxPassword.Location = new System.Drawing.Point(178, 61);
            this.TextBoxPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxPassword.Name = "TextBoxPassword";
            this.TextBoxPassword.PasswordChar = '*';
            this.TextBoxPassword.Size = new System.Drawing.Size(357, 29);
            this.TextBoxPassword.TabIndex = 1;
            // 
            // TextBoxUserId
            // 
            this.TextBoxUserId.Enabled = false;
            this.TextBoxUserId.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxUserId.Location = new System.Drawing.Point(179, 15);
            this.TextBoxUserId.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxUserId.Name = "TextBoxUserId";
            this.TextBoxUserId.Size = new System.Drawing.Size(356, 29);
            this.TextBoxUserId.TabIndex = 0;
            // 
            // LabelPassword
            // 
            this.LabelPassword.AutoSize = true;
            this.LabelPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelPassword.Location = new System.Drawing.Point(7, 60);
            this.LabelPassword.Name = "LabelPassword";
            this.LabelPassword.Size = new System.Drawing.Size(86, 20);
            this.LabelPassword.TabIndex = 28;
            this.LabelPassword.Text = "Password";
            // 
            // LabelUserId
            // 
            this.LabelUserId.AutoSize = true;
            this.LabelUserId.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelUserId.Location = new System.Drawing.Point(25, 21);
            this.LabelUserId.Name = "LabelUserId";
            this.LabelUserId.Size = new System.Drawing.Size(68, 20);
            this.LabelUserId.TabIndex = 27;
            this.LabelUserId.Text = "User Id";
            // 
            // ButtonTest
            // 
            this.ButtonTest.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonTest.Location = new System.Drawing.Point(65, 416);
            this.ButtonTest.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ButtonTest.Name = "ButtonTest";
            this.ButtonTest.Size = new System.Drawing.Size(212, 50);
            this.ButtonTest.TabIndex = 3;
            this.ButtonTest.Text = "Test Connection";
            this.ButtonTest.UseVisualStyleBackColor = true;
            this.ButtonTest.Click += new System.EventHandler(this.ButtonTest_Click);
            // 
            // TextBoxInitialCatalog
            // 
            this.TextBoxInitialCatalog.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxInitialCatalog.Location = new System.Drawing.Point(302, 114);
            this.TextBoxInitialCatalog.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxInitialCatalog.Name = "TextBoxInitialCatalog";
            this.TextBoxInitialCatalog.Size = new System.Drawing.Size(356, 29);
            this.TextBoxInitialCatalog.TabIndex = 1;
            // 
            // LabelInitialCatalog
            // 
            this.LabelInitialCatalog.AutoSize = true;
            this.LabelInitialCatalog.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelInitialCatalog.Location = new System.Drawing.Point(145, 118);
            this.LabelInitialCatalog.Name = "LabelInitialCatalog";
            this.LabelInitialCatalog.Size = new System.Drawing.Size(87, 20);
            this.LabelInitialCatalog.TabIndex = 21;
            this.LabelInitialCatalog.Text = "Database";
            // 
            // LabelServer
            // 
            this.LabelServer.AutoSize = true;
            this.LabelServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelServer.Location = new System.Drawing.Point(171, 68);
            this.LabelServer.Name = "LabelServer";
            this.LabelServer.Size = new System.Drawing.Size(61, 20);
            this.LabelServer.TabIndex = 20;
            this.LabelServer.Text = "Server";
            // 
            // ButtonVerifySql
            // 
            this.ButtonVerifySql.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonVerifySql.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonVerifySql.Location = new System.Drawing.Point(527, 416);
            this.ButtonVerifySql.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ButtonVerifySql.Name = "ButtonVerifySql";
            this.ButtonVerifySql.Size = new System.Drawing.Size(212, 50);
            this.ButtonVerifySql.TabIndex = 5;
            this.ButtonVerifySql.Text = "Verify Sql Server Schema";
            this.ButtonVerifySql.UseVisualStyleBackColor = true;
            this.ButtonVerifySql.Visible = false;
            this.ButtonVerifySql.Click += new System.EventHandler(this.ButtonVerifySql_Click);
            // 
            // ButtonSaveConnectionString
            // 
            this.ButtonSaveConnectionString.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveConnectionString.Location = new System.Drawing.Point(296, 416);
            this.ButtonSaveConnectionString.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ButtonSaveConnectionString.Name = "ButtonSaveConnectionString";
            this.ButtonSaveConnectionString.Size = new System.Drawing.Size(212, 50);
            this.ButtonSaveConnectionString.TabIndex = 4;
            this.ButtonSaveConnectionString.Text = "Save";
            this.ButtonSaveConnectionString.UseVisualStyleBackColor = true;
            this.ButtonSaveConnectionString.Click += new System.EventHandler(this.ButtonSaveConnectionString_Click);
            // 
            // TextBoxDataSource
            // 
            this.TextBoxDataSource.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxDataSource.Location = new System.Drawing.Point(303, 64);
            this.TextBoxDataSource.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxDataSource.Name = "TextBoxDataSource";
            this.TextBoxDataSource.Size = new System.Drawing.Size(356, 29);
            this.TextBoxDataSource.TabIndex = 0;
            // 
            // LabelSqlServerInterface
            // 
            this.LabelSqlServerInterface.AutoSize = true;
            this.LabelSqlServerInterface.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSqlServerInterface.Location = new System.Drawing.Point(347, 12);
            this.LabelSqlServerInterface.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelSqlServerInterface.Name = "LabelSqlServerInterface";
            this.LabelSqlServerInterface.Size = new System.Drawing.Size(110, 25);
            this.LabelSqlServerInterface.TabIndex = 18;
            this.LabelSqlServerInterface.Text = "SQL Server";
            // 
            // MBSqlServerBack
            // 
            this.MBSqlServerBack.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBSqlServerBack.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBSqlServerBack.Location = new System.Drawing.Point(1000, 10);
            this.MBSqlServerBack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBSqlServerBack.Name = "MBSqlServerBack";
            this.MBSqlServerBack.Size = new System.Drawing.Size(135, 76);
            this.MBSqlServerBack.TabIndex = 0;
            this.MBSqlServerBack.Text = "Back";
            this.MBSqlServerBack.UseSelectable = true;
            this.MBSqlServerBack.Click += new System.EventHandler(this.MBSqlServerBack_Click);
            // 
            // InterfaceFiles
            // 
            this.InterfaceFiles.BackColor = System.Drawing.Color.Turquoise;
            this.InterfaceFiles.Controls.Add(this.PanelFile);
            this.InterfaceFiles.Controls.Add(this.MBInterfaceFilesBack);
            this.InterfaceFiles.Controls.Add(this.LabelInterfaceFiles);
            this.InterfaceFiles.Controls.Add(this.ButtonSave);
            this.InterfaceFiles.Location = new System.Drawing.Point(4, 22);
            this.InterfaceFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.InterfaceFiles.Name = "InterfaceFiles";
            this.InterfaceFiles.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.InterfaceFiles.Size = new System.Drawing.Size(1147, 644);
            this.InterfaceFiles.TabIndex = 2;
            this.InterfaceFiles.Text = "InterfaceFiles";
            // 
            // PanelFile
            // 
            this.PanelFile.Controls.Add(this.ButtonLanguageDirectory);
            this.PanelFile.Controls.Add(this.label15);
            this.PanelFile.Controls.Add(this.LanguageDirectory);
            this.PanelFile.Controls.Add(this.ButtonFindCostCenterFile);
            this.PanelFile.Controls.Add(this.ButtonCostCenterDirectory);
            this.PanelFile.Controls.Add(this.ButtonMaintenanceFileDirectory);
            this.PanelFile.Controls.Add(this.label14);
            this.PanelFile.Controls.Add(this.label9);
            this.PanelFile.Controls.Add(this.label13);
            this.PanelFile.Controls.Add(this.CostCenterFileName);
            this.PanelFile.Controls.Add(this.CostCenterDirectory);
            this.PanelFile.Controls.Add(this.MaintenanceFileDirectory);
            this.PanelFile.Controls.Add(this.ButtonRootDirectory);
            this.PanelFile.Controls.Add(this.label10);
            this.PanelFile.Controls.Add(this.RootDirectory);
            this.PanelFile.Controls.Add(this.TextBoxMaintenanceFileFilter);
            this.PanelFile.Controls.Add(this.TextBoxHostOrderFileFilter);
            this.PanelFile.Controls.Add(this.label6);
            this.PanelFile.Controls.Add(this.label5);
            this.PanelFile.Controls.Add(this.ButtonFindImagesDirectory);
            this.PanelFile.Controls.Add(this.label2);
            this.PanelFile.Controls.Add(this.ImagesDirectory);
            this.PanelFile.Controls.Add(this.ButtonDocumentsDirectory);
            this.PanelFile.Controls.Add(this.ButtonLogFileDirectory);
            this.PanelFile.Controls.Add(this.label8);
            this.PanelFile.Controls.Add(this.label1);
            this.PanelFile.Controls.Add(this.DocumentsDirectory);
            this.PanelFile.Controls.Add(this.LogFileDirectory);
            this.PanelFile.Controls.Add(this.label7);
            this.PanelFile.Controls.Add(this.EnableLogging);
            this.PanelFile.Controls.Add(this.ButtonFindHostUploadDirectory);
            this.PanelFile.Controls.Add(this.label11);
            this.PanelFile.Controls.Add(this.HostUploadDirectory);
            this.PanelFile.Controls.Add(this.ButtonFindHostUploadFile);
            this.PanelFile.Controls.Add(this.label12);
            this.PanelFile.Controls.Add(this.HostUploadFile);
            this.PanelFile.Controls.Add(this.ButtonFindHostOrderDirectory);
            this.PanelFile.Controls.Add(this.label4);
            this.PanelFile.Controls.Add(this.HostOrderDirectory);
            this.PanelFile.Controls.Add(this.ButtonFindHostOrderFile);
            this.PanelFile.Controls.Add(this.label3);
            this.PanelFile.Controls.Add(this.HostOrderFile);
            this.PanelFile.Location = new System.Drawing.Point(27, 105);
            this.PanelFile.Name = "PanelFile";
            this.PanelFile.Size = new System.Drawing.Size(1076, 532);
            this.PanelFile.TabIndex = 32;
            // 
            // ButtonLanguageDirectory
            // 
            this.ButtonLanguageDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonLanguageDirectory.Location = new System.Drawing.Point(862, 466);
            this.ButtonLanguageDirectory.Name = "ButtonLanguageDirectory";
            this.ButtonLanguageDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonLanguageDirectory.TabIndex = 27;
            this.ButtonLanguageDirectory.Text = "&Browse";
            this.ButtonLanguageDirectory.UseVisualStyleBackColor = true;
            this.ButtonLanguageDirectory.Click += new System.EventHandler(this.ButtonLanguageDirectory_Click);
            // 
            // label15
            // 
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(18, 465);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(320, 26);
            this.label15.TabIndex = 103;
            this.label15.Text = "Language Directory";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LanguageDirectory
            // 
            this.LanguageDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LanguageDirectory.Location = new System.Drawing.Point(351, 465);
            this.LanguageDirectory.Name = "LanguageDirectory";
            this.LanguageDirectory.Size = new System.Drawing.Size(505, 26);
            this.LanguageDirectory.TabIndex = 26;
            this.LanguageDirectory.Text = "Language\\";
            // 
            // ButtonFindCostCenterFile
            // 
            this.ButtonFindCostCenterFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonFindCostCenterFile.Location = new System.Drawing.Point(862, 425);
            this.ButtonFindCostCenterFile.Name = "ButtonFindCostCenterFile";
            this.ButtonFindCostCenterFile.Size = new System.Drawing.Size(100, 26);
            this.ButtonFindCostCenterFile.TabIndex = 25;
            this.ButtonFindCostCenterFile.Text = "&Browse";
            this.ButtonFindCostCenterFile.UseVisualStyleBackColor = true;
            this.ButtonFindCostCenterFile.Click += new System.EventHandler(this.ButtonFindCostCenterFile_Click);
            // 
            // ButtonCostCenterDirectory
            // 
            this.ButtonCostCenterDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCostCenterDirectory.Location = new System.Drawing.Point(862, 386);
            this.ButtonCostCenterDirectory.Name = "ButtonCostCenterDirectory";
            this.ButtonCostCenterDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonCostCenterDirectory.TabIndex = 23;
            this.ButtonCostCenterDirectory.Text = "&Browse";
            this.ButtonCostCenterDirectory.UseVisualStyleBackColor = true;
            this.ButtonCostCenterDirectory.Click += new System.EventHandler(this.ButtonCostCenterDirectory_Click);
            // 
            // ButtonMaintenanceFileDirectory
            // 
            this.ButtonMaintenanceFileDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonMaintenanceFileDirectory.Location = new System.Drawing.Point(686, 346);
            this.ButtonMaintenanceFileDirectory.Name = "ButtonMaintenanceFileDirectory";
            this.ButtonMaintenanceFileDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonMaintenanceFileDirectory.TabIndex = 20;
            this.ButtonMaintenanceFileDirectory.Text = "&Browse";
            this.ButtonMaintenanceFileDirectory.UseVisualStyleBackColor = true;
            this.ButtonMaintenanceFileDirectory.Click += new System.EventHandler(this.ButtonMaintenanceFileDirectory_Click);
            // 
            // label14
            // 
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(18, 424);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(320, 26);
            this.label14.TabIndex = 99;
            this.label14.Text = "Cost Center File Name";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(18, 388);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(320, 26);
            this.label9.TabIndex = 99;
            this.label9.Text = "Cost Center Directory";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(138, 348);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(200, 26);
            this.label13.TabIndex = 99;
            this.label13.Text = "Maintenance Directory";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CostCenterFileName
            // 
            this.CostCenterFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CostCenterFileName.Location = new System.Drawing.Point(351, 424);
            this.CostCenterFileName.Name = "CostCenterFileName";
            this.CostCenterFileName.Size = new System.Drawing.Size(505, 26);
            this.CostCenterFileName.TabIndex = 24;
            this.CostCenterFileName.Text = "Costcntr.txt";
            // 
            // CostCenterDirectory
            // 
            this.CostCenterDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CostCenterDirectory.Location = new System.Drawing.Point(351, 386);
            this.CostCenterDirectory.Name = "CostCenterDirectory";
            this.CostCenterDirectory.Size = new System.Drawing.Size(505, 26);
            this.CostCenterDirectory.TabIndex = 22;
            this.CostCenterDirectory.Text = "C:\\Neutron\\Cost Center\\";
            // 
            // MaintenanceFileDirectory
            // 
            this.MaintenanceFileDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaintenanceFileDirectory.Location = new System.Drawing.Point(351, 346);
            this.MaintenanceFileDirectory.Name = "MaintenanceFileDirectory";
            this.MaintenanceFileDirectory.Size = new System.Drawing.Size(325, 26);
            this.MaintenanceFileDirectory.TabIndex = 19;
            this.MaintenanceFileDirectory.Text = "C:\\Neutron\\Maintenance\\";
            // 
            // ButtonRootDirectory
            // 
            this.ButtonRootDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonRootDirectory.Location = new System.Drawing.Point(862, 17);
            this.ButtonRootDirectory.Name = "ButtonRootDirectory";
            this.ButtonRootDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonRootDirectory.TabIndex = 1;
            this.ButtonRootDirectory.Text = "&Browse";
            this.ButtonRootDirectory.UseVisualStyleBackColor = true;
            this.ButtonRootDirectory.Visible = false;
            this.ButtonRootDirectory.Click += new System.EventHandler(this.ButtonRootDirectory_Click);
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(138, 19);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(200, 26);
            this.label10.TabIndex = 96;
            this.label10.Text = "Root Directory";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label10.Visible = false;
            // 
            // RootDirectory
            // 
            this.RootDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RootDirectory.Location = new System.Drawing.Point(351, 17);
            this.RootDirectory.Name = "RootDirectory";
            this.RootDirectory.Size = new System.Drawing.Size(505, 26);
            this.RootDirectory.TabIndex = 0;
            this.RootDirectory.Visible = false;
            // 
            // TextBoxMaintenanceFileFilter
            // 
            this.TextBoxMaintenanceFileFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxMaintenanceFileFilter.Location = new System.Drawing.Point(862, 345);
            this.TextBoxMaintenanceFileFilter.Name = "TextBoxMaintenanceFileFilter";
            this.TextBoxMaintenanceFileFilter.Size = new System.Drawing.Size(100, 26);
            this.TextBoxMaintenanceFileFilter.TabIndex = 21;
            this.TextBoxMaintenanceFileFilter.Text = "MNT.*";
            this.TextBoxMaintenanceFileFilter.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxHostOrderFileFilter
            // 
            this.TextBoxHostOrderFileFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxHostOrderFileFilter.Location = new System.Drawing.Point(756, 138);
            this.TextBoxHostOrderFileFilter.Name = "TextBoxHostOrderFileFilter";
            this.TextBoxHostOrderFileFilter.Size = new System.Drawing.Size(100, 26);
            this.TextBoxHostOrderFileFilter.TabIndex = 8;
            this.TextBoxHostOrderFileFilter.Text = "PR1.*";
            this.TextBoxHostOrderFileFilter.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(805, 346);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 26);
            this.label6.TabIndex = 91;
            this.label6.Text = "Filter";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(682, 138);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 26);
            this.label5.TabIndex = 91;
            this.label5.Text = "Filter";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonFindImagesDirectory
            // 
            this.ButtonFindImagesDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonFindImagesDirectory.Location = new System.Drawing.Point(862, 59);
            this.ButtonFindImagesDirectory.Name = "ButtonFindImagesDirectory";
            this.ButtonFindImagesDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonFindImagesDirectory.TabIndex = 3;
            this.ButtonFindImagesDirectory.Text = "&Browse";
            this.ButtonFindImagesDirectory.UseVisualStyleBackColor = true;
            this.ButtonFindImagesDirectory.Click += new System.EventHandler(this.ButtonFindImagesDirectory_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(138, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 26);
            this.label2.TabIndex = 89;
            this.label2.Text = "Images Directory";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ImagesDirectory
            // 
            this.ImagesDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImagesDirectory.Location = new System.Drawing.Point(351, 59);
            this.ImagesDirectory.Name = "ImagesDirectory";
            this.ImagesDirectory.Size = new System.Drawing.Size(505, 26);
            this.ImagesDirectory.TabIndex = 2;
            // 
            // ButtonDocumentsDirectory
            // 
            this.ButtonDocumentsDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonDocumentsDirectory.Location = new System.Drawing.Point(862, 264);
            this.ButtonDocumentsDirectory.Name = "ButtonDocumentsDirectory";
            this.ButtonDocumentsDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonDocumentsDirectory.TabIndex = 14;
            this.ButtonDocumentsDirectory.Text = "&Browse";
            this.ButtonDocumentsDirectory.UseVisualStyleBackColor = true;
            this.ButtonDocumentsDirectory.Click += new System.EventHandler(this.ButtonDocumentsDirectory_Click);
            // 
            // ButtonLogFileDirectory
            // 
            this.ButtonLogFileDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonLogFileDirectory.Location = new System.Drawing.Point(686, 305);
            this.ButtonLogFileDirectory.Name = "ButtonLogFileDirectory";
            this.ButtonLogFileDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonLogFileDirectory.TabIndex = 16;
            this.ButtonLogFileDirectory.Text = "&Browse";
            this.ButtonLogFileDirectory.UseVisualStyleBackColor = true;
            this.ButtonLogFileDirectory.Click += new System.EventHandler(this.ButtonLogFileDirectory_Click);
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(138, 264);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(200, 26);
            this.label8.TabIndex = 86;
            this.label8.Text = "Documents Directory";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(138, 305);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 26);
            this.label1.TabIndex = 86;
            this.label1.Text = "Log File Directory";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DocumentsDirectory
            // 
            this.DocumentsDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DocumentsDirectory.Location = new System.Drawing.Point(351, 264);
            this.DocumentsDirectory.Name = "DocumentsDirectory";
            this.DocumentsDirectory.Size = new System.Drawing.Size(505, 26);
            this.DocumentsDirectory.TabIndex = 13;
            // 
            // LogFileDirectory
            // 
            this.LogFileDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogFileDirectory.Location = new System.Drawing.Point(351, 305);
            this.LogFileDirectory.Name = "LogFileDirectory";
            this.LogFileDirectory.Size = new System.Drawing.Size(325, 26);
            this.LogFileDirectory.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(792, 305);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(138, 26);
            this.label7.TabIndex = 17;
            this.label7.Text = "Enable Logging";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // EnableLogging
            // 
            this.EnableLogging.AutoSize = true;
            this.EnableLogging.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EnableLogging.Location = new System.Drawing.Point(936, 311);
            this.EnableLogging.Name = "EnableLogging";
            this.EnableLogging.Size = new System.Drawing.Size(15, 14);
            this.EnableLogging.TabIndex = 18;
            this.EnableLogging.UseVisualStyleBackColor = true;
            // 
            // ButtonFindHostUploadDirectory
            // 
            this.ButtonFindHostUploadDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonFindHostUploadDirectory.Location = new System.Drawing.Point(862, 178);
            this.ButtonFindHostUploadDirectory.Name = "ButtonFindHostUploadDirectory";
            this.ButtonFindHostUploadDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonFindHostUploadDirectory.TabIndex = 10;
            this.ButtonFindHostUploadDirectory.Text = "&Browse";
            this.ButtonFindHostUploadDirectory.UseVisualStyleBackColor = true;
            this.ButtonFindHostUploadDirectory.Click += new System.EventHandler(this.ButtonFindHostUploadDirectory_Click);
            // 
            // label11
            // 
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(138, 178);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(200, 26);
            this.label11.TabIndex = 74;
            this.label11.Text = "HOST Upload Directory";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // HostUploadDirectory
            // 
            this.HostUploadDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HostUploadDirectory.Location = new System.Drawing.Point(351, 178);
            this.HostUploadDirectory.Name = "HostUploadDirectory";
            this.HostUploadDirectory.Size = new System.Drawing.Size(505, 26);
            this.HostUploadDirectory.TabIndex = 9;
            // 
            // ButtonFindHostUploadFile
            // 
            this.ButtonFindHostUploadFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonFindHostUploadFile.Location = new System.Drawing.Point(558, 221);
            this.ButtonFindHostUploadFile.Name = "ButtonFindHostUploadFile";
            this.ButtonFindHostUploadFile.Size = new System.Drawing.Size(100, 26);
            this.ButtonFindHostUploadFile.TabIndex = 12;
            this.ButtonFindHostUploadFile.Text = "&Browse";
            this.ButtonFindHostUploadFile.UseVisualStyleBackColor = true;
            this.ButtonFindHostUploadFile.Click += new System.EventHandler(this.ButtonFindHostUploadFile_Click);
            // 
            // label12
            // 
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(114, 221);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(224, 26);
            this.label12.TabIndex = 71;
            this.label12.Text = "HOST Upload File Name";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // HostUploadFile
            // 
            this.HostUploadFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HostUploadFile.Location = new System.Drawing.Point(351, 222);
            this.HostUploadFile.Name = "HostUploadFile";
            this.HostUploadFile.Size = new System.Drawing.Size(192, 26);
            this.HostUploadFile.TabIndex = 11;
            this.HostUploadFile.Text = "Upload.dat";
            // 
            // ButtonFindHostOrderDirectory
            // 
            this.ButtonFindHostOrderDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonFindHostOrderDirectory.Location = new System.Drawing.Point(862, 97);
            this.ButtonFindHostOrderDirectory.Name = "ButtonFindHostOrderDirectory";
            this.ButtonFindHostOrderDirectory.Size = new System.Drawing.Size(100, 26);
            this.ButtonFindHostOrderDirectory.TabIndex = 5;
            this.ButtonFindHostOrderDirectory.Text = "&Browse";
            this.ButtonFindHostOrderDirectory.UseVisualStyleBackColor = true;
            this.ButtonFindHostOrderDirectory.Click += new System.EventHandler(this.ButtonFindHostOrderDirectory_Click);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(138, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(200, 26);
            this.label4.TabIndex = 55;
            this.label4.Text = "HOST Order Directory";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // HostOrderDirectory
            // 
            this.HostOrderDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HostOrderDirectory.Location = new System.Drawing.Point(351, 97);
            this.HostOrderDirectory.Name = "HostOrderDirectory";
            this.HostOrderDirectory.Size = new System.Drawing.Size(505, 26);
            this.HostOrderDirectory.TabIndex = 4;
            // 
            // ButtonFindHostOrderFile
            // 
            this.ButtonFindHostOrderFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonFindHostOrderFile.Location = new System.Drawing.Point(558, 138);
            this.ButtonFindHostOrderFile.Name = "ButtonFindHostOrderFile";
            this.ButtonFindHostOrderFile.Size = new System.Drawing.Size(100, 26);
            this.ButtonFindHostOrderFile.TabIndex = 7;
            this.ButtonFindHostOrderFile.Text = "&Browse";
            this.ButtonFindHostOrderFile.UseVisualStyleBackColor = true;
            this.ButtonFindHostOrderFile.Click += new System.EventHandler(this.ButtonFindHostOrderFile_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(138, 136);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(200, 26);
            this.label3.TabIndex = 52;
            this.label3.Text = "HOST Order File";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // HostOrderFile
            // 
            this.HostOrderFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HostOrderFile.Location = new System.Drawing.Point(351, 137);
            this.HostOrderFile.Name = "HostOrderFile";
            this.HostOrderFile.Size = new System.Drawing.Size(192, 26);
            this.HostOrderFile.TabIndex = 6;
            // 
            // MBInterfaceFilesBack
            // 
            this.MBInterfaceFilesBack.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBInterfaceFilesBack.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBInterfaceFilesBack.Location = new System.Drawing.Point(1000, 10);
            this.MBInterfaceFilesBack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBInterfaceFilesBack.Name = "MBInterfaceFilesBack";
            this.MBInterfaceFilesBack.Size = new System.Drawing.Size(135, 76);
            this.MBInterfaceFilesBack.TabIndex = 1;
            this.MBInterfaceFilesBack.Text = "Back";
            this.MBInterfaceFilesBack.UseSelectable = true;
            this.MBInterfaceFilesBack.Click += new System.EventHandler(this.MBInterfaceFilesBack_Click);
            // 
            // LabelInterfaceFiles
            // 
            this.LabelInterfaceFiles.AutoSize = true;
            this.LabelInterfaceFiles.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelInterfaceFiles.Location = new System.Drawing.Point(455, 64);
            this.LabelInterfaceFiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelInterfaceFiles.Name = "LabelInterfaceFiles";
            this.LabelInterfaceFiles.Size = new System.Drawing.Size(168, 25);
            this.LabelInterfaceFiles.TabIndex = 82;
            this.LabelInterfaceFiles.Text = "Interface Settings";
            // 
            // ButtonSave
            // 
            this.ButtonSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSave.Location = new System.Drawing.Point(849, 10);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(137, 76);
            this.ButtonSave.TabIndex = 0;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
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
            this.LabelFormTitle.TabIndex = 22;
            this.LabelFormTitle.Text = "System";
            this.LabelFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mlUserInfo
            // 
            this.mlUserInfo.Location = new System.Drawing.Point(793, 35);
            this.mlUserInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mlUserInfo.Name = "mlUserInfo";
            this.mlUserInfo.Size = new System.Drawing.Size(380, 30);
            this.mlUserInfo.TabIndex = 21;
            this.mlUserInfo.Text = "Login ?";
            this.mlUserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelFormHeaderText
            // 
            this.LabelFormHeaderText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelFormHeaderText.Font = new System.Drawing.Font("Comic Sans MS", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormHeaderText.ForeColor = System.Drawing.Color.Turquoise;
            this.LabelFormHeaderText.Location = new System.Drawing.Point(27, 16);
            this.LabelFormHeaderText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelFormHeaderText.Name = "LabelFormHeaderText";
            this.LabelFormHeaderText.Size = new System.Drawing.Size(713, 62);
            this.LabelFormHeaderText.TabIndex = 20;
            this.LabelFormHeaderText.Text = "Neutron Warehouse Management";
            this.LabelFormHeaderText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // FrmSystem
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = MetroFramework.Forms.MetroFormBorderStyle.FixedSingle;
            this.ClientSize = new System.Drawing.Size(1200, 860);
            this.ControlBox = false;
            this.Controls.Add(this.LabelRecordCount);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.LabelFormTitle);
            this.Controls.Add(this.mlUserInfo);
            this.Controls.Add(this.LabelFormHeaderText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmSystem";
            this.Resizable = false;
            this.Text = "Order Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSystem_FormClosing);
            this.Load += new System.EventHandler(this.FrmSystem_Load);
            this.tabControl1.ResumeLayout(false);
            this.Main.ResumeLayout(false);
            this.Main.PerformLayout();
            this.SqlServer.ResumeLayout(false);
            this.PanelSql.ResumeLayout(false);
            this.PanelSql.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.InterfaceFiles.ResumeLayout(false);
            this.InterfaceFiles.PerformLayout();
            this.PanelFile.ResumeLayout(false);
            this.PanelFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LabelRecordCount;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage InterfaceFiles;
        private MetroFramework.Controls.MetroButton MBInterfaceFilesBack;
        private System.Windows.Forms.Label LabelFormTitle;
        private MetroFramework.Controls.MetroLabel mlUserInfo;
        private System.Windows.Forms.Label LabelFormHeaderText;
        private System.Windows.Forms.TabPage SqlServer;
        private MetroFramework.Controls.MetroButton MBSqlServerBack;
        private System.Windows.Forms.TabPage Main;
        private MetroFramework.Controls.MetroButton MBMainClose;
        private MetroFramework.Controls.MetroButton MBStartUpload;
        private MetroFramework.Controls.MetroButton MBMainSqlServer;
        private MetroFramework.Controls.MetroButton MBStartLoader;
        private MetroFramework.Controls.MetroButton MBMainInterfaceFile;
        private System.Windows.Forms.Panel PanelSql;
        private System.Windows.Forms.Button ButtonTest;
        private System.Windows.Forms.TextBox TextBoxInitialCatalog;
        private System.Windows.Forms.Label LabelInitialCatalog;
        private System.Windows.Forms.Label LabelServer;
        private System.Windows.Forms.Button ButtonVerifySql;
        private System.Windows.Forms.Button ButtonSaveConnectionString;
        private System.Windows.Forms.TextBox TextBoxDataSource;
        private System.Windows.Forms.Label LabelSqlServerInterface;
        private System.Windows.Forms.Panel PanelFile;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox EnableLogging;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Label LabelInterfaceFiles;
        private System.Windows.Forms.Button ButtonFindHostUploadDirectory;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox HostUploadDirectory;
        private System.Windows.Forms.Button ButtonFindHostUploadFile;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox HostUploadFile;
        private System.Windows.Forms.Button ButtonFindHostOrderDirectory;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox HostOrderDirectory;
        private System.Windows.Forms.Button ButtonFindHostOrderFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox HostOrderFile;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox TextBoxPassword;
        private System.Windows.Forms.TextBox TextBoxUserId;
        private System.Windows.Forms.Label LabelPassword;
        private System.Windows.Forms.Label LabelUserId;
        private System.Windows.Forms.Label LabelConnectionString;
        private System.Windows.Forms.Button ButtonLogFileDirectory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LogFileDirectory;
        private System.Windows.Forms.CheckBox CheckBoxSqlServerAuthentication;
        private System.Windows.Forms.Button ButtonFindImagesDirectory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ImagesDirectory;
        private System.Windows.Forms.TextBox TextBoxStationToPrint;
        private System.Windows.Forms.TextBox TextBoxOrderToPrint;
        private System.Windows.Forms.TextBox TextBoxMaintenanceFileFilter;
        private System.Windows.Forms.TextBox TextBoxHostOrderFileFilter;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button ButtonDocumentsDirectory;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox DocumentsDirectory;
        private System.Windows.Forms.Button ButtonRootDirectory;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox RootDirectory;
        private System.Windows.Forms.Button ButtonMaintenanceFileDirectory;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox MaintenanceFileDirectory;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button ButtonFindCostCenterFile;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox CostCenterDirectory;
        private System.Windows.Forms.Button ButtonCostCenterDirectory;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox CostCenterFileName;
        private System.Windows.Forms.Button ButtonLanguageDirectory;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox LanguageDirectory;
        private MetroFramework.Controls.MetroButton MBRunUpload;
        private MetroFramework.Controls.MetroButton MBRunLoaderOnce;
    }
}