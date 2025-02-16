using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;

namespace Neutron.Forms
{
    public partial class FrmEditLocationDefinition : Form
    {
        private readonly int _id;
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private readonly GenericRepository<SizeCode> _repoSizeCode;
        private readonly GenericRepository<VelocityCode> _repoVelocityCode;
        private readonly GenericRepository<HeightCode> _repoHeightCode;
        private readonly GenericRepository<Location> _repoLocation;
        private readonly GenericRepository<StorageDevice> _repoStorageDevice;

        private Location _location;
        private readonly Func<NeutronDb> _contextFactory;

        public FrmEditLocationDefinition(int id, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            InitializeComponent();
            _id = id;
            _repoSizeCode = new GenericRepository<SizeCode>(contextFactory);
            _repoVelocityCode = new GenericRepository<VelocityCode>(contextFactory);
            _repoHeightCode = new GenericRepository<HeightCode>(contextFactory);
            _repoLocation = new GenericRepository<Location>(contextFactory);
            _repoStorageDevice = new GenericRepository<StorageDevice>(contextFactory);
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            SetupViewEditForm();
            FillForm();
        }

        public void FillForm()
        {
            _location = _repoLocation.FindByKey(_id);
            if (_location == null || _location.Area == null || _location.SizeCode == null || _location.VelocityCode == null || _location.HeightCode == null) return;
            TextBoxViewEditArea.Text = _location.Area.Name;

            var storageDevice = _repoStorageDevice.FindBy(r => r.StorageDeviceNumber == _location.Loc1).FirstOrDefault();
            if (storageDevice is null) return;

            ComboBoxViewEditDevice.SelectedValue = storageDevice.Id;
            TextBoxViewEditLoc2.Text = _location.Loc2.ToString();
            TextBoxViewEditLoc3.Text = _location.Loc3.ToString();
            TextBoxViewEditLoc4.Text = _location.Loc4.ToString();
            TextBoxViewEditLoc5.Text = _location.Loc5.ToString();
            TextBoxViewEditSlot.Text = _location.Slot;
            ComboBoxViewEditSizeCode.SelectedIndex = ComboBoxViewEditSizeCode.FindStringExact(_location.SizeCode.Name);
            ComboBoxViewEditVelocityCode.SelectedIndex = ComboBoxViewEditVelocityCode.FindStringExact(_location.VelocityCode.Name);
            ComboBoxViewEditHeightCode.SelectedIndex = ComboBoxViewEditHeightCode.FindStringExact(_location.HeightCode.Name);
            TextBoxViewEditLocationCode.Text = _location.LocationCode;
            CheckBoxViewEditInUse.Checked = _location.InUse;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void ButtonSave_Click(object sender, EventArgs e)
        {
            try
            {
                await UpdateLocation();
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging purposes
                Debug.WriteLine(ex.ToString());
                // Show a message box to the user
                MessageBox.Show("An error occurred while updating the location. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateLocation()
        {
            if (_location == null) return;

            var rec = _repoLocation.FindByKey(_location.Id);
            if (rec == null) return;

            await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.LocationModify, rec);

            rec.AreaId = _location.AreaId;
            if (ComboBoxViewEditDevice.SelectedItem is StorageDevice device)
            {
                rec.Loc1 = device.StorageDeviceNumber;
            }
            else
            {
                // Handle the error, for example, throw an exception or return
                throw new InvalidOperationException("SelectedItem is not a StorageDevice.");
            }
            if (int.TryParse(TextBoxViewEditLoc2.Text, out int loc2))
            {
                rec.Loc2 = loc2;
            }
            else
            {
                // Handle the error, for example, throw an exception or return
                throw new InvalidOperationException("TextBoxViewEditLoc2 does not contain a valid integer.");
            }
            if (int.TryParse(TextBoxViewEditLoc3.Text, out int loc3))
            {
                rec.Loc3 = loc3;
            }
            else
            {
                // Handle the error, for example, throw an exception or return
                throw new InvalidOperationException("TextBoxViewEditLoc3 does not contain a valid integer.");
            }
            if (int.TryParse(TextBoxViewEditLoc4.Text, out int loc4))
            {
                rec.Loc4 = loc4;
            }
            else
            {
                // Handle the error, for example, throw an exception or return
                throw new InvalidOperationException("TextBoxViewEditLoc4 does not contain a valid integer.");
            }
            if (int.TryParse(TextBoxViewEditLoc5.Text, out int loc5))
            {
                rec.Loc5 = loc5;
            }
            else
            {
                // Handle the error, for example, throw an exception or return
                throw new InvalidOperationException("TextBoxViewEditLoc5 does not contain a valid integer.");
            }
            if (ComboBoxViewEditSizeCode.SelectedValue is int sizeCodeId)
            {
                rec.SizeCodeId = sizeCodeId;
            }
            else
            {
                // Handle the error, for example, throw an exception or return
                throw new InvalidOperationException("ComboBoxViewEditSizeCode.SelectedValue is not an integer.");
            }
            if (ComboBoxViewEditHeightCode.SelectedValue is int heightCodeId)
            {
                rec.HeightCodeId = heightCodeId;
            }
            else
            {
                // Handle the error, for example, throw an exception or return
                throw new InvalidOperationException("ComboBoxViewEditHeightCode.SelectedValue is not an integer.");
            }
            if (ComboBoxViewEditVelocityCode.SelectedValue is int velocityCodeId)
            {
                rec.VelocityCodeId = velocityCodeId;
            }
            else
            {
                // Handle the error, for example, throw an exception or return
                throw new InvalidOperationException("ComboBoxViewEditVelocityCode.SelectedValue is not an integer.");
            }

            rec.LocationCode = TextBoxViewEditLocationCode.Text;
            rec.InUse = CheckBoxViewEditInUse.Checked;
            await _repoLocation.UpdateAsync(rec);
            await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.LocationModify, rec);
            _location = rec;
        }

        private void SetupComboBox<T>(ComboBox comboBox, GenericRepository<T> repository) where T : class, IEntity
        {
            comboBox.DataSource = repository.All();
            comboBox.DisplayMember = "Name";
            comboBox.ValueMember = "Id";
        }
        private void SetupViewEditForm()
        {
            SetupComboBox(ComboBoxViewEditSizeCode, _repoSizeCode);
            SetupComboBox(ComboBoxViewEditVelocityCode, _repoVelocityCode);
            SetupComboBox(ComboBoxViewEditHeightCode, _repoHeightCode);
            SetupComboBox(ComboBoxViewEditDevice, _repoStorageDevice);
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmEditLocationDefinition",
                    resourceDir: languageDirectory, usingResourceSet: null);
                UpdateUIElementsWithLocalizedText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }

        // ReSharper disable once InconsistentNaming
        private void UpdateUIElementsWithLocalizedText()
        {
            CheckBoxViewEditInUse.Text = _resourceManager.GetString("InUse");
            LabelViewEditSlot.Text = _resourceManager.GetString("Slot");
            LabelViewEditTag.Text = _resourceManager.GetString("Tag");
            LabelViewEditBack.Text = _resourceManager.GetString("Back");
            LabelViewEditOver.Text = _resourceManager.GetString("Over");
            LabelViewEditTray.Text = _resourceManager.GetString("Tray");
            LabelViewEditDevice.Text = _resourceManager.GetString("Device");
            LabelViewEditArea.Text = _resourceManager.GetString("Area");
            LabelViewEditLocationCode.Text = _resourceManager.GetString("LocationCode");
            LabelViewEditHeight.Text = _resourceManager.GetString("Height");
            LabelViewEditVelocity.Text = _resourceManager.GetString("Velocity");
            LabelViewEditSize.Text = _resourceManager.GetString("Size");
            ButtonCancel.Text = _resourceManager.GetString("Cancel");
            ButtonSave.Text = _resourceManager.GetString("Save");
            Text = _resourceManager.GetString("FrmEditLocationDefinition");
        }


    }
}
