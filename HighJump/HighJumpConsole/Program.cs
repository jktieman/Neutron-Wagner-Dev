using HighJump;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HighJumpConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            GetOutbound();
        }

        private static void GetOutbound()
        {
            using(var context = new HighJumpContext())
            {
                var newRecords = context.t_al_host_carousel_outbound.Where(o => o.status == "N").ToList();
                Thread.Sleep(1000);
                var newRecordsSecondPass = context.t_al_host_carousel_outbound.Where(o => o.status == "N").ToList();
                if (newRecords.Count() == newRecordsSecondPass.Count())
                {
                    // process newRecords
                    var orderNumbers = newRecords.Select(s => s.container_label).Distinct();
                    foreach (var orderNum in orderNumbers)
                    {
                        Console.WriteLine("Processing Order. " + orderNum);
                        var orderAndLines = newRecords.Select(s => s.container_label == orderNum).ToList();
                        CreatePr1(orderAndLines);
                        UpdateOutboundToProcessing(newRecords, orderNum);
                        ProcessInbound(newRecords, orderNum);
                    }
                }
                Console.WriteLine("Finished");
                Console.ReadLine();
            }
        }

        private static void ProcessInbound(List<t_al_host_carousel_outbound> newRecords, string orderNum)
        {
            using (var context = new HighJumpContext())
            {
                foreach (var rec in newRecords.Where(n => n.container_label == orderNum))
                {
                    var inBound = new t_al_host_carousel_inbound();
                    
                    inBound.container_label = rec.container_label;
                    inBound.employee_id = "1234";
                    inBound.item_number = rec.item_number;
                    inBound.pick_quantity = Double.Parse(rec.pick_quantity.ToString());
                    inBound.status = "N";
                    inBound.inserted_by = "CAROUSEL";
                    inBound.inserted_date = DateTime.Now;
                    inBound.updated_by = "CAROUSEL";
                    inBound.updated_date = DateTime.Now;

                    Console.WriteLine("Updating Outbound Status to P. " + rec.container_label + "  " + rec.item_number);
                    context.t_al_host_carousel_inbound.Add(inBound);
                }
                context.SaveChanges();
            }
        }

        private static void CreatePr1(List<bool> orderAndLines)
        {
            Console.WriteLine("Generating PR1  File.");
        }

        private static void UpdateOutboundToProcessing(List<t_al_host_carousel_outbound> newRecords, string orderNum)
        {
            using (var context = new HighJumpContext())
            {
                foreach (var rec in newRecords.Where(n => n.container_label == orderNum))
                {
                    var outBound = context.t_al_host_carousel_outbound.Find(rec.host_carousel_outbound_id);
                    outBound.status = "P";
                    Console.WriteLine("Updating Outbound Status to P. " + outBound.container_label + "  " + outBound.item_number);
                }
                context.SaveChanges();
            }
        }
    }
}
