using Neutron.Models;
using NeutronCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using NeutronCore.Enums;
using System.Threading;

namespace Neutron.Forms
{
    public partial class FrmCostCenter : Form
    {
        private CostCenterManager _costCenterManager;
        public string CostCenterCode = String.Empty;
        public string CostCenterName = String.Empty;
        public ActionCode CostCenterActionCode = ActionCode.PickHot;
        private IDynamicLogger _logger;
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public FrmCostCenter()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            Init();
        }

        private void Init()
        {
        
            FillCostCenterComboBox();
            RadioButtonCostCenter.Checked = true;
            ComboBoxCostCenter.Visible = true;
            TextBoxFindCostCenter.Visible = true;
            RadioButtonCostCenter.Visible = true;
            RadioButtonPick.Text = "Pick";  //    $"{_resourceManager.GetString($"Pick")}";
            RadioButtonPick.Tag = "Pick";
            _logger = NeutronCore.Global.Logger.SetupLogger("CostCenter");
        }


        private void FillCostCenterComboBox()
        {
            var costCenterPath = LoaderSettings.GetCostCenterPath();
            _costCenterManager = new CostCenterManager(costCenterPath);
            var costCenterList = _costCenterManager.GetCostCenterListAsync();
            ComboBoxCostCenter.DataSource = costCenterList;
            ComboBoxCostCenter.DisplayMember = "Name";
            ComboBoxCostCenter.ValueMember = "Code";
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            CostCenterCode = ComboBoxCostCenter.SelectedValue.ToString();
            CostCenterName = ComboBoxCostCenter.Text;
            CostCenterActionCode = GetActionCode(); 
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ComboBoxCostCenter_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void TextBoxFindCostCenter_Leave(object sender, EventArgs e)
        {
            var search = TextBoxFindCostCenter.Text;
            var costCenterList = _costCenterManager.GetCostCenterList(search.ToLower());

            ComboBoxCostCenter.DataSource = costCenterList;
            ComboBoxCostCenter.DisplayMember = "Name";
            ComboBoxCostCenter.ValueMember = "Code";
            //ComboBoxCostCenter.DroppedDown = true;
        }
        private void ComboBoxCostCenter_TextChanged(object sender, EventArgs e)
        {

            try
            {
                var t = ((ComboBox)sender).SelectedItem;
                if (t != null)
                {
                    var cost = (CostCenter)t;
                    var n = cost.Name;
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.LogDetailAsync($"Error reading Cost Center Text Changed: {ex.Message} {Environment.NewLine} {ex.InnerException}"));

            }
        }
        private void TextBoxFindCostCenter_KeyPress(object sender, KeyPressEventArgs e)
        {
        }
        private void TextBoxFindCostCenter_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyData == Keys.Enter)
            {
                //file CostCenter Combo Box
                var search = TextBoxFindCostCenter.Text;
                var costCenterList = _costCenterManager.GetCostCenterList(search.ToLower());
                ComboBoxCostCenter.DataSource = costCenterList;
                ComboBoxCostCenter.DisplayMember = "Name";
                ComboBoxCostCenter.ValueMember = "Code";
                ComboBoxCostCenter.DroppedDown = true;
            }
        }
        //private void HotAction_Enter(object sender, EventArgs e)
        //{
        //    if (!_useCostCenter) return;
        //    TextBoxFindCostCenter.Focus();
        //}


        private ActionCode GetActionCode()
        {
            var result = ActionCode.PickHot;
            var radioButtons = new List<RadioButton> { RadioButtonPick, RadioButtonWarranty, RadioButtonScrap, RadioButtonOther, RadioButtonCostCenter };
                foreach (var item in radioButtons)
                {
                    if (item.Checked)
                    {
                        switch (item.Tag.ToString())
                        {
                            case "Pick":
                                result = ActionCode.PickHot;
                                break;
                            case "Warranty":
                                result = ActionCode.WarrantyHotPick;
                                break;
                            case "Scrap":
                                result = ActionCode.ScrapHotPick;
                                break;
                            case "Other":
                                result = ActionCode.OtherHotPick;
                                break;
                            case "Cost Center":
                                result = ActionCode.CostCenterHotPick;
                                break;
                            default:
                                result = ActionCode.PickHot;
                                break;
                        }
                    }
                }
           
            return result;
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmHotAction",
                    resourceDir: languageDirectory, usingResourceSet: null);

                GroupBoxHotActions.Text = _resourceManager.GetString($"TransactionType");
                RadioButtonCostCenter.Text = _resourceManager.GetString($"CostCenter");
                RadioButtonOther.Text = _resourceManager.GetString($"Other");
                RadioButtonScrap.Text = _resourceManager.GetString($"Scrap");
                RadioButtonWarranty.Text = _resourceManager.GetString($"Warranty");
                RadioButtonPick.Text = _resourceManager.GetString($"Pick");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }
        //private void RadioButtonHotAction(object sender, EventArgs e)
        //{
        //    if (RadioButtonPick.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonPick.Text;
        //    }
        //    else if (RadioButtonWarranty.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonWarranty.Text;
        //    }
        //    else if (RadioButtonScrap.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonScrap.Text;
        //    }
        //    else if (RadioButtonOther.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonOther.Text;
        //    }
        //    else if (RadioButtonCostCenter.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonCostCenter.Text;
        //    }
        //}
    }
}
