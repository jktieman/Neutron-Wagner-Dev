using System.Windows.Forms;

namespace Neutron.Models
{
    /// <summary>
    /// Represents data associated with a grid row, including order details and a reference to a <see cref="TextBox"/>.
    /// </summary>
    /// <remarks>
    /// This class is used to encapsulate information such as the row index, order identifiers, and associated text box control.
    /// It is utilized in various operations involving grid data processing and user input handling.
    /// </remarks>
    public class GridData
    {
        public int RowIndex { get; set; }
        public int OrderId { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public TextBox TextBox { get; set; }
    }
}
