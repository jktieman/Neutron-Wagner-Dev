namespace Neutron.Forms
{
    partial class FrmNewItemAuth
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmNewItemAuth));
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.LabelEnterPassword = new System.Windows.Forms.Label();
            this.TextBoxAuthCode = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(209, 233);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(152, 76);
            this.ButtonCancel.TabIndex = 2;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonOk
            // 
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(42, 233);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(152, 76);
            this.ButtonOk.TabIndex = 1;
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // LabelEnterPassword
            // 
            this.LabelEnterPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelEnterPassword.Location = new System.Drawing.Point(82, 139);
            this.LabelEnterPassword.Name = "LabelEnterPassword";
            this.LabelEnterPassword.Size = new System.Drawing.Size(222, 43);
            this.LabelEnterPassword.TabIndex = 4;
            this.LabelEnterPassword.Text = "Enter Password";
            this.LabelEnterPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxAuthCode
            // 
            this.TextBoxAuthCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxAuthCode.Location = new System.Drawing.Point(42, 46);
            this.TextBoxAuthCode.Name = "TextBoxAuthCode";
            this.TextBoxAuthCode.PasswordChar = '*';
            this.TextBoxAuthCode.Size = new System.Drawing.Size(319, 80);
            this.TextBoxAuthCode.TabIndex = 0;
            this.TextBoxAuthCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxAuthCode.UseSystemPasswordChar = true;
            // 
            // FrmNewItemAuth
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SlateGray;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(403, 370);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.LabelEnterPassword);
            this.Controls.Add(this.TextBoxAuthCode);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmNewItemAuth";
            this.Text = "FrmNewItemAuth";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Label LabelEnterPassword;
        private System.Windows.Forms.TextBox TextBoxAuthCode;
    }
}