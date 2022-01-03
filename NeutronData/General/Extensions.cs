using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public static bool CheckConnection(this DbContext context)
        {
            try
            {
                context.Database.Connection.Open();
                context.Database.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The connection to the database has failed.{Environment.NewLine}" +
                                $"{ex.Message}{Environment.NewLine}" +
                                $"Server: {context.Database.Connection.DataSource}{Environment.NewLine}" +
                                $"Database: {context.Database.Connection.Database}{Environment.NewLine}" +
                                $"Connection String: {context.Database.Connection.ConnectionString}{Environment.NewLine}", "Database Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }
    }
}
