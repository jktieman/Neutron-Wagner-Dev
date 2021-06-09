using NeutronData.DataContexts;
using NeutronData.Interfaces;
using System;
using System.Linq;
using System.Windows.Forms;

namespace NeutronData.Repositories
{
    public class AkaRepository : IAkaRepository
    {
        private readonly NeutronDb _context = new NeutronDb();

        public string Get(string aka)
        {
            var item = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(aka))
                {
                    var rec = _context.AkaTypes.Find(aka);
                    item = rec != null ? rec.Item : aka;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            return item;
        }

        public string GetUpc(string item)
        {
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
                MessageBox.Show($@"AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            return upc;
        }
    }
}
