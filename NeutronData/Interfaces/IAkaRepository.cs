using System.Collections.Generic;
using NeutronData.Models;

namespace NeutronData.Interfaces
{
    public interface IAkaRepository
    {
        string Get(string aka);
        string GetUpc(string item);
        List<string> GetAkas(string item);
        void Insert(AkaType aka);
        AkaType GetAka(string aka);
        void Delete(AkaType aka);
        void Update(AkaType aka);
        void Save();
        void Dispose();
    }
}
