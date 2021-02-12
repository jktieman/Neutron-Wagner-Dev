using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NeutronData.DataContexts;
using NeutronData.Models.Lookups;

namespace Neutron.Global
{
    public static class LineStatusManager
    {
        private static bool _inited = false;

        static LineStatusManager()
        {
            if (_inited) return;
            SaveLineStatusToDatabase();
        }

        public static void SaveLineStatusToDatabase()
        {
            if (_inited) return;
            //Run this one time at startup
            //break down the ActionCode Enum into a List and save to the database.
            //TODO Change LineStatus into an ENUM

            var linestats = new List<LineStatus>()
            {
                new LineStatus() {Id = 1, Name = "Available", Sequence = 10},
                new LineStatus() {Id = 2, Name = "Hold", Sequence = 20},
                new LineStatus() {Id = 3, Name = "Picking", Sequence = 30},
                new LineStatus() {Id = 4, Name = "Partial", Sequence = 40},
                new LineStatus() {Id = 5, Name = "Deleted", Sequence = 50},
                new LineStatus() {Id = 6, Name = "Complete", Sequence = 60},
                new LineStatus() {Id = 7, Name = "Returned", Sequence = 70},
                new LineStatus() {Id = 8, Name = "Archive", Sequence = 80},
                new LineStatus() {Id = 9, Name = "Skipped", Sequence = 90}
            };

            try
            {
                using (var db = new NeutronDb())
                {
                    //var exists = db.Database
                    //                 .SqlQuery<int?>(@"
                    //     SELECT 1 FROM sys.tables AS T
                    //     INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
                    //     WHERE S.Name = 'dbo' AND T.Name = 'LineStatus'")
                    //                 .SingleOrDefault() != null;

                    //if (!exists)
                    //{
                    //    db.Database.ExecuteSqlCommand(
                    //        "CREATE TABLE [dbo].[LineStatus] ([Id] [int] NOT NULL, Name varchar(64) not null, [Sequence] [int] not null)");
                    //}
                    //else
                    //{
                    //    db.Database.ExecuteSqlCommand("TRUNCATE TABLE LineStatus");
                    //}

                    foreach (var stat in linestats)
                    {
                        var rec = db.LineStatus.Find(stat.Id);
                        if (rec != null)
                        {
                            if (rec.Name == stat.Name) continue;
                            rec.Name = stat.Name;
                            rec.Sequence = stat.Sequence;
                            db.SaveChanges();
                        }
                        else
                        {
                            db.LineStatus.Add(stat);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save Line Status Codes to Database Failed.  {Environment.NewLine} {ex.Message}{Environment.NewLine}" +
                    $"{ex.InnerException}{Environment.NewLine} {ex.StackTrace}");
            }

            //set static variable to show the action codes have been created.
            _inited = true;
        }
    }
}