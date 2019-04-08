using Equin.ApplicationFramework;
using MetroFramework.Forms;
using Neutron.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.ModelViews;
using NeutronCore.Extensions;
using NeutronData.Models.Lookups;
using Neutron.Interfaces;

namespace Neutron.Forms
{
    public partial class FrmGroups : MetroForm
    {
        private BindingSource bindingSource = new BindingSource();
        private GenericRepository<Location> repoLocation = new GenericRepository<Location>(new NeutronDb());
        private GenericRepository<SizeCode> repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
        private GenericRepository<VelocityCode> repoVelocityCode = new GenericRepository<VelocityCode>(new NeutronDb());
        private GenericRepository<HeightCode> repoHeightCode = new GenericRepository<HeightCode>(new NeutronDb());
        private GenericRepository<Station> repoStation = new GenericRepository<Station>(new NeutronDb());
        INomenclature nomenclature;

        public FrmGroups(INomenclature nomenclature)
        {
            InitializeComponent();
            this.nomenclature = nomenclature;
            SetupGrid();
            SetupTabControl();
            SetupNewForm();
            SetupViewEditForm();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
        }

        private void FrmLocations_Load(object sender, EventArgs e)
        {

            int id = RefreshData();
            SetupViewEditBindings();
            this.AutoValidate = AutoValidate.Disable;
        }
        // Set the focus to the passed in recId if it's passed in
        private int RefreshData(int recId = 0)
        {
            int idx = 0;
            IEnumerable<Location> recs = repoLocation.AllInclude(h => h.HeightCode, h => h.SizeCode, h => h.VelocityCode, h => h.Station);
            IEnumerable<LocationView> projection = recs.Select(r => new LocationView
            { Id = r.Id, StationId = r.StationId, StationName = r.Station.Name, Loc1 = r.Loc1
                    , Loc2 = r.Loc2 , Loc3 = r.Loc3, Loc4 = r.Loc4, Loc5 = r.Loc5, Slot = r.Slot
                    , SizeCodeId = r.SizeCodeId, VelocityCodeId = r.VelocityCodeId, HeightCodeId = r.HeightCodeId
                    , SizeCodeName = r.SizeCode.Name, VelocityCodeName = r.VelocityCode.Name, HeightCodeName = r.HeightCode.Name
            }).OrderBy(o => o.StationId)
                        .ThenBy(o => o.Loc1)
                        .ThenBy(o => o.Loc2)
                        .ThenBy(o => o.Loc3)
                        .ThenBy(o => o.Loc4)
                        .ThenBy(o => o.Loc5)
                        .ThenBy(o => o.SizeCodeName)
                        .ThenBy(o => o.VelocityCodeName)
                        .ThenBy(o => o.HeightCodeName)
                        .ToList();
            bindingSource.DataSource = projection;
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.DataSource = bindingSource;
            if (GetRecordCount() > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(recId);
                }
                DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                DataGridView1.Refresh();
                DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                DataGridView1.Rows[idx].Selected = true;
            }
            return idx;
        }

