using AlliedLogger;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NeutronData.Interfaces;

namespace NeutronData.Repositories
{
    public class ItemDefinitionsRepository : IItemDefinitionsRepository
    {
        private readonly IDynamicLogger _logger;

        public ItemDefinitionsRepository()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("ItemDefinitionsRepository");
        }

        //public IEnumerable<ItemDefinitionView> GetAllItemDefinitionViews(string find = "")
        //{
        //    List<ItemDefinitionView> recs = null;
        //    try
        //    {
        //        using (var context = new NeutronDb())
        //        {
        //            var param = new SqlParameter("@Find", find);
        //            recs = context.Database.SqlQuery<ItemDefinitionView>(sql: "usp_GetItemDefinitionViewFind @Find", parameters: new object[] { param }).ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync("Get All Location Views Error. " + ex.Message + " " + ex.InnerException);
        //    }

        //    return recs;
        //}
        //public IEnumerable<ItemDefinitionView> FindItemDefinitionViewsByWorkstation(string find = "", int workstationId = 0)
        //{
        //    var recs = new List<ItemDefinitionView>();
        //    if (workstationId == 0) return recs;
        //    _logger.LogDetailAsync(msg: "Get All Item Definition Views Start");
        //    try
        //    {
        //        using (var context = new NeutronDb())
        //        {
        //            var param = new SqlParameter("@Find", find);
        //            var paramStation = new SqlParameter("@WorkstationId", workstationId);
        //            recs = context.Database.SqlQuery<ItemDefinitionView>("usp_GetItemDefinitionViewFind_Workstation @Find, @WorkstationId ", param, paramStation).ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync("Get All Item Definition Views Error. " + ex.Message + " " + ex.InnerException);
        //    }
        //    _logger.LogDetailAsync("Get All Item Definition Views End: " + recs.Count.ToString());
        //    return recs;
        //}

        public IEnumerable<ItemDefinitionView> FindItemDefinitionViewsByItem(string item)
        {
            var recs = new List<ItemDefinitionView>();
            if (string.IsNullOrEmpty(item)) return recs;
            _logger.LogDetailAsync(msg: "Get All Item Definition Views by Item -- Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@ITEM", item);
                    recs = context.Database.SqlQuery<ItemDefinitionView>("usp_GetItemDefinitionViewsByItem @ITEM ", param).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync("Get All Item Definition Views By Item Error. " + ex.Message + " " + ex.InnerException);
            }
            _logger.LogDetailAsync("Get All Item Definition Views By Item -- End: " + recs.Count.ToString());
            return recs;
        }
        public IEnumerable<ItemDefinitionView> FindItemDefinitionViewsByArea(string find = "", int areaid = 0)
        {
            var recs = new List<ItemDefinitionView>();
            if (areaid == 0) return recs;
            _logger.LogDetailAsync(msg: "Get All Item Definition Views Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@FIND", find);
                    var paramStation = new SqlParameter("@AREAID", areaid);
                    recs = context.Database.SqlQuery<ItemDefinitionView>("usp_GetItemDefinitionViewFind_Area @FIND, @AREAID ", param, paramStation).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync("Get All Item Definition Views Error. " + ex.Message + " " + ex.InnerException);
            }
            _logger.LogDetailAsync("Get All Item Definition Views End: " + recs.Count.ToString());
            return recs;
        }


        /// <summary>
        /// Get all Item Definition Views for ALL Areas
        /// </summary>
        /// <param name="find">Looks at Item and Description for a match</param>
        /// <returns></returns>
        public IEnumerable<ItemDefinitionView> FindItemDefinitionViews(string find = "")
        {
            var recs = new List<ItemDefinitionView>();

                _logger.LogDetailAsync(msg: "Get All Item Definition Views Start");
                try
                {
                    using (var context = new NeutronDb())
                    {
                        var param = new SqlParameter("@Find", find);
                        
                        recs = context.Database.SqlQuery<ItemDefinitionView>(sql: "usp_GetItemDefinitionViewFind @Find "
                            , parameters: new object[] { param }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync("Get All Item Definition Views Error. " + ex.Message + " " + ex.InnerException);
                }
                _logger.LogDetailAsync("Get All Item Definition Views End: " + recs.Count.ToString());
            
            return recs;
        }

        public IEnumerable<NewItemView> GetNewItemViews(string find = "")
        {
            var recs = new List<NewItemView>();


            _logger.LogDetailAsync(msg: "Get All New Item Views Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@FIND", find);

                    recs = context.Database.SqlQuery<NewItemView>(sql: "usp_GetNewItemViews @Find "
                        , parameters: new object[] { param }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync("Get All New Item Views Error. " + ex.Message + " " + ex.InnerException);
            }
            _logger.LogDetailAsync("Get All New Item Views End: " + recs.Count.ToString());

            return recs;
        }

    }
}
