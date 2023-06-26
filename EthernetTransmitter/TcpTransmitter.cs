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

            if (string.IsNullOrWhiteSpace(ipAddress) || port == 0) return;
            try
            {
                _server = new SimpleTcpServer(ipAddress, port);

                _server.Events.ClientConnected += Events_ClientConnected;
                _server.Events.ClientDisconnected += Events_ClientDisconnected;
                _server.Events.DataReceived += Events_DataReceived;
                _server.Events.DataSent += Events_DataSent;
                _server.Start();
            }
            catch (Exception ex)
            {
                _logger.LogAsync($"Startup: {ex.Message}");
            }
        }

        //public void OnTransmitterDataEvent(object sender, string formText, bool transmitting, string displayCsv, string flashCode = "")
        //{
        //    var args = new TransmitterEventArgs(formText, transmitting, displayCsv, flashCode);
        //    TransmitterDataEvent?.Invoke(sender, args);
        //}

        public void CloseConnection()
        {
            try
            {
                _server.Events.ClientConnected -= Events_ClientConnected;
                _server.Events.ClientDisconnected -= Events_ClientDisconnected;
                _server.Events.DataReceived -= Events_DataReceived;
                _server.Events.DataSent -= Events_DataSent;
                _server.Stop();
                _server.Dispose();

            }
            catch (Exception ex)
            {
                _logger.LogAsync($"{ex.Message}");
            }
        }

        private void Events_DataSent(object sender, DataSentEventArgs e)
        {
            _logger.Log($"Data Sent: {e.BytesSent}");
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            byte[] data = e.Data.ToArray();
            var text = Encoding.UTF8.GetString(data);
           // Mediator.GetInstance().OnIptiButtonPressed(this, new ResponseInfo());
            _logger.Log($"[{e.IpPort}]: {text}");
            _logger.Log($"[{e.IpPort}] HEX: {data.ByteArrayToHexString()}{Environment.NewLine}");
            if (text.Contains("OC"))
            {
                var command = text.Substring(1, 4);
                SendAck(command);
            }
        }

        private void Events_ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            _logger.Log($"[{e.IpPort}] client disconnected: {e.Reason}");
            IsClientConnected = false;
        }

        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            ClientIpPort = e.IpPort;
            _logger.Log($"[{e.IpPort}] Client Connected: {e.Reason}");
            IsClientConnected = true;
        }

        public void SendData(string value)
        {
            try
            {
                if (!_server.IsListening) return;
                // once a client has connected...
                var command = new Put2LightCommand().GetCommand(value);
                _logger.Log($"{value.GetCheckDigit()}");
                _logger.Log($"Command: {command}{Environment.NewLine}");
                _server.Send(ClientIpPort, command);
            }
            catch (Exception ex)
            {
                _logger.Log($"SendData: {ex.Message}");
            }

        }

        private void SendAck(string cmd)
        {
            var command = new Put2LightCommand().GetAck(cmd);
            _logger.Log($"Send ACK: {command}{Environment.NewLine}");
            _logger.Log($"Send ACK: {command.StringToByteArray().ByteArrayToHexString()}{Environment.NewLine}");
            _server.Send(ClientIpPort, command);
        }

    }
}
