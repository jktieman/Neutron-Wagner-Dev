using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JsonManager;
using Neutron.Models;
using NeutronCore.Enums;
using NeutronCore.Extensions;

namespace Neutron.Forms
{
    public partial class FrmDefineActionGroup : Form
    {
        private readonly IJsonData _jsonData;
        public List<ActionIdString> ActionIds;

        public FrmDefineActionGroup(IJsonData jsonData)
        {
            _jsonData = jsonData;
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {

            var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode)))
                .Select(r => new EnumModel { Id = (int)r, Name = r.GetEnumDescription() }).ToList();

            CheckedListBox.DataSource = new BindingSource(actionCodes, null);
            CheckedListBox.DisplayMember = "Name";
            CheckedListBox.ValueMember = "Id";

            var currentIds = _jsonData.LoadFile<ActionIdString>().CsvIdString;
            if (string.IsNullOrEmpty(currentIds)) return;
            var nums = currentIds.Split(',').Select(int.Parse).ToArray();
            if (nums.Length <= 0) return;
            for (var i = 0; i < CheckedListBox.Items.Count; i++)
            {
                foreach (var num in nums)
                {
                    if (((EnumModel)CheckedListBox.Items[i]).Id == num)
                    {
                        CheckedListBox.SetItemChecked(i, true);
                    }
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
            _jsonData.SaveFile<ActionIdString>(actionId);

            Close();
        }

        private ActionIdString GetActionIds()
        {
            var actionList = new List<string>();

            foreach (EnumModel item in CheckedListBox.CheckedItems)
            {
                actionList.Add(item.Id.ToString());
            }

            var result = string.Join(",", actionList);
            var actionId = new ActionIdString { CsvIdString = string.Join(",", actionList) };
            return actionId;
        }
    }
}
