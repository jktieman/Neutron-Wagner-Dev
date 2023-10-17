using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using Logger = NeutronCore.Global.Logger;

namespace NeutronData.General
{
    public class EnumManager : IEnumManager
    {
        private readonly IDynamicLogger _logger;

        public EnumManager()
        {
            _logger = Logger.SetupLogger(@"EnumManager");
        }
        public void SaveActionCodesToDatabase()
        {
            //Run this one time at startup
            //break down the ActionCode Enum into a List and save to the database.
            _logger.LogDetailAsync("Save Action Codes to Database Started");

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
               _logger.LogDetailAsync($"Save Action Codes to Database Failed.  {Environment.NewLine} {ex.Message}{Environment.NewLine}" +
                                $"{ex.InnerException}{Environment.NewLine} {ex.StackTrace}");
            }
        }

        public void SaveLineStatusToDatabase()
        {
            _logger.LogDetailAsync("Save Line Status to Database Started");

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
                _logger.LogDetailAsync($@"Save Line Status to Database Failed.  {Environment.NewLine} {ex.Message}{Environment.NewLine}" +
                                       $"{ex.InnerException}{Environment.NewLine} {ex.StackTrace}");
            }
        }

        //public void SaveLocationTypesToDatabase()
        //{
        //    _logger.LogDetailAsync("Save Location Types Enum to Database Started");

        //    //Run this one time at startup
        //    //break down the LocationType Enum into a List and save to the database.
        //    var recs = ((LocationTypeEnum[])Enum.GetValues(typeof(LocationTypeEnum)))
        //        .Select(r => new LocationType { Id = (int)r, Name = r.GetEnumDescription() }).ToList();
        //    try
        //    {
        //        using (var db = new NeutronDb())
        //        {
        //            var exists = db.Database
        //                .SqlQuery<int?>(@"
        //                 SELECT 1 FROM sys.tables AS T
        //                 INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
        //                 WHERE S.Name = 'dbo' AND T.Name = 'LocationTypes'")
        //                .SingleOrDefault() != null;

        //            if (!exists)
        //            {
        //                db.Database.ExecuteSqlCommand("CREATE TABLE [dbo].[LocationTypes] ([Id] [int] NOT NULL, Name varchar(64) not null)");
        //            }

        //            foreach (var rec in recs)
        //            {
        //                var code = db.LocationTypes.Find(rec.Id);
        //                if (code == null)
        //                {
        //                    db.LocationTypes.AddOrUpdate(rec);
        //                }
        //                else
        //                {
        //                    code.Name = rec.Name;
        //                    db.LocationTypes.AddOrUpdate(code);
        //                }
        //            }
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($@"Save LocationTypeEnum to Database Failed.  {Environment.NewLine} {ex.Message}{Environment.NewLine}" +
        //                               $"{ex.InnerException}{Environment.NewLine} {ex.StackTrace}");
        //    }

        //}
    }
}
