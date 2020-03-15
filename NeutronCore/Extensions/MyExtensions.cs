using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeutronCore.Extensions
{
    public static class MyExtensions
    {
        public static DataTable MakeDataTable<T>(this IList<T> data)
        {
            var props =
            TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            for (var i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            var values = new object[props.Count];
            foreach (var item in data)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static List<T> ToList<T>(this DataGridViewSelectedRowCollection rows)
        {
            //example usage: [List<Location> | var] locations = dataGridViewLocations.SelectedRows.ToList<Location>();
            var list = new List<T>();
            for (var i = 0; i < rows.Count; i++)
            {
                list.Add((T) rows[i].DataBoundItem);
            }
            return list;
        }

        public static void FocusAndHighlightText(this TextBox textBox)
        {
            textBox.Focus();
            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.Text.Length;
        }
    }
}
