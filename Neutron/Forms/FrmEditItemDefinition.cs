using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
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
        
        private readonly GenericRepository<SizeCode> _repoSizeCode;
        private readonly GenericRepository<VelocityCode> _repoVelocityCode;
        private readonly GenericRepository<HeightCode> _repoHeightCode;
        private readonly GenericRepository<Workstation> _repoStation;
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition;
        private readonly GenericRepository<StorageType> _repoStorageType;
        private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue;
        private readonly Func<NeutronDb> _contextFactory;

        public ItemDefinition ItemDefinition { get; set; }

        public FrmEditItemDefinition(int id, IHistoryManager historyManager, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _historyManager = historyManager;
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _repoSizeCode = new GenericRepository<SizeCode>(contextFactory);
            _repoVelocityCode = new GenericRepository<VelocityCode>(contextFactory);
            _repoHeightCode = new GenericRepository<HeightCode>(contextFactory);
            _repoStation = new GenericRepository<Workstation>(contextFactory);
            _repoItemDefinition = new GenericRepository<ItemDefinition>(contextFactory);
            _repoStorageType = new GenericRepository<StorageType>(contextFactory);
            _repoUnitOfIssue = new GenericRepository<UnitOfIssue>(contextFactory);

            SetupViewEditForm();
            FillForm(id);
        }


        public void FillForm(int id)
        {
            ItemDefinition = _repoItemDefinition.FindByKeyInclude(
                r => r.Id == id,
                i => i.Area,
                i => i.UnitOfIssue,
                i => i.SizeCode,
                i => i.HeightCode,
                i => i.VelocityCode,
                i => i.StorageType);

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

        private async void ButtonSave_Click(object sender, EventArgs e)
        {
           await UpdateItemDefinition();
        }

        private async Task UpdateItemDefinition()
        {
            var rec = await _repoItemDefinition.FindByKeyIncludeAsync(
                r => r.Id == ItemDefinition.Id,
                i => i.Area);
            if (rec == null) return;
           
            await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);

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
            await _repoItemDefinition.UpdateAsync(rec);
           await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
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
