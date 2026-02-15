using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NeutronData.DataContexts;

namespace NeutronData.Repositories
{
    public class DeleteHelper
    {
        private readonly Func<NeutronDb> _contextFactory;

        public DeleteHelper(Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<DeleteResult> DeleteWithCheckAsync<T>(
            object keyValue,
            string tableName = null,
            string keyColumn = "Id") where T : class
        {
            tableName ??= $"{typeof(T).Name}s";

            using (var db = _contextFactory())
            {
                // Step 1: Run FK dependency check via stored procedure
                var deps = await db.Database.SqlQuery<DeleteDependencyResult>(
                    "EXEC usp_TestDeleteDependencies @TableName, @KeyColumn, @KeyValue",
                    new SqlParameter("@TableName", tableName),
                    new SqlParameter("@KeyColumn", keyColumn),
                    new SqlParameter("@KeyValue", keyValue)
                ).ToListAsync();

                // If any dependencies exist, block the delete
                if (deps.Any(d => d.DependentRowCount > 0))
                {
                    var blocking = deps
                        .Where(d => d.DependentRowCount > 0)
                        .Select(d => $"{d.ReferencingTable} has {d.DependentRowCount} related rows.");

                    return new DeleteResult
                    {
                        Success = false,
                        Message = "Delete blocked due to related data:\n" + string.Join("\n", blocking),
                        Dependencies = deps
                    };
                }

                // Step 2: Load the entity using EF
                var entity = await db.Set<T>().FindAsync(keyValue);

                if (entity == null)
                {
                    return new DeleteResult
                    {
                        Success = false,
                        Message = "Record not found.",
                        Dependencies = deps
                    };
                }

                // Step 3: Delete using EF
                db.Set<T>().Remove(entity);
                await db.SaveChangesAsync();

                return new DeleteResult
                {
                    Success = true,
                    Message = "Record deleted successfully.",
                    Dependencies = deps
                };
            }
        }

        // ------------------------------------------------------------
        // PRIVATE: FK dependency check (SP call)
        // ------------------------------------------------------------
        private async Task<List<DeleteDependencyResult>> CheckDeleteAsync(
            string tableName, string keyColumn, object keyValue)
        {
            using (var db = _contextFactory())
            {
                return await db.Database.SqlQuery<DeleteDependencyResult>(
                    "EXEC sp_TestDeleteDependencies @TableName, @KeyColumn, @KeyValue",
                    new SqlParameter("@TableName", tableName),
                    new SqlParameter("@KeyColumn", keyColumn),
                    new SqlParameter("@KeyValue", keyValue)
                ).ToListAsync();
            }
        }


        public class DeleteDependencyResult
        {
            
            public string FkName { get; set; }
            public string ReferencingTable { get; set; }
            public string ReferencingColumn { get; set; }
            public int DependentRowCount { get; set; }
            public string DeleteImpact { get; set; }
        }

        public class DeleteResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<DeleteDependencyResult> Dependencies { get; set; } = new();
        }

    }

}