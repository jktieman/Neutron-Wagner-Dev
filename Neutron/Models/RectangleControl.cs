using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Models
{
    public class RectangleControl
    {
        public RectangleControl(Rectangle rectangle, Control control)
        {
            this.Rectangle = rectangle;
            this.Control = control;
        }

        public Rectangle Rectangle { get; set; }
        public Control Control { get; set; }
    }
}
