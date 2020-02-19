using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using JsonManager;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Enums;
using NeutronCore.Extensions;

namespace Neutron.Forms
{
    public partial class FrmDefineActionGroup : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _enumResourceManager;
        private readonly IJsonData _jsonData;
        public List<ActionIdString> ActionIds;

        public FrmDefineActionGroup(IJsonData jsonData)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _jsonData = jsonData;

            InitForm();
        }

        private void InitForm()
        {
            SetupCheckListBox();
            //var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode)))
             //   .Select(r => new EnumModel { Id = (int)r, Name = r.GetEnumDescription() }).ToList();

            //CheckedListBox.DataSource = new BindingSource(actionCodes, null);
            //CheckedListBox.DisplayMember = "Name";
            //CheckedListBox.ValueMember = "Id";

            //var currentIds = _jsonData.LoadFile<ActionIdString>().CsvIdString;
            //if (string.IsNullOrEmpty(currentIds)) return;
            //var nums = currentIds.Split(',').Select(int.Parse).ToArray();
            //if (nums.Length <= 0) return;
            //for (var i = 0; i < CheckedListBox.Items.Count; i++)
            //{
            //    foreach (var num in nums)
            //    {
            //        if (((EnumModel)CheckedListBox.Items[i]).Id == num)
            //        {
            //            CheckedListBox.SetItemChecked(i, true);
            //        }
            //    }
            //}
        }

        private void SetupCheckListBox()
        {
            var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode))).ToList();
            var codes = new Dictionary<int, string>();
            foreach (var code in actionCodes)
            {
                //codes.Add((int)code, code.GetEnumDescription());
                codes.Add((int)code, _enumResourceManager.GetString(code.ToString()));
            }
            CheckedListBox.DataSource = new BindingSource(codes, null);
            CheckedListBox.DisplayMember = "Value";
            CheckedListBox.ValueMember = "Key";
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            var actionId = GetActionIds();
            _jsonData.SaveFile<ActionIdString>(actionId);

            Close();
        }
        //private string GetCodes()
        //{
        //    var codes = new List<string>();
        //    foreach (KeyValuePair<int, string> item in CheckedListBox.CheckedItems)
        //    {
        //        codes.Add(item.Key.ToString());
        //    }
        //    var result = string.Join(",", codes);
        //    return result;
        //}

        private ActionIdString GetActionIds()
        {
            var actionList = new List<string>();

            foreach (KeyValuePair<int, string> item in CheckedListBox.CheckedItems)
            {
                actionList.Add(item.Key.ToString());
            }

            var result = string.Join(",", actionList);
            var actionId = new ActionIdString { CsvIdString = string.Join(",", actionList) };
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
                this.Text = _resourceManager.GetString("DefineActionGroup");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}
