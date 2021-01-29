using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NeutronCore.Extensions
{
    public static class DataGridViewExtensions
    {
        /// <summary>
        /// Provides very fast and basic column sizing for large data sets.
        /// </summary>
        public static void FastAutoSizeColumns(this DataGridView targetGrid)
        {
            // We need to iterate through all the data in the grid and a DataTable supports enumeration.
            var gridTable = targetGrid.Table();
            // Create a graphics object from the target grid. Used for measuring text size.
            using (var gfx = targetGrid.CreateGraphics())
            {
                // Iterate through the columns.
                for (var ii = 0; ii < gridTable.Columns.Count; ii++)
                {
                    // Leverage Linq enumerator to rapidly collect all the rows into a string array, making sure to exclude null values.
                    var i = ii;
                    var colStringCollection = gridTable.AsEnumerable()
                        .Where(r => r.Field<object>(i) != null)
                        .Select(r => r.Field<object>(i).ToString()).ToArray();

                    // Sort the string array by string lengths.
                        colStringCollection = colStringCollection.OrderBy((x) => x.Length).ToArray();

                        // Get the last and longest string in the array.
                        var longestColString = colStringCollection.Last();

                        // Use the graphics object to measure the string size.
                        var colWidth = gfx.MeasureString(longestColString, targetGrid.Font);

                    var headerText = targetGrid.Columns[i].HeaderText;
                    var font = targetGrid.Columns[i].HeaderCell.Style.Font;

                    //     targetGrid.Columns[i].HeaderCell.Style.Font
                    var headerWidth = gfx.MeasureString(headerText, font);

                    if (colWidth.Width > headerWidth.Width)
                    {
                        targetGrid.Columns[i].Width = (int)colWidth.Width + 10;
                    }
                    else // Otherwise, set the column width to the header width.
                    {
                        targetGrid.Columns[i].Width = (int)headerWidth.Width + 10;
                    }

                    // If the calculated width is larger than the column header width, set the new column width.
                    //if (colWidth.Width > targetGrid.Columns[i].HeaderCell.Size.Width)
                    //{
                    //    targetGrid.Columns[i].Width = (int)colWidth.Width;
                    //}
                    //else // Otherwise, set the column width to the header width.
                    //{
                    //    targetGrid.Columns[i].Width = targetGrid.Columns[i].HeaderCell.Size.Width;
                    //}
                }
            }
        }

        public static DataTable Table(this DataGridView grid)
        {
            var dataTable = new DataTable();
            foreach (DataGridViewColumn col in grid.Columns)
            {
                dataTable.Columns.Add(col.HeaderText, typeof(string));
            }

            foreach (DataGridViewRow row in grid.Rows)
            {
                var dr = dataTable.NewRow();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    dr[cell.ColumnIndex] = cell.Value;
                }
                dataTable.Rows.Add(dr);
            }
            return dataTable;
        }
    }
}
