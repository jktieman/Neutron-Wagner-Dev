using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using JsonManager;
using Neutron.Extensions;
using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace Neutron.Forms
{
    public partial class FrmDefineUserGroup : Form
    {
        private readonly IJsonData _jsonData;
        public List<UserIdString> UserIds;

        public FrmDefineUserGroup(IJsonData jsonData)
        {
            _jsonData = jsonData;
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {
            using (var db = new NeutronDb())
            {
                CheckedListBox.DataSource = db.Users.OrderBy(o => o.Lastname).ToList();
                CheckedListBox.DisplayMember = "FullName";
                CheckedListBox.ValueMember = "Id";
            }

            var currentIds = _jsonData.LoadFile<UserIdString>().CsvIdString;
            if (string.IsNullOrEmpty(currentIds)) return;
            var nums = currentIds.Split(',').Select(int.Parse).ToArray();
            if (nums.Length <= 0) return;
            for (var i = 0; i < CheckedListBox.Items.Count; i++)
            {
                foreach (var num in nums)
                {
                    if (((User)CheckedListBox.Items[i]).Id == num)
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
            var userId = GetUserIds();
            _jsonData.SaveFile<UserIdString>(userId);

            Close();
        }

        private UserIdString GetUserIds()
        {
            var userList = new List<string>();

            foreach (User item in CheckedListBox.CheckedItems)
            {
                userList.Add(item.Id.ToString());
            }

            var result = string.Join(",", userList);
            var userId = new UserIdString { CsvIdString = string.Join(",", userList) };
            return userId;
        }

    }
}
