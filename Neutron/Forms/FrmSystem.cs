using JsonManager;
using MetroFramework.Forms;
using Neutron.Global;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronLoader;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using AlliedLogger;
using AlliedPostOffice;
using AlliedPostOffice.Concrete;
using Neutron.Models;
using NeutronData.Models;
using NeutronEvents;
using SqlSchemaManager;

namespace Neutron.Forms
{
    public partial class FrmSystem : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private bool CloseButtonPressed { get; set; }
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private string _rootDirectory;
        private readonly IDynamicLogger _logger;
        private SendEmail _sendEmail;
        private readonly IStoredProcedureManager _storedProcedureManager;
        private readonly bool _standAlone;
        private readonly bool _emailEnabled;

        public FrmSystem(IJsonData jsonData, IDynamicLogger logger, IStoredProcedureManager storedProcedureManager, bool standAlone = false)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            // SetCulture(_cultureInfo.Name);
            _jsonData = jsonData;
            _neutronVariables = jsonData.LoadFile<NeutronVariables>();
            _neutronLicense = jsonData.LoadFile<NeutronLicense>();
            _logger = logger;
            SetupEmail();
            _storedProcedureManager = storedProcedureManager;
            _standAlone = standAlone;
            _emailEnabled = _neutronVariables.EnableEmailNotification;
            KeyPreview = true;
            HideTabControlTabs();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            CloseButtonPressed = false;
            SetLoaderButtonText();
            SetUploadButtonText();
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
            Mediator.GetInstance().StartStopUpload += (s, e) => StartStopUploadAction(e.StartStop);
        }

        private void SetupEmail()
        {
            _sendEmail = null;
            if (_neutronVariables.EnableEmailNotification)
            {
                try
                {
                    var emailServerSettings = _jsonData.LoadFile<EmailSettings>();
                    var emailListing = _jsonData.LoadFile<List<EmailAddressData>>();
                    var emailProcessor = new EmailProcessor(emailServerSettings);

                    _sendEmail = new SendEmail(emailProcessor, emailListing);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to setup Email Notification. {Environment.NewLine}{ex.Message}");
                }
            }
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

        private void SetLoaderButtonText()
        {
            if (GlobalVar.LoaderRunning)
            {
                MBStartLoader.Text = "Stop Loader";
            }
            else
            {
                MBStartLoader.Text = "Start Loader";
            }
        }

        private void SetUploadButtonText()
        {
            if (GlobalVar.UploadRunning)
            {
                MBStartUpload.Text = "Stop Upload";
            }
            else
            {
                MBStartUpload.Text = "Start Upload";
            }
        }

        private void StartStopLoaderAction(string startStop)
        {
            if (startStop == "Start")
            {
                MBStartLoader.Text = "Stop Loader";
                GlobalVar.LoaderRunning = true;
            }
            else
            {
                MBStartLoader.Text = "Start Loader";
                GlobalVar.LoaderRunning = false;
            }
        }

        private void StartStopUploadAction(string startStop)
        {
            if (startStop == "Start")
            {
                MBStartUpload.Text = "Stop Upload";
                GlobalVar.LoaderRunning = true;
            }
            else
            {
                MBStartUpload.Text = "Start Upload";
                GlobalVar.LoaderRunning = false;
            }
        }

        private void MBStartLoader_Click(object sender, EventArgs e)
        {
            if (!GlobalVar.LoaderRunning)
            {
                GlobalVar.UploadRunning = true;
                if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
                {
                    _sendEmail.StartUp();
                }

                MBRunLoaderOnce.Enabled = false;
            }
            else
            {
                GlobalVar.UploadRunning = false;
                if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
                {
                    _sendEmail.ShutDown();
                }

                MBRunLoaderOnce.Enabled = true;
            }

            Mediator.GetInstance().OnStartStopLoader(this, !GlobalVar.LoaderRunning ? "Start" : "Stop");
        }

        private void MBRunLoaderOnce_Click(object sender, EventArgs e)
        {
            MBRunLoaderOnce.Enabled = false;
            RunLoaderOnce();
            MBRunLoaderOnce.Enabled = true;
        }

        private void RunLoaderOnce()
        {
            if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
            {
                _sendEmail.StartUpSingleRun(new List<string>());
            }

            Mediator.GetInstance().OnRunLoaderOnce(this);
        }

        private void MBStartUpload_Click(object sender, EventArgs e)
        {
            if (!GlobalVar.UploadRunning)
            {
                GlobalVar.UploadRunning = true;
                if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
                {
                    _sendEmail.StartUp();
                }

                MBRunUpload.Enabled = false;
                Mediator.GetInstance().OnStartStopUpload(this, "Start");
            }
            else
            {
                GlobalVar.UploadRunning = false;
                if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
                {
                    _sendEmail.ShutDown();
                }

                MBRunUpload.Enabled = true;
                Mediator.GetInstance().OnStartStopUpload(this, "Stop");
            }

            // Mediator.GetInstance().OnStartStopUpload(this, !GlobalVar.UploadRunning ? "Start" : "Stop");
        }

        private void MBRunUpload_Click(object sender, EventArgs e)
        {
            MBRunUpload.Enabled = false;
            RunUploadOnce();
            MBRunUpload.Enabled = true;
        }

        private void RunUploadOnce()
        {
            if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
            {
                _sendEmail.StartUpSingleRunUpload(new List<string>());
            }

            Mediator.GetInstance().OnRunUploadOnce(this);
        }

        private void MBMainClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
            if(_standAlone) Close();
        }

        private void HideTabControlTabs()
        {
            var controls = GetTabControls(this, typeof(TabControl));
            foreach (var control1 in controls)
            {
                var control = (TabControl)control1;
                control.Appearance = TabAppearance.FlatButtons;
                control.ItemSize = new Size(0, 1);
                control.SizeMode = TabSizeMode.Fixed;
                foreach (TabPage tab in control.TabPages)
                {
                    tab.Text = string.Empty;
                }
            }
        }

        private IEnumerable<Control> GetTabControls(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();
            var enumerable = controls.ToList();
            return enumerable.SelectMany(c => GetTabControls(c, type)).Concat(enumerable).Where(c => c.GetType() == type);
        }

        private void FrmSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CloseButtonPressed)
            {
                e.Cancel = true;
            }
        }

        private void MBMainSqlServer_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Sql Server Setup";
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = SqlServer;
        }

        private void MBMainInterfaceFiles_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Interface Settings";
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = InterfaceFiles;
        }

        private void MBSqlServerBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "System";
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = Main;
        }

        private void MBInterfaceFilesBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "System";
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = Main;
        }

        private void FrmSystem_Load(object sender, EventArgs e)
        {
            var connectionString = GetConnectionString();

            TextBoxDataSource.Text = connectionString.DataSource;
            TextBoxInitialCatalog.Text = connectionString.InitialCatalog;
            TextBoxUserId.Text = connectionString.UserID;
            TextBoxPassword.Text = connectionString.Password;

            LabelConnectionString.Text = connectionString.ConnectionString;

            var neutronConfig = _jsonData.LoadFile<NeutronRootDirectory>();
            _rootDirectory = neutronConfig.RootDirectory;

            LoaderSettings.SetRootDirectory(_rootDirectory);

            LoaderSettings.Init();
            CommonDirectory.Text = LoaderSettings.GetCommonDirectory();
            RootDirectory.Text = LoaderSettings.GetRootDirectory();
            ImagesDirectory.Text = LoaderSettings.GetImagesDirectory();
            HostOrderDirectory.Text = LoaderSettings.GetHostOrderDirectory();
            HostOrderFile.Text = LoaderSettings.GetHostOrderFile();
            HostUploadDirectory.Text = LoaderSettings.GetHostUploadDirectory();
            HostUploadFile.Text = LoaderSettings.GetHostUploadFile();
            EnableLogging.Checked = Convert.ToBoolean(LoaderSettings.EnableLogging);
            CheckBoxAppendFile.Checked = Convert.ToBoolean(LoaderSettings.AppendFile);
            LogFileDirectory.Text = LoaderSettings.GetLogFileDirectory();
            TextBoxHostOrderFileFilter.Text = LoaderSettings.GetHostOrderFileFilter();
            TextBoxMaintenanceFileFilter.Text = LoaderSettings.GetMaintenanceFileFilter();
            DocumentsDirectory.Text = LoaderSettings.GetDocumentsDirectory();
            MaintenanceFileDirectory.Text = LoaderSettings.GetMaintenanceFileDirectory();
            CostCenterDirectory.Text = LoaderSettings.GetCostCenterDirectory();
            CostCenterFileName.Text = LoaderSettings.GetCostCenterFile();
            LanguageDirectory.Text = LoaderSettings.GetLanguageDirectory();
        }

        private void ButtonSaveConnectionString_Click(object sender, EventArgs e)
        {
            SaveConnectionString();
            ButtonSaveConnectionString.Enabled = false;
        }

        private void SaveConnectionString()
        {
            var builder = GetConnectionString();

            builder.DataSource = TextBoxDataSource.Text;
            builder.InitialCatalog = TextBoxInitialCatalog.Text;

            if (CheckBoxSqlServerAuthentication.Checked)
            {
                builder.UserID = TextBoxUserId.Text;
                builder.Password = TextBoxPassword.Text;
                builder.IntegratedSecurity = false;
                builder.PersistSecurityInfo = true;
            }
            else
            {
                builder.Remove("User Id");
                builder.Remove("Password");
                builder.IntegratedSecurity = true;
            }

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connSection = (ConnectionStringsSection)config.GetSection(sectionName: "connectionStrings");
            connSection.ConnectionStrings["Neutron"].ConnectionString = builder.ConnectionString;
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");
            LabelConnectionString.Text = builder.ConnectionString;
        }

        private SqlConnectionStringBuilder GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder();

            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["Neutron"].ConnectionString;
                builder = new SqlConnectionStringBuilder(connectionString);
            }
            catch (ConfigurationErrorsException ex)
            {
                _logger.LogDetailAsync($"Get Connection String Configuration Error {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                    _logger.LogDetailAsync($"Get Connection String Configuration Error - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogDetailAsync($"Get Connection String Key Not Found {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                    _logger.LogDetailAsync($"Get Connection String Key Not Found - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }
            catch (FormatException ex)
            {
                _logger.LogDetailAsync($"Get Connection String Format Error {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                    _logger.LogDetailAsync($"Get Connection String Format Error - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger.LogDetailAsync($"Get Connection String Argument Error {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                    _logger.LogDetailAsync($"Get Connection String Argument Error - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Get Connection String Unknown Error {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                    _logger.LogDetailAsync($"Get Connection String Unknown Error - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }

            return builder;
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            ButtonSaveConnectionString.Enabled = TestDatabaseConnection();
        }

        private bool TestDatabaseConnection()
        {
            var result = false;
            var builder = GetConnectionString();
            try
            {
                errorProvider.Clear();
                if (string.IsNullOrEmpty(TextBoxDataSource.Text))
                {
                    errorProvider.SetError(TextBoxDataSource, "Required");
                    return false;
                }

                if (string.IsNullOrEmpty(TextBoxInitialCatalog.Text))
                {
                    errorProvider.SetError(TextBoxInitialCatalog, "Required");
                    return false;
                }

                if (CheckBoxSqlServerAuthentication.Checked)
                {
                    if (string.IsNullOrEmpty(TextBoxUserId.Text))
                    {
                        errorProvider.SetError(TextBoxUserId, "Required");
                        return false;
                    }

                    if (string.IsNullOrEmpty(TextBoxPassword.Text))
                    {
                        errorProvider.SetError(TextBoxPassword, "Required");
                        return false;
                    }
                }

                builder.DataSource = TextBoxDataSource.Text;
                builder.InitialCatalog = TextBoxInitialCatalog.Text;
                if (CheckBoxSqlServerAuthentication.Checked)
                {
                    builder.UserID = TextBoxUserId.Text;
                    builder.Password = TextBoxPassword.Text;
                    builder.IntegratedSecurity = false;
                    builder.PersistSecurityInfo = true;
                }
                else
                {
                    builder.Remove("User Id");
                    builder.Remove("Password");
                    builder.IntegratedSecurity = true;
                }

                LabelConnectionString.Text = builder.ConnectionString;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Connection Test Error {Environment.NewLine}{ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogDetailAsync(
                        $"Connection Test Error Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
                }

                MessageBox.Show($"Connection Test Error {Environment.NewLine}{ex.Message}");
            }

            try
            {
                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    MessageBox.Show(@"Connection Established.  Save Configuration.");
                    result = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Connection Test Failed {Environment.NewLine}{ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogDetailAsync(
                        $"Connection Test Failed Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
                }

                MessageBox.Show($"Connection Test Failed {Environment.NewLine}{ex.Message}");
            }

            return result;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            var root = new NeutronRootDirectory { RootDirectory = RootDirectory.Text };
            _jsonData.SaveFile(root);
            LoaderSettings.SetCommonDirectory(CommonDirectory.Text);
            LoaderSettings.SetRootDirectory(RootDirectory.Text);
            LoaderSettings.SetImagesDirectory(ImagesDirectory.Text);
            LoaderSettings.SetHostOrderDirectory(HostOrderDirectory.Text);
            LoaderSettings.SetHostOrderFile(HostOrderFile.Text);
            LoaderSettings.SetHostUploadDirectory(HostUploadDirectory.Text);
            LoaderSettings.SetHostUploadFile(HostUploadFile.Text);
            LoaderSettings.AppendFile = CheckBoxAppendFile.Checked.ToString().ToLower();
            LoaderSettings.EnableLogging = EnableLogging.Checked.ToString().ToLower();
            LoaderSettings.SetLogFileDirectory(LogFileDirectory.Text);
            LoaderSettings.SetHostOrderFileFilter(TextBoxHostOrderFileFilter.Text);
            LoaderSettings.SetMaintenanceFileFilter(TextBoxMaintenanceFileFilter.Text);
            LoaderSettings.SetDocumentsDirectory(DocumentsDirectory.Text);
            LoaderSettings.SetMaintenanceFileDirectory(MaintenanceFileDirectory.Text);
            LoaderSettings.SetCostCenterDirectory(CostCenterDirectory.Text);
            LoaderSettings.SetCostCenterFile(CostCenterFileName.Text);
            LoaderSettings.SetLanguageDirectory(LanguageDirectory.Text);
            LoaderSettings.Save();
        }

        private void ButtonFindHostOrderFile_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                HostOrderFile.Text = openFileDialog1.SafeFileName;
            }
        }

        private void ButtonFindHostOrderDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                HostOrderDirectory.Text = $"{path}";
            }
        }

        private void ButtonFindHostUploadDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                HostUploadDirectory.Text = $"{path}";
            }
        }

        private void ButtonFindHostUploadFile_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                HostUploadFile.Text = openFileDialog1.SafeFileName;
            }
        }

        private void CheckBoxSqlServerAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxSqlServerAuthentication.Checked)
            {
                groupBox1.Enabled = CheckBoxSqlServerAuthentication.Checked;
                TextBoxUserId.Enabled = true;
                TextBoxPassword.Enabled = true;
            }
            else
            {
                groupBox1.Enabled = CheckBoxSqlServerAuthentication.Checked;
                TextBoxUserId.Enabled = false;
                TextBoxPassword.Enabled = false;
            }
        }

        private void ButtonLogFileDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                LogFileDirectory.Text = $"{path}";
            }
        }

       private void ButtonFindImagesDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                ImagesDirectory.Text = $"{path}";
            }
        }

        private void ButtonDocumentsDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                DocumentsDirectory.Text = $"{path}";
            }
        }

        private void ButtonCommonDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                CommonDirectory.Text = $"{path}";
            }
        }

        private void ButtonRootDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                RootDirectory.Text = $"{path}";
            }
        }

        private void ButtonMaintenanceFileDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                MaintenanceFileDirectory.Text = $"{path}";
            }
        }

        private void ButtonFindCostCenterFile_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                CostCenterFileName.Text = openFileDialog1.SafeFileName;
            }
        }

        private void ButtonCostCenterDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                CostCenterDirectory.Text = $"{path}";
            }
        }

        private void ButtonLanguageDirectory_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                LanguageDirectory.Text = $"{path}";
            }
        }

        private void ButtonVerifySql_Click(object sender, EventArgs e)
        {
            _storedProcedureManager.Connection = new SqlConnection(GetConnectionString().ConnectionString);
            _storedProcedureManager.Execute();
        }
    }
}