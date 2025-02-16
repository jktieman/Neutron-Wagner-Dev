using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;


namespace NeutronData.Repositories;
public class PrintJobRepository : IPrintJobRepository
{
    private readonly Func<NeutronDb> _contextFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheOptions _cacheOptions;

    public PrintJobRepository(Func<NeutronDb> contextFactory, IMemoryCache memoryCache, IOptions<MemoryCacheOptions> cacheOptions)
    {
        _contextFactory = contextFactory;
        _memoryCache = memoryCache;
        _cacheOptions = cacheOptions.Value;
        PreloadCache();
    }

    public void PreloadCache()
    {

        using (var context = _contextFactory())
        {
            var printJobs = context.PrintJobs.ToList();

            // Cache entry options
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            foreach (var printJob in printJobs)
            {
                _memoryCache.Set(printJob.Id, printJob, cacheEntryOptions);
            }

        }
    }

    public PrintJob GetPrintJob(int orderId)
    {
        using (var context = _contextFactory())
        {
            if (!_memoryCache.TryGetValue(orderId, out PrintJob cacheData))
            {
                var sqlParameter = new SqlParameter("@ORDERID", orderId);
                var printJob = context.Database.SqlQuery<PrintJob>("EXECUTE dbo.usp_GetPrintJob @ORDERID", sqlParameter).FirstOrDefault();

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                    SlidingExpiration = TimeSpan.FromMinutes(20),
                    Size = 1
                };
                return printJob;
            }

            return cacheData;
        }
    }

    public void Insert(PrintJob printJob)
    {
        using (var context = _contextFactory())
        {
            // Perform database operations
            // Insert PrintJob into database
            context.PrintJobs.Add(printJob);
            context.SaveChanges();
        }
    }

    public void Save()
    {
        using (var context = _contextFactory())
        {
            // Perform database operations
            context.SaveChanges();
        }
    }

    public void Update(PrintJob printJob)
    {
        using (var context = _contextFactory())
        {
            // Perform database operations
            var existingPrintJob = context.PrintJobs.FirstOrDefault(pj => pj.Id == printJob.Id);
            if (existingPrintJob != null)
            {
                // insert new PrintJob record
                context.PrintJobs.AddOrUpdate(printJob);
                context.SaveChanges();
            }
        }
    }
}

