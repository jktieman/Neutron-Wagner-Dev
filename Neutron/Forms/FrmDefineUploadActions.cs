using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using JsonManager;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Enums;

namespace Neutron.Forms
{
    public partial class FrmDefineUploadActions : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _enumResourceManager;
        private readonly IJsonData _jsonData;
        private readonly List<ActionCode> _actionCodes;
        public string CurrentIds;
        public List<UploadActionIdString> ActionIds;
        private bool _checkAllActions;

        public FrmDefineUploadActions(IJsonData jsonData, List<ActionCode> actionCodes, string currentIds)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _jsonData = jsonData;
            _actionCodes = actionCodes;
            CurrentIds = currentIds;

            InitForm();
        }

        private void InitForm()
        {
            SetupCheckedListBox();
        }

        private void SetupCheckedListBox()
        {
            var actionCodeDictionary = NeutronCore.Extensions.EnumExtensions.EnumToDictionary<ActionCode>();
            if (!string.IsNullOrEmpty(CurrentIds))
            {
               var nums = CurrentIds.Split(',').Select(int.Parse).ToList();


                CheckedListBox.DataSource = new BindingSource(actionCodeDictionary, null);
                CheckedListBox.DisplayMember = "Value";
                CheckedListBox.ValueMember = "Key";

                var indexes = new List<int>();

                foreach (var num in nums)
                {
                    foreach (var item in CheckedListBox.Items)
                    {
                        var key = ((KeyValuePair<int,string>) item).Key;
                        if (key.Equals(num))
                        {
                            indexes.Add(CheckedListBox.Items.IndexOf(item));
                        }
                    }
                }

                foreach (var index in indexes)
                {
                    CheckedListBox.SetItemCheckState(index, CheckState.Checked);
                }
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            var actionId = GetActionIds();
            _jsonData.SaveFile<UploadActionIdString>(actionId);
            CurrentIds = actionId.CsvIdString;
            Close();
        }

        private UploadActionIdString GetActionIds()
        {
            var actionList = new List<string>();

            foreach (KeyValuePair<int, string> item in CheckedListBox.CheckedItems)
            {
                actionList.Add(item.Key.ToString());
            }

            var result = string.Join(",", actionList);
            var actionId = new UploadActionIdString { CsvIdString = string.Join(",", actionList) };
            return actionId;
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmDefineActionGroup",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _enumResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "EnumDescriptions",
                    resourceDir: languageDirectory, usingResourceSet: null);
                ButtonCancel.Text = _resourceManager.GetString("Cancel");
                ButtonSave.Text = _resourceManager.GetString("Save");
                Text = _resourceManager.GetString("DefineActionGroup");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

        private void ButtonCheckAllActions_Click(object sender, EventArgs e)
        {
            SelectAllActionCheckBoxes(checkThem: true);
        }

        private void ButtonClearAllActions_Click(object sender, EventArgs e)
        {
            SelectAllActionCheckBoxes(checkThem: false);
        }

        private void SelectAllActionCheckBoxes(bool checkThem)
        {
            for (var i = 0; i < (CheckedListBox.Items.Count); i++)
            {
                CheckedListBox.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        private void SelectActionCheckBoxes(bool checkThem)
        {
            for (var i = 0; i < (CheckedListBox.Items.Count); i++)
            {
                CheckedListBox.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
            }
        }

    }
}