        public int IndexOf(int value)
        {
            int count = bindingSource.Count;
            int itemIndex = -1;
            for (int i = 0; i < count; i++)
            {
                int rec = ((LocationView) bindingSource[i]).Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }

        private void SetupViewEditBindings()
        {
            TextBoxViewEditId.DataBindings.Add("Text", bindingSource, "Id");
            ComboBoxViewEditStation.DataBindings.Add("SelectedValue", bindingSource, "StationId");
            TextBoxViewEditLoc1.DataBindings.Add("Text", bindingSource, "Loc1");
            TextBoxViewEditLoc2.DataBindings.Add("Text", bindingSource, "Loc2");
            TextBoxViewEditLoc3.DataBindings.Add("Text", bindingSource, "Loc3");
            TextBoxViewEditLoc4.DataBindings.Add("Text", bindingSource, "Loc4");
            TextBoxViewEditLoc5.DataBindings.Add("Text", bindingSource, "Loc5");
            TextBoxViewEditSlot.DataBindings.Add("Text", bindingSource, "Slot");
            ComboBoxViewEditSizeCode.DataBindings.Add("SelectedValue", bindingSource, "SizeCodeId");
            ComboBoxViewEditVelocityCode.DataBindings.Add("SelectedValue", bindingSource, "VelocityCodeId");
            ComboBoxViewEditHeightCode.DataBindings.Add("SelectedValue", bindingSource, "HeightCodeId");
        }

        private int GetRecordCount()
        {
            //int count = bindingListView.Count;
            int count = bindingSource.Count;
            LabelRecordCount.Text = string.Format("Records: {0}", count.ToString());
            return count;
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
        }

        #region Find Functions
        private void MButtonFind_Click(object sender, EventArgs e)
        {
            FindRecord(TextBoxFind.Text.Trim());
        }

        private void FindRecord(string s)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    RefreshData();
                }
                else
                {
                    IEnumerable<Location> recs = repoLocation.AllInclude(h => h.HeightCode, h => h.SizeCode, h => h.VelocityCode, h => h.Station);
                    IEnumerable<LocationView> projection = recs.Select(r => new LocationView
                    {
                        Id = r.Id,
                        StationId = r.StationId,
                        StationName = r.Station.Name,
                        Loc1 = r.Loc1,
                        Loc2 = r.Loc2,
                        Loc3 = r.Loc3,
                        Loc4 = r.Loc4,
                        Loc5 = r.Loc5,
                        Slot = r.Slot,
                        SizeCodeId = r.SizeCodeId,
                        VelocityCodeId = r.VelocityCodeId,
                        HeightCodeId = r.HeightCodeId,
                        SizeCodeName = r.SizeCode.Name,
                        VelocityCodeName = r.VelocityCode.Name,
                        HeightCodeName = r.HeightCode.Name
                    }).OrderBy(o => o.StationId)
                        .ThenBy(o => o.Loc1)
                        .ThenBy(o => o.Loc2)
                        .ThenBy(o => o.Loc3)
                        .ThenBy(o => o.Loc4)
                        .ThenBy(o => o.Loc5)
                        .ThenBy(o => o.SizeCodeName)
                        .ThenBy(o => o.VelocityCodeName)
                        .ThenBy(o => o.HeightCodeName)
                        .ToList();;

                    bindingSource.DataSource = projection.Where(d => d.Slot.Contains(s)).ToList();
                        
                    GetRecordCount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Find Error: " + ex.Message);
            }
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                FindRecord(TextBoxFind.Text.Trim());
            }
        }
        #endregion

        #region Button Clicks
        private void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            RefreshData();
            TextBoxFind.Focus();
        }

        private void MButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
        }

        private void MButtonNew_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
        }

        private void MbViewEditListing_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbViewEditNew_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
        }

        private void MbViewEditClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MbViewEditSave_Click(object sender, EventArgs e)
        {
            UpdateViewEdit();
        }

        private void MbNewListing_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbNewViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
        }

        private void MbNewSave_Click(object sender, EventArgs e)
        {
            SaveNew();
        }

        private void MbNewClose_Click(object sender, EventArgs e)
        {
            this.Close();
        } 
        #endregion

        private void SaveNew()
        {
            if (this.ValidateChildren())
            {
                try
                {
                    int station = (ComboBoxViewEditStation.SelectedItem as Station).Id;
                    string slt = CreateSlot(station, TextBoxNewLoc1.Text, TextBoxNewLoc2.Text, TextBoxNewLoc3.Text, TextBoxNewLoc4.Text, TextBoxNewLoc5.Text);
                    var rec = new Location
                    {
                        StationId = station,
                        Loc1 = (TextBoxNewLoc1.Text).ParseInt(),
                        Loc2 = (TextBoxNewLoc2.Text).ParseInt(),
                        Loc3 = (TextBoxNewLoc3.Text).ParseInt(),
                        Loc4 = (TextBoxNewLoc4.Text).ParseInt(),
                        Loc5 = (TextBoxNewLoc5.Text).ParseInt(),
                        Slot = slt,
                        SizeCodeId = (ComboBoxViewEditSizeCode.SelectedItem as SizeCode).Id,
                        VelocityCodeId = (ComboBoxViewEditVelocityCode.SelectedItem as VelocityCode).Id,
                        HeightCodeId = (ComboBoxViewEditHeightCode.SelectedItem as HeightCode).Id
                    };
                    if (!ValidateFields(rec))
                    {
                        MessageBox.Show("Invalid Entry.");
                        return;
                    }

                    if (IsDuplicate(rec))
                    {
                        MessageBox.Show("Duplicate Entry.");
                        return;
                    }
                    TextBoxNewSlot.Text = slt;
                    repoLocation.Insert(rec);
                    RefreshData(rec.Id);
                    tabControl1.SelectedTab = tabPage1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save Error. " + ex.Message + ex.InnerException + ex.InnerException.InnerException);
                }
            }else
            {
                MessageBox.Show("Field Validation Error");
            }
        }

        private void UpdateViewEdit()
        {
            int id = ((LocationView)bindingSource.Current).Id;
            int station = (ComboBoxViewEditStation.SelectedItem as Station).Id;
            string slt = CreateSlot(station, TextBoxViewEditLoc1.Text, TextBoxViewEditLoc2.Text, TextBoxViewEditLoc3.Text, TextBoxViewEditLoc4.Text, TextBoxViewEditLoc5.Text);
            var rec = new Location
            {
                Id = id,
                StationId = station,
                Loc1 = (TextBoxViewEditLoc1.Text).ParseInt(),
                Loc2 = (TextBoxViewEditLoc2.Text).ParseInt(),
                Loc3 = (TextBoxViewEditLoc3.Text).ParseInt(),
                Loc4 = (TextBoxViewEditLoc4.Text).ParseInt(),
                Loc5 = (TextBoxViewEditLoc5.Text).ParseInt(),
                Slot = slt,
                SizeCodeId = (ComboBoxViewEditSizeCode.SelectedItem as SizeCode).Id,
                VelocityCodeId = (ComboBoxViewEditVelocityCode.SelectedItem as VelocityCode).Id,
                HeightCodeId = (ComboBoxViewEditHeightCode.SelectedItem as HeightCode).Id
            };
            if (!ValidateFields(rec))
            {
                MessageBox.Show("Invalid Entry.");
                return;
            }
            TextBoxViewEditSlot.Text = slt;
            repoLocation.Update(rec);
            RefreshData(rec.Id);
            tabControl1.SelectedTab = tabPage1;
        }

        private bool ValidateFields(Location rec)
        {
            if (!IntegerValidator(rec.Loc1))
            {
                return false;
            }
            if (!IntegerValidator(rec.Loc2))
            {
                return false;
            }
            if (!IntegerValidator(rec.Loc3))
            {
                return false;
            }
            if (!IntegerValidator(rec.Loc4))
            {
                return false;
            }
            if (!IntegerValidator(rec.Loc5))
            {
                return false;
            }
            return true;
        }

        private bool StringValidator(string input)
        {
            string pattern = "[^a-zA-Z]";
            if (Regex.IsMatch(input, pattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //validate integer 
        private bool IntegerValidator(int input)
        {
            string pattern = "^[0-9]+$";
            if (Regex.IsMatch(input.ToString(), pattern))
            {
                if (input <= 0)
                {
                    MessageBox.Show("Entry must be greater than zero.");
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsDuplicate(Location recIn)
        {
            Location rec = repoLocation.FindBy(f => f.Slot == recIn.Slot).FirstOrDefault();
            if (rec != null)
            {
                MessageBox.Show("Record already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            return false;
        }

        private string CreateSlot(int station, string loc1, string loc2, string loc3, string loc4, string loc5)
        {
            var sb = new StringBuilder();
            sb.Append(station.ToString().PadLeft(2, '0'));
            sb.Append(loc1.PadLeft(2, '0'));
            sb.Append(loc2.PadLeft(2, '0'));
            sb.Append(loc3.PadLeft(2, '0'));
            sb.Append(loc4.PadLeft(2, '0'));
            sb.Append(loc5.PadLeft(2, '0'));
            return sb.ToString();
        }

        #region Form Setup
        private void SetupGrid()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            int w = (DataGridView1.Width - 60) / 10;

            DataGridViewColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "StationName";
            col.HeaderText = "Station";
            col.Width = w;
            col.Name = "StationName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc1";
            col.HeaderText = nomenclature.LabelDevice;
            col.Width = w;
            col.Name = "Loc1";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc2";
            col.HeaderText = nomenclature.LabelTray;
            col.Width = w;
            col.Name = "Loc2";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc3";
            col.HeaderText = nomenclature.LabelOver;
            col.Width = w;
            col.Name = "Loc3";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc4";
            col.HeaderText = nomenclature.LabelBack;
            col.Width = w;
            col.Name = "Loc4";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc5";
            col.HeaderText = "Tag";
            col.Width = w;
            col.Name = "Loc5";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "SizeCodeName";
            col.HeaderText = "Size Code";
            col.Width = w;
            col.Name = "SizeCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "VelocityCodeName";
            col.HeaderText = "Velocity Code";
            col.Width = w;
            col.Name = "VelocityCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "HeightCodeName";
            col.HeaderText = "Height Code";
            col.Width = w;
            col.Name = "HeightCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Slot";
            col.HeaderText = "Slot";
            col.Visible = true;
            col.Width = w + 5;
            col.Name = "Slot";
            DataGridView1.Columns.Add(col);
        }

        private void SetupTabControl()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.Text = string.Empty;
            }
        }
        private void SetupNewForm()
        {
            LabelFindDescription.Text = "Search any part of Slot field";

            ComboBoxNewSizeCode.DataSource = repoSizeCode.All();
            ComboBoxNewSizeCode.DisplayMember = "Name";
            ComboBoxNewSizeCode.ValueMember = "Id";

            ComboBoxNewVelocityCode.DataSource = repoVelocityCode.All();
            ComboBoxNewVelocityCode.DisplayMember = "Name";
            ComboBoxNewVelocityCode.ValueMember = "Id";

            ComboBoxNewHeightCode.DataSource = repoHeightCode.All();
            ComboBoxNewHeightCode.DisplayMember = "Name";
            ComboBoxNewHeightCode.ValueMember = "Id";

            ComboBoxNewStation.DataSource = repoStation.All();
            ComboBoxNewStation.DisplayMember = "Name";
            ComboBoxNewStation.ValueMember = "Id";
        }

        private void SetupViewEditForm()
        {
            LabelFindDescription.Text = "Search any part of Slot field";

            ComboBoxViewEditSizeCode.DataSource = repoSizeCode.All();
            ComboBoxViewEditSizeCode.DisplayMember = "Name";
            ComboBoxViewEditSizeCode.ValueMember = "Id";

            ComboBoxViewEditVelocityCode.DataSource = repoVelocityCode.All();
            ComboBoxViewEditVelocityCode.DisplayMember = "Name";
            ComboBoxViewEditVelocityCode.ValueMember = "Id";

            ComboBoxViewEditHeightCode.DataSource = repoHeightCode.All();
            ComboBoxViewEditHeightCode.DisplayMember = "Name";
            ComboBoxViewEditHeightCode.ValueMember = "Id";

            ComboBoxViewEditStation.DataSource = repoStation.All();
            ComboBoxViewEditStation.DisplayMember = "Name";
            ComboBoxViewEditStation.ValueMember = "Id";
        }
        #endregion

        #region Return Key Functions
        private void TextBoxNewLoc1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewLoc2.Focus();
            }
        }

        private void TextBoxNewLoc2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewLoc3.Focus();
            }
        }

        private void TextBoxNewLoc3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewLoc4.Focus();
            }
        }

        private void TextBoxNewLoc4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewLoc5.Focus();
            }
        }

        private void TextBoxNewLoc5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxNewSizeCode.Focus();
            }
        }

        private void ComboBoxNewSizeCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxNewVelocityCode.Focus();
            }
        }

        private void ComboBoxNewVelocityCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxNewHeightCode.Focus();
            }
        }

        private void ComboBoxNewHeightCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewLoc2.Focus();
            }
        }

        private void TextBoxViewEditLoc1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditLoc2.Focus();
            }
        }

        private void TextBoxViewEditLoc2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditLoc3.Focus();
            }
        }

        private void TextBoxViewEditLoc3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditLoc4.Focus();
            }
        }

        private void TextBoxViewEditLoc4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditLoc5.Focus();
            }
        }

        private void TextBoxViewEditLoc5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxViewEditSizeCode.Focus();
            }
        }

        private void ComboBoxViewEditSizeCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxViewEditVelocityCode.Focus();
            }
        }

        private void ComboBoxViewEditVelocityCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxViewEditHeightCode.Focus();
            }
        }

        private void ComboBoxViewEditHeightCode_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditLoc1.Focus();
            }
        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                TextBoxViewEditLoc1.Focus();
            }
            if (tabControl1.SelectedIndex == 2)
            {
                TextBoxNewLoc1.Focus();
            }
        }
        #endregion

        private void MbViewEditDelete_Click(object sender, EventArgs e)
        {
            int id = ((LocationView) bindingSource.Current).Id;
            repoLocation.Delete(id);
            RefreshData();
            tabControl1.SelectedTab = tabPage1;
        }
    }
}
