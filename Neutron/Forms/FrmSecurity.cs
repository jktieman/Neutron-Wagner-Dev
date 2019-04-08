using Neutron.Enums;
using Neutron.Global;
using Neutron.Interfaces;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Neutron
{
    public partial class FrmSecurity : Form
    {
        private readonly SecureDb context = new SecureDb();
        private readonly NeutronDb contextNeutron = new NeutronDb();

        private bool checkAllUsers;
        private bool checkAllSecureItems;
        private SecureItem[] secureItems;
        INomenclature nomenclature;

        public FrmSecurity(INomenclature nomenclature)
        {
            InitializeComponent();
            this.nomenclature = nomenclature;
            ButtonDeleteEditUser.Enabled = false;
            SetupGrids();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            secureItems = ((NeutronSecurity[]) Enum.GetValues(typeof(NeutronSecurity)))
                .Select(c => new SecureItem() { SecureItemId = (int) c, Name = c.GetEnumDescription() }).ToArray();

            // SecureItem[] secureItems = context.SecureItems.ToArray();
            var groups = context.Groups.ToArray();
            var users = context.Users.Where(u => u.Disabled == false).ToArray();
            var allUsers = context.Users.ToList();

            ListViewUsers.Items.AddRange(users.Select(r => new ListViewItem { Text = r.Fullname, Tag = r }).ToArray());
            ListViewGroups.Items.AddRange(groups.Select(r => new ListViewItem { Text = r.Name, Tag = r }).ToArray());
            ListViewSecureItems.Items.AddRange(secureItems.Select(r => new ListViewItem { Text = r.Name, Tag = r }).ToArray());

            ComboBoxGroups.DataSource = groups.ToList();
            ComboBoxGroups.DisplayMember = "Name";
            ComboBoxGroups.ValueMember = "GroupId";

            ComboBoxUsers.DataSource = allUsers;
            ComboBoxUsers.DisplayMember = "FullName";
            ComboBoxUsers.ValueMember = "Id";

        }

        private void LoadGroups()
        {
            ListViewGroups.Clear();
            var groups = context.Groups.ToArray();
            ListViewGroups.Items.AddRange(groups.Select(r => new ListViewItem { Text = r.Name, Tag = r }).ToArray());
            ComboBoxGroups.DataSource = groups.ToList();
        }

        private void LoadUsers()
        {
            ListViewUsers.Clear();
            var users = context.Users.Where(u => u.Disabled == false).ToArray();
            ListViewUsers.Items.AddRange(users.Select(r => new ListViewItem { Text = r.Fullname, Tag = r }).ToArray());
        }

        private void LoadSecureItems()
        {
            ListViewSecureItems.Clear();
            // SecureItem[] secureItems = context.SecureItems.ToArray();
            ListViewSecureItems.Items.AddRange(secureItems.Select(r => new ListViewItem { Text = r.Name, Tag = r }).ToArray());
        }

        public bool CheckAllUsers
        {
            get
            {
                return checkAllUsers;
            }
            set
            {
                checkAllUsers = value;
                ButtonSelectUsers.Text = checkAllUsers ? "Clear All" : "Check All";
            }
        }

        private void ButtonSelectUsers_Click(object sender, EventArgs e)
        {
            CheckAllUsers = !CheckAllUsers;
            if (CheckAllUsers == true)
            {
                CheckUsers();
            }
            else
            {
                ClearUsers();
            }
        }

        private void CheckUsers()
        {
            foreach (ListViewItem item in ListViewUsers.Items)
            {
                item.Checked = true;
            }
        }

        private void CheckUsers(List<User> users)
        {
            foreach (ListViewItem item in ListViewUsers.Items)
            {
                item.Checked = false;  // turn it off first 
                foreach (var usr in users)
                {
                    if (((User) item.Tag).Id == usr.Id)
                    {
                        item.Checked = true;
                        break;
                    }

                }

            }
        }

        private void ClearUsers()
        {
            foreach (ListViewItem item in ListViewUsers.Items)
            {
                item.Checked = false;
            }
        }

        //------
        private void ButtonSaveNewGroup_Click(object sender, EventArgs e)
        {
            if (TextBoxNewGroup.Text.Length > 3)
            {
                context.Groups.Add(new Group { Name = TextBoxNewGroup.Text });
                context.SaveChanges();
                LoadGroups();
                RefreshUsersAndSecureItems();
            }
        }

        private void ShowChecked_Click(object sender, EventArgs e)
        {
            //string msg = string.Empty;
            //foreach (ListViewItem item in ListViewGroups.Items)
            //{
            //    if (item.Checked)
            //    {
            //        msg += item.Tag + " " + item.Text + "\r\n";
            //    }

            //}

            //MessageBox.Show(msg);
        }

        private void ComboBoxGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshUsersAndSecureItems();
        }

        private void RefreshUsersAndSecureItems()
        {
            var group = ComboBoxGroups.SelectedItem as Group;
            var users = context.GroupUser.Where(r => r.GroupId == group.GroupId).Select(u => u.User).ToList();
            var secureItems = context.GroupSecureItem.Where(r => r.GroupId == group.GroupId).Select(u => u.SecureItem).ToList();
            //var grup = context.Users.Where(u => u.)
            //group.Users = context.Users.Where(c => c.Groups == c.Id).ToList();
            //group.SecureItems = context.SecureItems.Where(c => c.GroupId == group.Id).ToList();
            // List<User> usersInGroup = context.GroupUser.Where(g => g.GroupId == group.GroupId).Select(s => s.Users.ToList()).; 
            //usersWithThisGroup.SecureItems = context.SecureItems.Where(a => a.Groups. == group.);
            CheckUsers(users);
            CheckSecureItems(secureItems);
            UpdateInformation();
        }

        private void UpdateInformation()
        {
            var group = ComboBoxGroups.SelectedItem as Group;
            if (TabControlSecurity.SelectedTab == TabControlSecurity.TabPages["TabPageUsers"])
            {
                LabelUserGroupInformation.Text = $"Checked Users Are Members of the {group.Name} Group.";
            }
            else if (TabControlSecurity.SelectedTab == TabControlSecurity.TabPages["TabPageSecureItems"])
            {
                LabelSecureItemsInformation.Text = $"The {group.Name} Group has Access to All Checked Items.";
            }
        }

        private void ButtonSaveUsers_Click(object sender, EventArgs e)
        {
            var group = ComboBoxGroups.SelectedItem as Group;
            var users = context.Users.Where(u => u.Disabled == false).ToList();
            foreach (var user in users)
            {
                var gu = context.GroupUser.Where(g => g.GroupId == group.GroupId && g.UserId == user.Id).FirstOrDefault();
                if (gu != null)
                {
                    context.GroupUser.Remove(gu);
                }
            }
            context.SaveChanges();
            SaveSelectedUsers(group);

        }

        private void SaveSelectedUsers(Group group)
        {
            try
            {
                foreach (ListViewItem item in ListViewUsers.Items)
                {
                    if (item.Checked)
                    {
                        var user = (User) item.Tag;
                        var groupUser = new GroupUser { GroupId = group.GroupId, UserId = user.Id };
                        var gu = context.GroupUser.Find(group.GroupId, user.Id);
                        if (gu == null)
                        {
                            context.GroupUser.Add(groupUser);
                        }
                    }
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Saving User: {ex.Message}\r\n {ex.InnerException}");
            }
        }

        private void DisableSelectedUsers(Group group)
        {
            foreach (ListViewItem item in ListViewUsers.Items)
            {
                if (item.Checked)
                {
                    var user = (User) item.Tag;
                    user.Disabled = true;
                }
            }
            context.SaveChanges();
        }

        private void ButtonSaveNewUser_Click(object sender, EventArgs e)
        {
            var group = ComboBoxGroups.SelectedItem as Group;

            var user = new User
            {
                EmpId = TextBoxEmpId.Text,
                Pin = TextBoxPin.Text,
                Firstname = TextBoxFirstname.Text,
                Lastname = TextBoxLastname.Text,
                Username = TextBoxUsername.Text,
                Password = TextBoxPassword.Text,
                Disabled = CheckBoxDisabled.Checked,
                HomeLocationId = contextNeutron.Locations.FirstOrDefault().Id,
                // LocationGroupId = 1

            };
            context.Users.Add(user);
            context.GroupUser.Add(new GroupUser { GroupId = group.GroupId, UserId = user.Id });

            context.SaveChanges();

            ClearFields();
            LoadUsers();
            RefreshUsersAndSecureItems();
        }

        private void ClearFields()
        {
            TextBoxEmpId.Text = string.Empty;
            TextBoxPin.Text = string.Empty;
            TextBoxFirstname.Text = string.Empty;
            TextBoxLastname.Text = string.Empty;
            TextBoxUsername.Text = string.Empty;
            TextBoxPassword.Text = string.Empty;
            CheckBoxDisabled.Checked = false;
        }

        private void ButtonSaveSecureItems_Click(object sender, EventArgs e)
        {
            var group = ComboBoxGroups.SelectedItem as Group;

            //List<SecureItem> secureItems = this.secureItems.ToList(); //    context.SecureItems.ToList();
            foreach (var item in secureItems)
            {
                var gs = context.GroupSecureItem.Where(g => g.GroupId == group.GroupId
                    && g.SecureItemId == item.SecureItemId).FirstOrDefault();
                if (gs != null)
                {
                    context.GroupSecureItem.Remove(gs);
                }
            }
            context.SaveChanges();
            SaveSelectedSecureItems(group);
            RefreshUsersAndSecureItems();

        }

        private void SaveSelectedSecureItems(Group group)
        {
            foreach (ListViewItem item in ListViewSecureItems.Items)
            {
                if (item.Checked)
                {
                    var secureItem = item.Tag as SecureItem;
                    var groupSecureItem = new GroupSecureItem { GroupId = group.GroupId, SecureItemId = secureItem.SecureItemId };
                    var gs = context.GroupSecureItem.Find(group.GroupId, secureItem.SecureItemId);
                    if (gs == null)
                    {
                        context.GroupSecureItem.Add(groupSecureItem);
                    }
                }
            }
            context.SaveChanges();
        }

        public bool CheckAllSecureItems
        {
            get
            {
                return checkAllSecureItems;
            }
            set
            {
                checkAllSecureItems = value;
                ButtonSelectSecureItems.Text = checkAllSecureItems ? "Clear All" : "Check All";
            }
        }

        private void ButtonSelectSecureItems_Click(object sender, EventArgs e)
        {
            CheckAllSecureItems = !CheckAllSecureItems;
            if (CheckAllSecureItems == true)
            {
                CheckSecureItems();
            }
            else
            {
                ClearSecureItems();
            }
        }

        private void CheckSecureItems()
        {
            foreach (ListViewItem item in ListViewSecureItems.Items)
            {
                item.Checked = true;
            }
        }

        private void CheckSecureItems(List<SecureItem> secureItems)
        {
            foreach (ListViewItem item in ListViewSecureItems.Items)
            {
                item.Checked = false;  // turn it off first 
                foreach (var act in secureItems)
                {
                    if (((SecureItem) item.Tag).SecureItemId == act.SecureItemId)
                    {
                        item.Checked = true;
                        break;
                    }

                }
            }
        }

        private void ClearSecureItems()
        {
            foreach (ListViewItem item in ListViewSecureItems.Items)
            {
                item.Checked = false;
            }
        }

        private void ButtonDisableUsers_ClientSizeChanged(object sender, EventArgs e)
        {

        }

        private void TabControlSecurity_TabIndexChanged(object sender, EventArgs e)
        {
            UpdateInformation();
        }

        private void ButtonFindEditUser_Click(object sender, EventArgs e)
        {
            FindEditUser();


        }

        private void FindEditUser()
        {
            
            ButtonDeleteEditUser.Enabled = false;
            var recs = new List<NeutronData.ModelViews.HistoryView>();
            var empId = TextBoxEmpIdEditUser.Text.Trim();
            //var user = context.Users.FirstOrDefault(u => u.EmpId.ToLower() == empId.ToLower());
            if (!string.IsNullOrEmpty(empId))
            {
            //    TextBoxPinEditUser.Text = user.Pin;
            //    TextBoxFirstnameEditUser.Text = user.Firstname;
            //    TextBoxLastnameEditUser.Text = user.Lastname;
            //    TextBoxUsernameEditUser.Text = user.Username;
            //    TextBoxPasswordEditUser.Text = user.Password;
            //    CheckBoxDisabledEditUser.Checked = user.Disabled;
            recs = GlobalVar.HistoryManager.GetHistoryRecordsByUser(empId).Take(50)
                .OrderByDescending(h => h.ActionDateTime).ToList();    
                if (recs.Count() == 0)
                {
                    ButtonDeleteEditUser.Enabled = true;
                }

                DataGridView1.DataSource = recs;
            }
            else
            {
                MessageBox.Show($"Unable To Find User.");
                ComboBoxUsers.Focus();
                ClearGrid();
            }
        }

        private void ClearGrid()
        {
               DataGridView1.Rows.Clear();
               DataGridView1.Refresh();
        }

        private void ButtonDeleteEditUser_Click(object sender, EventArgs e)
        {
            var empId = TextBoxEmpIdEditUser.Text.Trim();
            var user = context.Users.Where(u => u.EmpId.ToLower() == empId.ToLower()).FirstOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                context.SaveChanges();
                ClearEditUserFields();
                TextBoxEmpIdEditUser.Focus();
                ButtonDeleteEditUser.Enabled = false;
            }
            ButtonDeleteEditUser.Enabled = false;
        }

        private void ButtonSaveEditUser_Click(object sender, EventArgs e)
        {
            var empId = TextBoxEmpIdEditUser.Text.Trim();
            try
            {
                var user = context.Users.Where(u => u.EmpId.ToLower() == empId.ToLower()).FirstOrDefault();
                if (user != null)
                {
                    user.Pin = TextBoxPinEditUser.Text;
                    user.Firstname = TextBoxFirstnameEditUser.Text;
                    user.Lastname = TextBoxLastnameEditUser.Text;
                    user.Username = TextBoxUsernameEditUser.Text;
                    user.Password = TextBoxPasswordEditUser.Text;
                    user.Disabled = CheckBoxDisabledEditUser.Checked;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable To Save Changes.  {ex.Message}\r\n {ex.InnerException}");
                TextBoxEmpIdEditUser.Focus();
            }
            ButtonDeleteEditUser.Enabled = false;
        }



        private void ButtonClearEditUserFields_Click(object sender, EventArgs e)
        {
            ClearEditUserFields();
        }

        private void ClearEditUserFields()
        {
            TextBoxEmpIdEditUser.Text = string.Empty;
            TextBoxPinEditUser.Text = string.Empty;
            TextBoxFirstnameEditUser.Text = string.Empty;
            TextBoxLastnameEditUser.Text = string.Empty;
            TextBoxUsernameEditUser.Text = string.Empty;
            TextBoxPasswordEditUser.Text = string.Empty;
            CheckBoxDisabledEditUser.Checked = false;
            ButtonDeleteEditUser.Enabled = false;
            ClearGrid();
        }

        private void TextBoxEmpIdEditUser_Enter(object sender, EventArgs e)
        {
            HighLightText((TextBox) sender); 
        }

        private void HighLightText(TextBox textBox)
        {
if (!String.IsNullOrEmpty(textBox.Text))
            {
                textBox.SelectionStart = 0;
                textBox.SelectionLength = textBox.Text.Length;
            }
        }

        private void SetupGrids()
        {

            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;

            var col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ActionCodeName";
            col.HeaderText = "Action";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "ActionCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ActionDateTime";
            col.HeaderText = "Date";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "ActionDateTime";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Ord1";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Ord2";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Item";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Description";
            DataGridView1.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn();
            //col.DataPropertyName = "OrderStatusName";
            //col.HeaderText = "Job Status";
            //col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //col.Name = "OrderStatusName";
            //col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            //DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "RequestedQuantity";
            col.HeaderText = "Requested";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "RequestedQuantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "IssuedQuantity";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.HeaderText = "Issued";
            col.Name = "IssuedQuantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            DataGridView1.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn();
            //col.DataPropertyName = "StationName";
            //col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            //col.HeaderText = "Station";
            //col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            //col.Name = "StationName";
            //DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc1";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.HeaderText = nomenclature.LabelDevice;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Loc1";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc2";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.HeaderText = nomenclature.LabelTray;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Loc2";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc3";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.HeaderText = nomenclature.LabelOver;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Loc3";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc4";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.HeaderText = nomenclature.LabelBack;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Loc4";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc5";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.HeaderText = "Tag";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Loc5";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Slot";
            col.HeaderText = "Slot";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Slot";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridView1.Columns.Add(col);
        }

        private void ComboBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            var user = ((ComboBox) sender).SelectedItem as User;
            TextBoxEmpIdEditUser.Text = user.EmpId;
            TextBoxFirstnameEditUser.Text = user.Firstname;
            TextBoxLastnameEditUser.Text = user.Lastname;
            TextBoxPasswordEditUser.Text = user.Password;
            TextBoxPinEditUser.Text = user.Pin;
            TextBoxUsernameEditUser.Text = user.Username;
            CheckBoxDisabledEditUser.Checked = user.Disabled;

            FindEditUser();
        }
    }
}

