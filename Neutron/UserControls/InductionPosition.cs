using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.UserControls
{
    public partial class InductionPosition : UserControl
    {
        public InductionPosition(int position)
        {
            InitializeComponent();
            LabelPos.Text = position.ToString();
            
        }
    }
}
