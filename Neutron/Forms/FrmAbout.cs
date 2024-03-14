using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using AlliedLicenseGenerator.Core;
using AlliedLicenseGenerator.Core.Extensions;
using AlliedLicenseVerifier;
using AlliedLogger;

namespace Neutron.Forms
{
    public partial class FrmAbout : MetroForm
    {
        private readonly IDynamicLogger _logger;

        public FrmAbout()
        {
            InitializeComponent();
            _logger = NeutronCore.Global.Logger.SetupLogger("About");
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var currentPath = AppDomain.CurrentDomain.BaseDirectory;
            var licenseFile = Path.Combine(currentPath, @"License.lic");
            var publicKeyFile = Path.Combine(currentPath, @"PublicKey.xml");
            var licenseManager = new AlliedLicenseManager();
            var result = licenseManager.ExamineLicense(licenseFile, assembly.GetName(), publicKeyFile);
         Task.Run(() =>  _logger.LogDetailAsync("{result.LicenseStatus.GetDescription()}"));
            MessageBox.Show($"{result.LicenseStatus.GetDescription()}");
        }

        private void FrmAbout_Load(object sender, EventArgs e)
        {
            // Trademark symbol ALT + 0153
            // Copyright symbol ALT + 0169

            var licenseDetail = GetLicenseInfo();
            this.Text = $@"About {licenseDetail.Product} ™";
            LabelProductName.Text = $@"{licenseDetail.Product} ™";
            LabelLicensee.Text = licenseDetail.Licensee;
            if (licenseDetail.LicenseStatus == LicenseStatus.ValidLicense)
            {
                LabelSerialNumber.Text = $@"License #: {licenseDetail.SerialNumber}";
                LabelExpireDate.Text = $@"Valid Thru: {licenseDetail.ExpireDate}";
                LabelCoveredVersion.Text = $@"Licensed Version: {licenseDetail.CoveredVersion}";
                LabelSupervisorStations.Text = $@"Supervisor Stations: {licenseDetail.SupervisorStations}";
                LabelWorkStations.Text = $@"Work Stations: {licenseDetail.WorkStations}";
                LabelRackStations.Text = $@"Rack Stations: {licenseDetail.RackStations}";
            }
            else
            {
                LabelLicensee.Text = $@"{licenseDetail.LicenseStatus.GetDescription()}";
                LabelSerialNumber.Visible = false;
                LabelExpireDate.Visible = false;
                LabelCoveredVersion.Visible = false;
                LabelSupervisorStations.Visible = false;
                LabelWorkStations.Visible = false;
                LabelRackStations.Visible = false;
            }

         Task.Run(() =>  _logger.LogDetailAsync($"License Status: {LabelLicensee.Text}"));

        }

        private LicenseFileDetail GetLicenseInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            LabelVersion.Text = $@"Version: {assembly.GetName().Version}";
            var currentPath = AppDomain.CurrentDomain.BaseDirectory;
            var licenseFile = Path.Combine(currentPath, @"License.lic");
            var publicKeyFile = Path.Combine(currentPath, @"PublicKey.xml");
            var licenseManager = new AlliedLicenseManager();
            return licenseManager.ExamineLicense(licenseFile, assembly.GetName(), publicKeyFile);
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}
