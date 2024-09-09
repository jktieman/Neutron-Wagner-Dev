using Neutron.Enums;
using Neutron.Global;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NeutronCore.Global;
using NeutronCore;
namespace Neutron.Forms
{
    public partial class FrmSecurity : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _gridResourceManager;
        private readonly SecureDb _context = new SecureDb();
        private readonly NeutronDb _contextNeutron = new NeutronDb();
        private bool _checkAllUsers;
        private bool _checkAllSecureItems;
        private SecureItem[] _secureItems;
        private readonly NeutronVariables _neutronVariables;
        private readonly IHistoryManager _historyManager;
        private User _currentUser;
        private List<User> _allUsers = new List<User>();
        public bool CloseButtonPressed { get; set; }

        public FrmSecurity(NeutronVariables neutronVariables, IHistoryManager historyManager)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _neutronVariables = neutronVariables;
            _historyManager = historyManager;
            ButtonDeleteEditUser.Enabled = false;
            CloseButtonPressed = false;
            SetupGrids();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            _secureItems = ((NeutronSecurity[])Enum.GetValues(typeof(NeutronSecurity)))
                .Select(c => new SecureItem() { SecureItemId = (int)c, Name = c.GetEnumDescription() }).ToArray();
            // SecureItem[] secureItems = context.SecureItems.ToArray();
            var groups = _context.Groups.ToArray();
            var users = _context.Users.Where(u => u.Disabled == false).OrderBy(o => o.Lastname).ThenBy(p => p.Firstname).ToArray();
            _allUsers = _context.Users.OrderBy(o => o.Lastname).ThenBy(p => p.Firstname).ToList();
            ListViewUsers.Items.AddRange(users.Select(r => new ListViewItem { Text = r.Fullname, Tag = r }).ToArray());
            ListViewGroups.Items.AddRange(groups.Select(r => new ListViewItem { Text = r.Name, Tag = r }).ToArray());
            ListViewSecureItems.Items.AddRange(_secureItems.Select(r => new ListViewItem { Text = r.Name, Tag = r }).ToArray());
            ComboBoxGroups.DataSource = groups.ToList();
            ComboBoxGroups.DisplayMember = "Name";
            ComboBoxGroups.ValueMember = "GroupId";
            ComboBoxUsers.DataSource = _allUsers;
            ComboBoxUsers.DisplayMember = "FullName";
            ComboBoxUsers.ValueMember = "Id";
            ComboBoxPreferredLanguage.DataSource = _context.Languages.ToList();
            ComboBoxPreferredLanguage.DisplayMember = "Name";
            ComboBoxPreferredLanguage.ValueMember = "Id";
            ComboBoxEditPreferredLanguage.DataSource = _context.Languages.ToList();
            ComboBoxEditPreferredLanguage.DisplayMember = "Name";
            ComboBoxEditPreferredLanguage.ValueMember = "Id";
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                //parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        private void FrmSecurity_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }

