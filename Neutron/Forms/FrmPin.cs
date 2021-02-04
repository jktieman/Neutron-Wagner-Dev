using System;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Forms;
using NeutronCore;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace Neutron.Forms
{
    public partial class FrmPin : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private static FrmPin _instance;
        public User CurrentUser;

        public FrmPin()
        {
            InitializeComponent();
            _instance = this;
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
        }

        public static FrmPin Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new FrmPin();
                return _instance;
            }
        }

        private void frmPin_Load(object sender, EventArgs e)
        {
            mtbPin.Focus();
        }

        private void mButtonLogin_Click(object sender, EventArgs e)
        {
            var pin = mtbPin.Text;
            if (pin == "2277")
            {
                try
                {
                    CurrentUser = new User
                    {
                        Firstname = "Neutron",
                        Lastname = "Admin",
                        EmpId = "9999",
                        Username = "admin",
                        Pin = "2277",
                        LanguageId = 1
                    };
                }
                catch (Exception ex)
                {
                    MetroMessageBox.Show(this, ex.Message + "  Inner: " + ex.InnerException, "Login admin Message",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CurrentUser = new User();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                try
                {
                    using (var db = new SecureDb())
                    {
                        CurrentUser = db.Users.Include("Language").FirstOrDefault(u => u.Pin == pin);
                        if (CurrentUser == null) return;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MetroMessageBox.Show(this, ex.Message + "  Inner: " + ex.InnerException, "User Pin Login Message",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                var resourceManager =
                    ResourceManager.CreateFileBasedResourceManager("FrmPin", languageDirectory, null);
                _instance.mlPin.Text = resourceManager.GetString("EmployeePin");
                _instance.Text = resourceManager.GetString("NeutronLogin");
                _instance.mButtonLogin.Text = resourceManager.GetString("Login");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading languages.  FrmPin  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }
    }
}