namespace Neutron.Forms
{
    partial class FrmEBin
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.PanelTop = new System.Windows.Forms.Panel();
            this.MBHotStore = new MetroFramework.Controls.MetroButton();
            this.MBHotPick = new MetroFramework.Controls.MetroButton();
            this.TextBoxFindItem = new System.Windows.Forms.TextBox();
            this.ButtonClearFindItem = new System.Windows.Forms.Button();
            this.MBHotActionClose = new MetroFramework.Controls.MetroButton();
            this.MBFindItem = new MetroFramework.Controls.MetroButton();
            this.DataGridViewEBin = new System.Windows.Forms.DataGridView();
            this.PanelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewEBin)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelTop
            // 
            this.PanelTop.Controls.Add(this.MBHotStore);
            this.PanelTop.Controls.Add(this.MBHotPick);
            this.PanelTop.Controls.Add(this.TextBoxFindItem);
            this.PanelTop.Controls.Add(this.ButtonClearFindItem);
            this.PanelTop.Controls.Add(this.MBHotActionClose);
            this.PanelTop.Controls.Add(this.MBFindItem);
            this.PanelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.PanelTop.Location = new System.Drawing.Point(0, 0);
            this.PanelTop.Name = "PanelTop";
            this.PanelTop.Size = new System.Drawing.Size(1019, 89);
            this.PanelTop.TabIndex = 0;
            // 
            // MBHotStore
            // 
            this.MBHotStore.BackColor = System.Drawing.Color.Green;
            this.MBHotStore.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBHotStore.Enabled = false;
            this.MBHotStore.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBHotStore.Location = new System.Drawing.Point(177, 9);
            this.MBHotStore.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBHotStore.Name = "MBHotStore";
            this.MBHotStore.Size = new System.Drawing.Size(150, 72);
            this.MBHotStore.TabIndex = 11;
            this.MBHotStore.Text = "Store";
            this.MBHotStore.UseCustomBackColor = true;
            this.MBHotStore.UseSelectable = true;
            // 
            // MBHotPick
            // 
            this.MBHotPick.BackColor = System.Drawing.Color.Red;
            this.MBHotPick.Enabled = false;
            this.MBHotPick.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBHotPick.Location = new System.Drawing.Point(12, 9);
            this.MBHotPick.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBHotPick.Name = "MBHotPick";
            this.MBHotPick.Size = new System.Drawing.Size(150, 72);
            this.MBHotPick.Style = MetroFramework.MetroColorStyle.Red;
            this.MBHotPick.TabIndex = 10;
            this.MBHotPick.Text = "Pick";
            this.MBHotPick.UseCustomBackColor = true;
            this.MBHotPick.UseSelectable = true;
            // 
            // TextBoxFindItem
            // 
            this.TextBoxFindItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxFindItem.Location = new System.Drawing.Point(353, 27);
            this.TextBoxFindItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextBoxFindItem.Name = "TextBoxFindItem";
            this.TextBoxFindItem.Size = new System.Drawing.Size(281, 38);
            this.TextBoxFindItem.TabIndex = 12;
            this.TextBoxFindItem.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ButtonClearFindItem
            // 
            this.ButtonClearFindItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonClearFindItem.Location = new System.Drawing.Point(640, 27);
            this.ButtonClearFindItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ButtonClearFindItem.Name = "ButtonClearFindItem";
            this.ButtonClearFindItem.Size = new System.Drawing.Size(36, 38);
            this.ButtonClearFindItem.TabIndex = 13;
            this.ButtonClearFindItem.Text = "X";
            this.ButtonClearFindItem.UseVisualStyleBackColor = true;
            this.ButtonClearFindItem.Click += new System.EventHandler(this.ButtonClearFindItem_Click);
            // 
            // MBHotActionClose
            // 
            this.MBHotActionClose.BackColor = System.Drawing.Color.RoyalBlue;
            this.MBHotActionClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.MBHotActionClose.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBHotActionClose.Location = new System.Drawing.Point(857, 9);
            this.MBHotActionClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBHotActionClose.Name = "MBHotActionClose";
            this.MBHotActionClose.Size = new System.Drawing.Size(150, 72);
            this.MBHotActionClose.TabIndex = 15;
            this.MBHotActionClose.Text = "Close";
            this.MBHotActionClose.UseSelectable = true;
            this.MBHotActionClose.Click += new System.EventHandler(this.MBHotActionClose_Click);
            // 
            // MBFindItem
            // 
            this.MBFindItem.FontSize = MetroFramework.MetroButtonSize.Tall;
            this.MBFindItem.Location = new System.Drawing.Point(692, 9);
            this.MBFindItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MBFindItem.Name = "MBFindItem";
            this.MBFindItem.Size = new System.Drawing.Size(150, 72);
            this.MBFindItem.TabIndex = 14;
            this.MBFindItem.Text = "Search";
            this.MBFindItem.UseSelectable = true;
            this.MBFindItem.Click += new System.EventHandler(this.MBFindItem_Click);
            // 
            // DataGridViewEBin
            // 
            this.DataGridViewEBin.AllowUserToAddRows = false;
            this.DataGridViewEBin.AllowUserToDeleteRows = false;
            this.DataGridViewEBin.BackgroundColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridViewEBin.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.DataGridViewEBin.ColumnHeadersHeight = 28;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DataGridViewEBin.DefaultCellStyle = dataGridViewCellStyle2;
            this.DataGridViewEBin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGridViewEBin.Location = new System.Drawing.Point(0, 89);
            this.DataGridViewEBin.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridViewEBin.MultiSelect = false;
            this.DataGridViewEBin.Name = "DataGridViewEBin";
            this.DataGridViewEBin.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridViewEBin.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DataGridViewEBin.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.DataGridViewEBin.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DataGridViewEBin.RowTemplate.Height = 28;
            this.DataGridViewEBin.RowTemplate.ReadOnly = true;
            this.DataGridViewEBin.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DataGridViewEBin.Size = new System.Drawing.Size(1019, 614);
            this.DataGridViewEBin.TabIndex = 6;
            // 
            // FrmEBin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 703);
            this.Controls.Add(this.DataGridViewEBin);
            this.Controls.Add(this.PanelTop);
            this.Name = "FrmEBin";
            this.Text = "FrmEBin";
            this.PanelTop.ResumeLayout(false);
            this.PanelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewEBin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelTop;
        private System.Windows.Forms.DataGridView DataGridViewEBin;
        private MetroFramework.Controls.MetroButton MBHotStore;
        private MetroFramework.Controls.MetroButton MBHotPick;
        private System.Windows.Forms.TextBox TextBoxFindItem;
        private System.Windows.Forms.Button ButtonClearFindItem;
        private MetroFramework.Controls.MetroButton MBHotActionClose;
        private MetroFramework.Controls.MetroButton MBFindItem;
    }
}