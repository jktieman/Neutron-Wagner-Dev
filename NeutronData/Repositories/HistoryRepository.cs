using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using AlliedLogger;
using AsyncAwaitBestPractices;


namespace NeutronData.Repositories;
public class HistoryRepository : IHistoryRepository
{
    private Func<NeutronDb> _contextFactory;
    private IDynamicLogger _logger;

    public HistoryRepository(Func<NeutronDb> contextFactory)
    {
        _contextFactory = contextFactory;
        _logger = NeutronCore.Global.Logger.SetupLogger("HistoryRepository");
    }

    public void InsertHistoryRecord(History history)
    {
        try
        {
            using (var context = _contextFactory())
            {
                object[] parameters =
                {
                    new SqlParameter("@ActionCode", history.ActionCode),
                    new SqlParameter("@ActionCodeName", history.ActionCodeName),
                    new SqlParameter("@ActionDateTime", history.ActionDateTime),
                    new SqlParameter("@Ord1", (object)history.Ord1 ?? string.Empty),
                    new SqlParameter("@Ord2", (object)history.Ord2 ?? string.Empty),
                    new SqlParameter("@Item", (object)history.Item ?? string.Empty),
                    new SqlParameter("@Description", (object)history.Description ?? string.Empty),
                    new SqlParameter("@RequestedQuantity", (object)history.RequestedQuantity ?? 0),
                    new SqlParameter("@IssuedQuantity", (object)history.IssuedQuantity ?? 0),
                    new SqlParameter("@AreaId", (object)history.AreaId ?? 0),
                    new SqlParameter("@Loc1", (object)history.Loc1 ?? 0),
                    new SqlParameter("@Loc2", (object)history.Loc2 ?? 0),
                    new SqlParameter("@Loc3", (object)history.Loc3 ?? 0),
                    new SqlParameter("@Loc4", (object)history.Loc4 ?? 0),
                    new SqlParameter("@Loc5", (object)history.Loc5 ?? 0),
                    new SqlParameter("@Slot", (object)history.Slot ?? string.Empty),
                    new SqlParameter("@OrderId", (object)history.OrderId ?? DBNull.Value),
                    new SqlParameter("@OrderDetailId", (object)history.OrderDetailId ?? DBNull.Value),
                    new SqlParameter("@EmpId", (object)history.EmpId ?? DBNull.Value),
                    new SqlParameter("@TransmitDateTime", (object)history.TransmitDateTime ?? DBNull.Value),
                    new SqlParameter("@CostCenter", (object)history.CostCenter ?? string.Empty),
                    new SqlParameter("@Priority", (object)history.Priority ?? 99),
                    new SqlParameter("@LoadDate", (object)history.LoadDate ?? DBNull.Value),
                    new SqlParameter("@OrderInfo", (object)history.OrderInfo ?? string.Empty),
                    new SqlParameter("@OrderDetailInfo", (object)history.OrderDetailInfo ?? string.Empty),
                    new SqlParameter("@TypeCode", (object)history.TypeCode ?? string.Empty),
                    new SqlParameter("@PrimeBin", (object)history.PrimeBin ?? string.Empty),
                    new SqlParameter("@NewBin", (object)history.NewBin ?? string.Empty),
                    new SqlParameter("@TroubleBit", (object)history.TroubleBit ?? DBNull.Value),
                    new SqlParameter("@WorkstationId", (object)history.WorkstationId ?? DBNull.Value)
                };

                // Fix: Use SqlQuery with the correct method signature
                context.Database.ExecuteSqlCommand(
                    "EXEC usp_SaveHistoryRecord @ActionCode, @ActionCodeName, @ActionDateTime, @Ord1, @Ord2, @Item, @Description, @RequestedQuantity, @IssuedQuantity, @AreaId, @Loc1, @Loc2, @Loc3, @Loc4, @Loc5, @Slot, @OrderId, @OrderDetailId, @EmpId, @TransmitDateTime, @CostCenter, @Priority, @LoadDate, @OrderInfo, @OrderDetailInfo, @TypeCode, @PrimeBin, @NewBin, @TroubleBit, @WorkstationId",
                    parameters);
                _logger.LogDetailAsync($"Save History: Order: {history.Ord1} TO: {history.Ord2} Item: {history.Item} ");
            }
        }
        catch (SqlException sex)
        {
            _logger.LogDetailAsync($@"SQL Exception: Order: {history.Ord1} TO: {history.Ord2} Item: {history.Item}{Environment.NewLine} {sex.Message}").SafeFireAndForget();
        }
        catch (Exception ex)
        {
            _logger.LogDetailAsync($@"Order: {history.Ord1} TO: {history.Ord2} Item: {history.Item}{Environment.NewLine} {ex.Message}").SafeFireAndForget();
        }
    }
}
