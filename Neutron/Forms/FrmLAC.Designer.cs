namespace Neutron.Forms
{
    partial class FrmLAC
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLAC));
            this.TabControlLAC = new System.Windows.Forms.TabControl();
            this.TabPageUsers = new System.Windows.Forms.TabPage();
            this.LabelUserRoleInformation = new System.Windows.Forms.Label();
            this.ButtonSaveUsers = new System.Windows.Forms.Button();
            this.ButtonSelectUsers = new System.Windows.Forms.Button();
            this.ListViewUsers = new System.Windows.Forms.ListView();
            this.TabPageRoles = new System.Windows.Forms.TabPage();
            this.ButtonSaveNewRole = new System.Windows.Forms.Button();
            this.TextBoxNewRole = new System.Windows.Forms.TextBox();
            this.LabelNewRoleName = new System.Windows.Forms.Label();
            this.ListViewRoles = new System.Windows.Forms.ListView();
            this.TabPageDevice1 = new System.Windows.Forms.TabPage();
            this.LabelDevice1Information = new System.Windows.Forms.Label();
            this.ButtonSaveDevice1 = new System.Windows.Forms.Button();
            this.ButtonSelectDevice1 = new System.Windows.Forms.Button();
            this.ListViewDevice1 = new System.Windows.Forms.ListView();
            this.TabPageDevice2 = new System.Windows.Forms.TabPage();
            this.LabelDevice2Information = new System.Windows.Forms.Label();
            this.ButtonSaveDevice2 = new System.Windows.Forms.Button();
            this.ButtonSelectDevice2 = new System.Windows.Forms.Button();
            this.ListViewDevice2 = new System.Windows.Forms.ListView();
            this.TabPageDevice3 = new System.Windows.Forms.TabPage();
            this.LabelDevice3Information = new System.Windows.Forms.Label();
            this.ButtonSaveDevice3 = new System.Windows.Forms.Button();
            this.ButtonSelectDevice3 = new System.Windows.Forms.Button();
            this.ListViewDevice3 = new System.Windows.Forms.ListView();
            this.TabPageDevice4 = new System.Windows.Forms.TabPage();
            this.LabelDevice4Information = new System.Windows.Forms.Label();
            this.ButtonSaveDevice4 = new System.Windows.Forms.Button();
            this.ButtonSelectDevice4 = new System.Windows.Forms.Button();
            this.ListViewDevice4 = new System.Windows.Forms.ListView();
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
            this.ComboBoxRoles = new System.Windows.Forms.ComboBox();
            this.LabelSelectRole = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ComboBoxStation = new System.Windows.Forms.ComboBox();
            this.TabControlLAC.SuspendLayout();
            this.TabPageUsers.SuspendLayout();
            this.TabPageRoles.SuspendLayout();
            this.TabPageDevice1.SuspendLayout();
            this.TabPageDevice2.SuspendLayout();
            this.TabPageDevice3.SuspendLayout();
            this.TabPageDevice4.SuspendLayout();
            this.TabPageNewUser.SuspendLayout();
            this.PanelNewUser.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControlLAC
            // 
            this.TabControlLAC.Controls.Add(this.TabPageUsers);
            this.TabControlLAC.Controls.Add(this.TabPageRoles);
            this.TabControlLAC.Controls.Add(this.TabPageDevice1);
            this.TabControlLAC.Controls.Add(this.TabPageDevice2);
            this.TabControlLAC.Controls.Add(this.TabPageDevice3);
            this.TabControlLAC.Controls.Add(this.TabPageDevice4);
            this.TabControlLAC.Controls.Add(this.TabPageNewUser);
            this.TabControlLAC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabControlLAC.Location = new System.Drawing.Point(8, 105);
            this.TabControlLAC.Margin = new System.Windows.Forms.Padding(4);
            this.TabControlLAC.Name = "TabControlLAC";
            this.TabControlLAC.SelectedIndex = 0;
            this.TabControlLAC.Size = new System.Drawing.Size(700, 545);
            this.TabControlLAC.TabIndex = 2;
            this.TabControlLAC.SelectedIndexChanged += new System.EventHandler(this.TabControlLAC_SelectedIndexChanged);
            this.TabControlLAC.TabIndexChanged += new System.EventHandler(this.TabControlLAC_TabIndexChanged);
            // 
            // TabPageUsers
            // 
            this.TabPageUsers.Controls.Add(this.LabelUserRoleInformation);
            this.TabPageUsers.Controls.Add(this.ButtonSaveUsers);
            this.TabPageUsers.Controls.Add(this.ButtonSelectUsers);
            this.TabPageUsers.Controls.Add(this.ListViewUsers);
            this.TabPageUsers.Location = new System.Drawing.Point(4, 29);
            this.TabPageUsers.Name = "TabPageUsers";
            this.TabPageUsers.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageUsers.Size = new System.Drawing.Size(692, 512);
            this.TabPageUsers.TabIndex = 4;
            this.TabPageUsers.Text = "Users";
            this.TabPageUsers.UseVisualStyleBackColor = true;
            // 
            // LabelUserRoleInformation
            // 
            this.LabelUserRoleInformation.Location = new System.Drawing.Point(22, 17);
            this.LabelUserRoleInformation.Name = "LabelUserRoleInformation";
            this.LabelUserRoleInformation.Size = new System.Drawing.Size(648, 29);
            this.LabelUserRoleInformation.TabIndex = 11;
            this.LabelUserRoleInformation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.ListViewUsers.Size = new System.Drawing.Size(676, 384);
            this.ListViewUsers.TabIndex = 4;
            this.ListViewUsers.UseCompatibleStateImageBehavior = false;
            this.ListViewUsers.View = System.Windows.Forms.View.List;
            // 
            // TabPageRoles
            // 
            this.TabPageRoles.Controls.Add(this.ButtonSaveNewRole);
            this.TabPageRoles.Controls.Add(this.TextBoxNewRole);
            this.TabPageRoles.Controls.Add(this.LabelNewRoleName);
            this.TabPageRoles.Controls.Add(this.ListViewRoles);
            this.TabPageRoles.Location = new System.Drawing.Point(4, 29);
            this.TabPageRoles.Name = "TabPageRoles";
            this.TabPageRoles.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageRoles.Size = new System.Drawing.Size(692, 512);
            this.TabPageRoles.TabIndex = 5;
            this.TabPageRoles.Text = "Roles";
            this.TabPageRoles.UseVisualStyleBackColor = true;
            // 
            // ButtonSaveNewRole
            // 
            this.ButtonSaveNewRole.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveNewRole.Location = new System.Drawing.Point(463, 36);
            this.ButtonSaveNewRole.Name = "ButtonSaveNewRole";
            this.ButtonSaveNewRole.Size = new System.Drawing.Size(108, 33);
            this.ButtonSaveNewRole.TabIndex = 8;
            this.ButtonSaveNewRole.Text = "Save";
            this.ButtonSaveNewRole.UseVisualStyleBackColor = true;
            this.ButtonSaveNewRole.Click += new System.EventHandler(this.ButtonSaveNewRole_Click);
            // 
            // TextBoxNewRole
            // 
            this.TextBoxNewRole.Location = new System.Drawing.Point(294, 39);
            this.TextBoxNewRole.Name = "TextBoxNewRole";
            this.TextBoxNewRole.Size = new System.Drawing.Size(154, 26);
            this.TextBoxNewRole.TabIndex = 7;
            // 
            // LabelNewRoleName
            // 
            this.LabelNewRoleName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelNewRoleName.Location = new System.Drawing.Point(121, 41);
            this.LabelNewRoleName.Name = "LabelNewRoleName";
            this.LabelNewRoleName.Size = new System.Drawing.Size(163, 23);
            this.LabelNewRoleName.TabIndex = 6;
            this.LabelNewRoleName.Text = "New Role Name";
            this.LabelNewRoleName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ListViewRoles
            // 
            this.ListViewRoles.CheckBoxes = true;
            this.ListViewRoles.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListViewRoles.Location = new System.Drawing.Point(9, 107);
            this.ListViewRoles.Margin = new System.Windows.Forms.Padding(4);
            this.ListViewRoles.Name = "ListViewRoles";
            this.ListViewRoles.Size = new System.Drawing.Size(676, 384);
            this.ListViewRoles.TabIndex = 4;
            this.ListViewRoles.UseCompatibleStateImageBehavior = false;
            this.ListViewRoles.View = System.Windows.Forms.View.List;
            // 
            // TabPageDevice1
            // 
            this.TabPageDevice1.Controls.Add(this.LabelDevice1Information);
            this.TabPageDevice1.Controls.Add(this.ButtonSaveDevice1);
            this.TabPageDevice1.Controls.Add(this.ButtonSelectDevice1);
            this.TabPageDevice1.Controls.Add(this.ListViewDevice1);
            this.TabPageDevice1.Location = new System.Drawing.Point(4, 29);
            this.TabPageDevice1.Margin = new System.Windows.Forms.Padding(4);
            this.TabPageDevice1.Name = "TabPageDevice1";
            this.TabPageDevice1.Padding = new System.Windows.Forms.Padding(4);
            this.TabPageDevice1.Size = new System.Drawing.Size(692, 512);
            this.TabPageDevice1.TabIndex = 0;
            this.TabPageDevice1.Text = "Device 1";
            this.TabPageDevice1.UseVisualStyleBackColor = true;
            // 
            // LabelDevice1Information
            // 
            this.LabelDevice1Information.Location = new System.Drawing.Point(22, 17);
            this.LabelDevice1Information.Name = "LabelDevice1Information";
            this.LabelDevice1Information.Size = new System.Drawing.Size(648, 29);
            this.LabelDevice1Information.TabIndex = 13;
            this.LabelDevice1Information.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonSaveDevice1
            // 
            this.ButtonSaveDevice1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveDevice1.Location = new System.Drawing.Point(129, 67);
            this.ButtonSaveDevice1.Name = "ButtonSaveDevice1";
            this.ButtonSaveDevice1.Size = new System.Drawing.Size(108, 33);
            this.ButtonSaveDevice1.TabIndex = 9;
            this.ButtonSaveDevice1.Text = "Save";
            this.ButtonSaveDevice1.UseVisualStyleBackColor = true;
            this.ButtonSaveDevice1.Click += new System.EventHandler(this.ButtonSaveDevice1_Click);
            // 
            // ButtonSelectDevice1
            // 
            this.ButtonSelectDevice1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSelectDevice1.Location = new System.Drawing.Point(9, 67);
            this.ButtonSelectDevice1.Name = "ButtonSelectDevice1";
            this.ButtonSelectDevice1.Size = new System.Drawing.Size(108, 33);
            this.ButtonSelectDevice1.TabIndex = 3;
            this.ButtonSelectDevice1.Text = "Check All";
            this.ButtonSelectDevice1.UseVisualStyleBackColor = true;
            this.ButtonSelectDevice1.Click += new System.EventHandler(this.ButtonSelectDevice1_Click);
            // 
            // ListViewDevice1
            // 
            this.ListViewDevice1.CheckBoxes = true;
            this.ListViewDevice1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListViewDevice1.Location = new System.Drawing.Point(9, 107);
            this.ListViewDevice1.Margin = new System.Windows.Forms.Padding(4);
            this.ListViewDevice1.Name = "ListViewDevice1";
            this.ListViewDevice1.Size = new System.Drawing.Size(676, 384);
            this.ListViewDevice1.TabIndex = 2;
            this.ListViewDevice1.UseCompatibleStateImageBehavior = false;
            this.ListViewDevice1.View = System.Windows.Forms.View.List;
            this.ListViewDevice1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.ListViewDevice1_ItemChecked);
            // 
            // TabPageDevice2
            // 
            this.TabPageDevice2.Controls.Add(this.LabelDevice2Information);
            this.TabPageDevice2.Controls.Add(this.ButtonSaveDevice2);
            this.TabPageDevice2.Controls.Add(this.ButtonSelectDevice2);
            this.TabPageDevice2.Controls.Add(this.ListViewDevice2);
            this.TabPageDevice2.Location = new System.Drawing.Point(4, 29);
            this.TabPageDevice2.Margin = new System.Windows.Forms.Padding(4);
            this.TabPageDevice2.Name = "TabPageDevice2";
            this.TabPageDevice2.Padding = new System.Windows.Forms.Padding(4);
            this.TabPageDevice2.Size = new System.Drawing.Size(692, 512);
            this.TabPageDevice2.TabIndex = 1;
            this.TabPageDevice2.Text = "Device 2";
            this.TabPageDevice2.UseVisualStyleBackColor = true;
            // 
            // LabelDevice2Information
            // 
            this.LabelDevice2Information.Location = new System.Drawing.Point(22, 17);
            this.LabelDevice2Information.Name = "LabelDevice2Information";
            this.LabelDevice2Information.Size = new System.Drawing.Size(648, 29);
            this.LabelDevice2Information.TabIndex = 14;
            this.LabelDevice2Information.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonSaveDevice2
            // 
            this.ButtonSaveDevice2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveDevice2.Location = new System.Drawing.Point(130, 67);
            this.ButtonSaveDevice2.Name = "ButtonSaveDevice2";
            this.ButtonSaveDevice2.Size = new System.Drawing.Size(108, 33);
            this.ButtonSaveDevice2.TabIndex = 9;
            this.ButtonSaveDevice2.Text = "Save";
            this.ButtonSaveDevice2.UseVisualStyleBackColor = true;
            this.ButtonSaveDevice2.Click += new System.EventHandler(this.ButtonSaveDevice2_Click);
            // 
            // ButtonSelectDevice2
            // 
            this.ButtonSelectDevice2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSelectDevice2.Location = new System.Drawing.Point(10, 67);
            this.ButtonSelectDevice2.Name = "ButtonSelectDevice2";
            this.ButtonSelectDevice2.Size = new System.Drawing.Size(108, 33);
            this.ButtonSelectDevice2.TabIndex = 4;
            this.ButtonSelectDevice2.Text = "Check All";
            this.ButtonSelectDevice2.UseVisualStyleBackColor = true;
            this.ButtonSelectDevice2.Click += new System.EventHandler(this.ButtonSelectDevice2_Click);
            // 
            // ListViewDevice2
            // 
            this.ListViewDevice2.CheckBoxes = true;
            this.ListViewDevice2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListViewDevice2.Location = new System.Drawing.Point(9, 107);
            this.ListViewDevice2.Margin = new System.Windows.Forms.Padding(4);
            this.ListViewDevice2.Name = "ListViewDevice2";
            this.ListViewDevice2.Size = new System.Drawing.Size(676, 384);
            this.ListViewDevice2.TabIndex = 3;
            this.ListViewDevice2.UseCompatibleStateImageBehavior = false;
            this.ListViewDevice2.View = System.Windows.Forms.View.List;
            // 
            // TabPageDevice3
            // 
            this.TabPageDevice3.Controls.Add(this.LabelDevice3Information);
            this.TabPageDevice3.Controls.Add(this.ButtonSaveDevice3);
            this.TabPageDevice3.Controls.Add(this.ButtonSelectDevice3);
            this.TabPageDevice3.Controls.Add(this.ListViewDevice3);
            this.TabPageDevice3.Location = new System.Drawing.Point(4, 29);
            this.TabPageDevice3.Margin = new System.Windows.Forms.Padding(4);
            this.TabPageDevice3.Name = "TabPageDevice3";
            this.TabPageDevice3.Padding = new System.Windows.Forms.Padding(4);
            this.TabPageDevice3.Size = new System.Drawing.Size(692, 512);
            this.TabPageDevice3.TabIndex = 2;
            this.TabPageDevice3.Text = "Device 3";
            this.TabPageDevice3.UseVisualStyleBackColor = true;
            // 
            // LabelDevice3Information
            // 
            this.LabelDevice3Information.Location = new System.Drawing.Point(22, 17);
            this.LabelDevice3Information.Name = "LabelDevice3Information";
            this.LabelDevice3Information.Size = new System.Drawing.Size(648, 29);
            this.LabelDevice3Information.TabIndex = 14;
            this.LabelDevice3Information.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonSaveDevice3
            // 
            this.ButtonSaveDevice3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveDevice3.Location = new System.Drawing.Point(129, 67);
            this.ButtonSaveDevice3.Name = "ButtonSaveDevice3";
            this.ButtonSaveDevice3.Size = new System.Drawing.Size(108, 33);
            this.ButtonSaveDevice3.TabIndex = 9;
            this.ButtonSaveDevice3.Text = "Save";
            this.ButtonSaveDevice3.UseVisualStyleBackColor = true;
            this.ButtonSaveDevice3.Click += new System.EventHandler(this.ButtonSaveDevice3_Click);
            // 
            // ButtonSelectDevice3
            // 
            this.ButtonSelectDevice3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSelectDevice3.Location = new System.Drawing.Point(9, 67);
            this.ButtonSelectDevice3.Name = "ButtonSelectDevice3";
            this.ButtonSelectDevice3.Size = new System.Drawing.Size(108, 33);
            this.ButtonSelectDevice3.TabIndex = 5;
            this.ButtonSelectDevice3.Text = "Check All";
            this.ButtonSelectDevice3.UseVisualStyleBackColor = true;
            this.ButtonSelectDevice3.Click += new System.EventHandler(this.ButtonSelectDevice3_Click);
            // 
            // ListViewDevice3
            // 
            this.ListViewDevice3.CheckBoxes = true;
            this.ListViewDevice3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListViewDevice3.Location = new System.Drawing.Point(9, 107);
            this.ListViewDevice3.Margin = new System.Windows.Forms.Padding(4);
            this.ListViewDevice3.Name = "ListViewDevice3";
            this.ListViewDevice3.Size = new System.Drawing.Size(676, 384);
            this.ListViewDevice3.TabIndex = 3;
            this.ListViewDevice3.UseCompatibleStateImageBehavior = false;
            this.ListViewDevice3.View = System.Windows.Forms.View.List;
            // 
            // TabPageDevice4
            // 
            this.TabPageDevice4.Controls.Add(this.LabelDevice4Information);
            this.TabPageDevice4.Controls.Add(this.ButtonSaveDevice4);
            this.TabPageDevice4.Controls.Add(this.ButtonSelectDevice4);
            this.TabPageDevice4.Controls.Add(this.ListViewDevice4);
            this.TabPageDevice4.Location = new System.Drawing.Point(4, 29);
            this.TabPageDevice4.Margin = new System.Windows.Forms.Padding(4);
            this.TabPageDevice4.Name = "TabPageDevice4";
            this.TabPageDevice4.Padding = new System.Windows.Forms.Padding(4);
            this.TabPageDevice4.Size = new System.Drawing.Size(692, 512);
            this.TabPageDevice4.TabIndex = 3;
            this.TabPageDevice4.Text = "Device 4";
            this.TabPageDevice4.UseVisualStyleBackColor = true;
            // 
            // LabelDevice4Information
            // 
            this.LabelDevice4Information.Location = new System.Drawing.Point(22, 17);
            this.LabelDevice4Information.Name = "LabelDevice4Information";
            this.LabelDevice4Information.Size = new System.Drawing.Size(648, 29);
            this.LabelDevice4Information.TabIndex = 14;
            this.LabelDevice4Information.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonSaveDevice4
            // 
            this.ButtonSaveDevice4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSaveDevice4.Location = new System.Drawing.Point(129, 67);
            this.ButtonSaveDevice4.Name = "ButtonSaveDevice4";
            this.ButtonSaveDevice4.Size = new System.Drawing.Size(108, 33);
            this.ButtonSaveDevice4.TabIndex = 9;
            this.ButtonSaveDevice4.Text = "Save";
            this.ButtonSaveDevice4.UseVisualStyleBackColor = true;
            this.ButtonSaveDevice4.Click += new System.EventHandler(this.ButtonSaveDevice4_Click);
            // 
            // ButtonSelectDevice4
            // 
            this.ButtonSelectDevice4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSelectDevice4.Location = new System.Drawing.Point(9, 67);
            this.ButtonSelectDevice4.Name = "ButtonSelectDevice4";
            this.ButtonSelectDevice4.Size = new System.Drawing.Size(108, 33);
            this.ButtonSelectDevice4.TabIndex = 5;
            this.ButtonSelectDevice4.Text = "Check All";
            this.ButtonSelectDevice4.UseVisualStyleBackColor = true;
            this.ButtonSelectDevice4.Click += new System.EventHandler(this.ButtonSelectDevice4_Click);
            // 
            // ListViewDevice4
            // 
            this.ListViewDevice4.CheckBoxes = true;
            this.ListViewDevice4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListViewDevice4.Location = new System.Drawing.Point(9, 107);
            this.ListViewDevice4.Margin = new System.Windows.Forms.Padding(4);
            this.ListViewDevice4.Name = "ListViewDevice4";
            this.ListViewDevice4.Size = new System.Drawing.Size(676, 384);
            this.ListViewDevice4.TabIndex = 3;
            this.ListViewDevice4.UseCompatibleStateImageBehavior = false;
            this.ListViewDevice4.View = System.Windows.Forms.View.List;
            // 
            // TabPageNewUser
            // 
            this.TabPageNewUser.Controls.Add(this.PanelNewUser);
            this.TabPageNewUser.Location = new System.Drawing.Point(4, 29);
            this.TabPageNewUser.Name = "TabPageNewUser";
            this.TabPageNewUser.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageNewUser.Size = new System.Drawing.Size(692, 512);
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
            this.CheckBoxDisabled.TabIndex = 16;
            this.CheckBoxDisabled.Text = "Disabled";
            this.CheckBoxDisabled.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBoxDisabled.UseVisualStyleBackColor = true;
            // 
            // ButtonSaveNewUser
            // 
            this.ButtonSaveNewUser.Location = new System.Drawing.Point(392, 141);
            this.ButtonSaveNewUser.Name = "ButtonSaveNewUser";
            this.ButtonSaveNewUser.Size = new System.Drawing.Size(135, 31);
            this.ButtonSaveNewUser.TabIndex = 15;
            this.ButtonSaveNewUser.Text = "Save";
            this.ButtonSaveNewUser.UseVisualStyleBackColor = true;
            this.ButtonSaveNewUser.Click += new System.EventHandler(this.ButtonSaveNewUser_Click);
            // 
            // TextBoxPassword
            // 
            this.TextBoxPassword.Location = new System.Drawing.Point(392, 99);
            this.TextBoxPassword.Name = "TextBoxPassword";
            this.TextBoxPassword.Size = new System.Drawing.Size(135, 26);
            this.TextBoxPassword.TabIndex = 9;
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
            this.TextBoxLastname.TabIndex = 10;
            // 
            // TextBoxUsername
            // 
            this.TextBoxUsername.Location = new System.Drawing.Point(124, 99);
            this.TextBoxUsername.Name = "TextBoxUsername";
            this.TextBoxUsername.Size = new System.Drawing.Size(148, 26);
            this.TextBoxUsername.TabIndex = 11;
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
            this.TextBoxFirstname.TabIndex = 12;
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
            this.TextBoxPin.TabIndex = 13;
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
            this.TextBoxEmpId.TabIndex = 14;
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
            // ComboBoxRoles
            // 
            this.ComboBoxRoles.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboBoxRoles.FormattingEnabled = true;
            this.ComboBoxRoles.Location = new System.Drawing.Point(375, 10);
            this.ComboBoxRoles.Name = "ComboBoxRoles";
            this.ComboBoxRoles.Size = new System.Drawing.Size(151, 28);
            this.ComboBoxRoles.TabIndex = 3;
            this.ComboBoxRoles.SelectedIndexChanged += new System.EventHandler(this.ComboBoxRoles_SelectedIndexChanged);
            // 
            // LabelSelectRole
            // 
            this.LabelSelectRole.AutoSize = true;
            this.LabelSelectRole.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSelectRole.Location = new System.Drawing.Point(191, 14);
            this.LabelSelectRole.Name = "LabelSelectRole";
            this.LabelSelectRole.Size = new System.Drawing.Size(165, 20);
            this.LabelSelectRole.TabIndex = 5;
            this.LabelSelectRole.Text = "Select Access Role";
            this.LabelSelectRole.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(233, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Select Station";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ComboBoxStation
            // 
            this.ComboBoxStation.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboBoxStation.FormattingEnabled = true;
            this.ComboBoxStation.Location = new System.Drawing.Point(375, 53);
            this.ComboBoxStation.Name = "ComboBoxStation";
            this.ComboBoxStation.Size = new System.Drawing.Size(151, 28);
            this.ComboBoxStation.TabIndex = 6;
            this.ComboBoxStation.SelectedIndexChanged += new System.EventHandler(this.ComboBoxStation_SelectedIndexChanged);
            // 
            // FrmLAC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(716, 654);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ComboBoxStation);
            this.Controls.Add(this.LabelSelectRole);
            this.Controls.Add(this.ComboBoxRoles);
            this.Controls.Add(this.TabControlLAC);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmLAC";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Location Access Control";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.TabControlLAC.ResumeLayout(false);
            this.TabPageUsers.ResumeLayout(false);
            this.TabPageRoles.ResumeLayout(false);
            this.TabPageRoles.PerformLayout();
            this.TabPageDevice1.ResumeLayout(false);
            this.TabPageDevice2.ResumeLayout(false);
            this.TabPageDevice3.ResumeLayout(false);
            this.TabPageDevice4.ResumeLayout(false);
            this.TabPageNewUser.ResumeLayout(false);
            this.PanelNewUser.ResumeLayout(false);
            this.PanelNewUser.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TabControl TabControlLAC;
        private System.Windows.Forms.TabPage TabPageDevice1;
        private System.Windows.Forms.ListView ListViewDevice1;
        private System.Windows.Forms.TabPage TabPageDevice2;
        private System.Windows.Forms.TabPage TabPageDevice3;
        private System.Windows.Forms.TabPage TabPageDevice4;
        private System.Windows.Forms.ListView ListViewDevice2;
        private System.Windows.Forms.ListView ListViewDevice3;
        private System.Windows.Forms.ListView ListViewDevice4;
        private System.Windows.Forms.TabPage TabPageUsers;
        private System.Windows.Forms.ListView ListViewUsers;
        private System.Windows.Forms.ComboBox ComboBoxRoles;
        private System.Windows.Forms.TabPage TabPageRoles;
        private System.Windows.Forms.ListView ListViewRoles;
        private System.Windows.Forms.Button ButtonSelectUsers;
        private System.Windows.Forms.Button ButtonSelectDevice1;
        private System.Windows.Forms.Button ButtonSelectDevice2;
        private System.Windows.Forms.Button ButtonSelectDevice3;
        private System.Windows.Forms.Button ButtonSelectDevice4;
        private System.Windows.Forms.Label LabelSelectRole;
        private System.Windows.Forms.Button ButtonSaveNewRole;
        private System.Windows.Forms.TextBox TextBoxNewRole;
        private System.Windows.Forms.Label LabelNewRoleName;
        private System.Windows.Forms.Button ButtonSaveUsers;
        private System.Windows.Forms.Button ButtonSaveDevice1;
        private System.Windows.Forms.Button ButtonSaveDevice2;
        private System.Windows.Forms.Button ButtonSaveDevice3;
        private System.Windows.Forms.Button ButtonSaveDevice4;
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
        private System.Windows.Forms.Label LabelUserRoleInformation;
        private System.Windows.Forms.Label LabelDevice1Information;
        private System.Windows.Forms.Label LabelDevice2Information;
        private System.Windows.Forms.Label LabelDevice3Information;
        private System.Windows.Forms.Label LabelDevice4Information;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ComboBoxStation;
    }
}

