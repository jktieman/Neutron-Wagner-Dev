using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MetroFramework.Controls;
using Neutron.Models;
using NeutronData.Models;
using NeutronData.DataContexts;

namespace Neutron.UserControls
{
    public partial class UcItemDefinition : MetroUserControl
    {
        private List<RectangleControl> controls;
        private Size containerOrigSize;
        private List<ItemDefinition> itemDefinitions;
        private bool newRecord = false;
        //private GenericRepository<ItemDefinition> repo = new GenericRepository<ItemDefinition>(new NeutronDb());
        //private BindingListView<ItemDefinition> itemDefinitionsBindingSource;
        private NeutronDb db = new NeutronDb();

        private bool isLoading = true;

        public UcItemDefinition()
        {
            InitializeComponent();
            this.StyleManager = metroStyleManager1;
            metroStyleManager1.Theme = MetroFramework.MetroThemeStyle.Light;
            metroStyleManager1.Style = MetroFramework.MetroColorStyle.Green;
            controls = new List<RectangleControl>();
        }

        private void ResizeChildenControls()
        {
            foreach (var item in controls)
            {
                ResizeControl(item.Rectangle, item.Control);
            }
        }

        private void ResizeControl(Rectangle origRect, Control control)
        {
            float xRatio = (float) (this.Width) / (float) (containerOrigSize.Width);
            float yRatio = (float) (this.Height) / (float) (containerOrigSize.Height);
            var newX = (int) (origRect.X * xRatio);
            var newY = (int) (origRect.Y * yRatio);
            var newWidth = (int) (origRect.Width * xRatio);
            var newHeight = (int) (origRect.Height * yRatio);
            control.Location = new Point(newX, newY);
            control.Size = new Size(newWidth, newHeight);
        }

        private void mbSave_Click(object sender, EventArgs e)
        {
            ItemDefinition rec;
            if (newRecord == true)
            {
                rec = CreateNewRecord();
            }
            else
            {
                rec = UpdateRecord();
            }
            SaveRecord(rec);
            TabControlItemDefinitions.SelectedTab = TabPageList;
        }

        private void SaveRecord(ItemDefinition rec)
        {
            try
            {
                if (rec.Id <= 0)
                {
                    db.Database.Log = Console.WriteLine;
                    db.ItemDefinitions.Add(rec);
                }
                else
                {
                    db.Database.Log = Console.WriteLine;
                    db.ItemDefinitions.Attach(rec);
                    db.Entry(rec).State = EntityState.Modified;
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Item Definition SaveRecord Error: " + ex.Message);
            }
        }

        private ItemDefinition UpdateRecord()
        {
            var rec = new ItemDefinition()
            {
                Id = int.Parse(mtbId.Text),
                Item = mtbItem.Text,
                Description = mtbDescription.Text,
                UnitOfIssueId = int.Parse(mtbUnitOfIssue.Text),
                LocationMax = int.Parse(mtbLocationMax.Text),
                LocationMin = int.Parse(mtbLocationMin.Text),
                SizeCodeId = int.Parse(mtbSizeCode.Text),
                VelocityCodeId = int.Parse(mtbVelocityCode.Text),
                StorageTypeId = 2, // int.Parse(mtbStorageType.Text),
                Weight = float.Parse(mtbWeight.Text),
                Scale = bool.Parse(mcbScale.Checked.ToString()),
                SystemMax = int.Parse(mtbSystemMax.Text),
                SystemMin = int.Parse(mtbSystemMin.Text),
                HeightCodeId = int.Parse(mtbHeightCode.Text),
                StationId = int.Parse(mtbStation.Text)
            };
            return rec;
        }

        private ItemDefinition CreateNewRecord()
        {
            var rec = new ItemDefinition()
            {
                Item = mtbItemNew.Text,
                Description = mtbDescriptionNew.Text,
                UnitOfIssueId = int.Parse(mtbUnitOfIssueNew.Text),
                LocationMax = int.Parse(mtbLocationMaxNew.Text),
                LocationMin = int.Parse(mtbLocationMinNew.Text),
                SizeCodeId = int.Parse(mtbSizeCodeNew.Text),
                VelocityCodeId = int.Parse(mtbVelocityCodeNew.Text),
                StorageTypeId = 2, // int.Parse(mtbStorageType.Text),
                Weight = float.Parse(mtbWeightNew.Text),
                Scale = bool.Parse(mcbScaleNew.Checked.ToString()),
                SystemMax = int.Parse(mtbSystemMaxNew.Text),
                SystemMin = int.Parse(mtbSystemMinNew.Text),
                HeightCodeId = int.Parse(mtbHeightCodeNew.Text),
                StationId = int.Parse(mtbStationNew.Text)
            };
            return rec;
        }

        private void mbNew_Click(object sender, EventArgs e)
        {
            newRecord = true;
            TabControlItemDefinitions.SelectedTab = TabPageNew;
        }



        private void ClearFields()
        {
            mtbItem.Text = string.Empty;
            mtbDescription.Text = string.Empty;
            mtbUnitOfIssue.Text = string.Empty;
            mtbLocationMax.Text = "0";
            mtbLocationMin.Text = "0";
            mtbSizeCode.Text = "0";
            mtbVelocityCode.Text = "0";
            mtbStorageType.Text = "0";
            mtbWeight.Text = "0";
            mcbScale.Checked = false;
            mtbSystemMax.Text = "0";
            mtbSystemMin.Text = "0";
            mtbHeightCode.Text = "0";
            mtbStation.Text = "1";

        }

        private void mbFind_Click(object sender, EventArgs e)
        {
            FindRecord(mtbFind.Text.Trim());
        }

        private void FindRecord(string s)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    db.Database.Log = Console.Write;
                    BindingSourceItemDefinition.DataSource = db.ItemDefinitions.ToList();
                }
                else
                {
                    db.Database.Log = Console.Write;
                    BindingSourceItemDefinition.DataSource = db.ItemDefinitions.Where(d => d.Item.Contains(s) || d.Description.Contains(s)).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("UcItemDefinition_Find Error: " + ex.Message);
            }
        }



