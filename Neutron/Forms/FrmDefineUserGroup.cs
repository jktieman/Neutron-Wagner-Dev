using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using JsonManager;
using Neutron.Extensions;
using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace Neutron.Forms
{
    public partial class FrmDefineUserGroup : Form
    {
        private readonly IJsonData _jsonData;
        public List<UserIdString> UserIds;
        private ProductivityGroup _currentGroup = null;
        private List<ProductivityGroup> _currentGroups = new List<ProductivityGroup>();
        private bool _formInitialized = false;
        private bool _itemCheckEnabled = true;
        private string fileName = "ProductivityGroups";
        public FrmDefineUserGroup(IJsonData jsonData)
        {
            _jsonData = jsonData;
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {
            using (var db = new NeutronDb())
            {
                CheckedListBoxUsers.DataSource = db.Users.OrderBy(o => o.Lastname).ToList();
                CheckedListBoxUsers.DisplayMember = "FullName";
                CheckedListBoxUsers.ValueMember = "Id";
            }

            _currentGroups = _jsonData.LoadFile<List<ProductivityGroup>>(fileName);
            if (_currentGroups.Count > 0)
            {
                foreach (var productivityGroup in _currentGroups)
                {
                    CheckedListBoxGroups.Items.Add(productivityGroup);
                }

                // CheckedListBoxGroups.DataSource = _currentGroups;
                CheckedListBoxGroups.DisplayMember = "Name";
                CheckedListBoxGroups.ValueMember = "Name";

                CheckedListBoxGroups.SetItemCheckState(0, CheckState.Checked);
                _currentGroup = (ProductivityGroup)CheckedListBoxGroups.Items[0];
                TextBoxGroupName.Text = _currentGroup.Name;
                LabelGroupName.Text = $"Edit {_currentGroup.Name} Group";
                TextBoxGroupName.Enabled = false;
                ButtonRemove.Enabled = true;
            }

            UpdateCheckedListBoxUsers();

            _formInitialized = true;

            //------------------
            //using (var db = new NeutronDb())
            //{
            //    CheckedListBox.DataSource = db.Users.OrderBy(o => o.Lastname).ToList();
            //    CheckedListBox.DisplayMember = "FullName";
            //    CheckedListBox.ValueMember = "Id";
            //}

            //var currentIds = _jsonData.LoadFile<UserIdString>().CsvIdString;
            //if (string.IsNullOrEmpty(currentIds)) return;
            //var nums = currentIds.Split(',').Select(int.Parse).ToArray();
            //if (nums.Length <= 0) return;
            //for (var i = 0; i < CheckedListBox.Items.Count; i++)
            //{
            //    foreach (var num in nums)
            //    {
            //        if (((User)CheckedListBox.Items[i]).Id == num)
            //        {
            //            CheckedListBox.SetItemChecked(i, true);
            //        }
            //    }
            //}

            //var currentGroups = _jsonData.LoadFile<List<ProductivityGroup>>("ProductivityGroups");
            //if (currentGroups.Count == 0) return;
            //foreach (var productivityGroup in currentGroups)
            //{
            //    CheckedListBoxGroups.Items.Add(productivityGroup);
            //}

            //CheckedListBoxGroups.DataSource = currentGroups;
            //CheckedListBoxGroups.DisplayMember = "Name";
            //CheckedListBoxGroups.ValueMember = "Name";
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            var groupName = TextBoxGroupName.Text.Trim();

            if (_currentGroup == null)
            {
                //New Group
                if (groupName.Length > 0)
                {

                    _currentGroup = new ProductivityGroup { Name = groupName, UserIdString = GetUserIds() };
                    _currentGroups.Add(_currentGroup);
                }
                else
                {
                    MessageBox.Show("Invalid Group Name.");
                }
            }
            else  //Existing Group
            {
                _currentGroup.Name = groupName;
                _currentGroup.UserIdString = GetUserIds();
            }

            _jsonData.SaveFile(fileName, _currentGroups);

            UpdateCheckedListBoxGroups();
        }

        private UserIdString GetUserIds()
        {
            var userList = new List<string>();

            foreach (User item in CheckedListBoxUsers.CheckedItems)
            {
                userList.Add(item.Id.ToString());
            }

            var result = string.Join(",", userList);
            var userId = new UserIdString { CsvIdString = string.Join(",", userList) };
            return userId;
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            if (_currentGroup != null)
            {
                _currentGroups.Remove(_currentGroup);
                _currentGroup = null;
                _jsonData.SaveFile(fileName, _currentGroups);
                SelectAllUserCheckBoxes(false);
                UpdateCheckedListBoxGroups();
            }
        }

        private void CheckedListBoxGroups_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_formInitialized || !_itemCheckEnabled) return;
            if (e.NewValue != CheckState.Checked)
            {
                _currentGroup = null;
                LabelGroupName.Text = "Add New Group";
                TextBoxGroupName.Text = string.Empty;
                TextBoxGroupName.Enabled = true;
                TextBoxGroupName.Focus();
                SelectAllUserCheckBoxes(false);
                _itemCheckEnabled = false;
                UpdateCheckedListBoxGroups();
                _itemCheckEnabled = true;
                return;
            }

            var selectedIndexes = CheckedListBoxGroups.CheckedIndices;
            if (selectedIndexes.Count > 0)
            {
                _itemCheckEnabled = false;
                CheckedListBoxGroups.SetItemChecked(selectedIndexes[0], false);
                _itemCheckEnabled = true;
            }

            _currentGroup = (ProductivityGroup)CheckedListBoxGroups.SelectedItem;
            LabelGroupName.Text = $"Edit {_currentGroup.Name} Group";
            TextBoxGroupName.Text = _currentGroup.Name;
            TextBoxGroupName.Enabled = true;
            ButtonRemove.Enabled = true;

            UpdateCheckedListBoxUsers();

        }

        private void UpdateCheckedListBoxUsers()
        {
            SelectAllUserCheckBoxes(false);
            if (_currentGroup != null)
            {
                var currentUserIds = _currentGroup.UserIdString.CsvIdString;
                if (!string.IsNullOrEmpty(currentUserIds))
                {
                    var nums = currentUserIds.Split(',').Select(int.Parse).ToArray();
                    if (nums.Length > 0)
                    {
                        for (var i = 0; i < CheckedListBoxUsers.Items.Count; i++)
                        {
                            foreach (var num in nums)
                            {
                                if (((User)CheckedListBoxUsers.Items[i]).Id == num)
                                {
                                    CheckedListBoxUsers.SetItemChecked(i, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateCheckedListBoxGroups()
        {
            SelectAllGroupCheckBoxes(false);
            if (_currentGroup == null)
            {
                LabelGroupName.Text = "Add New Group";
                TextBoxGroupName.Text = string.Empty;
                TextBoxGroupName.Enabled = true;
                TextBoxGroupName.Focus();
                if (_currentGroups.Count > 0)
                {
                    CheckedListBoxGroups.Items.Clear();
                    foreach (var productivityGroup in _currentGroups)
                    {
                        CheckedListBoxGroups.Items.Add(productivityGroup);
                    }
                    CheckedListBoxGroups.ClearSelected();
                }
                //SelectAllUserCheckBoxes(false);
            }
            else
            {
                if (_currentGroups.Count > 0)
                {
                    var index = 0;
                    for (var i = 0; i < _currentGroups.Count; i++)
                    {
                        if (_currentGroups[i].Name != _currentGroup.Name) continue;
                        index = i;
                        break;
                    }
                    CheckedListBoxGroups.Items.Clear();
                    foreach (var productivityGroup in _currentGroups)
                    {
                        CheckedListBoxGroups.Items.Add(productivityGroup);
                    }
                    CheckedListBoxGroups.ClearSelected();

                    // CheckedListBoxGroups.DataSource = null;
                    // CheckedListBoxGroups.DataSource = _currentGroups;
                    // CheckedListBoxGroups.DisplayMember = "Name";
                    // CheckedListBoxGroups.ValueMember = "Name";
                    _currentGroup = _currentGroups[index];
                    _itemCheckEnabled = false;
                    CheckedListBoxGroups.SetItemCheckState(index, CheckState.Checked);
                    _itemCheckEnabled = true;
                    CheckedListBoxGroups.SelectedIndex = index;

                    TextBoxGroupName.Text = _currentGroup.Name;
                    LabelGroupName.Text = $"Edit {_currentGroup.Name} Group";
                    ButtonRemove.Enabled = true;

                    UpdateCheckedListBoxUsers();
                }
            }
        }

        private void ButtonCheckAllUsers_Click(object sender, EventArgs e)
        {
            SelectAllUserCheckBoxes(checkThem: true);
        }

        private void ButtonClearAllUsers_Click(object sender, EventArgs e)
        {
            SelectAllUserCheckBoxes(checkThem: false);
        }

        private void SelectAllUserCheckBoxes(bool checkThem)
        {
            for (var i = 0; i < (CheckedListBoxUsers.Items.Count); i++)
            {
                CheckedListBoxUsers.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
            }
            CheckedListBoxUsers.ClearSelected();
        }

        private void SelectAllGroupCheckBoxes(bool checkThem)
        {
            _itemCheckEnabled = false;
            for (var i = 0; i < (CheckedListBoxGroups.Items.Count); i++)
            {
                CheckedListBoxGroups.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
            }
            CheckedListBoxGroups.ClearSelected();
            _itemCheckEnabled = true;
        }
    }
}
