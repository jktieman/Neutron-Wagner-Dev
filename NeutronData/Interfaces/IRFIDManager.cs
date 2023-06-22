using NeutronData.Models;

namespace NeutronData.Interfaces
{
    public interface IRFIDManager
    {
        bool IsValid(RFID rfid);
        void AddTag(RFID rfid);
        void RemoveTag(RFID rfid);
        RFID GetTag(string tag);
        void Clear();
    }
}