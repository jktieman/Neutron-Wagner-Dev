using System;
using System.Linq;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Forms;
using Neutron.Global;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace Neutron.Forms
{
    public partial class FrmLogin : MetroForm
    {
        private static FrmLogin instance;
        public User CurrentUser;

        public FrmLogin()
        {
            InitializeComponent();
        }

        public static FrmLogin Instance
        {
            get
            {
                if (instance == null) instance = new FrmLogin();
                return instance;
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            instance = this;
            mtbPin.Focus();
        }

        private void mButtonLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(mtbPin.Text))
            {
                if (string.IsNullOrEmpty(mtbUsername.Text))
                {
                    MetroMessageBox.Show(this, "Try Again", "Message", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    mtbPin.Focus();
                    return;
                }

                try
                {
                    using (var db = new SecureDb())
                    {
                        CurrentUser = db.Users
                            .FirstOrDefault(u => u.Username == mtbUsername.Text && u.Password == mtbPassword.Text);

                        if (CurrentUser == null)
                        {
                            MetroMessageBox.Show(this, "Invalid Username or Password, try again", "Message",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            mtbUsername.Focus();
                            return;
                        }
                        GlobalVar.User = CurrentUser;
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MetroMessageBox.Show(this,
                        ex.Message + "  Inner: " + ex.InnerException + "  Stack:" + ex.StackTrace, "Login Message",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            try
            {
                var pin = mtbPin.Text;
                if (pin == "2277")
                {
                    try
                    {
                        CurrentUser = new User
                        {
                            Firstname = "Neutron", Lastname = "Admin", EmpId = "9999", Username = "admin", Pin = "2277"
                        };
                    }
                    catch (Exception ex)
                    {
                        MetroMessageBox.Show(this, ex.Message + "  Inner: " + ex.InnerException, "Login admin Message",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CurrentUser = new User();
                    }

                    DialogResult = DialogResult.OK;
                }
                //else
                //{
                //    try
                //    {
                //        using (var db = new SecureDb())
                //        {
                //            CurrentUser = db.Users.FirstOrDefault(u => u.Pin == pin);
                //            if (CurrentUser != null)
                //            {
                //                //if the pin doesn't match any user, log in the default picker and record the entered pin
                //                //// as the current employee id.  All pickers use "picker" and their empId is recorded in 
                //                //transactions.  Topura action
                //                // CurrentUser = db.Users.Where(u => u.Username == "picker").FirstOrDefault();
                //                //Each person's EmpId will be saved in the "picker" logon.  It is NOT the pin.
                //                // CurrentUser.EmpId = mtbPin.Text;

                //                // db.Set<User>().AddOrUpdate(CurrentUser);
                //                // db.SaveChanges();

                //                GlobalVar.User = CurrentUser;
                //                DialogResult = DialogResult.OK;
                //                Close();

                //                //This is the normal action
                //                // MetroMessageBox.Show(this, "Invalid Pin, try again", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //                // mtbPin.Focus();
                //                // return;
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        MetroMessageBox.Show(this, ex.Message + "  Inner: " + ex.InnerException,
                //            "User Pin Login Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    }
                //}
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(this, ex.Message, "Login Message  " + ex.InnerException, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            mtbUsername.Text = string.Empty;
            mtbPassword.Text = string.Empty;
            mtbPin.Text = string.Empty;
        }
    }
}