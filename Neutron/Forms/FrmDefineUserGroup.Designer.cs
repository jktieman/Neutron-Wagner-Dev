namespace Neutron.Forms
{
    partial class FrmDefineUserGroup
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.ButtonClearAllUsers = new System.Windows.Forms.Button();
            this.ButtonCheckAllUsers = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonRemove = new System.Windows.Forms.Button();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.TextBoxGroupName = new System.Windows.Forms.TextBox();
            this.LabelClearCheckBox = new System.Windows.Forms.Label();
            this.LabelGroups = new System.Windows.Forms.Label();
            this.LabelUsers = new System.Windows.Forms.Label();
            this.LabelGroupName = new System.Windows.Forms.Label();
            this.CheckedListBoxGroups = new System.Windows.Forms.CheckedListBox();
            this.CheckedListBoxUsers = new System.Windows.Forms.CheckedListBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ButtonClearAllUsers);
            this.panel1.Controls.Add(this.ButtonCheckAllUsers);
            this.panel1.Controls.Add(this.ButtonCancel);
            this.panel1.Controls.Add(this.ButtonRemove);
            this.panel1.Controls.Add(this.ButtonSave);
            this.panel1.Controls.Add(this.TextBoxGroupName);
            this.panel1.Controls.Add(this.LabelClearCheckBox);
            this.panel1.Controls.Add(this.LabelGroups);
            this.panel1.Controls.Add(this.LabelUsers);
            this.panel1.Controls.Add(this.LabelGroupName);
            this.panel1.Controls.Add(this.CheckedListBoxGroups);
            this.panel1.Controls.Add(this.CheckedListBoxUsers);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(710, 624);
            this.panel1.TabIndex = 3;
            // 
            // ButtonClearAllUsers
            // 
            this.ButtonClearAllUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonClearAllUsers.Location = new System.Drawing.Point(181, 587);
            this.ButtonClearAllUsers.Name = "ButtonClearAllUsers";
            this.ButtonClearAllUsers.Size = new System.Drawing.Size(120, 28);
            this.ButtonClearAllUsers.TabIndex = 32;
            this.ButtonClearAllUsers.Text = "Clear All";
            this.ButtonClearAllUsers.UseVisualStyleBackColor = true;
            this.ButtonClearAllUsers.Click += new System.EventHandler(this.ButtonClearAllUsers_Click);
            // 
            // ButtonCheckAllUsers
            // 
            this.ButtonCheckAllUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCheckAllUsers.Location = new System.Drawing.Point(50, 587);
            this.ButtonCheckAllUsers.Name = "ButtonCheckAllUsers";
            this.ButtonCheckAllUsers.Size = new System.Drawing.Size(120, 28);
            this.ButtonCheckAllUsers.TabIndex = 33;
            this.ButtonCheckAllUsers.Text = "Check All";
            this.ButtonCheckAllUsers.UseVisualStyleBackColor = true;
            this.ButtonCheckAllUsers.Click += new System.EventHandler(this.ButtonCheckAllUsers_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(586, 587);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(110, 28);
            this.ButtonCancel.TabIndex = 7;
            this.ButtonCancel.Text = "Close";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonRemove
            // 
            this.ButtonRemove.Enabled = false;
            this.ButtonRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonRemove.Location = new System.Drawing.Point(346, 587);
            this.ButtonRemove.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonRemove.Name = "ButtonRemove";
            this.ButtonRemove.Size = new System.Drawing.Size(110, 28);
            this.ButtonRemove.TabIndex = 8;
            this.ButtonRemove.Text = "Remove";
            this.ButtonRemove.UseVisualStyleBackColor = true;
            this.ButtonRemove.Click += new System.EventHandler(this.ButtonRemove_Click);
            // 
            // ButtonSave
            // 
            this.ButtonSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSave.Location = new System.Drawing.Point(466, 587);
            this.ButtonSave.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(110, 28);
            this.ButtonSave.TabIndex = 8;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // TextBoxGroupName
            // 
            this.TextBoxGroupName.Location = new System.Drawing.Point(361, 553);
            this.TextBoxGroupName.Name = "TextBoxGroupName";
            this.TextBoxGroupName.Size = new System.Drawing.Size(325, 22);
            this.TextBoxGroupName.TabIndex = 6;
            // 
            // LabelClearCheckBox
            // 
            this.LabelClearCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelClearCheckBox.Location = new System.Drawing.Point(368, 494);
            this.LabelClearCheckBox.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelClearCheckBox.Name = "LabelClearCheckBox";
            this.LabelClearCheckBox.Size = new System.Drawing.Size(328, 19);
            this.LabelClearCheckBox.TabIndex = 5;
            this.LabelClearCheckBox.Text = "Clear Check Box to Add New Group";
            this.LabelClearCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelGroups
            // 
            this.LabelGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelGroups.Location = new System.Drawing.Point(395, 21);
            this.LabelGroups.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelGroups.Name = "LabelGroups";
            this.LabelGroups.Size = new System.Drawing.Size(272, 19);
            this.LabelGroups.TabIndex = 5;
            this.LabelGroups.Text = "Groups";
            this.LabelGroups.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelUsers
            // 
            this.LabelUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelUsers.Location = new System.Drawing.Point(44, 21);
            this.LabelUsers.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelUsers.Name = "LabelUsers";
            this.LabelUsers.Size = new System.Drawing.Size(274, 19);
            this.LabelUsers.TabIndex = 5;
            this.LabelUsers.Text = "Users";
            this.LabelUsers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelGroupName
            // 
            this.LabelGroupName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelGroupName.Location = new System.Drawing.Point(358, 525);
            this.LabelGroupName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelGroupName.Name = "LabelGroupName";
            this.LabelGroupName.Size = new System.Drawing.Size(227, 20);
            this.LabelGroupName.TabIndex = 5;
            this.LabelGroupName.Text = "Group Name";
            this.LabelGroupName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CheckedListBoxGroups
            // 
            this.CheckedListBoxGroups.CheckOnClick = true;
            this.CheckedListBoxGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckedListBoxGroups.FormattingEnabled = true;
            this.CheckedListBoxGroups.Location = new System.Drawing.Point(361, 44);
            this.CheckedListBoxGroups.Margin = new System.Windows.Forms.Padding(4);
            this.CheckedListBoxGroups.Name = "CheckedListBoxGroups";
            this.CheckedListBoxGroups.Size = new System.Drawing.Size(335, 446);
            this.CheckedListBoxGroups.TabIndex = 3;
            this.CheckedListBoxGroups.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxGroups_ItemCheck);
            // 
            // CheckedListBoxUsers
            // 
            this.CheckedListBoxUsers.CheckOnClick = true;
            this.CheckedListBoxUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckedListBoxUsers.FormattingEnabled = true;
            this.CheckedListBoxUsers.Location = new System.Drawing.Point(9, 44);
            this.CheckedListBoxUsers.Margin = new System.Windows.Forms.Padding(4);
            this.CheckedListBoxUsers.Name = "CheckedListBoxUsers";
            this.CheckedListBoxUsers.Size = new System.Drawing.Size(335, 531);
            this.CheckedListBoxUsers.TabIndex = 4;
            this.CheckedListBoxUsers.ThreeDCheckBoxes = true;
            // 
            // FrmDefineUserGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(734, 648);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmDefineUserGroup";
            this.Text = "Define User Group";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonRemove;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.TextBox TextBoxGroupName;
        private System.Windows.Forms.Label LabelGroupName;
        private System.Windows.Forms.CheckedListBox CheckedListBoxGroups;
        private System.Windows.Forms.CheckedListBox CheckedListBoxUsers;
        private System.Windows.Forms.Button ButtonClearAllUsers;
        private System.Windows.Forms.Button ButtonCheckAllUsers;
        private System.Windows.Forms.Label LabelClearCheckBox;
        private System.Windows.Forms.Label LabelGroups;
        private System.Windows.Forms.Label LabelUsers;
    }
}