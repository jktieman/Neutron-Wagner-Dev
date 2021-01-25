using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AlliedLogger;
using MetroFramework.Forms;
using Neutron.Builders;
using NeutronCore.Extensions;
using NeutronEvents;


namespace Neutron.Models
{
    public class ResponseManager
    {
        private readonly BlockingCollection<byte[]> _responseBlockingCollection;
        private readonly BlockingCollection<byte[]> _requestBlockingCollection;
        private readonly BlockingCollection<byte[]> _receivedBlockingCollection;
        private readonly List<byte> _bytes = new List<byte>();
        private readonly DynamicLogger _logger;
        public CancellationTokenSource Token = new CancellationTokenSource();


        public bool Transmit { get; set; }
        public bool Transmitting { get; set; }

        public ResponseManager(BlockingCollection<byte[]> responseBlockingCollection,
            BlockingCollection<byte[]> requestBlockingCollection, BlockingCollection<byte[]> receivedBlockingCollection,
            DynamicLogger logger)
        {
            _responseBlockingCollection = responseBlockingCollection;
            _requestBlockingCollection = requestBlockingCollection;
            _receivedBlockingCollection = receivedBlockingCollection;
            _logger = logger;
        }

        public void Start()
        {
            _logger.Log("ResponseManager: Start");

            try
            {
                foreach (var item in _receivedBlockingCollection.GetConsumingEnumerable())
                {
                    _logger.Log($"Start Process - Received Item:  {item.ByteArrayToStringX2()}");
                    // _frm.UpdateTextBox($"Response:  {string.Join(string.Empty, Array.ConvertAll(item, x => x.ToString("X2")))}");
                    ProcessReceived(item);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log("ResponseManager: Cancelled");
                return;
            }
        }

        private void ProcessReceived(byte[] input)
        {
            var cmd = new byte[0];
            _logger.Log($"Process Received - input:  {input.ByteArrayToStringX2()}");
            if (input.Length == 0)
            {
                _logger.Log($"Process Received - input: null or zero length");
                return;
            }

            if (input.Last() == Global.ETX)
            {
                var inp = input.Last();
                var last = inp.ToString("X2");
                _logger.Log($"Process Received - input: last char is a Terminating Char.  [{last}] ");
                _bytes.AddRange(input);
                var bList = new List<byte>();
                var buildingArray = false;
                foreach (var b in _bytes.ToList())
                {
                    if (b == 1)
                    {
                        if (!buildingArray)
                        {
                            bList = new List<byte> { b };
                            buildingArray = true;
                        }
                        else
                        {
                            _logger.Log("Process Received - Checking b = 1 and buildingArray is true... wrong.");
                        }
                    }
                    else if (b == 3)
                    {
                        if (buildingArray)
                        {
                            bList.Add(b);
                            var bArray = bList.ToArray();
                            var message = Encoding.UTF8.GetString(bArray);
                            _logger.Log($"Add [ {message} ] to ResponseBlockingCollection.");
                            _responseBlockingCollection.TryAdd(bArray, 50);
                            Mediator.GetInstance().OnBatchComplete($"Process Received - {message}");
                            buildingArray = false;
                            _logger.Log($"Process Received - byte array added: {message}");
                        }
                    }
                    else
                    {
                        if (buildingArray)
                        {
                            bList.Add(b);
                        }
                        else
                        {
                            //If you get here without an array being created
                            //It means you're starting in the middle of a command
                            //so LOG it and ignore it.
                            _logger.Log($"Process Received - Char's in Error: {b}");
                        }
                    }
                }
                _logger.Log("Process Received - Clear the Byte Array.");
                _bytes.Clear();
            }
            else
            {
                _logger.Log($"Process Received - ELSE: {input.ByteArrayToStringX2()}");
                _bytes.AddRange(input);
            }
        }

        public void StartResponseProcessor()
        {
            _logger.Log($"Start Response Processor: Transmit - {Transmit}  Token: {Token.Token.IsCancellationRequested} ");

            while (Transmit)
            {
                try
                {
                    foreach (var response in _responseBlockingCollection.GetConsumingEnumerable(Token.Token))
                    {
                        if (Token.IsCancellationRequested)
                        {
                            return;
                        }

                        if (response == null) continue;
                        _logger.Log(
                            $"Start Response Processor - ResponseBlockingCollection Loop: {response.ByteArrayToStringX2()}");
                        var responseInfo = new ResponseInfo();
                        new ResponseBuilder().BuildInfo(response.ByteArrayToString(), responseInfo);
                        if (responseInfo.RespondCommand != null)
                        {
                            CreateRequest(responseInfo);
                        }

                        Mediator.GetInstance().OnSerialPortWrite(this, $"No Response Needed  {responseInfo.ControllerNumber} - {responseInfo.DisplayNumber}");
                        Transmitting = false;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                Thread.Sleep(1000);
            }
        }

        private void CreateRequest(ResponseInfo responseInfo)
        {
            if (responseInfo.RespondCommand != null)
            {
                Mediator.GetInstance().OnSerialPortWrite(this, "Response Created - {responseInfo.RespondCommand}");
                _requestBlockingCollection.TryAdd(responseInfo.RespondCommand.StringToByteArray(), 50);
            }
            _logger.Log($"Create Request: {Environment.NewLine} {responseInfo.Information}");

            if (responseInfo.DisplayNumber != null)
            {
                Mediator.GetInstance().OnIptiButtonPressed(this, responseInfo);
            }
        }
    }
}
