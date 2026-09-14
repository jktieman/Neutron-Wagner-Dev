namespace Neutron.Forms
{
    partial class FrmExpiration
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
            this.LabelItem = new System.Windows.Forms.Label();
            this.LabelItemText = new System.Windows.Forms.Label();
            this.LabelDescription = new System.Windows.Forms.Label();
            this.LabelDescriptionText = new System.Windows.Forms.Label();
            this.LabelLotNumber = new System.Windows.Forms.Label();
            this.LabelExpirationDate = new System.Windows.Forms.Label();
            this.DateTimePickerExpirationDate = new System.Windows.Forms.DateTimePicker();
            this.TextBoxLotNumber = new System.Windows.Forms.TextBox();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LabelItem
            // 
            this.LabelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelItem.Location = new System.Drawing.Point(92, 27);
            this.LabelItem.Name = "LabelItem";
            this.LabelItem.Size = new System.Drawing.Size(287, 36);
            this.LabelItem.TabIndex = 0;
            this.LabelItem.Text = "Item";
            this.LabelItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelItemText
            // 
            this.LabelItemText.BackColor = System.Drawing.SystemColors.Window;
            this.LabelItemText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelItemText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelItemText.Location = new System.Drawing.Point(60, 63);
            this.LabelItemText.Name = "LabelItemText";
            this.LabelItemText.Size = new System.Drawing.Size(350, 36);
            this.LabelItemText.TabIndex = 0;
            this.LabelItemText.Text = "15448-548";
            this.LabelItemText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelDescription
            // 
            this.LabelDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelDescription.Location = new System.Drawing.Point(92, 114);
            this.LabelDescription.Name = "LabelDescription";
            this.LabelDescription.Size = new System.Drawing.Size(287, 36);
            this.LabelDescription.TabIndex = 0;
            this.LabelDescription.Text = "Description";
            this.LabelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelDescriptionText
            // 
            this.LabelDescriptionText.BackColor = System.Drawing.SystemColors.Window;
            this.LabelDescriptionText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelDescriptionText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelDescriptionText.Location = new System.Drawing.Point(60, 150);
            this.LabelDescriptionText.Name = "LabelDescriptionText";
            this.LabelDescriptionText.Size = new System.Drawing.Size(350, 36);
            this.LabelDescriptionText.TabIndex = 0;
            this.LabelDescriptionText.Text = "Gauze Pads";
            this.LabelDescriptionText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelLotNumber
            // 
            this.LabelLotNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelLotNumber.Location = new System.Drawing.Point(92, 199);
            this.LabelLotNumber.Name = "LabelLotNumber";
            this.LabelLotNumber.Size = new System.Drawing.Size(287, 36);
            this.LabelLotNumber.TabIndex = 0;
            this.LabelLotNumber.Text = "Lot Number";
            this.LabelLotNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelExpirationDate
            // 
            this.LabelExpirationDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelExpirationDate.Location = new System.Drawing.Point(92, 285);
            this.LabelExpirationDate.Name = "LabelExpirationDate";
            this.LabelExpirationDate.Size = new System.Drawing.Size(287, 36);
            this.LabelExpirationDate.TabIndex = 0;
            this.LabelExpirationDate.Text = "Expiration Date";
            this.LabelExpirationDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DateTimePickerExpirationDate
            // 
            this.DateTimePickerExpirationDate.Location = new System.Drawing.Point(60, 324);
            this.DateTimePickerExpirationDate.Name = "DateTimePickerExpirationDate";
            this.DateTimePickerExpirationDate.Size = new System.Drawing.Size(351, 30);
            this.DateTimePickerExpirationDate.TabIndex = 1;
            // 
            // TextBoxLotNumber
            // 
            this.TextBoxLotNumber.Location = new System.Drawing.Point(60, 238);
            this.TextBoxLotNumber.Name = "TextBoxLotNumber";
            this.TextBoxLotNumber.Size = new System.Drawing.Size(351, 30);
            this.TextBoxLotNumber.TabIndex = 2;
            this.TextBoxLotNumber.Text = "123456789";
            this.TextBoxLotNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ButtonSave
            // 
            this.ButtonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonSave.Location = new System.Drawing.Point(244, 487);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(152, 44);
            this.ButtonSave.TabIndex = 3;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(86, 487);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(152, 44);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // FrmExpiration
            // 
            this.AcceptButton = this.ButtonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(482, 553);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.TextBoxLotNumber);
            this.Controls.Add(this.DateTimePickerExpirationDate);
            this.Controls.Add(this.LabelExpirationDate);
            this.Controls.Add(this.LabelLotNumber);
            this.Controls.Add(this.LabelDescriptionText);
            this.Controls.Add(this.LabelDescription);
            this.Controls.Add(this.LabelItemText);
            this.Controls.Add(this.LabelItem);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FrmExpiration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Expiration Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelItem;
        private System.Windows.Forms.Label LabelItemText;
        private System.Windows.Forms.Label LabelDescription;
        private System.Windows.Forms.Label LabelDescriptionText;
        private System.Windows.Forms.Label LabelLotNumber;
        private System.Windows.Forms.Label LabelExpirationDate;
        private System.Windows.Forms.DateTimePicker DateTimePickerExpirationDate;
        private System.Windows.Forms.TextBox TextBoxLotNumber;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Button ButtonCancel;
    }
}