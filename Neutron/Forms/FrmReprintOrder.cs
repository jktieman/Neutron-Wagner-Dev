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
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public partial class FrmReprintOrder : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly NeutronVariables _neutronVariables;
        private readonly DocumentPrinterPreferences _documentPrinter;
        private readonly LabelPrinterPreferences _labelPrinter;
        private readonly IJsonData _jsonData;

        public FrmReprintOrder(IJsonData jsonData)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            // SetCulture(_cultureInfo.Name);
            _jsonData = jsonData;
            _neutronVariables = _jsonData.LoadFile<NeutronVariables>();
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
        }

        private void MBReprintPrint_Click(object sender, EventArgs e)
        {
            string ord = TextBoxReprintOrder.Text.Trim();
            if (!string.IsNullOrEmpty(ord))
            {
                Order order = _repoOrders.FindBy(r => r.Ord1 == ord).FirstOrDefault();
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
            if (_neutronVariables.EnableLabelPrinter)
            {
                Task.Run(() => ToteToPrint.Print(order, _labelPrinter));
            }
        }


        private void PrintDoc(Order order)
        {
            //Task.Run(() => logger.Log($"Printing Document. {order.Ord1}"));
            if (_neutronVariables.EnableDocumentPrinter)
            {
                Task.Run(() => DocumentToPrint.Print(order.Ord1, _documentPrinter, order.Ord2));
            }
        }

    }
}
