namespace Neutron.Forms
{
    partial class FrmCostCenter
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
            this.LabelHotActionFind = new System.Windows.Forms.Label();
            this.TextBoxFindCostCenter = new System.Windows.Forms.TextBox();
            this.ComboBoxCostCenter = new System.Windows.Forms.ComboBox();
            this.RadioButtonCostCenter = new System.Windows.Forms.RadioButton();
            this.RadioButtonOther = new System.Windows.Forms.RadioButton();
            this.RadioButtonScrap = new System.Windows.Forms.RadioButton();
            this.RadioButtonWarranty = new System.Windows.Forms.RadioButton();
            this.RadioButtonPick = new System.Windows.Forms.RadioButton();
            this.GroupBoxHotActions = new System.Windows.Forms.GroupBox();
            this.ButtonCancel = new MetroFramework.Controls.MetroButton();
            this.ButtonSave = new MetroFramework.Controls.MetroButton();
            this.GroupBoxHotActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // LabelHotActionFind
            // 
            this.LabelHotActionFind.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelHotActionFind.Location = new System.Drawing.Point(181, 156);
            this.LabelHotActionFind.Name = "LabelHotActionFind";
            this.LabelHotActionFind.Size = new System.Drawing.Size(75, 22);
            this.LabelHotActionFind.TabIndex = 16;
            this.LabelHotActionFind.Text = "Find";
            this.LabelHotActionFind.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextBoxFindCostCenter
            // 
            this.TextBoxFindCostCenter.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxFindCostCenter.Location = new System.Drawing.Point(262, 153);
            this.TextBoxFindCostCenter.Name = "TextBoxFindCostCenter";
            this.TextBoxFindCostCenter.Size = new System.Drawing.Size(138, 29);
            this.TextBoxFindCostCenter.TabIndex = 0;
            this.TextBoxFindCostCenter.Leave += new System.EventHandler(this.TextBoxFindCostCenter_Leave);
            // 
            // ComboBoxCostCenter
            // 
            this.ComboBoxCostCenter.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.ComboBoxCostCenter.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.ComboBoxCostCenter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBoxCostCenter.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboBoxCostCenter.FormattingEnabled = true;
            this.ComboBoxCostCenter.Location = new System.Drawing.Point(418, 153);
            this.ComboBoxCostCenter.Name = "ComboBoxCostCenter";
            this.ComboBoxCostCenter.Size = new System.Drawing.Size(497, 32);
            this.ComboBoxCostCenter.TabIndex = 1;
            // 
            // RadioButtonCostCenter
            // 
            this.RadioButtonCostCenter.BackColor = System.Drawing.Color.Transparent;
            this.RadioButtonCostCenter.Checked = true;
            this.RadioButtonCostCenter.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RadioButtonCostCenter.Location = new System.Drawing.Point(21, 153);
            this.RadioButtonCostCenter.Name = "RadioButtonCostCenter";
            this.RadioButtonCostCenter.Size = new System.Drawing.Size(186, 28);
            this.RadioButtonCostCenter.TabIndex = 2;
            this.RadioButtonCostCenter.TabStop = true;
            this.RadioButtonCostCenter.Tag = "Cost Center";
            this.RadioButtonCostCenter.Text = "Cost Center";
            this.RadioButtonCostCenter.UseVisualStyleBackColor = false;
            // 
            // RadioButtonOther
            // 
            this.RadioButtonOther.BackColor = System.Drawing.Color.Transparent;
            this.RadioButtonOther.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RadioButtonOther.Location = new System.Drawing.Point(21, 119);
            this.RadioButtonOther.Name = "RadioButtonOther";
            this.RadioButtonOther.Size = new System.Drawing.Size(133, 28);
            this.RadioButtonOther.TabIndex = 6;
            this.RadioButtonOther.Tag = "Other";
            this.RadioButtonOther.Text = "Other";
            this.RadioButtonOther.UseVisualStyleBackColor = false;
            // 
            // RadioButtonScrap
            // 
            this.RadioButtonScrap.BackColor = System.Drawing.Color.Transparent;
            this.RadioButtonScrap.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RadioButtonScrap.Location = new System.Drawing.Point(21, 85);
            this.RadioButtonScrap.Name = "RadioButtonScrap";
            this.RadioButtonScrap.Size = new System.Drawing.Size(133, 28);
            this.RadioButtonScrap.TabIndex = 5;
            this.RadioButtonScrap.Tag = "Scrap";
            this.RadioButtonScrap.Text = "Scrap";
            this.RadioButtonScrap.UseVisualStyleBackColor = false;
            // 
            // RadioButtonWarranty
            // 
            this.RadioButtonWarranty.BackColor = System.Drawing.Color.Transparent;
            this.RadioButtonWarranty.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RadioButtonWarranty.Location = new System.Drawing.Point(21, 51);
            this.RadioButtonWarranty.Name = "RadioButtonWarranty";
            this.RadioButtonWarranty.Size = new System.Drawing.Size(167, 28);
            this.RadioButtonWarranty.TabIndex = 4;
            this.RadioButtonWarranty.Tag = "Warranty";
            this.RadioButtonWarranty.Text = "Warranty";
            this.RadioButtonWarranty.UseVisualStyleBackColor = false;
            // 
            // RadioButtonPick
            // 
            this.RadioButtonPick.BackColor = System.Drawing.Color.Transparent;
            this.RadioButtonPick.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RadioButtonPick.Location = new System.Drawing.Point(21, 17);
            this.RadioButtonPick.Name = "RadioButtonPick";
            this.RadioButtonPick.Size = new System.Drawing.Size(133, 28);
            this.RadioButtonPick.TabIndex = 3;
            this.RadioButtonPick.Tag = "Pick";
            this.RadioButtonPick.Text = "Pick";
            this.RadioButtonPick.UseVisualStyleBackColor = false;
            // 
            // GroupBoxHotActions
            // 
            this.GroupBoxHotActions.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.GroupBoxHotActions.Controls.Add(this.LabelHotActionFind);
            this.GroupBoxHotActions.Controls.Add(this.TextBoxFindCostCenter);
            this.GroupBoxHotActions.Controls.Add(this.ComboBoxCostCenter);
            this.GroupBoxHotActions.Controls.Add(this.RadioButtonCostCenter);
            this.GroupBoxHotActions.Controls.Add(this.RadioButtonOther);
            this.GroupBoxHotActions.Controls.Add(this.RadioButtonScrap);
            this.GroupBoxHotActions.Controls.Add(this.RadioButtonWarranty);
            this.GroupBoxHotActions.Controls.Add(this.RadioButtonPick);
            this.GroupBoxHotActions.Location = new System.Drawing.Point(22, 26);
            this.GroupBoxHotActions.Name = "GroupBoxHotActions";
            this.GroupBoxHotActions.Size = new System.Drawing.Size(932, 205);
            this.GroupBoxHotActions.TabIndex = 1;
            this.GroupBoxHotActions.TabStop = false;
            this.GroupBoxHotActions.Text = "Transaction Type";
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.ButtonCancel.Location = new System.Drawing.Point(614, 238);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(162, 76);
            this.ButtonCancel.TabIndex = 4;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseSelectable = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonSave
            // 
            this.ButtonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonSave.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.ButtonSave.Location = new System.Drawing.Point(792, 238);
            this.ButtonSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(162, 76);
            this.ButtonSave.TabIndex = 3;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseSelectable = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // FrmCostCenter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(974, 332);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.GroupBoxHotActions);
            this.Name = "FrmCostCenter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cost Center";
            this.GroupBoxHotActions.ResumeLayout(false);
            this.GroupBoxHotActions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LabelHotActionFind;
        private System.Windows.Forms.TextBox TextBoxFindCostCenter;
        private System.Windows.Forms.ComboBox ComboBoxCostCenter;
        private System.Windows.Forms.RadioButton RadioButtonCostCenter;
        private System.Windows.Forms.RadioButton RadioButtonOther;
        private System.Windows.Forms.RadioButton RadioButtonScrap;
        private System.Windows.Forms.RadioButton RadioButtonWarranty;
        private System.Windows.Forms.RadioButton RadioButtonPick;
        private System.Windows.Forms.GroupBox GroupBoxHotActions;
        private MetroFramework.Controls.MetroButton ButtonCancel;
        private MetroFramework.Controls.MetroButton ButtonSave;
    }
}