using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using AlliedLogger;
using AsyncAwaitBestPractices;

namespace NeutronData.Repositories
{
    public class AkaRepository : IAkaRepository
    {
        private readonly NeutronDb _context = new NeutronDb();
        private IDynamicLogger _logger;

        public AkaRepository()
        {
            Init();
        }

        private void Init()
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
            _logger.LogDetailAsync($@"AKA Get: {aka}").SafeFireAndForget();
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
                _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
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
            _logger.LogDetailAsync($@"AKA GetUPC: {item}").SafeFireAndForget();
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
                _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
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
            _logger.LogDetailAsync($@"AKA GetAKAs: {item}").SafeFireAndForget();

            var list = new List<string>();

            // check to see if item is null or empty
            if (string.IsNullOrEmpty(item)) return list;

            try
            {
                var recs = _context.AkaTypes.Where(r => r.Item == item);
                if (recs.Any())
                {
                    list.AddRange(recs.Select(rec => rec.Aka));
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
            }
            return list;
        }


        // Get an AKA record
        public AkaType GetAka(string aka)
        {
            _logger.LogDetailAsync($@"AKA GetAka: {aka}").SafeFireAndForget();
            if (string.IsNullOrEmpty(aka)) return null;

            AkaType rec = null;
            try
            {
                rec = _context.AkaTypes.FirstOrDefault(r => r.Aka == aka);
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
            }

            return rec;
        }

        public void Insert(AkaType aka)
        {
                if (aka == null)
                {
                    _logger.LogDetailAsync("Attempted to insert null AkaType").SafeFireAndForget();
                    return;
                }
                try
                {
                    _context.AkaTypes.Add(aka);
                    _context.SaveChanges();
                    _logger.LogDetailAsync($@"Successfully inserted AKA Item: {aka.Item}, AKA: {aka.Aka}").SafeFireAndForget();
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync($@"Error inserting AKA: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
                }
        }

        // delete aka
        public void Delete(AkaType aka)
        {
            _logger.LogDetailAsync($"AKA Delete: {aka}").SafeFireAndForget();
            if (aka == null)
            {
                _logger.LogDetailAsync("AKA Delete: Attempted to delete null AKA").SafeFireAndForget();
                return;
            }
            try
            {
                var rec = _context.AkaTypes.Find(aka.Aka);
                if (rec != null)
                {
                    _context.AkaTypes.Remove(rec);
                    _context.SaveChanges();
                }
                else
                {
                    _logger.LogDetailAsync($"AKA Delete: No AKA found with AKA: {aka.Aka}").SafeFireAndForget();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
            }
        }


        public void Update(AkaType aka)
        {
            if (aka == null)
            {
                throw new ArgumentNullException(nameof(aka), @"Provided AKA type is null.");
            }
            try
            {
                var rec = _context.AkaTypes.Find(aka.Aka);
                if (rec != null)
                {
                    rec.Item = aka.Item;
                    _context.SaveChanges();
                }
                else
                {
                    _logger.LogDetailAsync($"AKA Update: No record found for AKA: {aka.Aka}").SafeFireAndForget();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"AKA Update Error: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
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
                var errorMessage = $"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}";
                _logger.LogDetailAsync(errorMessage).SafeFireAndForget();
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
