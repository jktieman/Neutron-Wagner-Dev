using JsonManager;
using MetroFramework.Forms;
using Neutron.Global;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronLoader;
using System;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AlliedLogger;
using NeutronEvents;
using Timer = System.Threading.Timer;

namespace Neutron.Forms
{
    public partial class FrmSystem : MetroForm
    {
        private bool CloseButtonPressed { get; set; }
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private string _configFilePath;
        private string _rootDirectory;
        private readonly DynamicLogger _logger;

        public FrmSystem(IJsonData jsonData, DynamicLogger logger)
        {
            InitializeComponent();
            _jsonData = jsonData;
            _neutronVariables = jsonData.LoadFile<NeutronVariables>();
            _neutronLicense = jsonData.LoadFile<NeutronLicense>();
            _logger = logger;
            KeyPreview = true;
            HideTabControlTabs();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            CloseButtonPressed = false;
            SetLoaderButtonText();
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
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

        private void MBStartLoader_Click(object sender, EventArgs e)
        {
            Mediator.GetInstance().OnStartStopLoader(this, !GlobalVar.LoaderRunning ? "Start" : "Stop");
           
        }

        private void MBCreateHostUploadFile_Click(object sender, EventArgs e)
        {
            CreateHostUploadFile();
        }

        public void CreateHostUploadFile()
        {
            var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
            uploadProcessor.CreateHostFile();
        }

        private void MBMainClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
        }

        private void HideTabControlTabs()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(width: 0, height: 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.Text = string.Empty;
            }
        }

