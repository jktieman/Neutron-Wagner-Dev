using Equin.ApplicationFramework;
using MetroFramework.Forms;
using NeutronCore.Extensions;
using Neutron.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public partial class FrmSizeCodes : MetroForm
    {
        private BindingSource bindingSource = new BindingSource();
        private GenericRepository<SizeCode> repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());

        public FrmSizeCodes()
        {
            InitializeComponent();
            SetupGrid();
            SetupTabControl();
            SetupNewForm();
            SetupViewEditForm();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
        }

        private void FrmSizeCodes_Load(object sender, EventArgs e)
        {
            int id = RefreshData();
            SetupViewEditBindings();
            this.AutoValidate = AutoValidate.Disable;
        }

        private int RefreshData(int recId = 0)
        {
            int idx = 0;
            List<SizeCode> recs = repoSizeCode.All().OrderBy(o => o.Sequence).ToList();
            bindingSource.DataSource = recs;
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
                int rec = ((SizeCode) bindingSource[i]).Id;
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
            TextBoxViewEditName.DataBindings.Add("Text", bindingSource, "Name");
            TextBoxViewEditSequence.DataBindings.Add("Text", bindingSource, "Sequence");
        }

        private int GetRecordCount()
        {
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
                    bindingSource.DataSource = repoSizeCode.All().OrderBy(o => o.Sequence).ToList();
                    GetRecordCount();
                }
                else
                {
                    bindingSource.DataSource = repoSizeCode.All().OrderBy(o => o.Sequence).Where(d => d.Name.Contains(s)).ToList();
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
                    var rec = new SizeCode
                    {
                        Name = TextBoxNewName.Text,
                        Sequence = (TextBoxNewSequence.Text).ParseInt()
                    };
                    if (IsDuplicate(rec))
                    {
                        MessageBox.Show("Duplicate Entry.");
                        return;
                    }
                    repoSizeCode.Insert(rec);
                    RefreshData(rec.Id);
                    tabControl1.SelectedTab = tabPage1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save Error. " + ex.Message + ex.InnerException + ex.InnerException.InnerException);
                }
            }
            else
            {
                MessageBox.Show("Field Validation Error");
            }
        }

        private void UpdateViewEdit()
        {
            int id = ((SizeCode) bindingSource.Current).Id;
            var rec = new SizeCode
            {
                Id = id,
                Name = TextBoxViewEditName.Text,
                Sequence = (TextBoxViewEditSequence.Text).ParseInt()
            };
            if (!ValidateFields(rec))
            {
                MessageBox.Show("Invalid Entry.");
                return;
            }
            repoSizeCode.Update(rec);
            RefreshData(rec.Id);
            tabControl1.SelectedTab = tabPage1;
        }

        private bool ValidateFields(SizeCode rec)
        {
            if (!StringValidator(rec.Name))
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


        private bool IsDuplicate(SizeCode recIn)
        {
            SizeCode rec = repoSizeCode.FindBy(f => f.Name == recIn.Name).FirstOrDefault();
            if (rec != null)
            {
                MessageBox.Show("Record already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            return false;
        }

        #region Form Setup
        private void SetupGrid()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            int w = (DataGridView1.Width - 60) / 4;

            DataGridViewColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Name";
            col.HeaderText = "Name";
            col.Width = w;
            col.Name = "Name";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Sequence";
            col.HeaderText = "Sequence";
            col.Width = w;
            col.Name = "Sequence";
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
            LabelFindDescription.Text = "Search any part of Name field";

            //ComboBoxNewSizeCode.DataSource = repoSizeCode.All();
            //ComboBoxNewSizeCode.DisplayMember = "Name";
            //ComboBoxNewSizeCode.ValueMember = "Id";

            //ComboBoxNewVelocityCode.DataSource = repoVelocityCode.All();
            //ComboBoxNewVelocityCode.DisplayMember = "Name";
            //ComboBoxNewVelocityCode.ValueMember = "Id";

            //ComboBoxNewHeightCode.DataSource = repoHeightCode.All();
            //ComboBoxNewHeightCode.DisplayMember = "Name";
            //ComboBoxNewHeightCode.ValueMember = "Id";

            //ComboBoxNewStation.DataSource = repoStation.All();
            //ComboBoxNewStation.DisplayMember = "Name";
            //ComboBoxNewStation.ValueMember = "Id";
        }

        private void SetupViewEditForm()
        {
            //ComboBoxViewEditSizeCode.DataSource = repoSizeCode.All();
            //ComboBoxViewEditSizeCode.DisplayMember = "Name";
            //ComboBoxViewEditSizeCode.ValueMember = "Id";

            //ComboBoxViewEditVelocityCode.DataSource = repoVelocityCode.All();
            //ComboBoxViewEditVelocityCode.DisplayMember = "Name";
            //ComboBoxViewEditVelocityCode.ValueMember = "Id";

            //ComboBoxViewEditHeightCode.DataSource = repoHeightCode.All();
            //ComboBoxViewEditHeightCode.DisplayMember = "Name";
            //ComboBoxViewEditHeightCode.ValueMember = "Id";

            //ComboBoxViewEditStation.DataSource = repoStation.All();
            //ComboBoxViewEditStation.DisplayMember = "Name";
            //ComboBoxViewEditStation.ValueMember = "Id";
        }
        #endregion

        #region Return Key Functions
        private void TextBoxNewName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                MbViewEditSave.Focus();
            }
        }

        private void TextBoxViewEditName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                MbViewEditSave.Focus();
            }
        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                TextBoxViewEditName.Focus();
            }
            if (tabControl1.SelectedIndex == 2)
            {
                TextBoxNewName.Focus();
            }
        }
        #endregion
        #region Validation
        //public bool ValidName(string name, out string errorMessage)
        //{
        //    if (name.Length == 0)
        //    {
        //        errorMessage = "Name is required.";
        //        return false;
        //    }
        //    errorMessage = string.Empty;
        //    return true;

        //}

        private void TextBoxNewName_Validating(object sender, CancelEventArgs e)
        {
            if (TextBoxNewName.Text.Length == 0)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }

        //private void TextBoxNewName_Validated(object sender, EventArgs e)
        //{
        //    errorProvider.SetError(TextBoxNewName, "");
        //}

        #endregion

        private void MbViewEditDelete_Click(object sender, EventArgs e)
        {
            int id = ((SizeCode) bindingSource.Current).Id;
            repoSizeCode.Delete(id);
            RefreshData();
            tabControl1.SelectedTab = tabPage1;
        }
    }
}
