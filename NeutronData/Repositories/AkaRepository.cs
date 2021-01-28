using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
