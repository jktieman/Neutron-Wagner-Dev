using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonManager;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace Neutron.Forms
{
    public partial class FrmDefineUserGroup : Form
    {
        private readonly IJsonData _jsonData;
        public List<UserId> UserIds;

        public FrmDefineUserGroup(IJsonData jsonData)
        {
            _jsonData = jsonData;
            InitForm();
        }

        private void InitForm()
        {
            using (var db = new NeutronDb())
            {
                CheckedListBox.DataSource = db.Users.ToList();
                CheckedListBox.DisplayMember = "FullName";
                CheckedListBox.ValueMember = "Id";
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            UserIds = GetUserIds();
            _jsonData.SaveFile<List<UserId>>(UserIds);
        }

        private List<UserId> GetUserIds()
        {
            var userList = new List<UserId> ();

            foreach (User item in CheckedListBox.CheckedItems)
            {
                userList.Add(new UserId {Id = item.Id}); 
            }

           // var result = string.Join(",", userList);

            return userList;
        }

        public class UserId
        {
            public int Id { get; set; }
        }
    }
}
