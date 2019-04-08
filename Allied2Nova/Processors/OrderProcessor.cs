using Allied2Nova.Extensions;
using Allied2Nova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Allied2Nova;

namespace Allied2Nova.Processors
{
    public class OrderProcessor
    {

        private IList<OrderRec> orderRecords = new List<OrderRec>();

        public OrderProcessor()
        {
        }

        public string GetOrder(string orderNumber)
        {
            var result = SafeNativeMethods.GETORDER(ref orderNumber);
            return result;
        }

        public IList<OrderRec> GetOrderRecords()
        {
            Init();
            return orderRecords;
        }

        public OrderRec Get(string order)
        {
            if (orderRecords.Count == 0)
            {
                Init();
            }
            return orderRecords.Where(c => c.Order.Trim() == order.Trim()).FirstOrDefault();
        }



        public void Init()
        {
            orderRecords = new List<OrderRec>();
            try
            {
                string[] lineDetail;
                string allRecs = SafeNativeMethods.GETORDERS();
                if (!string.IsNullOrWhiteSpace(allRecs))
                {
                    foreach (var line in allRecs.SplitToLines().ToArray())
                    {
                        lineDetail = line.Split('|');
                        var c = new OrderRec(lineDetail[0]
                            , lineDetail[1]
                            );
                        orderRecords.Add(c);
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(string.Format("ERROR - Cannot initialize OrderRecords. {0}", ex.Message));
            }
        }
    }
}