        private void LoadGroups()
        {
            ListViewGroups.Clear();
            var groups = _context.Groups.ToArray();
            ListViewGroups.Items.AddRange(groups.Select(r => new ListViewItem { Text = r.Name, Tag = r }).ToArray());
            ComboBoxGroups.DataSource = groups.ToList();
        }
        private void LoadUsers()
        {
            ListViewUsers.Clear();
            var users = _context.Users.Where(u => u.Disabled == false).OrderBy(o => o.Lastname).ThenBy(p => p.Firstname).ToArray();
            ListViewUsers.Items.AddRange(users.Select(r => new ListViewItem { Text = r.Fullname, Tag = r }).ToArray());
        }
        private void LoadSecureItems()
        {
            ListViewSecureItems.Clear();
            // SecureItem[] secureItems = context.SecureItems.ToArray();
            ListViewSecureItems.Items.AddRange(_secureItems.Select(r => new ListViewItem { Text = r.Name, Tag = r }).ToArray());
        }
        public bool CheckAllUsers
        {
            get { return _checkAllUsers; }
            set
            {
                _checkAllUsers = value;
                ButtonSelectUsers.Text = _checkAllUsers ? "Clear All" : "Check All";
            }
        }
        private void ButtonSelectUsers_Click(object sender, EventArgs e)
        {
            CheckAllUsers = !CheckAllUsers;
            if (CheckAllUsers)
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
                    if (((User)item.Tag).Id == usr.Id)
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
        private void ButtonSaveNewGroup_Click(object sender, EventArgs e)
        {
            if (TextBoxNewGroup.Text.Length > 3)
            {
                _context.Groups.Add(new Group { Name = TextBoxNewGroup.Text });
                _context.SaveChanges();
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
            //        msg += item.Tag + " " + item.Text + "{Environment.NewLine}";
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
            _allUsers = _context.Users.OrderBy(o => o.Lastname).ThenBy(p => p.Firstname).ToList();
            var group = (Group)ComboBoxGroups.SelectedItem;
            var users = _context.GroupUser.Where(r => r.GroupId == group.GroupId).Select(u => u.User).ToList();
            var secureItems = _context.GroupSecureItem.Where(r => r.GroupId == group.GroupId).Select(u => u.SecureItem).ToList();
            //var grup = context.Users.Where(u => u.)
            //group.Users = context.Users.Where(c => c.Groups == c.Id).ToList();
            //group.SecureItems = context.SecureItems.Where(c => c.GroupId == group.Id).ToList();
            // List<User> usersInGroup = context.GroupUser.Where(g => g.GroupId == group.GroupId).Select(s => s.Users.ToList()).; 
            //usersWithThisGroup.SecureItems = context.SecureItems.Where(a => a.Groups. == group.);
            CheckUsers(users);
            CheckSecureItems(secureItems);
            ComboBoxUsers.DataSource = _allUsers;
            UpdateInformation();
        }
        private void UpdateInformation()
        {
            var group = (Group)ComboBoxGroups.SelectedItem;
            if (TabControlSecurity.SelectedTab == TabControlSecurity.TabPages["TabPageUsers"])
            {
                LabelUserGroupInformation.Text = $"Checked Users Are Members of the {group?.Name} Group.";
            }
            else if (TabControlSecurity.SelectedTab == TabControlSecurity.TabPages["TabPageSecureItems"])
            {
                LabelSecureItemsInformation.Text = $"The {group?.Name} Group has Access to All Checked Items.";
            }
        }
        private void ButtonSaveUsers_Click(object sender, EventArgs e)
        {
            var group = (Group)ComboBoxGroups.SelectedItem;
            var users = _context.Users.Where(u => u.Disabled == false).OrderBy(o => o.Lastname).ThenBy(p => p.Firstname).ToList();
            foreach (var user in users)
            {
                var gu = _context.GroupUser.FirstOrDefault(g => g.GroupId == group.GroupId && g.UserId == user.Id);
                if (gu != null)
                {
                    _context.GroupUser.Remove(gu);
                }
            }
            _context.SaveChanges();
            SaveSelectedUsers(group);
            RefreshUsersAndSecureItems();
        }
        private void SaveSelectedUsers(Group group)
        {
            try
            {
                foreach (ListViewItem item in ListViewUsers.Items)
                {
                    if (item.Checked)
                    {
                        var user = (User)item.Tag;
                        var groupUser = new GroupUser { GroupId = group.GroupId, UserId = user.Id };
                        var gu = _context.GroupUser.Find(group.GroupId, user.Id);
                        if (gu == null)
                        {
                            _context.GroupUser.Add(groupUser);
                        }
                    }
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Saving User: {ex.Message}{Environment.NewLine} {ex.InnerException}");
            }
        }
        private void DisableSelectedUsers(Group group)
        {
            foreach (ListViewItem item in ListViewUsers.Items)
            {
                if (item.Checked)
                {
                    var user = (User)item.Tag;
                    user.Disabled = true;
                }
            }
            _context.SaveChanges();
        }
        private void ButtonSaveNewUser_Click(object sender, EventArgs e)
        {
            var group = (Group)ComboBoxGroups.SelectedItem;
            if (group != null)
            {
                if (VerifyFields())
                {
                    var user = new User
                    {
                        EmpId = TextBoxEmpId.Text.Trim(),
                        Pin = TextBoxPin.Text.Trim(),
                        Firstname = TextBoxFirstname.Text.Trim(),
                        Lastname = TextBoxLastname.Text.Trim(),
                        Username = TextBoxUsername.Text.Trim(),
                        Password = TextBoxPassword.Text.Trim(),
                        Disabled = CheckBoxDisabled.Checked,
                        LanguageId = ((Language)ComboBoxPreferredLanguage.SelectedItem).Id
                    };
                    _context.Users.Add(user);
                    _context.GroupUser.Add(new GroupUser { GroupId = group.GroupId, UserId = user.Id });
                    _context.SaveChanges();
                    ClearFields();
                    LoadUsers();
                    RefreshUsersAndSecureItems();
                }
            }
        }
        private bool VerifyFields()
        {
            var result = true;
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(TextBoxFirstname.Text.Trim()))
            {
                sb.AppendLine($"You must provide a first name.");
                result = false;
            }
            if (string.IsNullOrEmpty(TextBoxLastname.Text.Trim()))
            {
                sb.AppendLine($"You must provide a last name.");
                result = false;
            }
            if (string.IsNullOrEmpty(TextBoxEmpId.Text.Trim()))
            {
                sb.AppendLine($"You must provide an employee Id.");
                result = false;
            }
            if (string.IsNullOrEmpty(TextBoxPin.Text.Trim()))
            {
                sb.AppendLine($"You must provide a PIN number.");
                result = false;
            }
            if (!_neutronVariables.PinLoginOnly)
            {
                if (string.IsNullOrEmpty(TextBoxUsername.Text.Trim()))
                {
                    sb.AppendLine("Login requires a user name and password.");
                    sb.AppendLine($"You must provide a user name.");
                    result = false;
                }
                if (string.IsNullOrEmpty(TextBoxPassword.Text.Trim()))
                {
                    sb.AppendLine("Login requires a user name and password.");
                    sb.AppendLine($"You must provide a password.");
                    result = false;
                }
            }
            if (!result)
            {
                MessageBox.Show(sb.ToString());
            }
            return result;
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
            ComboBoxPreferredLanguage.SelectedIndex = 0;
        }
        private void ButtonSaveSecureItems_Click(object sender, EventArgs e)
        {
            var group = (Group)ComboBoxGroups.SelectedItem;
            foreach (var item in _secureItems)
            {
                var gs = _context.GroupSecureItem.FirstOrDefault(g => g.GroupId == group.GroupId
                                                                      && g.SecureItemId == item.SecureItemId);
                if (gs != null)
                {
                    _context.GroupSecureItem.Remove(gs);
                }
            }
            _context.SaveChanges();
            SaveSelectedSecureItems(group);
            RefreshUsersAndSecureItems();
        }
        private void SaveSelectedSecureItems(Group group)
        {
            foreach (ListViewItem item in ListViewSecureItems.Items)
            {
                if (item.Checked)
                {
                    var secureItem = (SecureItem)item.Tag;
                    if (secureItem != null)
                    {
                        var groupSecureItem = new GroupSecureItem { GroupId = group.GroupId, SecureItemId = secureItem.SecureItemId };
                        var gs = _context.GroupSecureItem.Find(group.GroupId, secureItem.SecureItemId);
                        if (gs == null)
                        {
                            _context.GroupSecureItem.Add(groupSecureItem);
                        }
                    }
                }
            }
            _context.SaveChanges();
        }
        public bool CheckAllSecureItems
        {
            get { return _checkAllSecureItems; }
            set
            {
                _checkAllSecureItems = value;
                ButtonSelectSecureItems.Text = _checkAllSecureItems ? "Clear All" : "Check All";
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
                    if (((SecureItem)item.Tag).SecureItemId == act.SecureItemId)
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
            var empId = TextBoxEmpIdEditUser.Text.Trim();
            try
            {
                var recs = _historyManager.GetHistoryRecordsByUser(empId).Take(50)
                                 .OrderByDescending(h => h.ActionDateTime).ToList();
                if (!recs.Any())
                {
                    ButtonDeleteEditUser.Enabled = true;
                }
                DataGridView1.DataSource = recs;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable To Find User.  {Environment.NewLine} {e.Message}");
            }
            ComboBoxUsers.Focus();
        }
        private void ClearGrid()
        {
            if (DataGridView1.RowCount > 0)
            {
                DataGridView1.Rows.Clear();
                DataGridView1.Refresh();
            }
        }
        private void ButtonDeleteEditUser_Click(object sender, EventArgs e)
        {
            var empId = TextBoxEmpIdEditUser.Text.Trim();
            var historyCount = 0;
            if (!string.IsNullOrEmpty(empId))
            {
                var user = _context.Users.FirstOrDefault(u => u.EmpId == empId);
                if (user != null)
                {
                    //Check History to see if they've done anything
                    //If they have just Disable else Remove
                    using (var db = new NeutronDb())
                    {
                        historyCount = db.History.Count(r => r.EmpId == empId);
                    }

                    if (historyCount == 0)
                    {
                        var recs = _context.GroupUser.Where(r => r.UserId == user.Id).ToList();
                        if (recs.Any())
                        {
                            foreach (var rec in recs)
                            {
                                _context.GroupUser.Remove(rec);
                            }
                        }

                        _context.Users.Remove(user);
                        _context.SaveChanges();
                    }
                    else  // Disable
                    {
                        user.Disabled = true;
                        _context.Users.AddOrUpdate(user);
                        _context.SaveChanges();
                    }

                    ClearEditUserFields();
                    TextBoxEmpIdEditUser.Focus();
                    ButtonDeleteEditUser.Enabled = false;
                }
            }
            ButtonDeleteEditUser.Enabled = false;
            RefreshUsersAndSecureItems();
        }
        private void ButtonSaveEditUser_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerifyEditFields())
                {
                    var user = _context.Users.Find(_currentUser.Id);
                    if (user != null)
                    {
                        user.EmpId = TextBoxEmpIdEditUser.Text.Trim();
                        user.Pin = TextBoxPinEditUser.Text.Trim();
                        user.Firstname = TextBoxFirstnameEditUser.Text.Trim();
                        user.Lastname = TextBoxLastnameEditUser.Text.Trim();
                        user.Username = TextBoxUsernameEditUser.Text.Trim();
                        user.Password = TextBoxPasswordEditUser.Text.Trim();
                        user.Disabled = CheckBoxDisabledEditUser.Checked;
                        user.LanguageId = ((Language)ComboBoxEditPreferredLanguage.SelectedItem).Id;
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable To Save Changes.  {ex.Message}{Environment.NewLine}{ex.InnerException}");
                TextBoxEmpIdEditUser.Focus();
            }
            ButtonDeleteEditUser.Enabled = false;
            RefreshUsersAndSecureItems();
        }

        private bool VerifyEditFields()
        {
            var result = true;
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(TextBoxFirstnameEditUser.Text.Trim()))
            {
                sb.AppendLine($"You must provide a first name.");
                result = false;
            }
            if (string.IsNullOrEmpty(TextBoxLastnameEditUser.Text.Trim()))
            {
                sb.AppendLine($"You must provide a last name.");
                result = false;
            }
            if (string.IsNullOrEmpty(TextBoxEmpIdEditUser.Text.Trim()))
            {
                sb.AppendLine($"You must provide an employee Id.");
                result = false;
            }
            if (string.IsNullOrEmpty(TextBoxPinEditUser.Text.Trim()))
            {
                sb.AppendLine($"You must provide a PIN number.");
                result = false;
            }
            if (!_neutronVariables.PinLoginOnly)
            {
                if (string.IsNullOrEmpty(TextBoxUsernameEditUser.Text.Trim()))
                {
                    sb.AppendLine("Login requires a user name and password.");
                    sb.AppendLine($"You must provide a user name.");
                    result = false;
                }
                if (string.IsNullOrEmpty(TextBoxPasswordEditUser.Text.Trim()))
                {
                    sb.AppendLine("Login requires a user name and password.");
                    sb.AppendLine($"You must provide a password.");
                    result = false;
                }
            }
            if (!result)
            {
                MessageBox.Show(sb.ToString());
            }
            return result;
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
            ComboBoxEditPreferredLanguage.SelectedIndex = 0;
            ClearGrid();
        }
        private void TextBoxEmpIdEditUser_Enter(object sender, EventArgs e)
        {
            HighLightText((TextBox)sender);
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
            DataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;
            var col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ActionCodeName";
            col.HeaderText = _gridResourceManager.GetString("ActionCodeName");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "ActionCodeName";
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ActionDateTime";
            col.HeaderText = _gridResourceManager.GetString("ActionDateTime");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "ActionDateTime";
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = _gridResourceManager.GetString("Ord1");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Ord1";
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = _gridResourceManager.GetString("Ord2");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Ord2";
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = _gridResourceManager.GetString("Item");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Item";
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = _gridResourceManager.GetString("Description");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.Name = "Description";
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "RequestedQuantity";
            col.HeaderText = _gridResourceManager.GetString("RequestedQuantity");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "RequestedQuantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "IssuedQuantity";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.HeaderText = _gridResourceManager.GetString("IssuedQuantity");
            col.Name = "IssuedQuantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            DataGridView1.Columns.Add(col);
        }

        private void ComboBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            var user = ((ComboBox)sender).SelectedItem as User;
            if (user != null)
            {
                _currentUser = user;
                TextBoxEmpIdEditUser.Text = string.IsNullOrEmpty(user?.EmpId) ? "" : user.EmpId;
                TextBoxFirstnameEditUser.Text = user.Firstname;
                TextBoxLastnameEditUser.Text = user.Lastname;
                TextBoxPasswordEditUser.Text = user.Password;
                TextBoxPinEditUser.Text = user.Pin;
                TextBoxUsernameEditUser.Text = user.Username;
                CheckBoxDisabledEditUser.Checked = user.Disabled;
                ComboBoxEditPreferredLanguage.SelectedValue = user.LanguageId;
            }
            FindEditUser();
        }
        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmSecurity",
               resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);
                TabPageUsers.Text = _resourceManager.GetString("Users");
                ButtonSaveUsers.Text = _resourceManager.GetString("Save");
                ButtonSelectUsers.Text = _resourceManager.GetString("CheckAll");
                TabPageGroups.Text = _resourceManager.GetString("SecurityGroups");
                ButtonSaveNewGroup.Text = _resourceManager.GetString("Save");
                LabelNewGroupName.Text = _resourceManager.GetString("NewSecurityGroup");
                TabPageSecureItems.Text = _resourceManager.GetString("SecureItems");
                ButtonSaveSecureItems.Text = _resourceManager.GetString("Save");
                ButtonSelectSecureItems.Text = _resourceManager.GetString("CheckAll");
                TabPageNewUser.Text = _resourceManager.GetString("NewUser");
                CheckBoxDisabled.Text = _resourceManager.GetString("Disabled");
                ButtonSaveNewUser.Text = _resourceManager.GetString("Save");
                LabelPassword.Text = _resourceManager.GetString("Password");
                LabelLastname.Text = _resourceManager.GetString("LastName");
                LabelUsername.Text = _resourceManager.GetString("UserName");
                LabelFirstname.Text = _resourceManager.GetString("FirstName");
                LabelPin.Text = _resourceManager.GetString("Pin");
                LabelEmpId.Text = _resourceManager.GetString("EmployeeId");
                TabPageEditUser.Text = _resourceManager.GetString("EditUser");
                CheckBoxDisabledEditUser.Text = _resourceManager.GetString("Disabled");
                ButtonDeleteEditUser.Text = _resourceManager.GetString("Delete");
                ButtonClearEditUser.Text = _resourceManager.GetString("Clear");
                ButtonSaveEditUser.Text = _resourceManager.GetString("Save");
                LabelEditPassword.Text = _resourceManager.GetString("Password");
                LabelEditLastname.Text = _resourceManager.GetString("LastName");
                LabelEditUsername.Text = _resourceManager.GetString("UserName");
                LabelEditLast50Records.Text = _resourceManager.GetString("Last50Records");
                LabelEditFirstname.Text = _resourceManager.GetString("FirstName");
                LabelEditPin.Text = _resourceManager.GetString("Pin");
                LabelEditEmpId.Text = _resourceManager.GetString("EmployeeId");
                LabelSelectGroup.Text = _resourceManager.GetString("SelectSecurityGroup");
                LabelPreferredLanguage.Text = _resourceManager.GetString("PreferredLanguage");
                LabelEditPreferredLanguage.Text = _resourceManager.GetString("PreferredLanguage");
                Text = _resourceManager.GetString("SecurityControl");
                _resourceManager.GetString("Message0");
                _resourceManager.GetString("Message1");
                _resourceManager.GetString("Message2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

        private void TabPageEditUser_Enter(object sender, EventArgs e)
        {
            _allUsers = _context.Users.OrderBy(o => o.Lastname).ThenBy(p => p.Firstname).ToList();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
            Close();
        }
    }
}
