using NeutronData.Models;
using System.Collections.Generic;

namespace NeutronData.Interfaces
{
    public interface INeutronNDefDbRepository
    {
        OperationStatus SaveAll(IList<NDefDb> recs);
        OperationStatus TruncateTable();
        void SaveOperationStatus(OperationStatus opStatus);
    }
}
