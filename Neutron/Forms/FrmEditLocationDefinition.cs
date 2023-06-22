using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;

namespace Neutron.Forms
{
    public partial class FrmEditLocationDefinition : Form
    {
        private readonly IHistoryManager _historyManager;
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());

        private readonly GenericRepository<VelocityCode> _repoVelocityCode =
            new GenericRepository<VelocityCode>(new NeutronDb());

        private readonly GenericRepository<HeightCode> _repoHeightCode =
            new GenericRepository<HeightCode>(new NeutronDb());

        private readonly GenericRepository<Location> _repoLocation =
            new GenericRepository<Location>(new NeutronDb());
        
        private readonly GenericRepository<StorageDevice> _repoStorageDevice =
            new GenericRepository<StorageDevice>(new NeutronDb());

        private Location _location;

        public FrmEditLocationDefinition(int id, IHistoryManager historyManager)
        {
            _historyManager = historyManager;
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _location = _repoLocation.FindByKey(id);
            SetupViewEditForm();
            FillForm();
        }

        public void FillForm()
        {
            if (_location == null) return;
            TextBoxViewEditArea.Text = _location.Area.Name;

            var storageDevice = _repoStorageDevice.FindBy(r => r.StorageDeviceNumber == _location.Loc1).FirstOrDefault();
            if (storageDevice is null) return;

            ComboBoxViewEditDevice.SelectedValue  = storageDevice.Id;
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

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            UpdateLocation();
        }

        private void UpdateLocation()
        {
            var rec = _repoLocation.FindByKey(_location.Id);

            if (rec == null) return;

            rec.AreaId = _location.AreaId;
            rec.Loc1 = ((StorageDevice)ComboBoxViewEditDevice.SelectedItem).StorageDeviceNumber;
            rec.Loc2 = Convert.ToInt32(TextBoxViewEditLoc2.Text);
            rec.Loc3 = Convert.ToInt32(TextBoxViewEditLoc3.Text);
            rec.Loc4 = Convert.ToInt32(TextBoxViewEditLoc4.Text);
            rec.Loc5 = Convert.ToInt32(TextBoxViewEditLoc5.Text);
            rec.SizeCodeId = Convert.ToInt32(ComboBoxViewEditSizeCode.SelectedValue);
            rec.HeightCodeId = Convert.ToInt32(ComboBoxViewEditHeightCode.SelectedValue);
            rec.VelocityCodeId = Convert.ToInt32(ComboBoxViewEditVelocityCode.SelectedValue);
            rec.LocationCode = TextBoxViewEditLocationCode.Text;
            rec.InUse = CheckBoxViewEditInUse.Checked;
            _repoLocation.Update(rec);
            _historyManager.SaveHistoryAsync(ActionCode.LocationModify, rec);
            _location = rec;
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

            ComboBoxViewEditDevice.DataSource = _repoStorageDevice.All();
            ComboBoxViewEditDevice.DisplayMember = "Name";
            ComboBoxViewEditDevice.ValueMember = "Id";

        }

        //private List<HardwareDeviceLookup> GetHardwareDeviceList()
        //{
        //    List<HardwareDeviceLookup> recs;

        //    using (var db = new NeutronDb())
        //    {
        //        recs = db.HardwareDevices.Where(r => r.WorkstationId == _location.AreaId)
        //            .Select(s => new HardwareDeviceLookup() { Id =s.DeviceNumber , Name = s.Name  })
        //            .ToList();
        //    }
        //    return recs;
        //}

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmEditLocationDefinition",
               resourceDir: languageDirectory, usingResourceSet: null);

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
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}
