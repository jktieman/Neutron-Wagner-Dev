using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;
using NeutronData.Models;

namespace Neutron.Models
{
    public sealed class Mediator
    {
        //static members
        private static readonly Mediator Instance = new Mediator();

        private Mediator()
        {
        }

        public static Mediator GetInstance()
        {
            return Instance;
        }

        public event EventHandler<SerialPortChangedEventArgs> SerialPortChanged;


        public void OnSerialPortChanged(object sender, string port)
        {
            (SerialPortChanged as EventHandler<SerialPortChangedEventArgs>)
                ?.Invoke(sender, new SerialPortChangedEventArgs { Port = port });
        }

        public event EventHandler<TransmitStateChangedEventArgs> TransmitStateChanged;

        public void OnTransmitStateChanged(object sender, bool state)
        {
            TransmitStateChanged?.Invoke(sender, new TransmitStateChangedEventArgs { State = state });
        }

        public event EventHandler<IptiButtonPressedEventArgs> IptiButtonPressed;

        public void OnIptiButtonPressed(object sender, ResponseInfo responseInfo)
        {
            IptiButtonPressed?.Invoke(sender, new IptiButtonPressedEventArgs { ResponseInfo = responseInfo });
        }

        public event EventHandler<OrderCompleteEventArgs> OrderComplete;

        public void OnOrderComplete(object sender, Order order)
        {
            OrderComplete?.Invoke(sender, new OrderCompleteEventArgs { Order = order });
        }

        public event EventHandler<StationOrderDetailsCompleteEventArgs> StationOrderDetailsComplete;

        public void OnStationOrderDetailsComplete(object sender, int stationNumber, List<OrderDetail> orderDetails)
        {
            StationOrderDetailsComplete?.Invoke(sender, new StationOrderDetailsCompleteEventArgs { StationNumber = stationNumber, OrderDetails = orderDetails });
        }

        public event EventHandler<BatchCompleteEventArgs> BatchComplete;

        public void OnBatchComplete(object sender, BindingSource bindingSource)
        {
            BatchComplete?.Invoke(sender, new BatchCompleteEventArgs {BindingSource = bindingSource});
        }
    }
}
