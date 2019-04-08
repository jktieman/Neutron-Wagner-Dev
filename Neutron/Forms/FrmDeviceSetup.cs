using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
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
    public partial class FrmDeviceSetup : Form
    {
        private GenericRepository<HardwareDevice> repoHardware = new GenericRepository<HardwareDevice>(new NeutronDb());
        private GenericRepository<Station> repoStation = new GenericRepository<Station>(new NeutronDb());
        private GenericRepository<DeviceType> repoDeviceType = new GenericRepository<DeviceType>(new NeutronDb());


        public FrmDeviceSetup()
        {
            InitializeComponent();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            SaveForm();
        }

        private void SaveForm()
        {

            var device = new HardwareDevice()
            {
                Id = string.IsNullOrEmpty(TextBoxId.Text) ? 0 : int.Parse(TextBoxId.Text),
                StationId = (ComboBoxStation.SelectedItem as Station).Id,
                DeviceTypeId = (ComboBoxDeviceType.SelectedItem as DeviceType).Id,
                NumberOfCarriers = int.Parse(TextBoxNumberOfCarriers.Text),
                CarrierWidth = int.Parse(TextBoxCarrierWidth.Text),
                CarrierDepth = int.Parse(TextBoxCarrierDepth.Text)
            };
            if (device.Id == 0)
            {
                repoHardware.Insert(device);
            }
            else
            {
                repoHardware.Update(device);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmDeviceSetup_Load(object sender, EventArgs e)
        {
            ComboBoxStation.DataSource = repoStation.All();
            ComboBoxStation.DisplayMember = "Name";
            ComboBoxStation.ValueMember = "Id";

            ComboBoxDeviceType.DataSource = repoDeviceType.All();
            ComboBoxDeviceType.DisplayMember = "Name";
            ComboBoxDeviceType.ValueMember = "Id";
        }
    }
}
