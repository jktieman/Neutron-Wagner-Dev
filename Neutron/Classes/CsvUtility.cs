using NeutronCore.Extensions;
using Neutron.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Classes
{
    public static class CsvUtility
    {
        public static void SaveToCsv(DataGridView dgv)
        {

            string rootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Neutron\CSV\");
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }
            string filename = "";
            var sfd = new SaveFileDialog();
            sfd.InitialDirectory = rootDirectory;
            sfd.Filter = "CSV (*.csv)|*.csv";
            sfd.FileName = "Output.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Data will be exported and you will be notified when it is ready.");
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                    }
                }
                int columnCount = dgv.ColumnCount;
                string columnNames = "";
                int startColumn = 0;
                var output = new string[dgv.RowCount + 1];
                for (int i = 0; i < columnCount; i++)
                {
                    string colName = dgv.Columns[i].HeaderText.ToString().FromCamelCase();
                    if (string.IsNullOrEmpty(colName) && i == 0)
                    {
                        startColumn = 1;
                    }
                    else
                    {
                        columnNames += colName + ",";
                    }

                }
                output[0] += columnNames;
                for (int i = 1; (i - 1) < dgv.RowCount; i++)
                {
                    for (int j = startColumn; j < columnCount; j++)
                    {
                        string result = dgv.Rows[i - 1].Cells[j].Value == null ? string.Empty : dgv.Rows[i - 1].Cells[j].Value.ToString();
                        output[i] += result + ",";
                    }
                }
                System.IO.File.WriteAllLines(sfd.FileName, output, System.Text.Encoding.UTF8);
                MessageBox.Show("Your file was generated and its ready for use.");
            }
        }
    }
}
