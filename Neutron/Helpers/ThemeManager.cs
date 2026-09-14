using MetroFramework;
using MetroFramework.Components;
using MetroFramework.Controls;
using MetroFramework.Forms;
using System.Drawing;
using System.Windows.Forms;

namespace Neutron.Helpers
{
    /// <summary>
    /// Centralized theme management for consistent modern look across all forms.
    /// </summary>
    public static class ThemeManager
    {
        // Change these two values to retheme the entire application
        public static MetroThemeStyle Theme { get; set; } = MetroThemeStyle.Light;
        public static MetroColorStyle AccentColor { get; set; } = MetroColorStyle.Teal;

        // Custom brand colors
        public static Color PrimaryBackground { get; } = Color.FromArgb(245, 247, 250);
        public static Color CardBackground { get; } = Color.White;
        public static Color HeaderColor { get; } = Color.FromArgb(33, 37, 41);
        public static Color SuccessColor { get; } = Color.FromArgb(40, 167, 69);
        public static Color WarningColor { get; } = Color.FromArgb(255, 193, 7);
        public static Color DangerColor { get; } = Color.FromArgb(220, 53, 69);
        public static Color MutedText { get; } = Color.FromArgb(108, 117, 125);

        public static void ApplyTheme(MetroForm form, MetroStyleManager styleManager)
        {
            styleManager.Theme = Theme;
            styleManager.Style = AccentColor;
            styleManager.Owner = form;

            form.Theme = Theme;
            form.Style = AccentColor;
            form.BackColor = PrimaryBackground;
            form.BorderStyle = MetroFormBorderStyle.FixedSingle;
            form.ShadowType = MetroFormShadowType.AeroShadow;
        }

        /// <summary>
        /// Applies a consistent style to a MetroTile.
        /// </summary>
        /// <param name="tile">The MetroTile to style.</param>
        /// <param name="color">The accent color for the tile.</param>
        /// <param name="fontSize">Font size (default: Tall).</param>
        /// <param name="fontWeight">Font weight (default: Bold).</param>
        public static void StyleMetroTile(MetroTile tile, MetroColorStyle color,
            MetroTileTextSize fontSize = MetroTileTextSize.Tall,
            MetroTileTextWeight fontWeight = MetroTileTextWeight.Bold)
        {
            tile.Style = color;
            tile.Theme = Theme;
            tile.TileTextFontSize = fontSize;
            tile.TileTextFontWeight = fontWeight;
            tile.UseSelectable = true;
        }

        /// <summary>
        /// Applies a consistent style to all MetroTiles within a container control.
        /// Each tile retains its existing color; only common properties are standardized.
        /// </summary>
        /// <param name="container">The parent control containing MetroTiles (e.g. MetroPanel).</param>
        /// <param name="fontSize">Font size (default: Tall).</param>
        /// <param name="fontWeight">Font weight (default: Bold).</param>
        public static void StyleAllMetroTiles(Control container,
            MetroTileTextSize fontSize = MetroTileTextSize.Tall,
            MetroTileTextWeight fontWeight = MetroTileTextWeight.Bold)
        {
            foreach (Control control in container.Controls)
            {
                if (control is MetroTile tile)
                {
                    tile.Theme = Theme;
                    tile.TileTextFontSize = fontSize;
                    tile.TileTextFontWeight = fontWeight;
                    tile.UseSelectable = true;
                }
            }
        }

        public static void StyleDataGridView(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = CardBackground;
            grid.GridColor = Color.FromArgb(222, 226, 230);
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 58, 64);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
            grid.ColumnHeadersHeight = 40;

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            grid.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.RowTemplate.Height = 36;

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        }

        public static void StyleStatusLabel(Label label, StatusType status)
        {
            label.Font = new Font("Segoe UI Semibold", 10F);
            label.Padding = new Padding(8, 4, 8, 4);

            switch (status)
            {
                case StatusType.Success:
                    label.BackColor = SuccessColor;
                    label.ForeColor = Color.White;
                    break;
                case StatusType.Warning:
                    label.BackColor = WarningColor;
                    label.ForeColor = HeaderColor;
                    break;
                case StatusType.Danger:
                    label.BackColor = DangerColor;
                    label.ForeColor = Color.White;
                    break;
                default:
                    label.BackColor = Color.Transparent;
                    label.ForeColor = MutedText;
                    break;
            }
        }
    }

    public enum StatusType
    {
        Info,
        Success,
        Warning,
        Danger
    }
}