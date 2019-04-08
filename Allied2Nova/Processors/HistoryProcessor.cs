using Allied2Nova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Allied2Nova.Processors
{
    public class HistoryProcessor
    {
        public bool DoesOrderHistoryExist(string orderNumber)
        {
            var result = GetFromNova(orderNumber);

            return result == null ? false : true ;

        }

        public HistoryRec GetFromNova(string orderNumber)
        {
            HistoryRec historyRec = null;
            try
            {
                var result = SafeNativeMethods.GETHISTORYBYORDER(ref orderNumber);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    string[] values = result.Split('|');
                    historyRec = new HistoryRec();
                    historyRec.Sku = values[0];
                    historyRec.ActDate = values[1];
                    historyRec.ActTime = values[2];
                    historyRec.AType = int.Parse(values[3]);
                    historyRec.Employee = values[4];
                    historyRec.ReqQty = int.Parse(values[5]);
                    historyRec.IssQty = int.Parse(values[6]);
                    historyRec.LoadDate = values[7];
                    historyRec.Order = values[8];
                    historyRec.Invoice = values[9];
                    historyRec.RftId = int.Parse(values[10]);
                    historyRec.SysNum = values[11];
                    historyRec.Car = values[12];
                    historyRec.Bin = int.Parse(values[13]);
                    historyRec.Level = int.Parse(values[14]);
                    historyRec.Part = int.Parse(values[15]);
                    historyRec.Slot = values[16];
                    return historyRec;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to retrieve History Information.\r\n " + ex.Message);
            }
            return historyRec;
        }

    }
}
