using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataTable = System.Data.DataTable;
using System.Text.RegularExpressions;
using AlliedLogger;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using NeutronEvents;
//using Microsoft.Office.Interop.Excel;

namespace ExcelManager;

public class ExcelService
{
    private string _filePath;
    private readonly IDynamicLogger _logger;

    public ExcelService()
    {
        _logger = NeutronCore.Global.Logger.SetupLogger("ExcelService");
    }
    private DataTable GetDataTable(DataGridView grid)
    {
        //var grid = new DataGridView();
        //grid.DataSource = bindingSource;

        var dt = new DataTable("Students");
        // Add two columns to the DataTable
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Age", typeof(int));

        // Add some rows to the DataTable
        dt.Rows.Add("Alice", 20);
        dt.Rows.Add("Bob", 19);
        dt.Rows.Add("Charlie", 21);
        return dt;


        //foreach (DataGridViewColumn column in grid.Columns)
        //{
        //    dt.Columns.Add(column.Name, typeof(string));
        //    foreach (DataGridViewRow row in grid.Rows)
        //    {
        //        DataRow dr = dt.NewRow();
        //        for (int i = 0; i < grid.Columns.Count; i++)
        //        {
        //            dr[i] = row.Cells[i].Value;
        //        }
        //        dt.Rows.Add(dr);
        //    }
        //}
        //return dt;

    }
    /// <summary>
    /// Creates an Excel file using the data from the DataTable provided
    /// </summary>
    /// <param name="dataTable"></param>
    public void Generate(DataTable dataTable)
    {
        if (dataTable == null) return;
        var fileName = GetFileName(dataTable.TableName);
        // SaveFile() returns the path to a new file
        var file = SaveFile(fileName);
        // Check if the file path is empty
        if (string.IsNullOrWhiteSpace(file)) return;
        //GenerateExcel(file, dataTable);
        // Export the DataTable to Excel
        ExportDataTableToExcel(dataTable, file);
       // Task.Run(() => _logger.LogDetailAsync($"Excel file created: {file}");
    }

    private string GetFileName(string tableName)
    {
        var fileName = tableName;
        if (string.IsNullOrWhiteSpace(tableName)) { return string.Empty; }

        if (tableName.Contains("Inventory"))
        {
            fileName = "Inventory";
        }
        if (tableName.Contains("Location")) { fileName = "Location"; }
        if (tableName.Contains("ItemDefinition")) { fileName = "ItemDefinition"; }

        return fileName;
    }

