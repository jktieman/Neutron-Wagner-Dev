using JsonManager;
using MetroFramework.Forms;
using Neutron.Global;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using AlliedLogger;
using AlliedPostOffice;
using AlliedPostOffice.Concrete;
using Neutron.Models;
using NeutronEvents;
using SqlSchemaManager;
//using Syncfusion.Windows.Forms;

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
        private ISendEmail _sendEmail;
        private readonly IStoredProcedureManager _storedProcedureManager;
        private readonly bool _standAlone;
        private readonly bool _emailEnabled;

        public FrmSystem(IJsonData jsonData, IStoredProcedureManager storedProcedureManager
            , NeutronVariables neutronVariables, NeutronLicense neutronLicense, ISendEmail sendEmail, bool standAlone = false)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _sendEmail = sendEmail;
            _logger = NeutronCore.Global.Logger.SetupLogger("System");
           // SetupEmail();
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
                // Stop the Loader
                MBStartLoader.Text = _resourceManager.GetString($"StopLoader"); 
                MBRunLoaderOnce.Enabled = false;
            }
            else
            {
                // Start the Loader
                MBStartLoader.Text = _resourceManager.GetString($"RunLoaderContinuously");
                MBRunLoaderOnce.Enabled = true;
            }
        }

        private void SetUploadButtonText()
        {
            if (GlobalVar.UploadRunning)
            {
                MBStartUpload.Text = _resourceManager.GetString($"StopUpload");
                MBRunUploadOnce.Enabled = false;
            }
            else
            {
                MBStartUpload.Text = _resourceManager.GetString($"RunUploadContinuously");
                MBRunUploadOnce.Enabled = true;
            }
        }

        private void StartStopLoaderAction(string startStop)
        {
            if (startStop == "Start")
            {
                MBStartLoader.Text = _resourceManager.GetString($"StopLoader");
                MBRunLoaderOnce.Enabled = false;
                GlobalVar.LoaderRunning = true;
            }
            else
            {
                //MBStartLoader.Text = "Start Loader";
                MBStartLoader.Text = _resourceManager.GetString($"RunLoaderContinuously");
                MBRunLoaderOnce.Enabled = true;
                GlobalVar.LoaderRunning = false;
            }
        }

        private void StartStopUploadAction(string startStop)
        {
            if (startStop == "Start")
            {
                MBStartUpload.Text = _resourceManager.GetString($"StopUpload");
                MBRunUploadOnce.Enabled = false;
                GlobalVar.LoaderRunning = true;
            }
            else
            {
                MBStartUpload.Text = MBStartUpload.Text = _resourceManager.GetString($"RunUploadContinuously");
                MBRunUploadOnce.Enabled = true;
                GlobalVar.LoaderRunning = false;
            }
        }

        private void MBStartLoader_Click(object sender, EventArgs e)
        {
            if (!GlobalVar.LoaderRunning)
            {
                GlobalVar.LoaderRunning = true;
                Mediator.GetInstance().OnStartStopLoader(this, "Start");
            }
            else
            {
                GlobalVar.LoaderRunning = false;
                Mediator.GetInstance().OnStartStopLoader(this, "Stop");
            }
        }
        private void MBStartUpload_Click(object sender, EventArgs e)
        {
            if (!GlobalVar.UploadRunning)
            {
                GlobalVar.UploadRunning = true;
                MBRunUploadOnce.Enabled = false;
                Mediator.GetInstance().OnStartStopUpload(this, "Start");
            }
            else
            {
                GlobalVar.UploadRunning = false;
                MBRunUploadOnce.Enabled = true;
                Mediator.GetInstance().OnStartStopUpload(this, "Stop");
            }
        }

        private void MBRunLoaderOnce_Click(object sender, EventArgs e)
        {
            MBRunLoaderOnce.Enabled = false;
            RunLoaderOnce();
            MBRunLoaderOnce.Enabled = true;
        }

        private void RunLoaderOnce()
        {
            if (GlobalVar.LoaderRunning)
            {
                MessageBox.Show("Loader is already running.", "Loader Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                GlobalVar.LoaderRunning = true;
                Mediator.GetInstance().OnRunLoaderOnce(this);
                GlobalVar.LoaderRunning = false;
            }
        }


        private void MBRunUploadOnce_Click(object sender, EventArgs e)
        {
            MBRunUploadOnce.Enabled = false;
            RunUploadOnce();
            MBRunUploadOnce.Enabled = true;
        }

        private void RunUploadOnce()
        {
            if (GlobalVar.UploadRunning)
            {
                MessageBox.Show("Upload is already running.", "Upload Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                GlobalVar.UploadRunning = true;
                Mediator.GetInstance().OnRunUploadOnce(this);
                GlobalVar.UploadRunning = false;
            }
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
            CheckBoxSqlServerAuthentication.Checked = true;
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
             _ = _logger.LogDetailAsync($"Get Connection String Configuration Error {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                 _ = _logger.LogDetailAsync($"Get Connection String Configuration Error - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }
            catch (KeyNotFoundException ex)
            {
             _ = _logger.LogDetailAsync($"Get Connection String Key Not Found {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                 _ = _logger.LogDetailAsync($"Get Connection String Key Not Found - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }
            catch (FormatException ex)
            {
             _ = _logger.LogDetailAsync($"Get Connection String Format Error {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                 _ = _logger.LogDetailAsync($"Get Connection String Format Error - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }
            catch (ArgumentException ex)
            {
             _ = _logger.LogDetailAsync($"Get Connection String Argument Error {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                 _ = _logger.LogDetailAsync($"Get Connection String Argument Error - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
             _ = _logger.LogDetailAsync($"Get Connection String Unknown Error {Environment.NewLine} {ex.Message}");
                if (ex.InnerException != null)
                 _ = _logger.LogDetailAsync($"Get Connection String Unknown Error - Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
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
             _ = _logger.LogDetailAsync($"Connection Test Error {Environment.NewLine}{ex.Message}");
                if (ex.InnerException != null)
                {
                 _ = _logger.LogDetailAsync(
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
             _ = _logger.LogDetailAsync($"Connection Test Failed {Environment.NewLine}{ex.Message}");
                if (ex.InnerException != null)
                {
                 _ = _logger.LogDetailAsync(
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

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();

                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmSystem",
                    resourceDir: languageDirectory, usingResourceSet: null);
                MBMainSqlServer.Text = _resourceManager.GetString("SQLServer");
                MBMainInterfaceFile.Text = _resourceManager.GetString("InterfaceInformation");
                MBStartLoader.Text = _resourceManager.GetString("RunLoaderContinuously");
                MBRunLoaderOnce.Text = _resourceManager.GetString("RunLoaderOnce");
                MBStartUpload.Text = _resourceManager.GetString("RunUploadContinuously");
                MBRunUploadOnce.Text = _resourceManager.GetString("RunUploadOnce");

                //MtHotAction.Text = _resourceManager.GetString("HotAction");
                //MtPick.Text = _resourceManager.GetString("Pick");
                //MtStore.Text = _resourceManager.GetString("Store");
                //MtUsers.Text = _resourceManager.GetString("Users");
                //MtLogOff.Text = _resourceManager.GetString("LogOff");
                //MtUtilities.Text = _resourceManager.GetString("Utilities");
                //MtSystem.Text = _resourceManager.GetString("System");
                //MtLac.Text = _resourceManager.GetString("LocationAccessControl");
                //ButtonPark.Text = _resourceManager.GetString("Park");
                //ButtonClose.Text = _resourceManager.GetString("Close");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading languages.  FrmSystem  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }
    }
}