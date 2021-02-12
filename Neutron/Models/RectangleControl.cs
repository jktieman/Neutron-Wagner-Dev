using System.Drawing;
using System.Windows.Forms;

namespace Neutron.Models
{
    public class RectangleControl
    {
        public RectangleControl(Rectangle rectangle, Control control)
        {
            Rectangle = rectangle;
            Control = control;
        }

        public Rectangle Rectangle { get; set; }
        public Control Control { get; set; }
    }
}
