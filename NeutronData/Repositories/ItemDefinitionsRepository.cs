using AlliedLogger;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace NeutronData.Repositories
{
    public class ItemDefinitionsRepository
    {
        public IEnumerable<ItemDefinitionView> GetAllItemDefinitionViews(string find = "")
        {
            List<ItemDefinitionView> recs = null;
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@Find", find);
                    recs = context.Database.SqlQuery<ItemDefinitionView>(sql: "usp_GetItemDefinitionViewFind @Find", parameters: new object[] { param }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get All Location Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public IEnumerable<ItemDefinitionView> FindItemDefinitionViewsByStation(string find = "", int stationId = 0)
        {
            var recs = new List<ItemDefinitionView>();
            if (stationId == 0) return recs;
            Logger.Log(msg: "Get All Item Definition Views Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@Find", find);
                    var paramStation = new SqlParameter("@StationId", stationId);
                    recs = context.Database.SqlQuery<ItemDefinitionView>("usp_GetItemDefinitionViewFind_Station @Find, @StationId ", param, paramStation).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get All Item Definition Views Error. " + ex.Message + " " + ex.InnerException);
            }
            Logger.Log("Get All Item Definition Views End: " + recs.Count.ToString());
            return recs;
        }

        public IEnumerable<ItemDefinitionView> FindItemDefinitionViews(string find = "")
        {
            var recs = new List<ItemDefinitionView>();
            

                Logger.Log(msg: "Get All Item Definition Views Start");
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
                    Logger.Log("Get All Item Definition Views Error. " + ex.Message + " " + ex.InnerException);
                }
                Logger.Log("Get All Item Definition Views End: " + recs.Count.ToString());
            
            return recs;
        }

        public IEnumerable<NewItemView> GetNewItemViews(string find = "")
        {
            var recs = new List<NewItemView>();


            Logger.Log(msg: "Get All New Item Views Start");
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
                Logger.Log("Get All New Item Views Error. " + ex.Message + " " + ex.InnerException);
            }
            Logger.Log("Get All New Item Views End: " + recs.Count.ToString());

            return recs;
        }

    }
}