        private void FrmSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CloseButtonPressed)
            {
                e.Cancel = true;
                return;
            }
        }

        private void MBMainSqlServer_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Sql Server Setup";
            LabelFormTitle.BackColor = Color.Turquoise;
            tabControl1.SelectedTab = SqlServer;
        }

        private void MBMainInterfaceFiles_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Interface Settings";
            LabelFormTitle.BackColor = Color.Turquoise;
            tabControl1.SelectedTab = InterfaceFiles;
        }


        private void MBMainSpare1_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Spare 1";
            LabelFormTitle.BackColor = Color.Turquoise;
            tabControl1.SelectedTab = Spare1;
        }


        private void MBSqlServerBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "System";
            LabelFormTitle.BackColor = Color.Turquoise;
            tabControl1.SelectedTab = Main;
        }

        private void MBInterfaceFilesBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "System";
            LabelFormTitle.BackColor = Color.Turquoise;
            tabControl1.SelectedTab = Main;
        }

        private void MBSpare1Back_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "System";
            LabelFormTitle.BackColor = Color.Turquoise;
            tabControl1.SelectedTab = Main;
        }

        private void MBSpare2Back_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "System";
            LabelFormTitle.BackColor = Color.Turquoise;
            tabControl1.SelectedTab = Main;
        }

        private void FrmSystem_Load(object sender, EventArgs e)
        {
            GetConnectionString();
           // _configFilePath = $"{Properties.Settings.Default.ConfigFilePath}";
            _rootDirectory = $"{Properties.Settings.Default.RootDirectory}";
            LoaderSettings.SetRootDirectory(_rootDirectory);
           // _configFilePath = $"{LoaderSettings.GetRootDirectory()}Configuration\\ConfigFile.Csv";

            LoaderSettings.Init();
            RootDirectory.Text = LoaderSettings.GetRootDirectory();
            ImagesDirectory.Text = LoaderSettings.GetImagesDirectory();
            HostOrderDirectory.Text = LoaderSettings.GetHostOrderDirectory();
            HostOrderFile.Text = LoaderSettings.GetHostOrderFile();
            HostUploadDirectory.Text = LoaderSettings.GetHostUploadDirectory();
            HostUploadFile.Text = LoaderSettings.GetHostUploadFile();
            EnableLogging.Checked = Convert.ToBoolean(LoaderSettings.EnableLogging);
            LogFileDirectory.Text = LoaderSettings.GetLogFileDirectory();
            TextBoxHostOrderFileFilter.Text = LoaderSettings.GetHostOrderFileFilter();
            TextBoxMaintenanceFileFilter.Text = LoaderSettings.GetMaintenanceFileFilter();
            DocumentsDirectory.Text = LoaderSettings.GetDocumentsDirectory();
            MaintenanceFileDirectory.Text = LoaderSettings.GetMaintenanceFileDirectory();
            CostCenterDirectory.Text = LoaderSettings.GetCostCenterDirectory();
            CostCenterFileName.Text = LoaderSettings.GetCostCenterFile();
        }

        private void ButtonSaveConnectionString_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Neutron"].ConnectionString;
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                DataSource = TextBoxDataSource.Text,
                InitialCatalog = TextBoxInitialCatalog.Text
            };
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
            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var connSection = (ConnectionStringsSection)config.GetSection(sectionName: "connectionStrings");
            connSection.ConnectionStrings["Neutron"].ConnectionString = builder.ConnectionString;
            config.Save(ConfigurationSaveMode.Modified);
            ButtonSaveConnectionString.Enabled = false;

            LabelConnectionString.Text = builder.ConnectionString;
        }

        private void GetConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Neutron"].ConnectionString;
            var builder = new SqlConnectionStringBuilder(connectionString);
            TextBoxDataSource.Text = builder.DataSource;
            TextBoxInitialCatalog.Text = builder.InitialCatalog;
            TextBoxUserId.Text = builder.UserID;
            TextBoxPassword.Text = builder.Password;

            LabelConnectionString.Text = builder.ConnectionString;
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            SqlConnectionStringBuilder builder;
            try
            {
                errorProvider.Clear();
                if (string.IsNullOrEmpty(TextBoxDataSource.Text))
                {
                    errorProvider.SetError(TextBoxDataSource, "Required");
                    return;
                }
                if (string.IsNullOrEmpty(TextBoxInitialCatalog.Text))
                {
                    errorProvider.SetError(TextBoxInitialCatalog, "Required");
                    return;
                }

                if (CheckBoxSqlServerAuthentication.Checked)
                {
                    if (string.IsNullOrEmpty(TextBoxUserId.Text))
                    {
                        errorProvider.SetError(TextBoxUserId, "Required");
                        return;
                    }
                    if (string.IsNullOrEmpty(TextBoxPassword.Text))
                    {
                        errorProvider.SetError(TextBoxPassword, "Required");
                        return;
                    }
                }


                string connectionString = ConfigurationManager.ConnectionStrings["Neutron"].ConnectionString;
                builder = new SqlConnectionStringBuilder(connectionString);
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

                MessageBox.Show("Text Error  " + ex.Message + " Inner:  " + ex.InnerException);
                throw;
            }
            try
            {
                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    MessageBox.Show(@"Connection Established.  Save Configuration.");
                    ButtonSaveConnectionString.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Failed: " + ex.Message + "  Inner:  " + ex.InnerException);
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            LoaderSettings.SetRootDirectory(RootDirectory.Text);
            Properties.Settings.Default.RootDirectory = LoaderSettings.GetRootDirectory();
            Properties.Settings.Default.Save();
           // _configFilePath = $"{LoaderSettings.GetRootDirectory()}Configuration\\ConfigFile.Csv";
           // string configFilePath = Properties.Settings.Default.ConfigFilePath;
            
            LoaderSettings.SetImagesDirectory(ImagesDirectory.Text);
            LoaderSettings.SetHostOrderDirectory(HostOrderDirectory.Text);
            LoaderSettings.SetHostOrderFile(HostOrderFile.Text);
            LoaderSettings.SetHostUploadDirectory(HostUploadDirectory.Text);
            LoaderSettings.SetHostUploadFile(HostUploadFile.Text);
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
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                HostOrderFile.Text = openFileDialog1.SafeFileName;
            }
        }

        private void ButtonFindHostOrderDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                HostOrderDirectory.Text = string.Format("{0}", path);
            }
        }

        private void ButtonFindHostUploadDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                HostUploadDirectory.Text = string.Format("{0}", path);
            }
        }

        private void ButtonFindHostUploadFile_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                HostUploadFile.Text = openFileDialog1.SafeFileName;
            }
        }

        private void CheckBoxSqlServerAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxSqlServerAuthentication.Checked == true)
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
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                LogFileDirectory.Text = string.Format("{0}", path);
            }
        }

        //private void CheckBoxIntegratedSecurity_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (CheckBoxIntegratedSecurity.Checked == true)
        //    {
        //        groupBox1.Enabled = false;
        //        CheckBoxSqlServerAuthentication.Enabled = false;
        //        TextBoxUserId.Enabled = false;
        //        TextBoxPassword.Enabled = false;
        //    }
        //    else
        //    {
        //        groupBox1.Enabled = CheckBoxSqlServerAuthentication.Checked;
        //        CheckBoxIntegratedSecurity.Enabled = true;
        //        TextBoxUserId.Enabled = false;
        //        TextBoxPassword.Enabled = false;
        //    }
        //}

        private void ButtonFindImagesDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                ImagesDirectory.Text = string.Format("{0}", path);
            }
        }

        private void ButtonDocumentsDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                DocumentsDirectory.Text = string.Format("{0}", path);
            }
        }

        private void ButtonRootDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                RootDirectory.Text = string.Format("{0}", path);
            }
        }

        private void MBMainSpare2_Click(object sender, EventArgs e)
        {

        }

        private void ButtonMaintenanceFileDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                MaintenanceFileDirectory.Text = string.Format("{0}", path);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var maintProcessor = new MasterMaintenanceProcessor();
            //maintProcessor.ProcessMasterMaintenanceFiles();
        }

        private void ButtonFindCostCenterFile_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                CostCenterFileName.Text = openFileDialog1.SafeFileName;
            }
        }

        private void ButtonCostCenterDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                CostCenterDirectory.Text = $"{path}";
            }
        }


        private void ButtonLanguageDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                LanguageDirectory.Text = $"{path}";
            }
        }
    }
}