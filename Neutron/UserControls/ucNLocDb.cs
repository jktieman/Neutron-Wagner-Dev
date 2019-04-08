using System;
using System.Collections.Generic;
using System.Linq;
using MetroFramework.Controls;
using NeutronData.DataContexts;

namespace Neutron.UserControls
{
    public partial class ucNLocDb : MetroUserControl
    {
        List<int> stations = new List<int>();
        List<int> cars = new List<int>();
        List<int> bins = new List<int>();

        public ucNLocDb()
        {
            InitializeComponent();
        }

        private void ucNLocDb_Load(object sender, EventArgs e)
        {
            using (NeutronDb db = new NeutronDb())
            {
                nLocDbBindingSource.DataSource = db.NLocDbs.ToList();
            }
        }
    }
}
