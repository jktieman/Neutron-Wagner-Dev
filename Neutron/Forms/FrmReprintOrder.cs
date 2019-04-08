using JsonManager;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using Ninject;
using PrintRequest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public partial class FrmReprintOrder : Form
    {
        private GenericRepository<Order> repoOrders = new GenericRepository<Order>(new NeutronDb());
        private NeutronVariables neutronVariables;
        private DocumentPrinterPreferences documentPrinter;
        private LabelPrinterPreferences labelPrinter;

        public FrmReprintOrder()
        {
            InitializeComponent();
            IJsonData jsonData = new JsonData();
            neutronVariables = jsonData.LoadFile<NeutronVariables>();
            documentPrinter = jsonData.LoadFile<DocumentPrinterPreferences>();
            labelPrinter = jsonData.LoadFile<LabelPrinterPreferences>();
        }

        private void MBReprintPrint_Click(object sender, EventArgs e)
        {
            string ord = TextBoxReprintOrder.Text.Trim();
            if (!string.IsNullOrEmpty(ord))
            {
                Order order = repoOrders.FindBy(r => r.Ord1 == ord).FirstOrDefault();
                if (order != null)
                {
                    if (CheckBoxDocument.Checked)
                    {
                        PrintDoc(order);
                    }

                    if (CheckBoxToteLabel.Checked)
                    {
                        PrintTote(order);
                    }
                }
            }
        }

        private void MBReprintCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PrintTote(Order order)
        {
            //Task.Run(() => logger.Log($"Printing Tote Label. {order.Ord1}"));
            if (neutronVariables.EnableLabelPrinter)
            {
                Task.Run(() => ToteToPrint.Print(order, labelPrinter));
            }
        }


        private void PrintDoc(Order order)
        {
            //Task.Run(() => logger.Log($"Printing Document. {order.Ord1}"));
            if (neutronVariables.EnableDocumentPrinter)
            {
                Task.Run(() => DocumentToPrint.Print(order.Ord1, documentPrinter, order.Ord2));
            }
        }

    }
}
