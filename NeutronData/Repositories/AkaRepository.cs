using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using AlliedLogger;

namespace NeutronData.Repositories
{
    public class AkaRepository : IAkaRepository
    {
        private readonly NeutronDb _context = new NeutronDb();
        private readonly IDynamicLogger _logger;

        public AkaRepository()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger(@"AKARepository");
        }
        
        
        /// <summary>
        /// Takes an AKA and returns the Item or an Empty String 
        /// </summary>
        /// <param name="aka">AKA String</param>
        /// <returns>Item or an Empty String</returns>
        public string Get(string aka)
        {
            _ = _logger.LogDetailAsync($@"AKA Get: {aka}");
            var item = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(aka))
                {
                    var rec = _context.AkaTypes.Find(aka);
                    item = rec != null ? rec.Item : string.Empty;
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            return item;
        }

        /// <summary>
        /// Takes an Item and returns the FIRST AKA it finds
        /// If you need a UPC code, make sure it is the first AKA or an Empty String
        /// </summary>
        /// <param name="item"></param>
        /// <returns>first AKA or an Empty String</returns>
        public string GetUpc(string item)
        {
            _ = _logger.LogDetailAsync($@"AKA GetUPC: {item}");
            var upc = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var rec = _context.AkaTypes.FirstOrDefault(r => r.Item == item);
                    if (rec != null)
                    {
                        upc = rec.Aka;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            return upc;
        }

        /// <summary>
        /// Get a List of AKAs for an Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns>List of AKA's</returns>
        public List<string> GetAkas(string item)
        {
            _ = _logger.LogDetailAsync($@"AKA GetAKAs: {item}");
            var list = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var recs = _context.AkaTypes.Where(r => r.Item == item);
                    if (recs != null)
                    {
                        list.AddRange(recs.Select(rec => rec.Aka));
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            return list;
        }


        // Get an AKA record
        public AkaType GetAka(string aka)
        {
            _ = _logger.LogDetailAsync($@"AKA GetAka: {aka}");
            AkaType rec = null;
            try
            {
                if (!string.IsNullOrEmpty(aka))
                {
                    rec = _context.AkaTypes.FirstOrDefault(r => r.Aka == aka);
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            return rec;
        }

        

        public void Insert(AkaType aka)
        {
            _ = _logger.LogDetailAsync($@"AKA Insert Item: {aka.Item}  AKA: {aka.Aka}");   
            if (aka == null) return;
            try
            {
                _context.AkaTypes.Add(aka);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

       // delete aka
       public void Delete(AkaType aka)
       {
           _ = _logger.LogDetailAsync($@"AKA Delete: {aka}");
           try
           {
               var rec = _context.AkaTypes.Find(aka.Aka);
               if (rec != null)
               {
                   _context.AkaTypes.Remove(rec);
                   _context.SaveChanges();
               }
           }
           catch (Exception ex)
           {
               _ = _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

       public void Update(AkaType aka)
       {
           try
           {
               var rec = _context.AkaTypes.Find(aka.Aka);
               if (rec != null)
               {
                   rec.Item = aka.Item;
                   _context.SaveChanges();
               }
           }
           catch (Exception ex)
           {
               _ = _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

       public void Save()
       {
           try
           {
               _context.SaveChanges();
           }
           catch (Exception ex)
           {
               _ = _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

       public void Dispose()
       {
            _context?.Dispose();
       }
    }
}
