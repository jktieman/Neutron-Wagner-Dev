using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronCore.Global;

namespace SqlSchemaManager
{
    public class StoredProcedureManager : IStoredProcedureManager
    {
        private SqlConnection _connection;
        private List<string> _storedProcedureNames = new List<string>(){ "usp_GetLocationViewsByAreaAndSlot" };
        private static readonly string StoredProceduresPath = $"SqlSchemaManager.SqlScripts.StoredProcedures";
        private readonly IDynamicLogger _logger;

        public StoredProcedureManager()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("StoredProcedureManager");
        }

        public SqlConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public void Execute()
        {
            //_storedProcedureNames = GetStoredProcedureNames();
         _ = _logger.LogDetailAsync("Execute Start");
            foreach (var storedProcedureName in _storedProcedureNames)
            {


                var sqlScript = string.Empty;
                var assembly = Assembly.GetAssembly(typeof(StoredProcedureManager));
                var resourceName = $"{StoredProceduresPath}.{storedProcedureName}";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    sqlScript = reader.ReadToEnd();
                }

                var dropProcQuery =
                    $@"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID         (N'{storedProcedureName}') AND type in
                        (N'P', N'PC'))  DROP PROCEDURE  [dbo].[{storedProcedureName}]";

                var getObjectId = $@"select object_id from sys.procedures where name = '{storedProcedureName}'";

                try
                {
                    using (var cmd = new SqlCommand(dropProcQuery, _connection))
                    {
                        _connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = sqlScript;
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = getObjectId;
                        var objectId = cmd.ExecuteScalar();
                        _connection.Close();
                    }
                }
                catch (Exception ex)
                {
                 _ = _logger.LogDetailAsync($"Connection Test Failed {Environment.NewLine}{ex.Message}");
                    if (ex.InnerException != null)
                    {
                     _ = _logger.LogDetailAsync($"{ex.Message}");
                     _ = _logger.LogDetailAsync(
                            $"Connection Test Failed Inner Exception {Environment.NewLine}{ex.InnerException.Message}");
                    }
                }
            }
        }

        private List<string> GetStoredProcedureNames()
        {
         _ = _logger.LogDetailAsync("GetStoredProcedureNames Start");

            var fileNames = new List<string>();
            var sqlPath = typeof(StoredProcedureManager).Assembly.Location;

            var directoryInfo = new DirectoryInfo($"{sqlPath}\\{StoredProceduresPath}");
            var fileInfos = directoryInfo.GetFiles();
            foreach (var fileName in fileInfos)
            {
                fileNames.Add(fileName.Name);
            }

            return fileNames;
        }
    }
}
