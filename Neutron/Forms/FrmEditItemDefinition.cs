using System;
using System.Windows.Forms;
using Neutron.Extensions;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;

namespace Neutron.Forms
{
    public partial class FrmEditItemDefinition : Form
    {

        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());

        private readonly GenericRepository<VelocityCode> _repoVelocityCode =
            new GenericRepository<VelocityCode>(new NeutronDb());

        private readonly GenericRepository<HeightCode> _repoHeightCode =
            new GenericRepository<HeightCode>(new NeutronDb());

        private readonly GenericRepository<LocationCode> _repoLocationCode =
            new GenericRepository<LocationCode>(new NeutronDb());

        private readonly GenericRepository<Station> _repoStation = new GenericRepository<Station>(new NeutronDb());

        private readonly GenericRepository<ItemDefinition> _repoItemDefinition =
            new GenericRepository<ItemDefinition>(new NeutronDb());

        private readonly GenericRepository<StorageType> _repoStorageType =
            new GenericRepository<StorageType>(new NeutronDb());

        private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue =
            new GenericRepository<UnitOfIssue>(new NeutronDb());

        public ItemDefinition ItemDefinition { get; set; }

        public FrmEditItemDefinition(int id)
        {
            InitializeComponent();
            SetupViewEditForm();
            FillForm(id);
        }


        public void FillForm(int id)
        {
            ItemDefinition = _repoItemDefinition.FindByKey(id);
            if (ItemDefinition != null)
            {
                TextBoxViewEditId.Text = ItemDefinition.Id.ToString();
                TextBoxViewEditStation.Text = ItemDefinition.Station.Name;
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
                ComboBoxViewEditLocationCode.SelectedIndex =
                    ComboBoxViewEditLocationCode.FindStringExact(ItemDefinition.LocationCode.Name);
                ComboBoxViewEditStorageType.SelectedIndex =
                    ComboBoxViewEditStorageType.FindStringExact(ItemDefinition.StorageType.Name);
                TextBoxViewEditWeight.Text = ItemDefinition.Weight.ToString("F4");
                CheckBoxViewEditScale.Checked = ItemDefinition.Scale;
            }
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
            var rec = _repoItemDefinition.FindByKey(TextBoxViewEditId.Text.ParseInt());
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
            rec.LocationCodeId = Convert.ToInt32(ComboBoxViewEditLocationCode.SelectedValue);
            rec.StorageTypeId = Convert.ToInt32(ComboBoxViewEditStorageType.SelectedValue);
            rec.Weight = Convert.ToSingle(TextBoxViewEditWeight.Text);
            rec.Scale = CheckBoxViewEditScale.Checked;
            _repoItemDefinition.Update(rec);
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

            ComboBoxViewEditLocationCode.DataSource = _repoLocationCode.All();
            ComboBoxViewEditLocationCode.DisplayMember = "Name";
            ComboBoxViewEditLocationCode.ValueMember = "Id";

            ComboBoxViewEditStation.DataSource = _repoStation.All();
            ComboBoxViewEditStation.DisplayMember = "Name";
            ComboBoxViewEditStation.ValueMember = "Id";

            ComboBoxViewEditStorageType.DataSource = _repoStorageType.All();
            ComboBoxViewEditStorageType.DisplayMember = "Name";
            ComboBoxViewEditStorageType.ValueMember = "Id";

            ComboBoxViewEditUnitOfIssue.DataSource = _repoUnitOfIssue.All();
            ComboBoxViewEditUnitOfIssue.DisplayMember = "Name";
            ComboBoxViewEditUnitOfIssue.ValueMember = "Id";
        }
    }
}
