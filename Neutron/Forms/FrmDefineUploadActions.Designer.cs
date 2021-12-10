namespace Neutron.Forms
{
    partial class FrmDefineUploadActions
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
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.CheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.ButtonClearAllActions = new System.Windows.Forms.Button();
            this.ButtonCheckAllActions = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(259, 528);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(103, 23);
            this.ButtonCancel.TabIndex = 2;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonSave
            // 
            this.ButtonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSave.Location = new System.Drawing.Point(160, 528);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(93, 23);
            this.ButtonSave.TabIndex = 1;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // CheckedListBox
            // 
            this.CheckedListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckedListBox.FormattingEnabled = true;
            this.CheckedListBox.Location = new System.Drawing.Point(22, 8);
            this.CheckedListBox.Name = "CheckedListBox";
            this.CheckedListBox.Size = new System.Drawing.Size(340, 514);
            this.CheckedListBox.TabIndex = 0;
            // 
            // ButtonClearAllActions
            // 
            this.ButtonClearAllActions.Location = new System.Drawing.Point(73, 528);
            this.ButtonClearAllActions.Name = "ButtonClearAllActions";
            this.ButtonClearAllActions.Size = new System.Drawing.Size(65, 23);
            this.ButtonClearAllActions.TabIndex = 6;
            this.ButtonClearAllActions.Text = "Clear All";
            this.ButtonClearAllActions.UseVisualStyleBackColor = true;
            this.ButtonClearAllActions.Click += new System.EventHandler(this.ButtonClearAllActions_Click);
            // 
            // ButtonCheckAllActions
            // 
            this.ButtonCheckAllActions.Location = new System.Drawing.Point(2, 528);
            this.ButtonCheckAllActions.Name = "ButtonCheckAllActions";
            this.ButtonCheckAllActions.Size = new System.Drawing.Size(65, 23);
            this.ButtonCheckAllActions.TabIndex = 5;
            this.ButtonCheckAllActions.Text = "Check All";
            this.ButtonCheckAllActions.UseVisualStyleBackColor = true;
            this.ButtonCheckAllActions.Click += new System.EventHandler(this.ButtonCheckAllActions_Click);
            // 
            // FrmDefineUploadActions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 561);
            this.Controls.Add(this.ButtonClearAllActions);
            this.Controls.Add(this.ButtonCheckAllActions);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.CheckedListBox);
            this.Name = "FrmDefineUploadActions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Define Upload Action";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.CheckedListBox CheckedListBox;
        private System.Windows.Forms.Button ButtonClearAllActions;
        private System.Windows.Forms.Button ButtonCheckAllActions;
    }
}