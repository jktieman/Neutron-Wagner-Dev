using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore;

namespace Neutron.Forms
{
    public partial class FrmShortCut : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;

        public FrmShortCut()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmShortCut",
                    resourceDir: languageDirectory, usingResourceSet: null);
                ButtonClose.Text = _resourceManager.GetString("Close");
                LabelEnterKey.Text = _resourceManager.GetString("EnterKey");
                LabelSpaceKey.Text = _resourceManager.GetString("SpaceKey");
                Label_L_Key.Text = _resourceManager.GetString("LKey");
                Label_A_Key.Text = _resourceManager.GetString("AKey");
                Label_S_Key.Text = _resourceManager.GetString("SKey");
                Label_Q_Key.Text = _resourceManager.GetString("QKey");
                Label_K_Key.Text = _resourceManager.GetString("KKey");
                Label_H_Key.Text = _resourceManager.GetString("HKey");
                this.Text = _resourceManager.GetString("ShortcutKeys");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}
