using System.Collections.Generic;
using System.Windows.Forms;

namespace NeutronData.General
{
    public static class Extensions
    {
        public static List<T> ToList<T>(this DataGridViewSelectedRowCollection rows)
        {
            var list = new List<T>();
            for (int i = 0; i < rows.Count; i++)
            {
                list.Add((T) rows[i].DataBoundItem);
            }
            return list;
        }
    }
}