    /// <summary>
    /// Takes a DataTable and exports it to an Excel file
    /// </summary>
    /// <param name="table"> A DataTable object</param>
    /// <param name="destination"> A file path</param>
    private void ExportDataTableToExcel(DataTable table, string destination)
    {
        try
        {
            using (var workbook = SpreadsheetDocument.Create(destination, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                //workbook.WorkbookPart.Workbook.Sheets = new Sheets();

                uint sheetId = 1;


                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                //Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetId, Name = table.TableName };

                var relationshipId = workbookPart.GetIdOfPart(worksheetPart);

                if (sheets.Elements<Sheet>().Any())
                {
                    sheetId =
                        sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                var sheet = new Sheet()
                {
                    Id = relationshipId,
                    SheetId = sheetId,
                    Name = table.TableName
                };
                sheets.Append(sheet);

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                var headerRow = new Row();

                var columns = new List<DataColumn>();
                foreach (DataColumn column in table.Columns)
                {

                    columns.Add(column);

                    var cell = new Cell();

                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(column.ColumnName);
                    headerRow.AppendChild(cell);
                }

                if (sheetData == null) return;
                {
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow dsrow in table.Rows)
                    {
                        var newRow = new Row();
                        foreach (var col in columns)
                        {
                            // test variable to see if it is a number
                            double test;
                            // if it is a number, add it to the cell as a number
                            var isNumber = double.TryParse(dsrow[col].ToString(), out test);

                            var cell = new Cell();
                            //cell.DataType = isNumber == false ? CellValues.String : CellValues.Number;
                            cell.DataType = col.DataType == typeof(int) ? CellValues.Number : CellValues.String;
                            cell.CellValue = new CellValue(dsrow[col].ToString());
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Mediator.GetInstance().OnGeneralError(this, $"Error Creating DataTable {Environment.NewLine}{ex.Message}");
        }
    }


    /// <summary>
    /// Takes a DataTable and exports it to an Excel file
    /// </summary>
    /// <param name="dataTable"> A DataTable object</param>
    /// <param name="destination"> A file path</param>
    private void ExportDataTableToExcel2(DataTable dataTable, string destination)
    {
        using (var document = SpreadsheetDocument.Create(destination, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Add column headers
            var headerRow = new Row();
            foreach (DataColumn column in dataTable.Columns)
            {
                var cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(column.ColumnName);
                headerRow.AppendChild(cell);
            }
            sheetData.AppendChild(headerRow);

            // Add rows
            foreach (DataRow row in dataTable.Rows)
            {
                var dataRow = new Row();
                foreach (DataColumn column in dataTable.Columns)
                {
                    var cell = new Cell();
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(row[column].ToString());
                    dataRow.AppendChild(cell);
                }
                sheetData.AppendChild(dataRow);
            }
        }

    }
    private void SetFilePath()
    {
        try
        {
            // Open file dialog
            var openFileDialog = new OpenFileDialog
            {
                Filter = @"Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = false
            };
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(openFileDialog.FileName)) return;
            _filePath = openFileDialog.FileName;
        }
        catch (Exception ex)
        {
            Mediator.GetInstance().OnGeneralError(this, $"Error Opening File {Environment.NewLine}{ex.Message}");
        }
    }

    public string SaveFile(string fileName)
    {
        try
        {
            // save file dialog
            var saveFileDialog = new SaveFileDialog
            {
                Filter = @"Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                OverwritePrompt = true,
                FileName = $"{fileName}.xlsx"
            };
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.OK) return string.Empty;
            if (string.IsNullOrWhiteSpace(saveFileDialog.FileName)) return string.Empty;
            _filePath = saveFileDialog.FileName;
            // Delete the file if it exists.
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
        catch (Exception ex)
        {
            Mediator.GetInstance().OnGeneralError(this, $"Error in Save File Dialog. {Environment.NewLine}{ex.Message}");
        }
        return _filePath;
    }
    public DataTable ReadData()
    {
        var dataTable = new DataTable();

        using (var spreadsheetDocument = SpreadsheetDocument.Open(_filePath, false))
        {
            var workbookPart = spreadsheetDocument.WorkbookPart;
            if (workbookPart == null) return dataTable;
            var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
            if (sheet == null) return dataTable;
            if (sheet.Id == null) return dataTable;
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

            foreach (var row in sheetData.Elements<Row>())
            {
                if (row.RowIndex == 1)
                {
                    //foreach (Cell cell in row.Elements<Cell>())
                    foreach (var cell in row.Elements<Cell>())
                    {
                        var columnName = GetColumnHeading(_filePath, sheet.Name, cell.CellReference);
                        dataTable.Columns.Add(columnName);
                    }
                }
                else
                {
                    var dataRow = dataTable.NewRow();
                    for (var i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        dataRow[i] = row.Descendants<Cell>().ElementAt(i).CellValue?.Text;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }
        }
        return dataTable;
    }
    public DataTable ReadData2()
    {
        var dataTable = new DataTable();

        try
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Open(_filePath, false))
            {

                var sharedStringTable = spreadsheetDocument.WorkbookPart.SharedStringTablePart.SharedStringTable;
                var workbookPart = spreadsheetDocument.WorkbookPart;
                if (workbookPart == null) return dataTable;
                var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                if (sheet == null) return dataTable;
                if (sheet.Id == null) return dataTable;
                var worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
                if (worksheetPart == null) return dataTable;

                var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                foreach (var row in sheetData.Elements<Row>())
                {
                    if (row.RowIndex == 1)
                    {
                        //foreach (Cell cell in row.Elements<Cell>())
                        foreach (var cell in row.Elements<Cell>())
                        {
                            var columnName = GetColumnHeading(_filePath, sheet.Name, cell.CellReference);
                            dataTable.Columns.Add(columnName);
                        }
                    }
                    else
                    {
                        var dataRow = dataTable.NewRow();

                        for (var i = 0; i < row.Descendants<Cell>().Count(); i++)
                        {
                            var cell = row.Descendants<Cell>().ElementAt(i);
                            dataRow[i] = GetCellText(cell, sharedStringTable);

                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }

            }
        }
        catch (Exception)
        {
            //MessageBox.Show($@"Error opening file. {Environment.NewLine} {ex.Message}");
            //Task.Run(() =>  _logger.LogDetailAsync($"Error opening file. {Environment.NewLine} {ex.Message}");
        }
        return dataTable;
    }

    public string GetCellText(Cell cell, in SharedStringTable sst)
    {
        if (cell.CellValue is null)
            return string.Empty;

        if ((cell.DataType is not null) &&
            (cell.DataType == CellValues.SharedString))
        {
            var ssid = int.Parse(cell.CellValue.Text);
            return sst.ChildElements[ssid].InnerText;
        }

        return cell.CellValue.Text;
    }

    // Given a document name, a worksheet name, and a cell name, gets the column of the cell and returns
    // the content of the first cell in that column.
    public string GetColumnHeading(string docName, string worksheetName, string cellName)
    {
        if (cellName == null)
        {
            //Task.Run(() =>  _logger.LogDetailAsync("GetColumnHeading: cellName is null");
            return string.Empty;
        }
        if (worksheetName == null)
        {
           // Task.Run(() =>  _logger.LogDetailAsync("GetColumnHeading: worksheetName is null");
            return string.Empty;
        }
        if (docName == null)
        {

           // Task.Run(() =>  _logger.LogDetailAsync("GetColumnHeading: docName is null");
            return string.Empty;
        }

        // Open the document as read-only.
        using (var document = SpreadsheetDocument.Open(docName, false))
        {
            if (document.WorkbookPart != null)
            {
                var sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == worksheetName);

                if (!sheets.Any())
                {
                    // The specified worksheet does not exist.
                    //Task.Run(() =>  _logger.LogDetailAsync("The specified worksheet does not exist."));
                    return string.Empty;
                }

                var worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);

                // Get the column name for the specified cell.
                var columnName = GetColumnName1(cellName);

                // Get the cells in the specified column and order them by row.
                IEnumerable<Cell> cells = worksheetPart.Worksheet.Descendants<Cell>().Where(c => string.Compare(GetColumnName1(c.CellReference.Value), columnName, true) == 0)
                    .OrderBy(r => GetRowIndex(r.CellReference));

                if (cells.Count() == 0)
                {
                    // The specified column does not exist.
                    return null;
                }

                // Get the first cell in the column.
                var headCell = cells.First();

                // If the content of the first cell is stored as a shared string, get the text of the first cell
                // from the SharedStringTablePart and return it. Otherwise, return the string value of the cell.
                if (headCell.DataType != null && headCell.DataType.Value == CellValues.SharedString)
                {
                    var shareStringPart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    var items = shareStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                    return items[int.Parse(headCell.CellValue.Text)].InnerText;
                }
                else
                {
                    return headCell.CellValue.Text;
                }
            }
        }
        return string.Empty;
    }
    // Given a cell name, parses the specified cell to get the column name.
    private string GetColumnName1(string cellName)
    {
        // Create a regular expression to match the column name portion of the cell name.
        var regex = new Regex("[A-Za-z]+");
        var match = regex.Match(cellName);

        return match.Value;
    }

    // Given a cell name, parses the specified cell to get the row index.
    private uint GetRowIndex(string cellName)
    {
        // Create a regular expression to match the row index portion the cell name.
        var regex = new Regex(@"\d+");
        var match = regex.Match(cellName);

        return uint.Parse(match.Value);
    }
    public DataTable Update()
    {
        SetFilePath();
        // var dataTable = ReadData2();  //
        var dataTable = ReadExcelFile();                              // ReadExcelFile();
        return dataTable;

    }
    public DataTable ReadExcelFile()
    {
        var dt = new DataTable();
        var errorMessage = "File Open Error";
        try
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Open(_filePath, false))
            {

                var workbookPart = spreadsheetDocument.WorkbookPart;
                if (workbookPart == null) return dt;
                var sheets = workbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                var relationshipId = sheets.First().Id.Value;
                if (relationshipId == null) return dt;
                
                var worksheetPart = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(relationshipId);
                var worksheet = worksheetPart.Worksheet;
                var sheetData = worksheet.GetFirstChild<SheetData>();
                var rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(0))
                {
                    dt.Columns.Add(GetCellValue(spreadsheetDocument, cell));
                }

                foreach (var row in rows.Skip(1))
                {
                    var tempRow = dt.NewRow();
                    var columnIndex = 0;
                    foreach (var cell in row.Descendants<Cell>())
                    {
                        if (cell.CellReference == null) errorMessage = "File was not opened in Excel.  Be sure to edit the file before trying to load changes.";
                        var idx = GetColumnIndex(GetColumnName(cell.CellReference));

                        while (columnIndex < idx - 1)
                        {
                            tempRow[columnIndex++] = "";
                        }
                        //while (GetColumnIndex(GetColumnName(cell.CellReference)) > columnIndex)
                        //{
                        //    tempRow[columnIndex++] = "";
                        //}
                        tempRow[columnIndex++] = GetCellValue(spreadsheetDocument, cell);
                    }
                    dt.Rows.Add(tempRow);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDetailAsync($"{errorMessage}{Environment.NewLine}{ex.Message}");

            Mediator.GetInstance().OnGeneralError(this, $"{errorMessage}{Environment.NewLine}{ex.Message}");
        }
        return dt;
    }

    private static string GetCellValue(SpreadsheetDocument document, Cell cell)
    {
        var stringTablePart = document.WorkbookPart.SharedStringTablePart;
        var value = cell.CellValue.InnerXml;

        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
        }
        else
        {
            return value;
        }
    }

    private static int GetColumnIndex(string columnName)
    {
        var index = 0;
        var mulitplier = 1;

        foreach (var c in columnName.ToUpper().Reverse())
        {
            index += mulitplier * ((int)c - 64);
            mulitplier *= 26;
        }

        return index;
    }

    private static string GetColumnName(string cellReference)
    {
        var regex = new Regex("[A-Za-z]+");
        var match = regex.Match(cellReference);
        return match.Value;
    }

}