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
        readonly NeutronDb context = new NeutronDb();

        public string Get(string aka)
        {
            string item = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(aka))
                {
                    AkaType rec = context.AkaTypes.Find(aka);
                    if (rec != null)
                    {
                        item = rec.Item;
                    }
                    else
                    {
                        item = aka;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"AKA Error: {ex.Message} \r\n {ex.InnerException}");
            }

            return item;
        }
    }
}
