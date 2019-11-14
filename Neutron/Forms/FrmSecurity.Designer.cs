namespace Neutron.Forms
{
    partial class FrmSecurity
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSecurity));
            this.TabControlSecurity = new System.Windows.Forms.TabControl();
            this.TabPageUsers = new System.Windows.Forms.TabPage();
            this.LabelUserGroupInformation = new System.Windows.Forms.Label();
            this.ButtonSaveUsers = new System.Windows.Forms.Button();
            this.ButtonSelectUsers = new System.Windows.Forms.Button();
            this.ListViewUsers = new System.Windows.Forms.ListView();
            this.TabPageGroups = new System.Windows.Forms.TabPage();
            this.ButtonSaveNewGroup = new System.Windows.Forms.Button();
            this.TextBoxNewGroup = new System.Windows.Forms.TextBox();
            this.LabelNewGroupName = new System.Windows.Forms.Label();
            this.ListViewGroups = new System.Windows.Forms.ListView();
            this.TabPageSecureItems = new System.Windows.Forms.TabPage();
            this.LabelSecureItemsInformation = new System.Windows.Forms.Label();
            this.ButtonSaveSecureItems = new System.Windows.Forms.Button();
            this.ButtonSelectSecureItems = new System.Windows.Forms.Button();
            this.ListViewSecureItems = new System.Windows.Forms.ListView();
            this.TabPageNewUser = new System.Windows.Forms.TabPage();
            this.PanelNewUser = new System.Windows.Forms.Panel();
            this.CheckBoxDisabled = new System.Windows.Forms.CheckBox();
            this.ButtonSaveNewUser = new System.Windows.Forms.Button();
            this.TextBoxPassword = new System.Windows.Forms.TextBox();
            this.LabelPassword = new System.Windows.Forms.Label();
            this.TextBoxLastname = new System.Windows.Forms.TextBox();
            this.TextBoxUsername = new System.Windows.Forms.TextBox();
            this.LabelLastname = new System.Windows.Forms.Label();
            this.LabelUsername = new System.Windows.Forms.Label();
            this.TextBoxFirstname = new System.Windows.Forms.TextBox();
            this.LabelFirstname = new System.Windows.Forms.Label();
            this.TextBoxPin = new System.Windows.Forms.TextBox();
            this.LabelPin = new System.Windows.Forms.Label();
            this.TextBoxEmpId = new System.Windows.Forms.TextBox();
            this.LabelEmpId = new System.Windows.Forms.Label();
            this.TabPageEditUser = new System.Windows.Forms.TabPage();
            this.DataGridView1 = new System.Windows.Forms.DataGridView();
            this.PanelEditUser = new System.Windows.Forms.Panel();
            this.CheckBoxDisabledEditUser = new System.Windows.Forms.CheckBox();
            this.ButtonDeleteEditUser = new System.Windows.Forms.Button();
            this.ButtonClearEditUser = new System.Windows.Forms.Button();
            this.ButtonSaveEditUser = new System.Windows.Forms.Button();
            this.TextBoxPasswordEditUser = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TextBoxLastnameEditUser = new System.Windows.Forms.TextBox();
            this.TextBoxUsernameEditUser = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TextBoxFirstnameEditUser = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TextBoxPinEditUser = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TextBoxEmpIdEditUser = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ComboBoxGroups = new System.Windows.Forms.ComboBox();
            this.LabelSelectGroup = new System.Windows.Forms.Label();
            this.ComboBoxUsers = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.TabControlSecurity.SuspendLayout();
            this.TabPageUsers.SuspendLayout();
            this.TabPageGroups.SuspendLayout();
            this.TabPageSecureItems.SuspendLayout();
            this.TabPageNewUser.SuspendLayout();
            this.PanelNewUser.SuspendLayout();
            this.TabPageEditUser.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView1)).BeginInit();
            this.PanelEditUser.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControlSecurity
            // 
            this.TabControlSecurity.Controls.Add(this.TabPageUsers);
            this.TabControlSecurity.Controls.Add(this.TabPageGroups);
            this.TabControlSecurity.Controls.Add(this.TabPageSecureItems);
            this.TabControlSecurity.Controls.Add(this.TabPageNewUser);
            this.TabControlSecurity.Controls.Add(this.TabPageEditUser);
            this.TabControlSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabControlSecurity.Location = new System.Drawing.Point(8, 45);
            this.TabControlSecurity.Margin = new System.Windows.Forms.Padding(4);
            this.TabControlSecurity.Name = "TabControlSecurity";
            this.TabControlSecurity.SelectedIndex = 0;
            this.TabControlSecurity.Size = new System.Drawing.Size(700, 605);
            this.TabControlSecurity.TabIndex = 2;
            this.TabControlSecurity.SelectedIndexChanged += new System.EventHandler(this.TabControlSecurity_TabIndexChanged);
            this.TabControlSecurity.TabIndexChanged += new System.EventHandler(this.TabControlSecurity_TabIndexChanged);
            // 
            // TabPageUsers
            // 
            this.TabPageUsers.Controls.Add(this.LabelUserGroupInformation);
            this.TabPageUsers.Controls.Add(this.ButtonSaveUsers);
            this.TabPageUsers.Controls.Add(this.ButtonSelectUsers);
            this.TabPageUsers.Controls.Add(this.ListViewUsers);
            this.TabPageUsers.Location = new System.Drawing.Point(4, 29);
            this.TabPageUsers.Name = "TabPageUsers";
            this.TabPageUsers.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageUsers.Size = new System.Drawing.Size(692, 572);
            this.TabPageUsers.TabIndex = 4;
            this.TabPageUsers.Text = "Users";
            this.TabPageUsers.UseVisualStyleBackColor = true;
            // 
            // LabelUserGroupInformation
            // 
            this.LabelUserGroupInformation.Location = new System.Drawing.Point(22, 17);
            this.LabelUserGroupInformation.Name = "LabelUserGroupInformation";
            this.LabelUserGroupInformation.Size = new System.Drawing.Size(648, 29);
            this.LabelUserGroupInformation.TabIndex = 10;
            this.LabelUserGroupInformation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonSaveUsers
            // 
            this.ButtonSaveUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveUsers.Location = new System.Drawing.Point(129, 67);
            this.ButtonSaveUsers.Name = "ButtonSaveUsers";
            this.ButtonSaveUsers.Size = new System.Drawing.Size(108, 33);
            this.ButtonSaveUsers.TabIndex = 9;
            this.ButtonSaveUsers.Text = "Save";
            this.ButtonSaveUsers.UseVisualStyleBackColor = true;
            this.ButtonSaveUsers.Click += new System.EventHandler(this.ButtonSaveUsers_Click);
            // 
            // ButtonSelectUsers
            // 
            this.ButtonSelectUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSelectUsers.Location = new System.Drawing.Point(9, 67);
            this.ButtonSelectUsers.Name = "ButtonSelectUsers";
            this.ButtonSelectUsers.Size = new System.Drawing.Size(108, 33);
            this.ButtonSelectUsers.TabIndex = 5;
            this.ButtonSelectUsers.Text = "Check All";
            this.ButtonSelectUsers.UseVisualStyleBackColor = true;
            this.ButtonSelectUsers.Click += new System.EventHandler(this.ButtonSelectUsers_Click);
            // 
            // ListViewUsers
            // 
            this.ListViewUsers.CheckBoxes = true;
            this.ListViewUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListViewUsers.Location = new System.Drawing.Point(9, 107);
            this.ListViewUsers.Margin = new System.Windows.Forms.Padding(4);
            this.ListViewUsers.Name = "ListViewUsers";
            this.ListViewUsers.Size = new System.Drawing.Size(676, 457);
            this.ListViewUsers.TabIndex = 4;
            this.ListViewUsers.UseCompatibleStateImageBehavior = false;
            this.ListViewUsers.View = System.Windows.Forms.View.List;
            // 
            // TabPageGroups
            // 
            this.TabPageGroups.Controls.Add(this.ButtonSaveNewGroup);
            this.TabPageGroups.Controls.Add(this.TextBoxNewGroup);
            this.TabPageGroups.Controls.Add(this.LabelNewGroupName);
            this.TabPageGroups.Controls.Add(this.ListViewGroups);
            this.TabPageGroups.Location = new System.Drawing.Point(4, 29);
            this.TabPageGroups.Name = "TabPageGroups";
            this.TabPageGroups.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageGroups.Size = new System.Drawing.Size(692, 572);
            this.TabPageGroups.TabIndex = 5;
            this.TabPageGroups.Text = "Security Groups";
            this.TabPageGroups.UseVisualStyleBackColor = true;
            // 
            // ButtonSaveNewGroup
            // 
            this.ButtonSaveNewGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveNewGroup.Location = new System.Drawing.Point(463, 36);
            this.ButtonSaveNewGroup.Name = "ButtonSaveNewGroup";
            this.ButtonSaveNewGroup.Size = new System.Drawing.Size(108, 33);
            this.ButtonSaveNewGroup.TabIndex = 8;
            this.ButtonSaveNewGroup.Text = "Save";
            this.ButtonSaveNewGroup.UseVisualStyleBackColor = true;
            this.ButtonSaveNewGroup.Click += new System.EventHandler(this.ButtonSaveNewGroup_Click);
            // 
            // TextBoxNewGroup
            // 
            this.TextBoxNewGroup.Location = new System.Drawing.Point(294, 39);
            this.TextBoxNewGroup.Name = "TextBoxNewGroup";
            this.TextBoxNewGroup.Size = new System.Drawing.Size(154, 26);
            this.TextBoxNewGroup.TabIndex = 7;
            // 
            // LabelNewGroupName
            // 
            this.LabelNewGroupName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNewGroupName.Location = new System.Drawing.Point(59, 41);
            this.LabelNewGroupName.Name = "LabelNewGroupName";
            this.LabelNewGroupName.Size = new System.Drawing.Size(225, 23);
            this.LabelNewGroupName.TabIndex = 6;
            this.LabelNewGroupName.Text = "New Security Group";
            this.LabelNewGroupName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ListViewGroups
            // 
            this.ListViewGroups.CheckBoxes = true;
            this.ListViewGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListViewGroups.Location = new System.Drawing.Point(9, 107);
            this.ListViewGroups.Margin = new System.Windows.Forms.Padding(4);
            this.ListViewGroups.Name = "ListViewGroups";
            this.ListViewGroups.Size = new System.Drawing.Size(676, 457);
            this.ListViewGroups.TabIndex = 4;
            this.ListViewGroups.UseCompatibleStateImageBehavior = false;
            this.ListViewGroups.View = System.Windows.Forms.View.List;
            // 
            // TabPageSecureItems
            // 
            this.TabPageSecureItems.Controls.Add(this.LabelSecureItemsInformation);
            this.TabPageSecureItems.Controls.Add(this.ButtonSaveSecureItems);
            this.TabPageSecureItems.Controls.Add(this.ButtonSelectSecureItems);
            this.TabPageSecureItems.Controls.Add(this.ListViewSecureItems);
            this.TabPageSecureItems.Location = new System.Drawing.Point(4, 29);
            this.TabPageSecureItems.Name = "TabPageSecureItems";
            this.TabPageSecureItems.Size = new System.Drawing.Size(692, 572);
            this.TabPageSecureItems.TabIndex = 7;
            this.TabPageSecureItems.Text = "Secure Items";
            this.TabPageSecureItems.UseVisualStyleBackColor = true;
            // 
            // LabelSecureItemsInformation
            // 
            this.LabelSecureItemsInformation.Location = new System.Drawing.Point(22, 17);
            this.LabelSecureItemsInformation.Name = "LabelSecureItemsInformation";
            this.LabelSecureItemsInformation.Size = new System.Drawing.Size(648, 29);
            this.LabelSecureItemsInformation.TabIndex = 12;
            this.LabelSecureItemsInformation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonSaveSecureItems
            // 
            this.ButtonSaveSecureItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveSecureItems.Location = new System.Drawing.Point(129, 67);
            this.ButtonSaveSecureItems.Name = "ButtonSaveSecureItems";
            this.ButtonSaveSecureItems.Size = new System.Drawing.Size(108, 33);
            this.ButtonSaveSecureItems.TabIndex = 11;
            this.ButtonSaveSecureItems.Text = "Save";
            this.ButtonSaveSecureItems.UseVisualStyleBackColor = true;
            this.ButtonSaveSecureItems.Click += new System.EventHandler(this.ButtonSaveSecureItems_Click);
            // 
            // ButtonSelectSecureItems
            // 
            this.ButtonSelectSecureItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSelectSecureItems.Location = new System.Drawing.Point(9, 67);
            this.ButtonSelectSecureItems.Name = "ButtonSelectSecureItems";
            this.ButtonSelectSecureItems.Size = new System.Drawing.Size(108, 33);
            this.ButtonSelectSecureItems.TabIndex = 10;
            this.ButtonSelectSecureItems.Text = "Check All";
            this.ButtonSelectSecureItems.UseVisualStyleBackColor = true;
            this.ButtonSelectSecureItems.Click += new System.EventHandler(this.ButtonSelectSecureItems_Click);
            // 
            // ListViewSecureItems
            // 
            this.ListViewSecureItems.CheckBoxes = true;
            this.ListViewSecureItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListViewSecureItems.Location = new System.Drawing.Point(9, 107);
            this.ListViewSecureItems.Margin = new System.Windows.Forms.Padding(4);
            this.ListViewSecureItems.Name = "ListViewSecureItems";
            this.ListViewSecureItems.Size = new System.Drawing.Size(676, 457);
            this.ListViewSecureItems.TabIndex = 5;
            this.ListViewSecureItems.UseCompatibleStateImageBehavior = false;
            this.ListViewSecureItems.View = System.Windows.Forms.View.List;
            // 
            // TabPageNewUser
            // 
            this.TabPageNewUser.Controls.Add(this.PanelNewUser);
            this.TabPageNewUser.Location = new System.Drawing.Point(4, 29);
            this.TabPageNewUser.Name = "TabPageNewUser";
            this.TabPageNewUser.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageNewUser.Size = new System.Drawing.Size(692, 572);
            this.TabPageNewUser.TabIndex = 6;
            this.TabPageNewUser.Text = "New User";
            this.TabPageNewUser.UseVisualStyleBackColor = true;
            // 
            // PanelNewUser
            // 
            this.PanelNewUser.BackColor = System.Drawing.Color.LightSteelBlue;
            this.PanelNewUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PanelNewUser.Controls.Add(this.CheckBoxDisabled);
            this.PanelNewUser.Controls.Add(this.ButtonSaveNewUser);
            this.PanelNewUser.Controls.Add(this.TextBoxPassword);
            this.PanelNewUser.Controls.Add(this.LabelPassword);
            this.PanelNewUser.Controls.Add(this.TextBoxLastname);
            this.PanelNewUser.Controls.Add(this.TextBoxUsername);
            this.PanelNewUser.Controls.Add(this.LabelLastname);
            this.PanelNewUser.Controls.Add(this.LabelUsername);
            this.PanelNewUser.Controls.Add(this.TextBoxFirstname);
            this.PanelNewUser.Controls.Add(this.LabelFirstname);
            this.PanelNewUser.Controls.Add(this.TextBoxPin);
            this.PanelNewUser.Controls.Add(this.LabelPin);
            this.PanelNewUser.Controls.Add(this.TextBoxEmpId);
            this.PanelNewUser.Controls.Add(this.LabelEmpId);
            this.PanelNewUser.Location = new System.Drawing.Point(76, 36);
            this.PanelNewUser.Name = "PanelNewUser";
            this.PanelNewUser.Size = new System.Drawing.Size(541, 210);
            this.PanelNewUser.TabIndex = 3;
            // 
            // CheckBoxDisabled
            // 
            this.CheckBoxDisabled.AutoSize = true;
            this.CheckBoxDisabled.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CheckBoxDisabled.Location = new System.Drawing.Point(41, 136);
            this.CheckBoxDisabled.Name = "CheckBoxDisabled";
            this.CheckBoxDisabled.Size = new System.Drawing.Size(98, 24);
            this.CheckBoxDisabled.TabIndex = 6;
            this.CheckBoxDisabled.Text = "Disabled";
            this.CheckBoxDisabled.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBoxDisabled.UseVisualStyleBackColor = true;
            // 
            // ButtonSaveNewUser
            // 
            this.ButtonSaveNewUser.Location = new System.Drawing.Point(424, 159);
            this.ButtonSaveNewUser.Name = "ButtonSaveNewUser";
            this.ButtonSaveNewUser.Size = new System.Drawing.Size(103, 31);
            this.ButtonSaveNewUser.TabIndex = 7;
            this.ButtonSaveNewUser.Text = "Save";
            this.ButtonSaveNewUser.UseVisualStyleBackColor = true;
            this.ButtonSaveNewUser.Click += new System.EventHandler(this.ButtonSaveNewUser_Click);
            // 
            // TextBoxPassword
            // 
            this.TextBoxPassword.Location = new System.Drawing.Point(392, 99);
            this.TextBoxPassword.Name = "TextBoxPassword";
            this.TextBoxPassword.Size = new System.Drawing.Size(135, 26);
            this.TextBoxPassword.TabIndex = 5;
            // 
            // LabelPassword
            // 
            this.LabelPassword.Location = new System.Drawing.Point(278, 102);
            this.LabelPassword.Name = "LabelPassword";
            this.LabelPassword.Size = new System.Drawing.Size(108, 20);
            this.LabelPassword.TabIndex = 3;
            this.LabelPassword.Text = "Password";
            this.LabelPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxLastname
            // 
            this.TextBoxLastname.Location = new System.Drawing.Point(392, 67);
            this.TextBoxLastname.Name = "TextBoxLastname";
            this.TextBoxLastname.Size = new System.Drawing.Size(135, 26);
            this.TextBoxLastname.TabIndex = 3;
            // 
            // TextBoxUsername
            // 
            this.TextBoxUsername.Location = new System.Drawing.Point(124, 99);
            this.TextBoxUsername.Name = "TextBoxUsername";
            this.TextBoxUsername.Size = new System.Drawing.Size(148, 26);
            this.TextBoxUsername.TabIndex = 4;
            // 
            // LabelLastname
            // 
            this.LabelLastname.Location = new System.Drawing.Point(278, 70);
            this.LabelLastname.Name = "LabelLastname";
            this.LabelLastname.Size = new System.Drawing.Size(108, 20);
            this.LabelLastname.TabIndex = 4;
            this.LabelLastname.Text = "Last Name";
            this.LabelLastname.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelUsername
            // 
            this.LabelUsername.Location = new System.Drawing.Point(10, 102);
            this.LabelUsername.Name = "LabelUsername";
            this.LabelUsername.Size = new System.Drawing.Size(108, 20);
            this.LabelUsername.TabIndex = 5;
            this.LabelUsername.Text = "User Name";
            this.LabelUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxFirstname
            // 
            this.TextBoxFirstname.Location = new System.Drawing.Point(124, 67);
            this.TextBoxFirstname.Name = "TextBoxFirstname";
            this.TextBoxFirstname.Size = new System.Drawing.Size(148, 26);
            this.TextBoxFirstname.TabIndex = 2;
            // 
            // LabelFirstname
            // 
            this.LabelFirstname.Location = new System.Drawing.Point(10, 70);
            this.LabelFirstname.Name = "LabelFirstname";
            this.LabelFirstname.Size = new System.Drawing.Size(108, 20);
            this.LabelFirstname.TabIndex = 6;
            this.LabelFirstname.Text = "First Name";
            this.LabelFirstname.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxPin
            // 
            this.TextBoxPin.Location = new System.Drawing.Point(392, 35);
            this.TextBoxPin.Name = "TextBoxPin";
            this.TextBoxPin.Size = new System.Drawing.Size(100, 26);
            this.TextBoxPin.TabIndex = 1;
            // 
            // LabelPin
            // 
            this.LabelPin.Location = new System.Drawing.Point(278, 38);
            this.LabelPin.Name = "LabelPin";
            this.LabelPin.Size = new System.Drawing.Size(108, 20);
            this.LabelPin.TabIndex = 7;
            this.LabelPin.Text = "Pin";
            this.LabelPin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxEmpId
            // 
            this.TextBoxEmpId.Location = new System.Drawing.Point(124, 35);
            this.TextBoxEmpId.Name = "TextBoxEmpId";
            this.TextBoxEmpId.Size = new System.Drawing.Size(100, 26);
            this.TextBoxEmpId.TabIndex = 0;
            // 
            // LabelEmpId
            // 
            this.LabelEmpId.Location = new System.Drawing.Point(10, 38);
            this.LabelEmpId.Name = "LabelEmpId";
            this.LabelEmpId.Size = new System.Drawing.Size(108, 23);
            this.LabelEmpId.TabIndex = 8;
            this.LabelEmpId.Text = "Employee Id";
            this.LabelEmpId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TabPageEditUser
            // 
            this.TabPageEditUser.Controls.Add(this.DataGridView1);
            this.TabPageEditUser.Controls.Add(this.PanelEditUser);
            this.TabPageEditUser.Location = new System.Drawing.Point(4, 29);
            this.TabPageEditUser.Name = "TabPageEditUser";
            this.TabPageEditUser.Size = new System.Drawing.Size(692, 572);
            this.TabPageEditUser.TabIndex = 8;
            this.TabPageEditUser.Text = "Edit User";
            this.TabPageEditUser.UseVisualStyleBackColor = true;
            // 
            // DataGridView1
            // 
            this.DataGridView1.AllowUserToAddRows = false;
            this.DataGridView1.AllowUserToDeleteRows = false;
            this.DataGridView1.AllowUserToOrderColumns = true;
            this.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView1.Location = new System.Drawing.Point(14, 262);
            this.DataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView1.Name = "DataGridView1";
            this.DataGridView1.ReadOnly = true;
            this.DataGridView1.RowTemplate.Height = 24;
            this.DataGridView1.Size = new System.Drawing.Size(665, 298);
            this.DataGridView1.TabIndex = 5;
            // 
            // PanelEditUser
            // 
            this.PanelEditUser.BackColor = System.Drawing.Color.LightSteelBlue;
            this.PanelEditUser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PanelEditUser.Controls.Add(this.ComboBoxUsers);
            this.PanelEditUser.Controls.Add(this.CheckBoxDisabledEditUser);
            this.PanelEditUser.Controls.Add(this.ButtonDeleteEditUser);
            this.PanelEditUser.Controls.Add(this.ButtonClearEditUser);
            this.PanelEditUser.Controls.Add(this.ButtonSaveEditUser);
            this.PanelEditUser.Controls.Add(this.TextBoxPasswordEditUser);
            this.PanelEditUser.Controls.Add(this.label1);
            this.PanelEditUser.Controls.Add(this.TextBoxLastnameEditUser);
            this.PanelEditUser.Controls.Add(this.TextBoxUsernameEditUser);
            this.PanelEditUser.Controls.Add(this.label2);
            this.PanelEditUser.Controls.Add(this.label3);
            this.PanelEditUser.Controls.Add(this.TextBoxFirstnameEditUser);
            this.PanelEditUser.Controls.Add(this.label7);
            this.PanelEditUser.Controls.Add(this.label4);
            this.PanelEditUser.Controls.Add(this.TextBoxPinEditUser);
            this.PanelEditUser.Controls.Add(this.label5);
            this.PanelEditUser.Controls.Add(this.TextBoxEmpIdEditUser);
            this.PanelEditUser.Controls.Add(this.label6);
            this.PanelEditUser.Location = new System.Drawing.Point(76, 17);
            this.PanelEditUser.Name = "PanelEditUser";
            this.PanelEditUser.Size = new System.Drawing.Size(541, 229);
            this.PanelEditUser.TabIndex = 4;
            // 
            // CheckBoxDisabledEditUser
            // 
            this.CheckBoxDisabledEditUser.AutoSize = true;
            this.CheckBoxDisabledEditUser.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CheckBoxDisabledEditUser.Location = new System.Drawing.Point(41, 163);
            this.CheckBoxDisabledEditUser.Name = "CheckBoxDisabledEditUser";
            this.CheckBoxDisabledEditUser.Size = new System.Drawing.Size(98, 24);
            this.CheckBoxDisabledEditUser.TabIndex = 6;
            this.CheckBoxDisabledEditUser.Text = "Disabled";
            this.CheckBoxDisabledEditUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBoxDisabledEditUser.UseVisualStyleBackColor = true;
            // 
            // ButtonDeleteEditUser
            // 
            this.ButtonDeleteEditUser.Enabled = false;
            this.ButtonDeleteEditUser.Location = new System.Drawing.Point(206, 186);
            this.ButtonDeleteEditUser.Name = "ButtonDeleteEditUser";
            this.ButtonDeleteEditUser.Size = new System.Drawing.Size(103, 31);
            this.ButtonDeleteEditUser.TabIndex = 7;
            this.ButtonDeleteEditUser.Text = "Delete";
            this.ButtonDeleteEditUser.UseVisualStyleBackColor = true;
            this.ButtonDeleteEditUser.Click += new System.EventHandler(this.ButtonDeleteEditUser_Click);
            // 
            // ButtonClearEditUser
            // 
            this.ButtonClearEditUser.Location = new System.Drawing.Point(315, 186);
            this.ButtonClearEditUser.Name = "ButtonClearEditUser";
            this.ButtonClearEditUser.Size = new System.Drawing.Size(103, 31);
            this.ButtonClearEditUser.TabIndex = 7;
            this.ButtonClearEditUser.Text = "Clear";
            this.ButtonClearEditUser.UseVisualStyleBackColor = true;
            this.ButtonClearEditUser.Click += new System.EventHandler(this.ButtonClearEditUserFields_Click);
            // 
            // ButtonSaveEditUser
            // 
            this.ButtonSaveEditUser.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonSaveEditUser.Location = new System.Drawing.Point(424, 186);
            this.ButtonSaveEditUser.Name = "ButtonSaveEditUser";
            this.ButtonSaveEditUser.Size = new System.Drawing.Size(103, 31);
            this.ButtonSaveEditUser.TabIndex = 7;
            this.ButtonSaveEditUser.Text = "Save";
            this.ButtonSaveEditUser.UseVisualStyleBackColor = true;
            this.ButtonSaveEditUser.Click += new System.EventHandler(this.ButtonSaveEditUser_Click);
            // 
            // TextBoxPasswordEditUser
            // 
            this.TextBoxPasswordEditUser.Location = new System.Drawing.Point(392, 126);
            this.TextBoxPasswordEditUser.Name = "TextBoxPasswordEditUser";
            this.TextBoxPasswordEditUser.Size = new System.Drawing.Size(135, 26);
            this.TextBoxPasswordEditUser.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(278, 129);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Password";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxLastnameEditUser
            // 
            this.TextBoxLastnameEditUser.Location = new System.Drawing.Point(392, 94);
            this.TextBoxLastnameEditUser.Name = "TextBoxLastnameEditUser";
            this.TextBoxLastnameEditUser.Size = new System.Drawing.Size(135, 26);
            this.TextBoxLastnameEditUser.TabIndex = 3;
            // 
            // TextBoxUsernameEditUser
            // 
            this.TextBoxUsernameEditUser.Location = new System.Drawing.Point(124, 126);
            this.TextBoxUsernameEditUser.Name = "TextBoxUsernameEditUser";
            this.TextBoxUsernameEditUser.Size = new System.Drawing.Size(148, 26);
            this.TextBoxUsernameEditUser.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(278, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Last Name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(10, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "User Name";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxFirstnameEditUser
            // 
            this.TextBoxFirstnameEditUser.Location = new System.Drawing.Point(124, 94);
            this.TextBoxFirstnameEditUser.Name = "TextBoxFirstnameEditUser";
            this.TextBoxFirstnameEditUser.Size = new System.Drawing.Size(148, 26);
            this.TextBoxFirstnameEditUser.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(10, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "First Name";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxPinEditUser
            // 
            this.TextBoxPinEditUser.Location = new System.Drawing.Point(392, 62);
            this.TextBoxPinEditUser.Name = "TextBoxPinEditUser";
            this.TextBoxPinEditUser.Size = new System.Drawing.Size(100, 26);
            this.TextBoxPinEditUser.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(278, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 20);
            this.label5.TabIndex = 7;
            this.label5.Text = "Pin";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxEmpIdEditUser
            // 
            this.TextBoxEmpIdEditUser.Location = new System.Drawing.Point(124, 62);
            this.TextBoxEmpIdEditUser.Name = "TextBoxEmpIdEditUser";
            this.TextBoxEmpIdEditUser.Size = new System.Drawing.Size(100, 26);
            this.TextBoxEmpIdEditUser.TabIndex = 0;
            this.TextBoxEmpIdEditUser.Enter += new System.EventHandler(this.TextBoxEmpIdEditUser_Enter);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(10, 65);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 23);
            this.label6.TabIndex = 8;
            this.label6.Text = "Employee Id";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ComboBoxGroups
            // 
            this.ComboBoxGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboBoxGroups.FormattingEnabled = true;
            this.ComboBoxGroups.Location = new System.Drawing.Point(378, 8);
            this.ComboBoxGroups.Name = "ComboBoxGroups";
            this.ComboBoxGroups.Size = new System.Drawing.Size(151, 28);
            this.ComboBoxGroups.TabIndex = 3;
            this.ComboBoxGroups.SelectedIndexChanged += new System.EventHandler(this.ComboBoxGroups_SelectedIndexChanged);
            // 
            // LabelSelectGroup
            // 
            this.LabelSelectGroup.AutoSize = true;
            this.LabelSelectGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSelectGroup.Location = new System.Drawing.Point(187, 12);
            this.LabelSelectGroup.Name = "LabelSelectGroup";
            this.LabelSelectGroup.Size = new System.Drawing.Size(185, 20);
            this.LabelSelectGroup.TabIndex = 5;
            this.LabelSelectGroup.Text = "Select Security Group";
            this.LabelSelectGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ComboBoxUsers
            // 
            this.ComboBoxUsers.FormattingEnabled = true;
            this.ComboBoxUsers.Location = new System.Drawing.Point(133, 13);
            this.ComboBoxUsers.Name = "ComboBoxUsers";
            this.ComboBoxUsers.Size = new System.Drawing.Size(285, 28);
            this.ComboBoxUsers.TabIndex = 9;
            this.ComboBoxUsers.SelectedIndexChanged += new System.EventHandler(this.ComboBoxUsers_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(10, 197);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(157, 20);
            this.label7.TabIndex = 6;
            this.label7.Text = "Last 50 Records";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FrmSecurity
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(716, 654);
            this.Controls.Add(this.LabelSelectGroup);
            this.Controls.Add(this.ComboBoxGroups);
            this.Controls.Add(this.TabControlSecurity);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmSecurity";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Security Control";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.TabControlSecurity.ResumeLayout(false);
            this.TabPageUsers.ResumeLayout(false);
            this.TabPageGroups.ResumeLayout(false);
            this.TabPageGroups.PerformLayout();
            this.TabPageSecureItems.ResumeLayout(false);
            this.TabPageNewUser.ResumeLayout(false);
            this.PanelNewUser.ResumeLayout(false);
            this.PanelNewUser.PerformLayout();
            this.TabPageEditUser.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView1)).EndInit();
            this.PanelEditUser.ResumeLayout(false);
            this.PanelEditUser.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TabControl TabControlSecurity;
        private System.Windows.Forms.TabPage TabPageUsers;
        private System.Windows.Forms.ListView ListViewUsers;
        private System.Windows.Forms.ComboBox ComboBoxGroups;
        private System.Windows.Forms.TabPage TabPageGroups;
        private System.Windows.Forms.ListView ListViewGroups;
        private System.Windows.Forms.Button ButtonSelectUsers;
        private System.Windows.Forms.Label LabelSelectGroup;
        private System.Windows.Forms.Button ButtonSaveNewGroup;
        private System.Windows.Forms.TextBox TextBoxNewGroup;
        private System.Windows.Forms.Label LabelNewGroupName;
        private System.Windows.Forms.Button ButtonSaveUsers;
        private System.Windows.Forms.TabPage TabPageNewUser;
        private System.Windows.Forms.Panel PanelNewUser;
        private System.Windows.Forms.Button ButtonSaveNewUser;
        private System.Windows.Forms.TextBox TextBoxPassword;
        private System.Windows.Forms.Label LabelPassword;
        private System.Windows.Forms.TextBox TextBoxLastname;
        private System.Windows.Forms.TextBox TextBoxUsername;
        private System.Windows.Forms.Label LabelLastname;
        private System.Windows.Forms.Label LabelUsername;
        private System.Windows.Forms.TextBox TextBoxFirstname;
        private System.Windows.Forms.Label LabelFirstname;
        private System.Windows.Forms.TextBox TextBoxPin;
        private System.Windows.Forms.Label LabelPin;
        private System.Windows.Forms.TextBox TextBoxEmpId;
        private System.Windows.Forms.Label LabelEmpId;
        private System.Windows.Forms.CheckBox CheckBoxDisabled;
        private System.Windows.Forms.TabPage TabPageSecureItems;
        private System.Windows.Forms.Button ButtonSaveSecureItems;
        private System.Windows.Forms.Button ButtonSelectSecureItems;
        private System.Windows.Forms.ListView ListViewSecureItems;
        private System.Windows.Forms.Label LabelUserGroupInformation;
        private System.Windows.Forms.Label LabelSecureItemsInformation;
        private System.Windows.Forms.TabPage TabPageEditUser;
        private System.Windows.Forms.Panel PanelEditUser;
        private System.Windows.Forms.CheckBox CheckBoxDisabledEditUser;
        private System.Windows.Forms.Button ButtonSaveEditUser;
        private System.Windows.Forms.TextBox TextBoxPasswordEditUser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TextBoxLastnameEditUser;
        private System.Windows.Forms.TextBox TextBoxUsernameEditUser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TextBoxFirstnameEditUser;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TextBoxPinEditUser;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TextBoxEmpIdEditUser;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button ButtonClearEditUser;
        private System.Windows.Forms.DataGridView DataGridView1;
        private System.Windows.Forms.Button ButtonDeleteEditUser;
        private System.Windows.Forms.ComboBox ComboBoxUsers;
        private System.Windows.Forms.Label label7;
    }
}

