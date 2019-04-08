using MetroFramework.Forms;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.BaseClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;

namespace Neutron.Forms
{
    public partial class FrmLookups : MetroForm
    {
        private List<LookupTable> _lookupTables = new List<LookupTable>();
        private string _currentTableName = string.Empty;
        private List<LookupData> _currentRecs;
        private BindingSource _bindingSource;
        public bool CloseButtonPressed { get; set; }

        public FrmLookups()
        {
            InitializeComponent();
            SetupGrid();
            CloseButtonPressed = false;
        }

        private void SetupGrid()
        {
            DataGridViewLookups.AutoGenerateColumns = false;
           // DataGridViewLookups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            int w = (DataGridViewLookups.Width - 60) / 4;

            DataGridViewColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Width = w;
            col.Name = "Id";
            col.Visible = false;
            DataGridViewLookups.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Sequence";
            col.HeaderText = "Sequence";
            col.Width = w;
            col.Name = "Sequence";
            DataGridViewLookups.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Name";
            col.HeaderText = "Name";
            col.Width = w * 3;
            col.Name = "Name";
            DataGridViewLookups.Columns.Add(col);
        }

        private int RefreshData(int recId = 0)
        {
            _currentTableName = ((LookupTable) ListBoxCodeNames.SelectedItem).TableName;
            LabelLookupName.Text = ((LookupTable) ListBoxCodeNames.SelectedItem).Name;
            _currentRecs = GetTableData(_currentTableName);
            _bindingSource = new BindingSource {DataSource = _currentRecs};
            DataGridViewLookups.DataSource = _bindingSource.DataSource; 

            var idx = 0;

            if (GetRecordCount() <= 0) return idx;
            if (recId != 0)
            {
                idx = IndexOf(recId);
            }
            DataGridViewLookups.FirstDisplayedScrollingRowIndex = DataGridViewLookups.Rows[idx].Index;
            DataGridViewLookups.Refresh();
            DataGridViewLookups.CurrentCell = DataGridViewLookups.Rows[idx].Cells[1];
            DataGridViewLookups.Rows[idx].Selected = true;
            return idx;
        }

        public int IndexOf(int value)
        {
            int count = DataGridViewLookups.RowCount; 
            int itemIndex = -1;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    int rec = ((ILookup) DataGridViewLookups.Rows[i].DataBoundItem).Id;
                    {
                        itemIndex = i;

                    }
                } 
            }
            return itemIndex;
        }

        private int GetRecordCount()
        {
            int count = DataGridViewLookups.RowCount;
            return count;
        }

        private void FrmLookups_Load(object sender, EventArgs e)
        {
            try
            {
                using (var context = new NeutronDb())
                {
                    _lookupTables = context.LookupTables.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Lookup Tables. " + ex.Message);
            }
            ListBoxCodeNames.DataSource = _lookupTables;
        }

        private List<LookupData> GetTableData(string tableName)
        {
            _currentTableName = tableName;
            var recs = new List<LookupData>();
            try
            {
                using (var context = new NeutronDb())
                {
                    switch (tableName)
                    {
                        case "SizeCodes":
                            recs = context.SizeCodes.Select (s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "VelocityCodes":
                            recs = context.VelocityCodes.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "HeightCodes":
                            recs = context.HeightCodes.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "LocationCodes":
                            recs = context.LocationCodes.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                       case "ShipMethods":
                            recs = context.ShipMethods.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "Shippers":
                            recs = context.Shippers.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "UnitOfIssues":
                            recs = context.UnitOfIssues.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Lookup Tables. " + ex.Message);
            }
            return recs;
        }

        private void ListBoxCodeNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            RefreshData();
        }

        private async void MButtonSave_Click(object sender, EventArgs e)
        {
            var s = _currentRecs;
            var t = DataGridViewLookups.DataSource;
            await UpdateTableData();
        }

        private async Task UpdateTableData()
        {
            try
            {
                using (var context = new NeutronDb())
                {
                    foreach (var rec in _currentRecs)
                    {
                        switch (_currentTableName)
                        {
                            case "SizeCodes":
                                var sizeCode = new SizeCode() {Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence};
                                context.SizeCodes.AddOrUpdate(sizeCode);
                                await context.SaveChangesAsync();
                                break;
                            case "VelocityCodes":
                                var velocityCode = new VelocityCode() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.VelocityCodes.AddOrUpdate(velocityCode);
                                await context.SaveChangesAsync();
                                break;
                            case "HeightCodes":
                                var heightCode = new HeightCode() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.HeightCodes.AddOrUpdate(heightCode);
                                await context.SaveChangesAsync();
                                break;
                            case "LocationCodes":
                                var locationCode = new LocationCode() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.LocationCodes.AddOrUpdate(locationCode);
                                await context.SaveChangesAsync();
                                break;
                            case "ShipMethods":
                                var shipMethod = new ShipMethod() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.ShipMethods.AddOrUpdate(shipMethod);
                                await context.SaveChangesAsync();
                                break;
                            case "Shippers":
                                var shipper = new Shipper() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.Shippers.AddOrUpdate(shipper);
                                await context.SaveChangesAsync();
                                break;
                            case "UnitOfIssues":
                                var item = new UnitOfIssue() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.UnitOfIssues.AddOrUpdate(item);
                                await context.SaveChangesAsync();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating Lookup Tables.  {ex.Message}");
            }
        }

        private void MBAddNew_Click(object sender, EventArgs e)
        {
            //var row = (DataGridViewRow) DataGridViewLookups.Rows[0].Clone();
            //row.Cells[0].Value = "";
            //row.Cells[1].Value = "100";
            //row.Cells[2].Value = "New";
           // DataGridViewLookups.Rows.Add(row);
            var r = new LookupData {Name = "", Sequence = 100};
            _bindingSource.Add(r);
            DataGridViewLookups.DataSource = null;
            DataGridViewLookups.DataSource = _bindingSource.DataSource;
            DataGridViewLookups.Update();
        }

        private void MButtonClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
            Close();
        }

        private void FrmLookups_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }
    }
}
