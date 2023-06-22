using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using NeutronData.Models;
using NeutronData.ModelViews;


namespace NeutronEvents
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
        //--------------------------------------------------------------------------

        public event EventHandler<WorkItEventArgs> WorkItMessageChng;
        public void OnWorkItMessageChng(WorkItEventArgs e)
        {
            if (WorkItMessageChng != null)
            {
                var eventListeners = WorkItMessageChng.GetInvocationList();
                foreach (var t in eventListeners)
                {
                    var methodToInvoke = (EventHandler<WorkItEventArgs>)t;
                    methodToInvoke.BeginInvoke(this, e, EndAsyncWorkItMessageChng, null);
                }
            }
        }

        public void EndAsyncWorkItMessageChng(IAsyncResult iar)
        {
            var ar = (AsyncResult)iar;
            var invokedMethod = (EventHandler<WorkItEventArgs>)ar.AsyncDelegate;
            try
            {
                invokedMethod.EndInvoke(iar);
            }
            catch
            {
                // Handle any exceptions that were thrown by the invoked method
                Console.WriteLine("An event listener went kaboom!");
            }
        }




        //-------------------------------------------------------------------
        public event EventHandler<WorkItEventArgs> WorkItMessageChange;

        public void OnWorkItMessageChange(object sender, List<string> list)
        {
            WorkItMessageChange?.Invoke(sender, new WorkItEventArgs(list) { MessageList = list });
        }

        public event EventHandler<SerialPortChangedEventArgs> SerialPortChanged;
        
        public void OnSerialPortChanged(object sender, string port)
        {
            (SerialPortChanged as EventHandler<SerialPortChangedEventArgs>)
                ?.Invoke(sender, new SerialPortChangedEventArgs { Port = port });
        }

        public event EventHandler<SerialPortWriteEventArgs> SerialPortWrite;

        public void OnSerialPortWrite(object sender, string request)
        {
            (SerialPortWrite as EventHandler<SerialPortWriteEventArgs>)
                ?.Invoke(sender, new SerialPortWriteEventArgs { Request = request });
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

        public event EventHandler<AcceptButtonPressedEventArgs> AcceptButtonPressed;

        public void OnAcceptButtonPressed(object sender, PickStop pickStop)
        {
            AcceptButtonPressed?.Invoke(sender, new AcceptButtonPressedEventArgs { PickStop = pickStop });
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



        public event EventHandler<EventArgs> BatchComplete;

        public void OnBatchComplete(object sender)
        {
            BatchComplete?.Invoke(sender, EventArgs.Empty);
        }

        public event EventHandler<StartStopEventArgs> StartStopLoader;

        public void OnStartStopLoader(object sender, string startStop)
        {
            StartStopLoader?.Invoke(this, new StartStopEventArgs {StartStop = startStop});
        }

        public event EventHandler<LoaderErrorEventArgs> LoaderError;

        public void OnLoaderError(object sender, string message)
        {
            LoaderError?.Invoke(this, new LoaderErrorEventArgs() { Message = message });
        }

        public event EventHandler<LoaderErrorEventArgs> GeneralError;

        public void OnGeneralError(object sender, string message)
        {
            GeneralError?.Invoke(this, new LoaderErrorEventArgs() { Message = message });
        }

        public event EventHandler<EventArgs> RunLoaderOnce;

        public void OnRunLoaderOnce(object sender)
        {
            RunLoaderOnce?.Invoke(sender, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> RunUploadOnce;

        public void OnRunUploadOnce(object sender)
        {
            RunUploadOnce?.Invoke(sender, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> InventoryFileCreated;

        public void OnInventoryFileCreated(object sender)
        {
            InventoryFileCreated?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<InventoryFileCreatedErrorEventArgs> InventoryFileCreatedError;

        public void OnInventoryFileCreatedError(object sender, string msg)
        {
            InventoryFileCreatedError?.Invoke(this, new InventoryFileCreatedErrorEventArgs {Text = msg});
        }

        public event EventHandler<StartStopEventArgs> StartStopUpload;

        public void OnStartStopUpload(object sender, string startStop)
        {
            StartStopUpload?.Invoke(this, new StartStopEventArgs { StartStop = startStop });
        }
    }
}