        //private void ShowDetail()
        //{
        //     mbViewEdit.Visible = false;
        //    mbNew.Visible = true;
        //    mbSave.Visible = true;
        //    mbDelete.Visible = true;
        //    mbFind.Text = "Back";

        //}

        //private void ShowGrid()
        //{
        //    mbViewEdit.Visible = true;
        //    mbNew.Visible = false;
        //    mbSave.Visible = false;
        //    mbDelete.Visible = false;
        //    mbFind.Text = "Find";

        //}

        //private void ShowDetailNew()
        //{
        //    mbViewEdit.Visible = false;
        //    mbNew.Visible = true;
        //    mbSave.Visible = true;
        //    mbDelete.Visible = true;
        //    mbFind.Text = "Back";
        //}

        private void mtbFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                mbFind_Click(sender, e);
            }
        }

        private void ButtonClearFind_Click(object sender, EventArgs e)
        {
            mtbFind.Text = "";
            mtbFind.Focus();
        }

        private void mbViewEdit_Click(object sender, EventArgs e)
        {
            TabControlItemDefinitions.SelectedTab = TabPageViewEdit;
        }

        private void mbDelete_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Yet");
            TabControlItemDefinitions.SelectedTab = TabPageList;
        }

        private void UcItemDefinition_Load(object sender, EventArgs e)
        {
            isLoading = true;
            TabControlItemDefinitions.SelectedTab = TabPageList;
            try
            {
                db.Database.Log = Console.Write;
                BindingSourceItemDefinition.DataSource = db.ItemDefinitions.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("UcItemDefinition_Load Error: " + ex.Message);
            }

            containerOrigSize = this.Size;

            foreach (Control control in this.Controls)
            {
                var rectangle = new Rectangle(control.Location.X, control.Location.Y, control.Width, control.Height);
                var rc = new RectangleControl(rectangle, control);
                controls.Add(rc);
            }
            isLoading = false;
        }

        private void UcItemDefinition_Resize(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                ResizeChildenControls();
            }
        }

        private void MetroGridItemDefinition_CellClick(object sender, DataGridViewCellEventArgs e)
        {


            //int count = ((MetroGrid) sender).SelectedRows.Count;

            //if (count == 1)
            //{
            //    DataGridViewRow selectedRow = MetroGridItemDefinition.CurrentRow;

            //    mtbId.Text = selectedRow.Cells[0].Value.ToString();
            //    mtbStation.Text = selectedRow.Cells[1].Value.ToString();
            //    mtbItem.Text = selectedRow.Cells[2].Value.ToString();
            //    mtbDescription.Text = selectedRow.Cells[3].Value.ToString();
            //    mtbUnitOfIssue.Text = selectedRow.Cells[4].Value.ToString();
            //    mtbSizeCode.Text = selectedRow.Cells[5].Value.ToString();
            //    mtbVelocityCode.Text = selectedRow.Cells[6].Value.ToString();
            //    mtbHeightCode.Text = selectedRow.Cells[7].Value.ToString();
            //    mtbLocationMax.Text = selectedRow.Cells[8].Value.ToString();
            //    mtbLocationMin.Text = selectedRow.Cells[9].Value.ToString();
            //    mtbSystemMax.Text = selectedRow.Cells[10].Value.ToString();
            //    mtbSystemMin.Text = selectedRow.Cells[11].Value.ToString();
            //    mtbStorageType.Text = selectedRow.Cells[12].Value.ToString();
            //    mtbWeight.Text = selectedRow.Cells[13].Value.ToString();
            //    mcbScale.Checked = bool.Parse(selectedRow.Cells[14].Value.ToString());
            //}
            //else
            //{
            //    mtbStation.Text = string.Empty;
            //    mtbItem.Visible = false;
            //    mtbDescription.Visible = false;
            //    mtbUnitOfIssue.Text = string.Empty;
            //    mtbLocationMax.Text = "0";
            //    mtbLocationMin.Text = "0";
            //    mtbSizeCode.Text = "0";
            //    mtbVelocityCode.Text = "0";
            //    mtbStorageType.Text = "0";
            //    mtbWeight.Text = "0";
            //    mcbScale.Checked = false;
            //    mtbSystemMax.Text = "0";
            //    mtbSystemMin.Text = "0";
            //    mtbHeightCode.Text = "0";

            //}

        }
    }
}
