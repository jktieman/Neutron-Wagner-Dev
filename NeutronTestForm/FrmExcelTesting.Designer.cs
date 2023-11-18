namespace NeutronTestForm
{
    partial class FrmExcelTesting
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
            this.TextBoxFilePath = new System.Windows.Forms.TextBox();
            this.LabelFilePath = new System.Windows.Forms.Label();
            this.ButtonReadFile = new System.Windows.Forms.Button();
            this.ButtonWriteFile = new System.Windows.Forms.Button();
            this.DataGridViewUsers = new System.Windows.Forms.DataGridView();
            this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewUsers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            this.SuspendLayout();
            // 
            // TextBoxFilePath
            // 
            this.TextBoxFilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxFilePath.Location = new System.Drawing.Point(209, 40);
            this.TextBoxFilePath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxFilePath.Name = "TextBoxFilePath";
            this.TextBoxFilePath.Size = new System.Drawing.Size(550, 29);
            this.TextBoxFilePath.TabIndex = 0;
            this.TextBoxFilePath.Text = "C:\\Neutron\\Excel\\Test.xlsx";
            // 
            // LabelFilePath
            // 
            this.LabelFilePath.AutoSize = true;
            this.LabelFilePath.Location = new System.Drawing.Point(83, 43);
            this.LabelFilePath.Name = "LabelFilePath";
            this.LabelFilePath.Size = new System.Drawing.Size(106, 20);
            this.LabelFilePath.TabIndex = 1;
            this.LabelFilePath.Text = "Test File Path";
            // 
            // ButtonReadFile
            // 
            this.ButtonReadFile.Location = new System.Drawing.Point(408, 370);
            this.ButtonReadFile.Name = "ButtonReadFile";
            this.ButtonReadFile.Size = new System.Drawing.Size(145, 38);
            this.ButtonReadFile.TabIndex = 2;
            this.ButtonReadFile.Text = "Read File";
            this.ButtonReadFile.UseVisualStyleBackColor = true;
            this.ButtonReadFile.Click += new System.EventHandler(this.ButtonReadFile_Click);
            // 
            // ButtonWriteFile
            // 
            this.ButtonWriteFile.Location = new System.Drawing.Point(576, 370);
            this.ButtonWriteFile.Name = "ButtonWriteFile";
            this.ButtonWriteFile.Size = new System.Drawing.Size(145, 38);
            this.ButtonWriteFile.TabIndex = 2;
            this.ButtonWriteFile.Text = "Write File";
            this.ButtonWriteFile.UseVisualStyleBackColor = true;
            this.ButtonWriteFile.Click += new System.EventHandler(this.ButtonWriteFile_Click);
            // 
            // DataGridViewUsers
            // 
            this.DataGridViewUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridViewUsers.Location = new System.Drawing.Point(12, 89);
            this.DataGridViewUsers.Name = "DataGridViewUsers";
            this.DataGridViewUsers.Size = new System.Drawing.Size(945, 243);
            this.DataGridViewUsers.TabIndex = 3;
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.SynchronizingObject = this;
            // 
            // FrmExcelTesting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 692);
            this.Controls.Add(this.DataGridViewUsers);
            this.Controls.Add(this.ButtonWriteFile);
            this.Controls.Add(this.ButtonReadFile);
            this.Controls.Add(this.LabelFilePath);
            this.Controls.Add(this.TextBoxFilePath);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FrmExcelTesting";
            this.Text = "FrmExcelTesting";
            this.Load += new System.EventHandler(this.FrmExcelTesting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewUsers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxFilePath;
        private System.Windows.Forms.Label LabelFilePath;
        private System.Windows.Forms.Button ButtonReadFile;
        private System.Windows.Forms.Button ButtonWriteFile;
        private System.Windows.Forms.DataGridView DataGridViewUsers;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
    }
}