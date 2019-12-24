using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace Neutron.Forms
{
    //Location Access Control
    public partial class FrmLAC : Form
    {
        private readonly SecureDb context = new SecureDb();
        private bool checkAllUsers;
        private bool checkAllDevice1;
        private bool checkAllDevice2;
        private bool checkAllDevice3;
        private bool checkAllDevice4;
        private int currentStationNumber;

        public FrmLAC()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Location[] locations = context.Locations.ToArray();
            Carrier[] carriers = context.Carriers.ToArray();
            Role[] roles = context.Roles.ToArray();
            User[] users = context.Users.ToArray();
            Station[] stations = context.Stations.ToArray();
            currentStationNumber = carriers.Min(c => c.StationNumber);

            ListViewDevice1.Items.AddRange(carriers.Where(r => r.StationNumber == currentStationNumber && r.DeviceNumber == 1).Select(c => new ListViewItem { Text = c.ToString(), Tag = c }).ToArray());
            ListViewDevice2.Items.AddRange(carriers.Where(r => r.StationNumber == currentStationNumber && r.DeviceNumber == 2).Select(c => new ListViewItem { Text = c.ToString(), Tag = c }).ToArray());
            ListViewDevice3.Items.AddRange(carriers.Where(r => r.StationNumber == currentStationNumber && r.DeviceNumber == 3).Select(c => new ListViewItem { Text = c.ToString(), Tag = c }).ToArray());
            ListViewDevice4.Items.AddRange(carriers.Where(r => r.StationNumber == currentStationNumber && r.DeviceNumber == 4).Select(c => new ListViewItem { Text = c.ToString(), Tag = c }).ToArray());
            ListViewUsers.Items.AddRange(users.Select(r => new ListViewItem { Text = r.Fullname, Tag = r }).ToArray());
            ListViewRoles.Items.AddRange(roles.Select(r => new ListViewItem { Text = r.RoleName, Tag = r }).ToArray());
            
            ComboBoxRoles.DataSource = roles.ToList();
            ComboBoxRoles.DisplayMember = "RoleName";
            ComboBoxRoles.ValueMember = "RoleId";

            ComboBoxStation.DataSource = stations.ToList();
            ComboBoxStation.DisplayMember = "Name";
            ComboBoxStation.ValueMember = "Id";


            UpdateInformation();
        }

        private void LoadRoles()
        {
            ListViewRoles.Clear();
            Role[] roles = context.Roles.ToArray();
            ListViewRoles.Items.AddRange(roles.Select(r => new ListViewItem { Text = r.RoleName, Tag = r }).ToArray());
            ComboBoxRoles.DataSource = roles.ToList();
        }

        private void LoadUsers()
        {
            ListViewUsers.Clear();
            User[] users = context.Users.ToArray();
            ListViewUsers.Items.AddRange(users.Select(r => new ListViewItem { Text = r.Fullname, Tag = r }).ToArray());
        }

        private void LoadDevice1()
        {
            ListViewDevice1.Clear();
            Carrier[] carriers = context.Carriers.ToArray();
            ListViewDevice1.Items.AddRange(carriers.Where(r => r.StationNumber == currentStationNumber && r.DeviceNumber == 1).Select(c => new ListViewItem { Text = c.ToString(), Tag = c }).ToArray());
        }

        private void LoadDevice2()
        {
            ListViewDevice2.Clear();
            Carrier[] carriers = context.Carriers.ToArray();
            ListViewDevice2.Items.AddRange(carriers.Where(r => r.StationNumber == currentStationNumber && r.DeviceNumber == 2).Select(c => new ListViewItem { Text = c.ToString(), Tag = c }).ToArray());
        }

        private void LoadDevice3()
        {
            ListViewDevice3.Clear();
            Carrier[] carriers = context.Carriers.ToArray();
            ListViewDevice3.Items.AddRange(carriers.Where(r => r.StationNumber == currentStationNumber && r.DeviceNumber == 3).Select(c => new ListViewItem { Text = c.ToString(), Tag = c }).ToArray());
        }

        private void LoadDevice4()
        {
            ListViewDevice4.Clear();
            Carrier[] carriers = context.Carriers.ToArray();
            ListViewDevice4.Items.AddRange(carriers.Where(r => r.StationNumber == currentStationNumber && r.DeviceNumber == 4).Select(c => new ListViewItem { Text = c.ToString(), Tag = c }).ToArray());
        }

        private void ListViewDevice1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;
            var carrier = (Carrier) item.Tag;
        }

        public bool CheckAllUsers
        {
            get { return checkAllUsers; }
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
                        //break;
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

        public bool CheckAllDevice1
        {
            get { return checkAllDevice1; }
            set
            {
                checkAllDevice1 = value;
                ButtonSelectDevice1.Text = checkAllDevice1 ? "Clear All" : "Check All";
            }
        }

        private void ButtonSelectDevice1_Click(object sender, EventArgs e)
        {
            CheckAllDevice1 = !CheckAllDevice1;
            if (CheckAllDevice1 == true)
            {
                CheckDevice1();
            }
            else
            {
                ClearDevice1();
            }
        }

        private void CheckDevice1()
        {
            foreach (ListViewItem item in ListViewDevice1.Items)
            {
                item.Checked = true;
            }
        }

        private void CheckDevice1(List<Carrier> carriers)
        {
            foreach (ListViewItem item in ListViewDevice1.Items)
            {
                item.Checked = false;  // turn it off first 
                foreach (var car in carriers)
                {
                    if (((Carrier) item.Tag).CarrierId == car.CarrierId)
                    {
                        item.Checked = true;
                        //break;
                    }

                }

            }
        }

        private void ClearDevice1()
        {
            foreach (ListViewItem item in ListViewDevice1.Items)
            {
                item.Checked = false;
            }
        }

        //------
        public bool CheckAllDevice2
        {
            get { return checkAllDevice2; }
            set
            {
                checkAllDevice2 = value;
                ButtonSelectDevice2.Text = checkAllDevice2 ? "Clear All" : "Check All";
            }
        }

        private void ButtonSelectDevice2_Click(object sender, EventArgs e)
        {
            CheckAllDevice2 = !CheckAllDevice2;
            if (CheckAllDevice2 == true)
            {
                CheckDevice2();
            }
            else
            {
                ClearDevice2();
            }
        }

        private void CheckDevice2()
        {
            foreach (ListViewItem item in ListViewDevice2.Items)
            {
                item.Checked = true;
            }
        }

        private void CheckDevice2(List<Carrier> carriers)
        {
            foreach (ListViewItem item in ListViewDevice2.Items)
            {
                item.Checked = false;  // turn it off first 
                foreach (var car in carriers)
                {
                    if (((Carrier) item.Tag).CarrierId == car.CarrierId)
                    {
                        item.Checked = true;
                        break;
                    }

                }

            }
        }

        private void ClearDevice2()
        {
            foreach (ListViewItem item in ListViewDevice2.Items)
            {
                item.Checked = false;
            }
        }

        //------
        public bool CheckAllDevice3
        {
            get { return checkAllDevice3; }
            set
            {
                checkAllDevice3 = value;
                ButtonSelectDevice3.Text = checkAllDevice3 ? "Clear All" : "Check All";
            }
        }

        private void ButtonSelectDevice3_Click(object sender, EventArgs e)
        {
            CheckAllDevice3 = !CheckAllDevice3;
            if (CheckAllDevice3 == true)
            {
                CheckDevice3();
            }
            else
            {
                ClearDevice3();
            }
        }

        private void CheckDevice3()
        {
            foreach (ListViewItem item in ListViewDevice3.Items)
            {
                item.Checked = true;
            }
        }

        private void CheckDevice3(List<Carrier> carriers)
        {
            foreach (ListViewItem item in ListViewDevice3.Items)
            {
                item.Checked = false;  // turn it off first 
                foreach (var car in carriers)
                {
                    if (((Carrier) item.Tag).CarrierId == car.CarrierId)
                    {
                        item.Checked = true;
                        break;
                    }

                }
            }
        }

        private void ClearDevice3()
        {
            foreach (ListViewItem item in ListViewDevice3.Items)
            {
                item.Checked = false;
            }
        }

        //------
        public bool CheckAllDevice4
        {
            get { return checkAllDevice4; }
            set
            {
                checkAllDevice4 = value;
                ButtonSelectDevice4.Text = checkAllDevice4 ? "Clear All" : "Check All";
            }
        }

        private void ButtonSelectDevice4_Click(object sender, EventArgs e)
        {
            CheckAllDevice4 = !CheckAllDevice4;
            if (CheckAllDevice4 == true)
            {
                CheckDevice4();
            }
            else
            {
                ClearDevice4();
            }
        }

        private void CheckDevice4()
        {
            foreach (ListViewItem item in ListViewDevice4.Items)
            {
                item.Checked = true;
            }
        }

        private void CheckDevice4(List<Carrier> carriers)
        {
            foreach (ListViewItem item in ListViewDevice4.Items)
            {
                item.Checked = false;  // turn it off first 
                foreach (var car in carriers)
                {
                    if (((Carrier) item.Tag).CarrierId == car.CarrierId)
                    {
                        item.Checked = true;
                        break;
                    }

                }

            }
        }

        private void ClearDevice4()
        {
            foreach (ListViewItem item in ListViewDevice4.Items)
            {
                item.Checked = false;
            }
        }

        private void ButtonSaveNewRole_Click(object sender, EventArgs e)
        {
            if (TextBoxNewRole.Text.Length > 3)
            {
                context.Roles.Add(new Role { RoleName = TextBoxNewRole.Text });
                context.SaveChanges();
                LoadRoles();
                RefreshUsersAndCarriers();
            }
        }

        private void ShowChecked_Click(object sender, EventArgs e)
        {
            //string msg = string.Empty;
            //foreach (ListViewItem item in ListViewRoles.Items)
            //{
            //    if (item.Checked)
            //    {
            //        msg += item.Tag + " " + item.Text + "\r\n";
            //    }

            //}

            //MessageBox.Show(msg);
        }
        private void ComboBoxStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var station = ComboBoxStation.SelectedItem as Station;
            currentStationNumber = station.StationNumber;
            RefreshUsersAndCarriers();
        }

        private void ComboBoxRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshUsersAndCarriers();
        }


        private void RefreshUsersAndCarriers()
        {
            var role = ComboBoxRoles.SelectedItem as Role;
            List<User> users = context.RoleUser.Where(r => r.RoleId == role.RoleId).Select(u => u.User).ToList();
            List<Carrier> carriers = context.RoleCarrier.Where(r => r.RoleId == role.RoleId).Select(u => u.Carrier).ToList();



            //Role usersWithThisRole = context.Roles.Include("Users").Include("Carriers")
            //    .Where(r => r.RoleId == role.RoleId).FirstOrDefault();
            CheckUsers(users);
            LoadDevice1();
            CheckDevice1(carriers);
            LoadDevice2();
            CheckDevice2(carriers);
            LoadDevice3();
            CheckDevice3(carriers);
            LoadDevice4();
            CheckDevice4(carriers);
            UpdateInformation();
        }


        private void ButtonSaveUsers_Click(object sender, EventArgs e)
        {
            var role = ComboBoxRoles.SelectedItem as Role;
            List<User> users = context.Users.Where(u => u.Disabled == false).ToList();
            foreach (var user in users)
            {
                RoleUser ru = context.RoleUser.Where(g => g.RoleId == role.RoleId && g.UserId == user.Id).FirstOrDefault();
                if (ru != null)
                {
                    context.RoleUser.Remove(ru);
                }
            }
            context.SaveChanges();
            SaveSelectedUsers(role);
            RefreshUsersAndCarriers();
            UpdateInformation();
        }

        private void SaveSelectedUsers(Role role)
        {
            try
            {
                foreach (ListViewItem item in ListViewUsers.Items)
                {
                    if (item.Checked)
                    {
                        var user = (User) item.Tag;
                        var roleUser = new RoleUser { RoleId = role.RoleId, UserId = user.Id };
                        RoleUser gu = context.RoleUser.Find(role.RoleId, user.Id);
                        if (gu == null)
                        {
                            context.RoleUser.Add(roleUser);
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

        private void ButtonSaveDevice1_Click(object sender, EventArgs e)
        {
            var role = ComboBoxRoles.SelectedItem as Role;
            List<Carrier> carriers = context.Carriers.Where(c => c.DeviceNumber == 1).ToList();
            foreach (var item in carriers)
            {
                RoleCarrier rc = context.RoleCarrier.Where(g => g.RoleId == role.RoleId
                    && g.CarrierId == item.CarrierId).FirstOrDefault();
                if (rc != null)
                {
                    context.RoleCarrier.Remove(rc);
                }
            }

            context.SaveChanges();
            SaveSelectedDevice1(role);
            RefreshUsersAndCarriers();
        }

        private void SaveSelectedDevice1(Role role)
        {
            foreach (ListViewItem item in ListViewDevice1.Items)
            {
                if (item.Checked)
                {
                    var carrier = item.Tag as Carrier;
                    var roleCarrier = new RoleCarrier { RoleId = role.RoleId, CarrierId = carrier.CarrierId };
                    RoleCarrier rc = context.RoleCarrier.Where(g => g.RoleId == role.RoleId
                    && g.CarrierId == carrier.CarrierId).FirstOrDefault();
                    if (rc == null)
                    {
                        context.RoleCarrier.Add(roleCarrier);
                    }
                }
            }
            context.SaveChanges();
        }

        private void ButtonSaveDevice2_Click(object sender, EventArgs e)
        {
            var role = (Role) ComboBoxRoles.SelectedItem;
            List<Carrier> carriers = context.Carriers.Where(c => c.DeviceNumber == 2).ToList();
            foreach (var item in carriers)
            {
                RoleCarrier rc = context.RoleCarrier.Where(g => g.RoleId == role.RoleId
                    && g.CarrierId == item.CarrierId).FirstOrDefault();
                if (rc != null)
                {
                    context.RoleCarrier.Remove(rc);
                }
            }
            context.SaveChanges();
            SaveSelectedDevice2(role);
            RefreshUsersAndCarriers();
        }

        private void SaveSelectedDevice2(Role role)
        {
            foreach (ListViewItem item in ListViewDevice2.Items)
            {
                if (item.Checked)
                {
                    var carrier = item.Tag as Carrier;
                    var roleCarrier = new RoleCarrier { RoleId = role.RoleId, CarrierId = carrier.CarrierId };
                    RoleCarrier rc = context.RoleCarrier.Where(g => g.RoleId == role.RoleId
                    && g.CarrierId == carrier.CarrierId).FirstOrDefault();
                    if (rc == null)
                    {
                        context.RoleCarrier.Add(roleCarrier);
                    }
                }
            }
            context.SaveChanges();
        }

        private void ButtonSaveDevice3_Click(object sender, EventArgs e)
        {
            var role = (Role) ComboBoxRoles.SelectedItem;
            List<Carrier> carriers = context.Carriers.Where(c => c.DeviceNumber == 3).ToList();
            foreach (var item in carriers)
            {
                RoleCarrier rc = context.RoleCarrier.Where(g => g.RoleId == role.RoleId
                    && g.CarrierId == item.CarrierId).FirstOrDefault();
                if (rc != null)
                {
                    context.RoleCarrier.Remove(rc);
                }
            }
            context.SaveChanges();
            SaveSelectedDevice3(role);
            RefreshUsersAndCarriers();
        }

        private void SaveSelectedDevice3(Role role)
        {
            foreach (ListViewItem item in ListViewDevice3.Items)
            {
                if (item.Checked)
                {
                    var carrier = item.Tag as Carrier;
                    var roleCarrier = new RoleCarrier { RoleId = role.RoleId, CarrierId = carrier.CarrierId };
                    RoleCarrier rc = context.RoleCarrier.Where(g => g.RoleId == role.RoleId
                    && g.CarrierId == carrier.CarrierId).FirstOrDefault();
                    if (rc == null)
                    {
                        context.RoleCarrier.Add(roleCarrier);
                    }
                }
            }
            context.SaveChanges();
        }

        private void ButtonSaveDevice4_Click(object sender, EventArgs e)
        {
            var role = (Role) ComboBoxRoles.SelectedItem;
            List<Carrier> carriers = context.Carriers.Where(c => c.DeviceNumber == 4).ToList();
            foreach (var item in carriers)
            {
                RoleCarrier rc = context.RoleCarrier.Where(g => g.RoleId == role.RoleId
                    && g.CarrierId == item.CarrierId).FirstOrDefault();
                if (rc != null)
                {
                    context.RoleCarrier.Remove(rc);
                }
            }
            context.SaveChanges();
            SaveSelectedDevice4(role);
            RefreshUsersAndCarriers();
        }

        private void SaveSelectedDevice4(Role role)
        {
            foreach (ListViewItem item in ListViewDevice4.Items)
            {
                if (item.Checked)
                {
                    var carrier = item.Tag as Carrier;
                    var roleCarrier = new RoleCarrier { RoleId = role.RoleId, CarrierId = carrier.CarrierId };
                    RoleCarrier rc = context.RoleCarrier.Where(g => g.RoleId == role.RoleId
                    && g.CarrierId == carrier.CarrierId).FirstOrDefault();
                    if (rc == null)
                    {
                        context.RoleCarrier.Add(roleCarrier);
                    }
                }
            }
            context.SaveChanges();
        }

        private void ButtonSaveNewUser_Click(object sender, EventArgs e)
        {
            var role = ComboBoxRoles.SelectedItem as Role;
            var user = new User
            {
                EmpId = TextBoxEmpId.Text,
                Pin = TextBoxPin.Text,
                Firstname = TextBoxFirstname.Text,
                Lastname = TextBoxLastname.Text,
                Username = TextBoxUsername.Text,
                Password = TextBoxPassword.Text,
                Disabled = CheckBoxDisabled.Checked
            };
            user.Roles.Add(role);
            context.Users.Add(user);
            context.SaveChanges();

            ClearFields();
            LoadUsers();
            RefreshUsersAndCarriers();
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

        private void UpdateInformation()
        {
            var role = ComboBoxRoles.SelectedItem as Role;
            if (TabControlLAC.SelectedTab == TabControlLAC.TabPages["TabPageUsers"])
            {
                LabelUserRoleInformation.Text = $"Checked Users Are Members of the {role.RoleName} Group.";
            }
            else if (TabControlLAC.SelectedTab == TabControlLAC.TabPages["TabPageDevice1"])
            {
                LabelDevice1Information.Text = $"The {role.RoleName} Group has Access to All Checked Locations.";
            }
            else if (TabControlLAC.SelectedTab == TabControlLAC.TabPages["TabPageDevice2"])
            {
                LabelDevice2Information.Text = $"The {role.RoleName} Group has Access to All Checked Locations.";
            }
            else if (TabControlLAC.SelectedTab == TabControlLAC.TabPages["TabPageDevice3"])
            {
                LabelDevice3Information.Text = $"The {role.RoleName} Group has Access to All Checked Locations.";
            }
            else if (TabControlLAC.SelectedTab == TabControlLAC.TabPages["TabPageDevice4"])
            {
                LabelDevice4Information.Text = $"The {role.RoleName} Group has Access to All Checked Locations.";
            }
        }

        private void TabControlLAC_TabIndexChanged(object sender, EventArgs e)
        {
            UpdateInformation();
        }

        private void TabControlLAC_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInformation();
        }

        
    }
}
