using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Interfaces;
public interface IHistoryRepository
{
    void InsertHistoryRecord(History history);
}
