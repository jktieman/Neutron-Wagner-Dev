using System.Data.SqlClient;

namespace SqlSchemaManager
{
    public interface IStoredProcedureManager
    {
        SqlConnection Connection { get; set; }

        void Execute();
    }
}
