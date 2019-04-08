namespace Neutron.UserControls
{
    partial class ucSpecialFunctions
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mtNovaConfiguration = new MetroFramework.Controls.MetroTile();
            this.mpTransfers = new MetroFramework.Controls.MetroPanel();
            this.mlTransfersHeading = new MetroFramework.Controls.MetroLabel();
            this.mlTransferNovaRec = new MetroFramework.Controls.MetroLink();
            this.mlTransferNDefDb = new MetroFramework.Controls.MetroLink();
            this.mlTransferNLocDb = new MetroFramework.Controls.MetroLink();
            this.mlTransferResDefTypes = new MetroFramework.Controls.MetroLink();
            this.mlTransferOCTypes = new MetroFramework.Controls.MetroLink();
            this.mlTransferReserveTypes = new MetroFramework.Controls.MetroLink();
            this.metroProgressBar1 = new MetroFramework.Controls.MetroProgressBar();
            this.mpTransfers.SuspendLayout();
            this.SuspendLayout();
            // 
            // mtNovaConfiguration
            // 
            this.mtNovaConfiguration.ActiveControl = null;
            this.mtNovaConfiguration.Location = new System.Drawing.Point(3, 3);
            this.mtNovaConfiguration.Name = "mtNovaConfiguration";
            this.mtNovaConfiguration.Size = new System.Drawing.Size(365, 147);
            this.mtNovaConfiguration.TabIndex = 0;
            this.mtNovaConfiguration.Text = "Nova Configuration";
            this.mtNovaConfiguration.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.mtNovaConfiguration.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.mtNovaConfiguration.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.mtNovaConfiguration.UseSelectable = true;
            // 
            // mpTransfers
            // 
            this.mpTransfers.BackColor = System.Drawing.SystemColors.Control;
            this.mpTransfers.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.mpTransfers.Controls.Add(this.metroProgressBar1);
            this.mpTransfers.Controls.Add(this.mlTransferResDefTypes);
            this.mpTransfers.Controls.Add(this.mlTransferOCTypes);
            this.mpTransfers.Controls.Add(this.mlTransferReserveTypes);
            this.mpTransfers.Controls.Add(this.mlTransferNLocDb);
            this.mpTransfers.Controls.Add(this.mlTransferNDefDb);
            this.mpTransfers.Controls.Add(this.mlTransferNovaRec);
            this.mpTransfers.Controls.Add(this.mlTransfersHeading);
            this.mpTransfers.HorizontalScrollbarBarColor = true;
            this.mpTransfers.HorizontalScrollbarHighlightOnWheel = false;
            this.mpTransfers.HorizontalScrollbarSize = 10;
            this.mpTransfers.Location = new System.Drawing.Point(7, 167);
            this.mpTransfers.Name = "mpTransfers";
            this.mpTransfers.Size = new System.Drawing.Size(1132, 337);
            this.mpTransfers.TabIndex = 1;
            this.mpTransfers.VerticalScrollbarBarColor = true;
            this.mpTransfers.VerticalScrollbarHighlightOnWheel = false;
            this.mpTransfers.VerticalScrollbarSize = 10;
            // 
            // mlTransfersHeading
            // 
            this.mlTransfersHeading.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.mlTransfersHeading.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.mlTransfersHeading.Location = new System.Drawing.Point(327, 18);
            this.mlTransfersHeading.Name = "mlTransfersHeading";
            this.mlTransfersHeading.Size = new System.Drawing.Size(478, 37);
            this.mlTransfersHeading.TabIndex = 2;
            this.mlTransfersHeading.Text = "Nova to Neutron Transfers";
            this.mlTransfersHeading.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // mlTransferNovaRec
            // 
            this.mlTransferNovaRec.FontSize = MetroFramework.MetroLinkSize.Tall;
            this.mlTransferNovaRec.Location = new System.Drawing.Point(19, 74);
            this.mlTransferNovaRec.Name = "mlTransferNovaRec";
            this.mlTransferNovaRec.Size = new System.Drawing.Size(526, 41);
            this.mlTransferNovaRec.TabIndex = 3;
            this.mlTransferNovaRec.Text = "Transfer NovaRec Records to Neutron Inventory";
            this.mlTransferNovaRec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlTransferNovaRec.UseSelectable = true;
            this.mlTransferNovaRec.Click += new System.EventHandler(this.mlTransferNovaRec_Click);
            // 
            // mlTransferNDefDb
            // 
            this.mlTransferNDefDb.FontSize = MetroFramework.MetroLinkSize.Tall;
            this.mlTransferNDefDb.Location = new System.Drawing.Point(19, 137);
            this.mlTransferNDefDb.Name = "mlTransferNDefDb";
            this.mlTransferNDefDb.Size = new System.Drawing.Size(526, 41);
            this.mlTransferNDefDb.TabIndex = 3;
            this.mlTransferNDefDb.Text = "Transfer NDefDb Records to Neutron Definitions";
            this.mlTransferNDefDb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlTransferNDefDb.UseSelectable = true;
            this.mlTransferNDefDb.Click += new System.EventHandler(this.mlTransferNovaRec_Click);
            // 
            // mlTransferNLocDb
            // 
            this.mlTransferNLocDb.FontSize = MetroFramework.MetroLinkSize.Tall;
            this.mlTransferNLocDb.Location = new System.Drawing.Point(19, 200);
            this.mlTransferNLocDb.Name = "mlTransferNLocDb";
            this.mlTransferNLocDb.Size = new System.Drawing.Size(526, 41);
            this.mlTransferNLocDb.TabIndex = 3;
            this.mlTransferNLocDb.Text = "Transfer NLocDb Records to Neutron Locations\r\n";
            this.mlTransferNLocDb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlTransferNLocDb.UseSelectable = true;
            this.mlTransferNLocDb.Click += new System.EventHandler(this.mlTransferNovaRec_Click);
            // 
            // mlTransferResDefTypes
            // 
            this.mlTransferResDefTypes.FontSize = MetroFramework.MetroLinkSize.Tall;
            this.mlTransferResDefTypes.Location = new System.Drawing.Point(605, 200);
            this.mlTransferResDefTypes.Name = "mlTransferResDefTypes";
            this.mlTransferResDefTypes.Size = new System.Drawing.Size(505, 41);
            this.mlTransferResDefTypes.TabIndex = 4;
            this.mlTransferResDefTypes.Text = "Transfer ResDefTypes Records to Neutron\r\n";
            this.mlTransferResDefTypes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlTransferResDefTypes.UseSelectable = true;
            this.mlTransferResDefTypes.Click += new System.EventHandler(this.metroLink1_Click);
            // 
            // mlTransferOCTypes
            // 
            this.mlTransferOCTypes.FontSize = MetroFramework.MetroLinkSize.Tall;
            this.mlTransferOCTypes.Location = new System.Drawing.Point(605, 137);
            this.mlTransferOCTypes.Name = "mlTransferOCTypes";
            this.mlTransferOCTypes.Size = new System.Drawing.Size(505, 41);
            this.mlTransferOCTypes.TabIndex = 5;
            this.mlTransferOCTypes.Text = "Transfer OCTypes Records to Neutron";
            this.mlTransferOCTypes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlTransferOCTypes.UseSelectable = true;
            this.mlTransferOCTypes.Click += new System.EventHandler(this.metroLink2_Click);
            // 
            // mlTransferReserveTypes
            // 
            this.mlTransferReserveTypes.FontSize = MetroFramework.MetroLinkSize.Tall;
            this.mlTransferReserveTypes.Location = new System.Drawing.Point(605, 74);
            this.mlTransferReserveTypes.Name = "mlTransferReserveTypes";
            this.mlTransferReserveTypes.Size = new System.Drawing.Size(505, 41);
            this.mlTransferReserveTypes.TabIndex = 6;
            this.mlTransferReserveTypes.Text = "Transfer ReserveTypes Records to Neutron";
            this.mlTransferReserveTypes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlTransferReserveTypes.UseSelectable = true;
            this.mlTransferReserveTypes.Click += new System.EventHandler(this.metroLink3_Click);
            // 
            // metroProgressBar1
            // 
            this.metroProgressBar1.Location = new System.Drawing.Point(30, 260);
            this.metroProgressBar1.Name = "metroProgressBar1";
            this.metroProgressBar1.Size = new System.Drawing.Size(1067, 23);
            this.metroProgressBar1.TabIndex = 7;
            // 
            // ucSpecialFunctions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mpTransfers);
            this.Controls.Add(this.mtNovaConfiguration);
            this.Name = "ucSpecialFunctions";
            this.Size = new System.Drawing.Size(1150, 790);
            this.mpTransfers.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTile mtNovaConfiguration;
        private MetroFramework.Controls.MetroPanel mpTransfers;
        private MetroFramework.Controls.MetroLink mlTransferNovaRec;
        private MetroFramework.Controls.MetroLabel mlTransfersHeading;
        private MetroFramework.Controls.MetroLink mlTransferNLocDb;
        private MetroFramework.Controls.MetroLink mlTransferNDefDb;
        private MetroFramework.Controls.MetroLink mlTransferResDefTypes;
        private MetroFramework.Controls.MetroLink mlTransferOCTypes;
        private MetroFramework.Controls.MetroLink mlTransferReserveTypes;
        private MetroFramework.Controls.MetroProgressBar metroProgressBar1;
    }
}
