using MetroFramework;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neutron.Global;
using MetroFramework.Forms;

namespace Neutron
{
    public partial class MainForm : MetroForm
    {
        GenericRepository<User> repoUser = new GenericRepository<User>(new NeutronDb());

        public MainForm()
        {
            InitializeComponent();
        }

        #region Main Panel

        private void Main_Enter(object sender, EventArgs e)
        {
            LabelUserInfo.Text = Variables.user.UserInfo;
        }

        private void MtInventory_Click(object sender, EventArgs e)
        {
            ShowTab("Inventory_Listing");
        }

        private void MtItemDefinitions_Click_1(object sender, EventArgs e)
        {
            ShowTab("ItemDefinitions");
        }

        private void MtLocations_Click(object sender, EventArgs e)
        {
            ShowTab("Locations");
        }

        private void MtHistory_Click(object sender, EventArgs e)
        {
            ShowTab("History");
        }

        private void MtLookups_Click(object sender, EventArgs e)
        {
            ShowTab("Lookups");
        }

        private void MtSearch_Click(object sender, EventArgs e)
        {
            ShowTab("Search");
        }

        private void MtUsers_Click(object sender, EventArgs e)
        {
            ShowTab("Users");
        }

        private void MtPick_Click(object sender, EventArgs e)
        {
            ShowTab("Pick");
        }

        private void MtStore_Click(object sender, EventArgs e)
        {
            ShowTab("Store");
        }

        private void MtSystem_Click(object sender, EventArgs e)
        {
            ShowTab("System");
        }

        private void Main_LogOff_Click(object sender, EventArgs e)
        {
            Variables.user = new User();
            LabelUserInfo.Text = Variables.user.UserInfo;
            ShowTab("Login");
        }

        #endregion

        #region Login Panel
        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxPin.Text))
            {

                if (string.IsNullOrEmpty(TextBoxUsername.Text))
                {
                    MessageBox.Show(this, "Try Again", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TextBoxPin.Focus();
                    return;
                }
                else
                {
                    try
                    {
                        User user = repoUser.All().Where(u => u.Username == TextBoxUsername.Text && u.Password == TextBoxPassword.Text).FirstOrDefault();
                        if (user == null)
                        {
                            MessageBox.Show("Invalid Username or Password, try again.", "Login Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            TextBoxUsername.Focus();
                            return;
                        }
                        else
                        {
                            ClearLoginFields();
                            MainTabControl.SelectedTab = MainTabControl.TabPages["Main"];
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Login Form Name " + ex.Message, "Login Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            try
            {
                int pin = int.Parse(TextBoxPin.Text);

                User user = repoUser.All().Where(u => u.Pin == pin).FirstOrDefault();
                if (user == null)
                {
                    MessageBox.Show("Invalid Pin, try again. ", "Login Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TextBoxPin.Focus();
                    return;
                }
                else
                {
                    Variables.user = user;
                    ClearLoginFields();
                    MainTabControl.SelectedTab = MainTabControl.TabPages["Main"];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login Form Pin " + ex.Message, "Login Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearLoginFields()
        {
            TextBoxUsername.Text = string.Empty;
            TextBoxPassword.Text = string.Empty;
            TextBoxPin.Text = string.Empty;
        }

        #endregion

        #region Utilities Area
        private void ShowTab(string tabName)
        {
            MainTabControl.SelectedTab = MainTabControl.TabPages[tabName];
        }


        #endregion

        private void MButtonClose_Click(object sender, EventArgs e)
        {
            ShowTab("Main");
        }

        private void Inventory_Enter(object sender, EventArgs e)
        {
            MessageBox.Show("Inventory_Enter Hello");
        }

        private void MButtonNew_Click(object sender, EventArgs e)
        {
            ShowTab("Inventory_New");
        }

        private void Inventory_New_Close_Click(object sender, EventArgs e)
        {
            ShowTab("Inventory_Listing");
        }


    }
}
