using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class RFIDManager : IRFIDManager
    {
        private List<RFID> _validTags = new List<RFID>();

        public RFIDManager() { }

        public bool IsValid(RFID rfid)
        {
            return _validTags.Contains(rfid);
        }

        public void AddTag(RFID rfid)
        {
            if (!_validTags.Contains(rfid))
            {
                _validTags.Add(rfid);
            }
        }
        public void RemoveTag(RFID rfid)
        {
            if (_validTags.Contains(rfid))
            {
                _validTags.Remove(rfid);
            }
        }

        public RFID GetTag(string tag)
        {
            var rfid = new RFID(tag);
            if (!_validTags.Contains(rfid)) return null ;
            return _validTags.Find(r => r.Tag == tag);
        }

        public void Clear()
        {
            _validTags.Clear();
        }
    }
}
