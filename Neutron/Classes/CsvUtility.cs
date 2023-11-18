using NeutronCore.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronEvents;

namespace Neutron.Classes
{
    public static class CsvUtility
    {
        private static readonly GenericRepository<Inventory> RepoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private static readonly char quote = '"';
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
                File.WriteAllLines(sfd.FileName, output, Encoding.UTF8);
                MessageBox.Show("Your file was generated and its ready for use.");
            }
        }

        public static void SaveToCsv(string fileName, int areaId)
        {
            try
            {
                var inventory = RepoInventory.All().Where(r => r.AreaId == areaId).OrderBy(o => o.ItemDefinition.Item).ToList();
                const string columnNames = "sku" +
                                           ",Description" +
                                           ",Unit-Of_Issue" +
                                           ",CAROUSEL" +
                                           ",Bin" +
                                           ",Level" +
                                           ",PARTITION" +
                                           ",QUANTITY" +
                                           ",RCV-DATE" +
                                           ",LAST C/C" +
                                           ",Sku-Type" +
                                           ",Scale" +
                                           ",Velocity-Class" +
                                           ",Size-Class" +
                                           ",Height-Class" +
                                           ",C/C-Class" +
                                           ",Sel-Class" +
                                           ",Segment rcvg" +
                                           ",O/C Trigger" +
                                           ",Weight" +
                                           ",Length" +
                                           ",Capacity" +
                                           ",Cube" +
                                           ",Trigger" +
                                           ",System Cap" +
                                           ",Sys Rep Trg" +
                                           ",Quarantined";

                var output = new List<string>();
                output.Add(columnNames);
                foreach (var inv in inventory)
                {
                    var sb = new StringBuilder();

                    sb.Append(inv.ItemDefinition.Item + ",");
                    sb.Append($"{quote}{inv.ItemDefinition.Description}{quote},");
                    sb.Append(inv.ItemDefinition.UnitOfIssue.Name + ",");
                    sb.Append(inv.Location.Loc1.ToString() + ",");
                    sb.Append(inv.Location.Loc2.ToString() + ",");
                    sb.Append(inv.Location.Loc3.ToString() + ",");
                    sb.Append(inv.Location.Loc4.ToString() + ",");
                    sb.Append(inv.Quantity.ToString() + ",");
                    sb.Append(
                        $@"{inv.ReceivedDate.Month.ToString().PadLeft(2, '0')}-{inv.ReceivedDate.Day.ToString().PadLeft(2, '0')}-{inv.ReceivedDate.Year},");
                    sb.Append(string.Empty + ",");
                    sb.Append("Random,");
                    sb.Append("No" + ",");
                    sb.Append(inv.ItemDefinition.VelocityCode.Name + ",");
                    sb.Append(inv.ItemDefinition.SizeCode.Name + ",");
                    sb.Append(inv.ItemDefinition.HeightCode.Name + ",");
                    sb.Append(string.Empty + ",");
                    sb.Append(string.Empty + ",");
                    sb.Append(string.Empty + ",");
                    sb.Append(string.Empty + ",");
                    sb.Append(string.Empty + ",");
                    sb.Append(string.Empty + ",");
                    sb.Append(inv.ItemDefinition.LocationMax.ToString() + ",");
                    sb.Append(string.Empty + ",");
                    sb.Append(inv.ItemDefinition.LocationMin.ToString() + ",");
                    sb.Append(inv.ItemDefinition.SystemMax.ToString() + ",");
                    sb.Append(inv.ItemDefinition.SystemMin.ToString() + ",");
                    sb.Append("No");

                    output.Add(sb.ToString());

                }

                File.WriteAllLines(fileName, output, Encoding.UTF8);
                Mediator.GetInstance().OnInventoryFileCreated(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                var msg = ($"Create Inventory File Error {Environment.NewLine}" +
                           $"Exception: {ex.Message} {Environment.NewLine}" +
                           $"Inner Exception: {ex.InnerException}");
                Mediator.GetInstance().OnInventoryFileCreatedError(null, msg);
            }
        }


        private static string GetFileName()
        {
            var rootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Neutron\CSV\");
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }
            var sfd = new SaveFileDialog
            {
                InitialDirectory = rootDirectory,
                Filter = "CSV (*.csv)|*.csv",
                FileName = "Output.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return sfd.FileName;
            MessageBox.Show("Data will be exported and you will be notified when it is ready.");
            if (!File.Exists(sfd.FileName)) return sfd.FileName;
            try
            {
                File.Delete(sfd.FileName);
            }
            catch (IOException ex)
            {
                MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
            }

            return sfd.FileName;
        }
    }
}
