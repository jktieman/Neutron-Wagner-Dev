using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using AlliedLogger;
using NeutronCore.Extensions;
using NeutronCore.StaticClasses;
using SuperSimpleTcp;

namespace EthernetTransmitter
{
    /// <summary>
    /// Represents a TCP transmitter for Ethernet communication.
    /// </summary>
    /// <remarks>
    /// This class is responsible for establishing a TCP connection, sending data, and closing the connection.
    /// It also provides information about the connection status and client IP port.
    /// </remarks>
    public class TcpTransmitter
    {
       
        public string ClientIpPort;
        /// <summary>
        /// Gets a value indicating whether a client is currently connected to the TCP transmitter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a client is connected; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This field is updated when a client connects or disconnects from the TCP transmitter.
        /// </remarks>
        public bool IsClientConnected;

        private readonly SimpleTcpServer _server;

        private readonly IDynamicLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTransmitter"/> class.
        /// </summary>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port number on which the server is listening.</param>
        /// <remarks>
        /// This constructor sets up a TCP server at the specified IP address and port.
        /// It also subscribes to various server events such as ClientConnected, ClientDisconnected, DataReceived, and DataSent.
        /// If the IP address is null or whitespace, or if the port is 0, the constructor returns without setting up the server.
        /// Any exceptions that occur during the setup are logged and then rethrown.
        /// </remarks>
        public TcpTransmitter(string ipAddress, int port)
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("TcpTransmitter");

            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP address cannot be null or whitespace.", nameof(ipAddress));
            if (port == 0)
                throw new ArgumentException("Port cannot be zero.", nameof(port));
            _logger = NeutronCore.Global.Logger.SetupLogger("TcpTransmitter");
            try
            {
                _logger.LogDetailAsync($"Startup: {ipAddress}:{port}").Wait();
                _server = new SimpleTcpServer(ipAddress, port);
                _server.Events.ClientConnected += Events_ClientConnected;
                _server.Events.ClientDisconnected += Events_ClientDisconnected;
                _server.Events.DataReceived += Events_DataReceived;
                _server.Events.DataSent += Events_DataSent;
                _server.Start();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Startup: {ex.Message}").Wait();
              //   throw; rethrow the exception after logging it
            }
        }


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
                _ = _logger.LogDetailAsync($"{ex}");
            }
        }

        private async void Events_DataSent(object sender, DataSentEventArgs e)
        {
            try
            {
                await _logger.LogDetailAsync($"Data Sent: {e.BytesSent}");
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Exception in Events_DataSent: {ex}");
            }
        }
        /// <summary>
        /// Handles the DataReceived event from the SimpleTcpServer.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A DataReceivedEventArgs that contains the event data.</param>
        /// <remarks>
        /// This method is triggered when data is received from the TCP client.
        /// It logs the received data and initiates its processing.
        /// </remarks>
        private async void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {

            var data = e.Data.ToArray();
            var text = Encoding.UTF8.GetString(data);

            await _logger.LogDetailAsync($"IP Port: [{e.IpPort}]  Data: {text}");

            ProcessDataReceived(text);
        }
        /// <summary>
        /// Processes the received data from the TCP connection.
        /// </summary>
        /// <param name="text">The received data as a string.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        /// <remarks>
        /// This method checks for specific patterns in the received data and responds accordingly.
        /// It logs the details of the received data and sends an acknowledgement if necessary.
        /// </remarks>
        private void ProcessDataReceived(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var character = ControlCharacters.ACK;  //  '\x06'; // instead of Convert.ToChar(6)

            var index = text.IndexOf(character);
            if (index != -1)
            {
                _ = _logger.LogDetailAsync($"ACK INDEX: {index}");
            }

            _ = _logger.LogDetailAsync($"ProcessDataReceived TEXT: {text}  LENGTH: {text.Length}");

            if (text.Length >= 7)
            {
                var command = text.Substring(1, 6);

                if (text.Contains("OC"))
                {
                    _ = _logger.LogDetailAsync($"Text Contains OC Send Response Command: {command}");
                    SendAck(command);
                }

                if (text.Contains("33") && text.Length > 10)
                {
                    _ = _logger.LogDetailAsync($"Text Contains 33 Send Response Command: {command}");
                    SendAck(command);
                }
            }
        }

        private void Events_ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            _ = _logger.LogDetailAsync($"IP Port: [{e.IpPort}] Client Disconnected Reason: {e.Reason}");
            IsClientConnected = false;
        }

        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            ClientIpPort = e.IpPort;
            _ = _logger.LogDetailAsync($"IP Port: [{e.IpPort}] Client Connected -- Disconnect Reason: {e.Reason}");
            IsClientConnected = true;
        }

        public void SendData(string value)
        {

            try
            {
                // once a client has connected...
                var command = new Put2LightCommand().GetCommand(value);
                _ = _logger.LogDetailAsync($"SEND DATA SEND DATA SEND DATA COMMAND: {command}");

                if (_server.IsListening)
                {
                    if (IsClientConnected)
                    {
                        _server.SendAsync(ClientIpPort, command);
                        // await Task to let the displays turn on before sending the next command
                        //await Task.Delay(100);
                        _ = _logger.LogDetailAsync($"SendData Command: {command.StringToByteArray().ByteArrayToHexString()}{Environment.NewLine}");
                    }
                    else
                    {
                        _ = _logger.LogDetailAsync($"Client is NOT Connected.");
                    }
                }
                else
                {
                    _ = _logger.LogDetailAsync($"Server is NOT Listening.");
                }
            }
            catch (SocketException ex)
            {
                _ = _logger.LogDetailAsync($"SendData SocketException: {ex.Message}");
                throw;
            }
            catch (IOException ex)  
            {
                _ = _logger.LogDetailAsync($"SendData IOException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"SendData Exception: {ex.Message}");
                    throw;
            }
            _ = _logger.LogDetailAsync($"SendData END");
        }

        private void SendAck(string cmd)
        {
            _ = _logger.LogDetailAsync($"Send ACK: {cmd}");
            
            var command = new Put2LightCommand().GetAck(cmd);
            
            _ = _logger.LogDetailAsync($"Send ACK: {command}{Environment.NewLine}");

            if (_server != null && ClientIpPort != null)
            {
                _server.SendAsync(ClientIpPort, command);
            }

            _ = _logger.LogDetailAsync($"Send ACK: {command.StringToByteArray().ByteArrayToHexString()}{Environment.NewLine}");
        }

    }
}
