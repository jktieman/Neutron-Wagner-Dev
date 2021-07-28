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
    public partial class FrmDefineActionGroup : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _enumResourceManager;
        private readonly IJsonData _jsonData;
        private readonly List<ActionCode> _actionCodes;
        private readonly string _currentIds;
        public List<ActionIdString> ActionIds;
        private bool _checkAllActions;

        public FrmDefineActionGroup(IJsonData jsonData, List<ActionCode> actionCodes, string currentIds)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _jsonData = jsonData;
            _actionCodes = actionCodes;
            _currentIds = currentIds;

            InitForm();
        }

        private void InitForm()
        {
            SetupCheckedListBox();
            ////var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode)))
            ////   .Select(r => new EnumModel { Id = (int)r, Name = r.GetEnumDescription() }).ToList();

            ////CheckedListBox.DataSource = new BindingSource(_actionCodes, null);
            ////CheckedListBox.DisplayMember = "Name";
            ////CheckedListBox.ValueMember = "Id";

            ////var currentIds = _jsonData.LoadFile<ActionIdString>().CsvIdString;
            //if (string.IsNullOrEmpty(_currentIds)) return;
            //var nums = _currentIds.Split(',').Select(int.Parse).ToList();
            //if (!nums.Any()) return;

            //for (var i = 0; i < nums.Count; i++)
            //{
            //    var idx = CheckedListBox.Items.IndexOf(   nums[i]);
            //    CheckedListBox.SetItemCheckState(idx, true ? CheckState.Checked : CheckState.Unchecked);
            //}



            //for (var i = 1; i <= CheckedListBox.Items.Count; i++)
            //{
            //    //  var drv = CheckedListBox[i] as Dictionary<int, string>;
            //    foreach (var num in nums)
            //    {
            //        if ((int)CheckedListBox.Items.Key == num)
            //        {
            //            CheckedListBox.SetItemChecked(i, true);
            //            continue;
            //        }
            //    }
            //}
            //for (var i = 0; i < CheckedListBox.Items.Count; i++)
            //{

            //}
        }

        private void SetupCheckedListBox()
        {
           
            var actionCodeDictionary = NeutronCore.Extensions.EnumExtensions.EnumToDictionary<ActionCode>();
            var currentIDs = _jsonData.LoadFile<ActionIdString>().CsvIdString;
            if (!string.IsNullOrEmpty(currentIDs))
            {
               var nums = currentIDs.Split(',').Select(int.Parse).ToList();


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


            //// _actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode))).ToList();
            //// _currentIDs = _jsonData.LoadFile<ActionIdString>().CsvIdString;
            //var codes = new Dictionary<int, string>();
            //foreach (var code in _actionCodes)
            //{
            //    //if (!string.IsNullOrEmpty(_currentIds))
            //    // {
            //    //var nums = _currentIds.Split(',').Select(int.Parse).ToArray();
            //    //if (nums.Length > 0)
            //    //{
            //    //    if (nums.Contains((int)code))
            //    //    {
            //    codes.Add((int)code, _enumResourceManager.GetString(code.ToString()));
            //    //    }
            //    // }
            //    // }
            //}
            //CheckedListBox.DataSource = new BindingSource(codes, null);
            //CheckedListBox.DisplayMember = "Value";
            //CheckedListBox.ValueMember = "Key";
        }

        //private void SetupCheckListBox()
        //{
        //    var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode))).ToList();
        //    var codes = new Dictionary<int, string>();
        //    foreach (var code in actionCodes)
        //    {
        //        //codes.Add((int)code, code.GetEnumDescription());
        //        codes.Add((int)code, _enumResourceManager.GetString(code.ToString()));
        //    }
        //    CheckedListBox.DataSource = new BindingSource(codes, null);
        //    CheckedListBox.DisplayMember = "Value";
        //    CheckedListBox.ValueMember = "Key";
        //}

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
                Text = _resourceManager.GetString("DefineActionGroup");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

        private void ButtonCheckAllActions_Click(object sender, EventArgs e)
        {
            //_checkAllActions = true;
            SelectAllActionCheckBoxes(checkThem: true);
            //var userIds = GetUserIds();
            //var codes = GetCodes();
            //GetData(userIds, codes);
            // _checkAllActions = false;
        }
        private void ButtonClearAllActions_Click(object sender, EventArgs e)
        {
            // _clearAllActions = true;
            SelectAllActionCheckBoxes(checkThem: false);
            // ClearAll();
            // _clearAllActions = false;
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
