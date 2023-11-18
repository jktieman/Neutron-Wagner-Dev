using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using ExcelManager;


namespace NeutronTestForm
{
    public partial class FrmExcelTesting : Form
    {
        public FrmExcelTesting()
        {
            InitializeComponent();
        }

        private void ButtonReadFile_Click(object sender, EventArgs e)
        {
            var excelService = new ExcelManager.ExcelService();
            excelService.ReadData2();

        }

        private void ButtonWriteFile_Click(object sender, EventArgs e)
        {
            var excelService = new ExcelManager.ExcelService();
            //var dataTable = ExportToExcel();  // GetDataTable(DataGridViewUsers);
            var dataTable = ToDataTable(Students);
            excelService.Generate(dataTable);
            //excelService.WriteData(dataTable);

            //var writeExcelService = new ExcelManager.ExcelService(TextBoxFilePath.Text, 1);
            //writeExcelService.WriteCell(0, 0, "Test");
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public void OpenFile()
        {
            // var excel = new ExcelService(@"C:\Neutron\Excel\ExcelTest.xlsx", 1);
            // MessageBox.Show(excel.ReadCell(0, 0));
        }

        private void FrmExcelTesting_Load(object sender, EventArgs e)
        {
            // OpenFile();
            DataGridViewUsers.DataSource = ExportToExcel();
            //var dt = GetDataTable(DataGridViewUsers);
        }

        private List<User> GetUsers()
        {
            var users = new List<User>();
            users.Add(new User { Id = 1, FirstName = "John", LastName = "Doe", Age = 30 });
            users.Add(new User { Id = 2, FirstName = "Jane", LastName = "Doe", Age = 25 });
            users.Add(new User { Id = 3, FirstName = "John", LastName = "Smith", Age = 40 });
            users.Add(new User { Id = 4, FirstName = "Jane", LastName = "Smith", Age = 35 });
            return users;
        }

        private DataTable GetDataTable(DataGridView dataGridView)
        {
            DataTable dt = new DataTable();
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                dt.Columns.Add(column.Name, column.ValueType);
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < dataGridView.Columns.Count; i++)
                    {
                        dr[i] = row.Cells[i].Value;
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        public DataTable ExportToExcel()
        {
            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Sex", typeof(string));
            table.Columns.Add("Subject1", typeof(int));
            table.Columns.Add("Subject2", typeof(int));
            table.Columns.Add("Subject3", typeof(int));
            table.Columns.Add("Subject4", typeof(int));
            table.Columns.Add("Subject5", typeof(int));
            table.Columns.Add("Subject6", typeof(int));
            table.Rows.Add(1, "Amar", "M", 78, 59, 72, 95, 83, 77);
            table.Rows.Add(2, "Mohit", "M", 76, 65, 85, 87, 72, 90);
            table.Rows.Add(3, "Garima", "F", 77, 73, 83, 64, 86, 63);
            table.Rows.Add(4, "jyoti", "F", 55, 77, 85, 69, 70, 86);
            table.Rows.Add(5, "Avinash", "M", 87, 73, 69, 75, 67, 81);
            table.Rows.Add(6, "Devesh", "M", 92, 87, 78, 73, 75, 72);
            return table;
        }

        public class Student
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Sex { get; set; }
            public int Subject1 { get; set; }
            public int Subject2 { get; set; }
            public int Subject3 { get; set; }
            public int Subject4 { get; set; }
            public int Subject5 { get; set; }
            public int Subject6 { get; set; }

            public Student(int id, string name, string sex, int subject1, int subject2, int subject3, int subject4, int subject5, int subject6 )
            {
                Id = id;
                Name = name;
                Sex = sex;
                Subject1 = subject1;
                Subject2 = subject2;
                Subject3 = subject3;
                Subject4 = subject4;
                Subject5 = subject5;
                Subject6 = subject6;
            }
        }

        public List<Student> Students => new List<Student>()
        {
            new Student(1, "Amar", "M", 78, 59, 72, 95, 83, 77),
            new Student(2, "Mohit", "M", 76, 65, 85, 87, 72, 90),
            new Student(3, "Garima", "F", 77, 73, 83, 64, 86, 63),
            new Student(4, "jyoti", "F", 55, 77, 85, 69, 70, 86),
            new Student(5, "Avinash", "M", 87, 73, 69, 75, 67, 81),
            new Student(6, "Devesh", "M", 92, 87, 78, 73, 75, 72)

        };
    }
}
