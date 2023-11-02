using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronCore.Extensions;
using NeutronEvents;
using SuperSimpleTcp;

namespace EthernetTransmitter
{
    public class TcpTransmitter
    {
        public char SOH = Convert.ToChar(1);
        public char ETX = Convert.ToChar(3);
        public char ACK = Convert.ToChar(6);
        public string ClientIpPort;
        public bool IsClientConnected = false;


        private readonly SimpleTcpServer _server;
        private readonly IDynamicLogger _logger;

        //Events
       // public event EventHandler<TransmitterEventArgs> TransmitterDataEvent;

        //Public Event UserInterfaceData _
        //    (ByVal action As String, ByVal formText As String, ByVal textColor As Color)

        //Public Event addReceiveDelegate(ByVal obj As Object, ByVal e As EventArgs)

        //Public Event readDataDelegate(ByVal obj As Object, ByVal e As EventArgs)

        //Public Event ReceivingSignal(ByVal b As Boolean)
        //public event EventHandler TransmittingSignal(bool b);



        public TcpTransmitter(string ipAddress, int port, IDynamicLogger logger)
        {
            _logger = logger;
            _ = _logger.LogDetailAsync($"Startup: {ipAddress}:{port}");

            if (string.IsNullOrWhiteSpace(ipAddress) || port == 0) return;
            try
            {
                _server = new SimpleTcpServer(ipAddress, port);

                _server.Events.ClientConnected += Events_ClientConnected;
                _server.Events.ClientDisconnected += Events_ClientDisconnected;
                _server.Events.DataReceived += Events_DataReceived;
                _server.Events.DataSent += Events_DataSent;
                _server.Start();
                _ = _logger.LogDetailAsync($"Startup Server Is Listening: {_server.IsListening}");
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Startup: {ex.Message}");
            }
        }

        //public void OnTransmitterDataEvent(object sender, string formText, bool transmitting, string displayCsv, string flashCode = "")
        //{
        //    var args = new TransmitterEventArgs(formText, transmitting, displayCsv, flashCode);
        //    TransmitterDataEvent?.Invoke(sender, args);
        //}

        public void CloseConnection()
        {
            _ = _logger.LogDetailAsync($"CloseConnection");
            try
            {
                _server.Events.ClientConnected -= Events_ClientConnected;
                _server.Events.ClientDisconnected -= Events_ClientDisconnected;
                _server.Events.DataReceived -= Events_DataReceived;
                _server.Events.DataSent -= Events_DataSent;
                _server.Stop();
                _server.Dispose();
                _ = _logger.LogDetailAsync($"CloseConnection Server Is Listening: {_server.IsListening}");

            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"{ex.Message}");
            }
        }

        private void Events_DataSent(object sender, DataSentEventArgs e)
        {
            _ = _logger.LogDetailAsync($"Data Sent: {e.BytesSent}");
        }

        private async void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            await _logger.LogDetailAsync($"Data Received: {e.IpPort}");
            byte[] data = e.Data.ToArray();
            var text = Encoding.UTF8.GetString(data);
           // Mediator.GetInstance().OnIptiButtonPressed(this, new ResponseInfo());
            await _logger.LogDetailAsync($"IP Port: [{e.IpPort}]  Data: {text}");
            await _logger.LogDetailAsync($"IP Port: [{e.IpPort}] HEX Data: {data.ByteArrayToHexString()}{Environment.NewLine}");
            if (text.Contains("OC"))
            {
                var command = text.Substring(1, 4);
                await _logger.LogDetailAsync($"Text Contains OC Send Response Command: {command}");
                await SendAck(command);
            }
        }

        private void Events_ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            _ = _logger.LogDetailAsync("Events_ClientDisconnected");
            _ = _logger.LogDetailAsync($"IP Port: [{e.IpPort}] Client Disconnected Reason: {e.Reason}");
            IsClientConnected = false;
        }

        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            _ = _logger.LogDetailAsync("Events_ClientConnected");
            ClientIpPort = e.IpPort;
            _ = _logger.LogDetailAsync($"IP Port: [{e.IpPort}] Client Connected -- Disconnect Reason: {e.Reason}");
            IsClientConnected = true;
        }

        public async Task SendData(string value)
        {
            await _logger.LogDetailAsync($"SendData: {value}");
            try
            {
                if (!_server.IsListening) return;
                // once a client has connected...
                var command = new Put2LightCommand().GetCommand(value);
                
                await _logger.LogDetailAsync($"SendData Command: {command}{Environment.NewLine}");
                if (IsClientConnected)
                {
                   await _server.SendAsync(ClientIpPort, command);
                   // await Task to let the displays turn on before sending the next command
                   await Task.Delay(100);
                    await _logger.LogDetailAsync($"SendData Command: {command.StringToByteArray().ByteArrayToHexString()}{Environment.NewLine}"); 
                }
                
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"SendData Exception: {ex.Message}");
            }

        }

        private async Task SendAck(string cmd)
        {
            await _logger.LogDetailAsync($"Send ACK: {cmd}");
            var command = new Put2LightCommand().GetAck(cmd);
            await _logger.LogDetailAsync($"Send ACK: {command}{Environment.NewLine}");
            await _server.SendAsync(ClientIpPort, command);
            await _logger.LogDetailAsync($"Send ACK: {command.StringToByteArray().ByteArrayToHexString()}{Environment.NewLine}");
        }

    }
}
