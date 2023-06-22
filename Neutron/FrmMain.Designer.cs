using NeutronData.Models;

namespace Neutron
{
    partial class FrmMain : MetroFramework.Forms.MetroForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.htmlToolTip1 = new MetroFramework.Drawing.Html.HtmlToolTip();
            this.metroToolTip1 = new MetroFramework.Components.MetroToolTip();
            this.LabelFormHeaderText = new System.Windows.Forms.Label();
            this.mlUserInfo = new MetroFramework.Controls.MetroLabel();
            this.metroStyleManager1 = new MetroFramework.Components.MetroStyleManager(this.components);
            this.metroPanelMain = new MetroFramework.Controls.MetroPanel();
            this.MtProductivity = new MetroFramework.Controls.MetroTile();
            this.MtHotAction = new MetroFramework.Controls.MetroTile();
            this.MtLogOff = new MetroFramework.Controls.MetroTile();
            this.MtStore = new MetroFramework.Controls.MetroTile();
            this.MtHistory = new MetroFramework.Controls.MetroTile();
            this.MtPick = new MetroFramework.Controls.MetroTile();
            this.MtUtilities = new MetroFramework.Controls.MetroTile();
            this.MtSystem = new MetroFramework.Controls.MetroTile();
            this.MtLac = new MetroFramework.Controls.MetroTile();
            this.MtUsers = new MetroFramework.Controls.MetroTile();
            this.MtItemDefinitions = new MetroFramework.Controls.MetroTile();
            this.MtLocations = new MetroFramework.Controls.MetroTile();
            this.MtInventory = new MetroFramework.Controls.MetroTile();
            this.LabelWarehouseManagement = new System.Windows.Forms.Label();
            this.ButtonPark = new System.Windows.Forms.Button();
            this.ButtonClose = new System.Windows.Forms.Button();
            this.GroupBoxLanguage = new System.Windows.Forms.GroupBox();
            this.RadioButtonFrenchCanadian = new System.Windows.Forms.RadioButton();
            this.RadioButtonEnglish = new System.Windows.Forms.RadioButton();
            this.PictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.BindingSourceLocations = new System.Windows.Forms.BindingSource(this.components);
            this.BindingSourceItemDefinition = new System.Windows.Forms.BindingSource(this.components);
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).BeginInit();
            this.metroPanelMain.SuspendLayout();
            this.GroupBoxLanguage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSourceLocations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSourceItemDefinition)).BeginInit();
            this.SuspendLayout();
            // 
            // htmlToolTip1
            // 
            this.htmlToolTip1.OwnerDraw = true;
            // 
            // metroToolTip1
            // 
            this.metroToolTip1.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroToolTip1.StyleManager = null;
            this.metroToolTip1.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // LabelFormHeaderText
            // 
            this.LabelFormHeaderText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.LabelFormHeaderText, "LabelFormHeaderText");
            this.LabelFormHeaderText.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.LabelFormHeaderText.Name = "LabelFormHeaderText";
            // 
            // mlUserInfo
            // 
            resources.ApplyResources(this.mlUserInfo, "mlUserInfo");
            this.mlUserInfo.Name = "mlUserInfo";
            // 
            // metroStyleManager1
            // 
            this.metroStyleManager1.Owner = null;
            this.metroStyleManager1.Style = MetroFramework.MetroColorStyle.Brown;
            // 
            // metroPanelMain
            // 
            resources.ApplyResources(this.metroPanelMain, "metroPanelMain");
            this.metroPanelMain.BackColor = System.Drawing.Color.WhiteSmoke;
            this.metroPanelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.metroPanelMain.Controls.Add(this.MtProductivity);
            this.metroPanelMain.Controls.Add(this.MtHotAction);
            this.metroPanelMain.Controls.Add(this.MtLogOff);
            this.metroPanelMain.Controls.Add(this.MtStore);
            this.metroPanelMain.Controls.Add(this.MtHistory);
            this.metroPanelMain.Controls.Add(this.MtPick);
            this.metroPanelMain.Controls.Add(this.MtUtilities);
            this.metroPanelMain.Controls.Add(this.MtSystem);
            this.metroPanelMain.Controls.Add(this.MtLac);
            this.metroPanelMain.Controls.Add(this.MtUsers);
            this.metroPanelMain.Controls.Add(this.MtItemDefinitions);
            this.metroPanelMain.Controls.Add(this.MtLocations);
            this.metroPanelMain.Controls.Add(this.MtInventory);
            this.metroPanelMain.HorizontalScrollbarBarColor = true;
            this.metroPanelMain.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanelMain.HorizontalScrollbarSize = 10;
            this.metroPanelMain.Name = "metroPanelMain";
            this.metroPanelMain.Style = MetroFramework.MetroColorStyle.Yellow;
            this.metroPanelMain.VerticalScrollbarBarColor = true;
            this.metroPanelMain.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanelMain.VerticalScrollbarSize = 9;
            // 
            // MtProductivity
            // 
            this.MtProductivity.ActiveControl = null;
            resources.ApplyResources(this.MtProductivity, "MtProductivity");
            this.MtProductivity.Name = "MtProductivity";
            this.MtProductivity.Style = MetroFramework.MetroColorStyle.Blue;
            this.MtProductivity.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtProductivity.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtProductivity.UseSelectable = true;
            this.MtProductivity.Click += new System.EventHandler(this.MtProductivity_Click);
            // 
            // MtHotAction
            // 
            this.MtHotAction.ActiveControl = null;
            this.MtHotAction.CausesValidation = false;
            resources.ApplyResources(this.MtHotAction, "MtHotAction");
            this.MtHotAction.Name = "MtHotAction";
            this.MtHotAction.Style = MetroFramework.MetroColorStyle.Red;
            this.MtHotAction.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtHotAction.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtHotAction.UseSelectable = true;
            this.MtHotAction.Click += new System.EventHandler(this.MtHotAction_Click);
            // 
            // MtLogOff
            // 
            this.MtLogOff.ActiveControl = null;
            resources.ApplyResources(this.MtLogOff, "MtLogOff");
            this.MtLogOff.Name = "MtLogOff";
            this.MtLogOff.Style = MetroFramework.MetroColorStyle.Black;
            this.MtLogOff.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtLogOff.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtLogOff.UseSelectable = true;
            this.MtLogOff.Click += new System.EventHandler(this.MtLogOff_Click);
            // 
            // MtStore
            // 
            this.MtStore.ActiveControl = null;
            resources.ApplyResources(this.MtStore, "MtStore");
            this.MtStore.Name = "MtStore";
            this.MtStore.Style = MetroFramework.MetroColorStyle.Magenta;
            this.MtStore.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtStore.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtStore.UseSelectable = true;
            this.MtStore.Click += new System.EventHandler(this.MtStore_Click);
            // 
            // MtHistory
            // 
            this.MtHistory.ActiveControl = null;
            resources.ApplyResources(this.MtHistory, "MtHistory");
            this.MtHistory.Name = "MtHistory";
            this.MtHistory.Style = MetroFramework.MetroColorStyle.Blue;
            this.MtHistory.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtHistory.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtHistory.UseSelectable = true;
            this.MtHistory.Click += new System.EventHandler(this.MtHistory_Click);
            // 
            // MtPick
            // 
            this.MtPick.ActiveControl = null;
            resources.ApplyResources(this.MtPick, "MtPick");
            this.MtPick.Name = "MtPick";
            this.MtPick.Style = MetroFramework.MetroColorStyle.Teal;
            this.MtPick.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtPick.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtPick.UseSelectable = true;
            this.MtPick.Click += new System.EventHandler(this.MtPick_Click);
            // 
            // MtUtilities
            // 
            this.MtUtilities.ActiveControl = null;
            resources.ApplyResources(this.MtUtilities, "MtUtilities");
            this.MtUtilities.Name = "MtUtilities";
            this.MtUtilities.Style = MetroFramework.MetroColorStyle.Silver;
            this.MtUtilities.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtUtilities.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtUtilities.UseSelectable = true;
            this.MtUtilities.Click += new System.EventHandler(this.MtUtilities_Click);
            // 
            // MtSystem
            // 
            this.MtSystem.ActiveControl = null;
            resources.ApplyResources(this.MtSystem, "MtSystem");
            this.MtSystem.Name = "MtSystem";
            this.MtSystem.Style = MetroFramework.MetroColorStyle.Green;
            this.MtSystem.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtSystem.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtSystem.UseSelectable = true;
            this.MtSystem.Click += new System.EventHandler(this.MtSystem_Click);
            // 
            // MtLac
            // 
            this.MtLac.ActiveControl = null;
            resources.ApplyResources(this.MtLac, "MtLac");
            this.MtLac.Name = "MtLac";
            this.MtLac.Style = MetroFramework.MetroColorStyle.Brown;
            this.MtLac.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtLac.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtLac.UseSelectable = true;
            this.MtLac.Click += new System.EventHandler(this.MtLac_Click);
            // 
            // MtUsers
            // 
            this.MtUsers.ActiveControl = null;
            resources.ApplyResources(this.MtUsers, "MtUsers");
            this.MtUsers.Name = "MtUsers";
            this.MtUsers.Style = MetroFramework.MetroColorStyle.Lime;
            this.MtUsers.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtUsers.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtUsers.UseSelectable = true;
            this.MtUsers.Click += new System.EventHandler(this.MtUsers_Click);
            // 
            // MtItemDefinitions
            // 
            this.MtItemDefinitions.ActiveControl = null;
            resources.ApplyResources(this.MtItemDefinitions, "MtItemDefinitions");
            this.MtItemDefinitions.Name = "MtItemDefinitions";
            this.MtItemDefinitions.Style = MetroFramework.MetroColorStyle.Orange;
            this.MtItemDefinitions.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtItemDefinitions.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtItemDefinitions.UseSelectable = true;
            this.MtItemDefinitions.Click += new System.EventHandler(this.MtItemDefinitions_Click);
            // 
            // MtLocations
            // 
            this.MtLocations.ActiveControl = null;
            resources.ApplyResources(this.MtLocations, "MtLocations");
            this.MtLocations.Name = "MtLocations";
            this.MtLocations.Style = MetroFramework.MetroColorStyle.Silver;
            this.MtLocations.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtLocations.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtLocations.UseSelectable = true;
            this.MtLocations.Click += new System.EventHandler(this.MtLocations_Click);
            // 
            // MtInventory
            // 
            this.MtInventory.ActiveControl = null;
            resources.ApplyResources(this.MtInventory, "MtInventory");
            this.MtInventory.Name = "MtInventory";
            this.MtInventory.Style = MetroFramework.MetroColorStyle.Green;
            this.MtInventory.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.MtInventory.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Bold;
            this.MtInventory.UseSelectable = true;
            this.MtInventory.UseTileImage = true;
            this.MtInventory.Click += new System.EventHandler(this.MtInventory_Click);
            // 
            // LabelWarehouseManagement
            // 
            resources.ApplyResources(this.LabelWarehouseManagement, "LabelWarehouseManagement");
            this.LabelWarehouseManagement.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.LabelWarehouseManagement.Name = "LabelWarehouseManagement";
            // 
            // ButtonPark
            // 
            resources.ApplyResources(this.ButtonPark, "ButtonPark");
            this.ButtonPark.Name = "ButtonPark";
            this.ButtonPark.UseVisualStyleBackColor = true;
            this.ButtonPark.Click += new System.EventHandler(this.ButtonPark_Click);
            // 
            // ButtonClose
            // 
            resources.ApplyResources(this.ButtonClose, "ButtonClose");
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.UseVisualStyleBackColor = true;
            this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // GroupBoxLanguage
            // 
            this.GroupBoxLanguage.Controls.Add(this.RadioButtonFrenchCanadian);
            this.GroupBoxLanguage.Controls.Add(this.RadioButtonEnglish);
            this.GroupBoxLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            resources.ApplyResources(this.GroupBoxLanguage, "GroupBoxLanguage");
            this.GroupBoxLanguage.Name = "GroupBoxLanguage";
            this.GroupBoxLanguage.TabStop = false;
            // 
            // RadioButtonFrenchCanadian
            // 
            resources.ApplyResources(this.RadioButtonFrenchCanadian, "RadioButtonFrenchCanadian");
            this.RadioButtonFrenchCanadian.Name = "RadioButtonFrenchCanadian";
            this.RadioButtonFrenchCanadian.UseVisualStyleBackColor = true;
            this.RadioButtonFrenchCanadian.CheckedChanged += new System.EventHandler(this.RadioButtonLanguage_CheckedChanged);
            // 
            // RadioButtonEnglish
            // 
            resources.ApplyResources(this.RadioButtonEnglish, "RadioButtonEnglish");
            this.RadioButtonEnglish.Checked = true;
            this.RadioButtonEnglish.Name = "RadioButtonEnglish";
            this.RadioButtonEnglish.TabStop = true;
            this.RadioButtonEnglish.UseVisualStyleBackColor = true;
            this.RadioButtonEnglish.CheckedChanged += new System.EventHandler(this.RadioButtonLanguage_CheckedChanged);
            // 
            // PictureBoxLogo
            // 
            this.PictureBoxLogo.Image = global::Neutron.Properties.Resources.Neutron_Logo;
            resources.ApplyResources(this.PictureBoxLogo, "PictureBoxLogo");
            this.PictureBoxLogo.Name = "PictureBoxLogo";
            this.PictureBoxLogo.TabStop = false;
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = MetroFramework.Forms.MetroFormBorderStyle.FixedSingle;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.GroupBoxLanguage);
            this.Controls.Add(this.ButtonClose);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ButtonPark);
            this.Controls.Add(this.PictureBoxLogo);
            this.Controls.Add(this.metroPanelMain);
            this.Controls.Add(this.mlUserInfo);
            this.Controls.Add(this.LabelWarehouseManagement);
            this.Controls.Add(this.LabelFormHeaderText);
            this.DisplayHeader = false;
            this.Name = "FrmMain";
            this.Theme = MetroFramework.MetroThemeStyle.Default;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmMain_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).EndInit();
            this.metroPanelMain.ResumeLayout(false);
            this.GroupBoxLanguage.ResumeLayout(false);
            this.GroupBoxLanguage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSourceLocations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSourceItemDefinition)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Drawing.Html.HtmlToolTip htmlToolTip1;
        private MetroFramework.Components.MetroToolTip metroToolTip1;
        private System.Windows.Forms.Label LabelFormHeaderText;
        private MetroFramework.Controls.MetroLabel mlUserInfo;
        private System.Windows.Forms.BindingSource BindingSourceItemDefinition;
        private MetroFramework.Components.MetroStyleManager metroStyleManager1;
        private System.Windows.Forms.BindingSource BindingSourceLocations;
        private MetroFramework.Controls.MetroPanel metroPanelMain;
        private MetroFramework.Controls.MetroTile MtHotAction;
        private MetroFramework.Controls.MetroTile MtLogOff;
        private MetroFramework.Controls.MetroTile MtStore;
        private MetroFramework.Controls.MetroTile MtHistory;
        private MetroFramework.Controls.MetroTile MtPick;
        private MetroFramework.Controls.MetroTile MtUtilities;
        private MetroFramework.Controls.MetroTile MtSystem;
        private MetroFramework.Controls.MetroTile MtLac;
        private MetroFramework.Controls.MetroTile MtUsers;
        private MetroFramework.Controls.MetroTile MtItemDefinitions;
        private MetroFramework.Controls.MetroTile MtLocations;
        private MetroFramework.Controls.MetroTile MtInventory;
        private System.Windows.Forms.Label LabelWarehouseManagement;
        private System.Windows.Forms.PictureBox PictureBoxLogo;
        private System.Windows.Forms.Button ButtonPark;
        private System.Windows.Forms.Button ButtonClose;
        private MetroFramework.Controls.MetroTile MtProductivity;
        private System.Windows.Forms.GroupBox GroupBoxLanguage;
        private System.Windows.Forms.RadioButton RadioButtonFrenchCanadian;
        private System.Windows.Forms.RadioButton RadioButtonEnglish;
        private System.Windows.Forms.Button button1;
    }
}

