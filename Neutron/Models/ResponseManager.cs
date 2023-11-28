using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AlliedLogger;
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
        private readonly IDynamicLogger _logger;
        public CancellationTokenSource Token = new CancellationTokenSource();


        public bool Transmit { get; set; }
        public bool Transmitting { get; set; }

        public ResponseManager(BlockingCollection<byte[]> responseBlockingCollection,
            BlockingCollection<byte[]> requestBlockingCollection, BlockingCollection<byte[]> receivedBlockingCollection)
        {
            _responseBlockingCollection = responseBlockingCollection;
            _requestBlockingCollection = requestBlockingCollection;
            _receivedBlockingCollection = receivedBlockingCollection;
            _logger = NeutronCore.Global.Logger.SetupLogger("ResponseManager");
        }

        public void Start()
        {
            _ = _logger.LogDetailAsync("ResponseManager: Start");

            try
            {
                foreach (var item in _receivedBlockingCollection.GetConsumingEnumerable())
                {
                    _ = _logger.LogDetailAsync($"Start Process - Received Item:  {item.ByteArrayToStringX2()}");
                    // _frm.UpdateTextBox($"Response:  {string.Join(string.Empty, Array.ConvertAll(item, x => x.ToString("X2")))}");
                    ProcessReceived(item);
                }
            }
            catch (OperationCanceledException)
            {
                _ = _logger.LogDetailAsync("ResponseManager: Cancelled");
                return;
            }
        }

        private void ProcessReceived(byte[] input)
        {
            var cmd = new byte[0];
            _ = _logger.LogDetailAsync($"Process Received - input:  {input.ByteArrayToStringX2()}");
            if (input.Length == 0)
            {
                _ = _logger.LogDetailAsync($"Process Received - input: null or zero length");
                return;
            }

            if (input.Last() == Global.ETX)
            {
                var inp = input.Last();
                var last = inp.ToString("X2");
                _ = _logger.LogDetailAsync($"Process Received - input: last char is a Terminating Char.  [{last}] ");
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
                            _ = _logger.LogDetailAsync("Process Received - Checking b = 1 and buildingArray is true... wrong.");
                        }
                    }
                    else if (b == 3)
                    {
                        if (buildingArray)
                        {
                            bList.Add(b);
                            var bArray = bList.ToArray();
                            var message = Encoding.UTF8.GetString(bArray);
                            _ = _logger.LogDetailAsync($"Add [ {message} ] to ResponseBlockingCollection.");
                            _responseBlockingCollection.TryAdd(bArray, 50);
                            Mediator.GetInstance().OnBatchComplete($"Process Received - {message}");
                            buildingArray = false;
                            _ = _logger.LogDetailAsync($"Process Received - byte array added: {message}");
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
                            _ = _logger.LogDetailAsync($"Process Received - Char's in Error: {b}");
                        }
                    }
                }
                _ = _logger.LogDetailAsync("Process Received - Clear the Byte Array.");
                _bytes.Clear();
            }
            else
            {
                _ = _logger.LogDetailAsync($"Process Received - ELSE: {input.ByteArrayToStringX2()}");
                _bytes.AddRange(input);
            }
        }

        public void StartResponseProcessor()
        {
            _ = _logger.LogDetailAsync($"Start Response Processor: Transmit - {Transmit}  Token: {Token.Token.IsCancellationRequested} ");

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
                        _ = _logger.LogDetailAsync(
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
            _ = _logger.LogDetailAsync($"Create Request: {Environment.NewLine} {responseInfo.Information}");

            if (responseInfo.DisplayNumber != null)
            {
                Mediator.GetInstance().OnIptiButtonPressed(this, responseInfo);
            }
        }
    }
}
