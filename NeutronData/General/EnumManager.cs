using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Models.Lookups;

namespace NeutronData.General
{
    public class EnumManager : IEnumManager
    {
        public void SaveActionCodesToDatabase()
        {
            //Run this one time at startup
            //break down the ActionCode Enum into a List and save to the database.
            var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode)))
                .Select(r => new ActionCodeItem { Id = (int)r, Name = r.GetEnumDescription() }).ToList();
            try
            {
                using (var db = new NeutronDb())
                {
                    var exists = db.Database
                        .SqlQuery<int?>(@"
                         SELECT 1 FROM sys.tables AS T
                         INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
                         WHERE S.Name = 'dbo' AND T.Name = 'ActionCodeItems'")
                        .SingleOrDefault() != null;
                    if (!exists)
                    {
                        db.Database.ExecuteSqlCommand("CREATE TABLE [dbo].[ActionCodeItems] ([Id] [int] NOT NULL, Name varchar(64) not null)");
                    }

                    foreach (var actionCode in actionCodes)
                    {
                        var code = db.ActionCodeItems.Find(actionCode.Id);
                        if (code != null)
                        {
                            code.Name = actionCode.Name;
                            db.ActionCodeItems.AddOrUpdate(code);
                        }
                        else
                        {
                            db.ActionCodeItems.AddOrUpdate(actionCode);
                        }
                         
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save Action Codes to Database Failed.  {Environment.NewLine} {ex.Message}{Environment.NewLine}" +
                                $"{ex.InnerException}{Environment.NewLine} {ex.StackTrace}");
            }
        }

        public void SaveLineStatusToDatabase()
        {
            //Run this one time at startup
            //break down the ActionCode Enum into a List and save to the database.
            var recs = ((LineStatus[])Enum.GetValues(typeof(LineStatus)))
                .Select(r => new LineStatusLookup { Id = (int)r, Name = r.GetEnumDescription()}).ToList();
            try
            {
                using (var db = new NeutronDb())
                {
                    var exists = db.Database
                        .SqlQuery<int?>(@"
                         SELECT 1 FROM sys.tables AS T
                         INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
                         WHERE S.Name = 'dbo' AND T.Name = 'LineStatusLookup'")
                        .SingleOrDefault() != null;

                    if (!exists)
                    {
                        db.Database.ExecuteSqlCommand("CREATE TABLE [dbo].[LineStatusLookup] ([Id] [int] NOT NULL, Name varchar(64) not null)");
                    }

                    foreach (var rec in recs)
                    {
                        var code = db.LineStatusLookup.Find(rec.Id);
                        if (code == null)
                        {
                            db.LineStatusLookup.AddOrUpdate(rec);
                        }
                        else
                        {
                            code.Name = rec.Name;
                            db.LineStatusLookup.AddOrUpdate(code);
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save Line Status to Database Failed.  {Environment.NewLine} {ex.Message}{Environment.NewLine}" +
                                $"{ex.InnerException}{Environment.NewLine} {ex.StackTrace}");
            }
        }
    }
}
