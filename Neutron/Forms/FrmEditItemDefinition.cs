using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore.Extensions;
using NeutronCore;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using StorageType = NeutronData.Models.Lookups.StorageType;

namespace Neutron.Forms
{
    public partial class FrmEditItemDefinition : Form
    {
        private readonly IHistoryManager _historyManager;
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());

        private readonly GenericRepository<VelocityCode> _repoVelocityCode =
            new GenericRepository<VelocityCode>(new NeutronDb());

        private readonly GenericRepository<HeightCode> _repoHeightCode =
            new GenericRepository<HeightCode>(new NeutronDb());

        private readonly GenericRepository<Workstation> _repoStation = new GenericRepository<Workstation>(new NeutronDb());

        private readonly GenericRepository<ItemDefinition> _repoItemDefinition =
            new GenericRepository<ItemDefinition>(new NeutronDb());

        private readonly GenericRepository<StorageType> _repoStorageType =
            new GenericRepository<StorageType>(new NeutronDb());

        private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue =
            new GenericRepository<UnitOfIssue>(new NeutronDb());

        public ItemDefinition ItemDefinition { get; set; }

        public FrmEditItemDefinition(int id, IHistoryManager historyManager)
        {
            _historyManager = historyManager;
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            SetupViewEditForm();
            FillForm(id);
        }


        public void FillForm(int id)
        {
            ItemDefinition = _repoItemDefinition.FindByKey(id);
            if (ItemDefinition == null) return;
            TextBoxViewEditArea.Text = ItemDefinition.Area.Name;
            TextBoxViewEditItem.Text = ItemDefinition.Item;
            TextBoxViewEditDescription.Text = ItemDefinition.Description;
            TextBoxViewEditSystemMax.Text = ItemDefinition.SystemMax.ToString("G");
            TextBoxViewEditSystemMin.Text = ItemDefinition.SystemMin.ToString("G");
            TextBoxViewEditLocationMax.Text = ItemDefinition.LocationMax.ToString("G");
            TextBoxViewEditLocationMin.Text = ItemDefinition.LocationMin.ToString("G");
            ComboBoxViewEditUnitOfIssue.SelectedIndex =
                ComboBoxViewEditUnitOfIssue.FindStringExact(ItemDefinition.UnitOfIssue.Name);
            ComboBoxViewEditSizeCode.SelectedIndex =
                ComboBoxViewEditSizeCode.FindStringExact(ItemDefinition.SizeCode.Name);
            ComboBoxViewEditHeightCode.SelectedIndex =
                ComboBoxViewEditHeightCode.FindStringExact(ItemDefinition.HeightCode.Name);
            ComboBoxViewEditVelocityCode.SelectedIndex =
                ComboBoxViewEditVelocityCode.FindStringExact(ItemDefinition.VelocityCode.Name);
            ComboBoxViewEditStorageType.SelectedIndex =
                ComboBoxViewEditStorageType.FindStringExact(ItemDefinition.StorageType.Name);
            TextBoxViewEditWeight.Text = ItemDefinition.Weight.ToString("F4");
            CheckBoxViewEditScale.Checked = ItemDefinition.Scale;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            UpdateItemDefinition();
        }

        private void UpdateItemDefinition()
        {
            var rec = _repoItemDefinition.FindByKey(ItemDefinition.Id);
            if (rec == null) return;
            rec.Description = TextBoxViewEditDescription.Text;
            rec.SystemMax = Convert.ToInt32(TextBoxViewEditSystemMax.Text);
            rec.SystemMin = Convert.ToInt32(TextBoxViewEditSystemMin.Text);
            rec.LocationMax = Convert.ToInt32(TextBoxViewEditLocationMax.Text);
            rec.LocationMin = Convert.ToInt32(TextBoxViewEditLocationMin.Text);
            rec.UnitOfIssueId = Convert.ToInt32(ComboBoxViewEditUnitOfIssue.SelectedValue);
            rec.SizeCodeId = Convert.ToInt32(ComboBoxViewEditSizeCode.SelectedValue);
            rec.HeightCodeId = Convert.ToInt32(ComboBoxViewEditHeightCode.SelectedValue);
            rec.VelocityCodeId = Convert.ToInt32(ComboBoxViewEditVelocityCode.SelectedValue);
            rec.StorageTypeId = Convert.ToInt32(ComboBoxViewEditStorageType.SelectedValue);
            rec.Weight = Convert.ToSingle(TextBoxViewEditWeight.Text);
            rec.Scale = CheckBoxViewEditScale.Checked;
            _repoItemDefinition.Update(rec);
            _historyManager.SaveHistoryAsync(ActionCode.ItemModify, ItemDefinition);
            ItemDefinition = rec;  
        }

        private void SetupViewEditForm()
        {
            ComboBoxViewEditSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxViewEditSizeCode.DisplayMember = "Name";
            ComboBoxViewEditSizeCode.ValueMember = "Id";

            ComboBoxViewEditVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxViewEditVelocityCode.DisplayMember = "Name";
            ComboBoxViewEditVelocityCode.ValueMember = "Id";

            ComboBoxViewEditHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxViewEditHeightCode.DisplayMember = "Name";
            ComboBoxViewEditHeightCode.ValueMember = "Id";

            ComboBoxViewEditStorageType.DataSource = _repoStorageType.All();
            ComboBoxViewEditStorageType.DisplayMember = "Name";
            ComboBoxViewEditStorageType.ValueMember = "Id";

            ComboBoxViewEditUnitOfIssue.DataSource = _repoUnitOfIssue.All();
            ComboBoxViewEditUnitOfIssue.DisplayMember = "Name";
            ComboBoxViewEditUnitOfIssue.ValueMember = "Id";
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmEditItemDefinition",
               resourceDir: languageDirectory, usingResourceSet: null);
                LabelViewEditWeight.Text = _resourceManager.GetString("Weight");
                CheckBoxViewEditScale.Text = _resourceManager.GetString("UseScale");
                LabelViewEditUnitOfIssue.Text = _resourceManager.GetString("UnitOfIssue");
                LabelViewEditStorageType.Text = _resourceManager.GetString("StorageType");
                LabelViewEditHeight.Text = _resourceManager.GetString("Height");
                LabelViewEditVelocity.Text = _resourceManager.GetString("Velocity");
                LabelViewEditSystemMin.Text = _resourceManager.GetString("SystemMin");
                LabelViewEditSize.Text = _resourceManager.GetString("Size");
                LabelViewEditSystemMax.Text = _resourceManager.GetString("SystemMax");
                LabelViewEditLocationMin.Text = _resourceManager.GetString("LocationMin");
                LabelViewEditLocationMax.Text = _resourceManager.GetString("LocationMax");
                LabelViewEditDescription.Text = _resourceManager.GetString("Description");
                LabelViewEditItem.Text = _resourceManager.GetString("Item");
                LabelViewEditArea.Text = _resourceManager.GetString("Area");
                ButtonSave.Text = _resourceManager.GetString("Save");
                ButtonCancel.Text = _resourceManager.GetString("Cancel");
                Text = _resourceManager.GetString("EditItemDefinition");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}
