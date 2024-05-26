using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronCore.Extensions;
using NeutronCore.StaticClasses;
using NeutronEvents;
using SuperSimpleTcp;
using AsyncAwaitBestPractices;


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
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly int _transmitDelay;
        public CancellationTokenSource _cancellationTokenSource { get; set; }
        public CancellationToken _cancellationToken { get; set; }
        public object sendLock { get; set; }

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

        private string _currentCommand;

        private SimpleTcpServer _server;

        private IDynamicLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTransmitter"/> class.
        /// </summary>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port number on which the server is listening.</param>
        /// <param name="transmitDelay"></param>
        /// <remarks>
        /// This constructor sets up a TCP server at the specified IP address and port.
        /// It also subscribes to various server events such as ClientConnected, ClientDisconnected, DataReceived, and DataSent.
        /// If the IP address is null or whitespace, or if the port is 0, the constructor returns without setting up the server.
        /// Any exceptions that occur during the setup are logged and then rethrown.
        /// </remarks>
        public TcpTransmitter(string ipAddress, int port, int transmitDelay)
        {
            _ipAddress = ipAddress;
            _port = port;
            _transmitDelay = transmitDelay;

            Init();
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("TcpTransmitter");
            _currentCommand = string.Empty;



            if (string.IsNullOrWhiteSpace(_ipAddress))
                throw new ArgumentException("IP address cannot be null or whitespace.", nameof(_ipAddress));

            _logger.LogDetailAsync($"IP address cannot be null or whitespace. {nameof(_ipAddress)}").SafeFireAndForget();
            if (_port == 0)
                throw new ArgumentException("Port cannot be zero.", nameof(_port));

            _logger.LogDetailAsync($"Port cannot be zero. {nameof(_ipAddress)}").SafeFireAndForget();
            try
            {
                _logger.LogDetailAsync($"Startup: {_ipAddress}:{_port}").SafeFireAndForget();

                _server = new SimpleTcpServer(_ipAddress, _port);
                _server.Events.ClientConnected += Events_ClientConnected;
                _server.Events.ClientDisconnected += Events_ClientDisconnected;
                _server.Events.DataReceived += Events_DataReceived;
                _server.Events.DataSent += Events_DataSent;

                _server.Keepalive.EnableTcpKeepAlives = true;
                _server.Keepalive.TcpKeepAliveInterval = 5;      // seconds to wait before sending subsequent keepalive
                _server.Keepalive.TcpKeepAliveTime = 5;          // seconds to wait before sending a keepalive
                _server.Keepalive.TcpKeepAliveRetryCount = 5;    // number of failed keepalive probes before terminating connection

                _server.Start();

            }

            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Startup Exception: {ex.Message}").SafeFireAndForget();
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
                _logger.LogDetailAsync($"{ex}").SafeFireAndForget();
            }
        }

        private async void Events_DataSent(object sender, DataSentEventArgs e)
        {
            try
            {
                _logger.LogDetailAsync($"Data Sent: {e.BytesSent}").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception in Events_DataSent: {ex}").SafeFireAndForget();
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

            _logger.LogDetailAsync($"IP Port: [{e.IpPort}]  Data: {text}").SafeFireAndForget();

            await ProcessDataReceived(text);
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
        private async Task ProcessDataReceived(string text)
        {
            var now = DateTime.Now;
            await _logger.LogDetailAsync($"{now.Second}:{now.Millisecond}  PROCESS DATA RECEIVED TEXT: {text}");
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                _logger.LogDetailAsync($"{now.Second}:{now.Millisecond}  PROCESS DATA RECEIVED _currentCommand Value = text: {_currentCommand} = {text}").SafeFireAndForget();

                if (!string.IsNullOrEmpty(_currentCommand))
                {
                    if (text.Contains(_currentCommand))
                    {
                        _currentCommand = string.Empty;
                    }
                }

                now = DateTime.Now;

                _logger.LogDetailAsync($"{now.Second}:{now.Millisecond}  PROCESS DATA RECEIVED _currentCommand Value: {_currentCommand}").SafeFireAndForget();

                //var character = ControlCharacters.ACK;  //  '\x06'; // instead of Convert.ToChar(6)

                //var index = text.IndexOf(character);
                //if (index != -1)
                //{
                //    await _logger.LogDetailAsync($"ACK INDEX: {index}");
                //}

                //await _logger.LogDetailAsync($"ProcessDataReceived TEXT: {text}  LENGTH: {text.Length}");

                //if (text.Length >= 7)
                //{
                //    var command = text.Substring(1, 4);

                //    if (text.Contains("OC"))
                //    {
                //        await _logger.LogDetailAsync($"Text Contains OC Send Response Command: {command}");
                //        await SendAck(command);
                //    }

                //    if (text.Contains("33") && text.Length > 10)
                //    {
                //        await _logger.LogDetailAsync($"Text Contains 33 Send Response Command: {command}");
                //        await SendAck(command);
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, $"Error Processing Received Data: {Environment.NewLine}{ex.Message}");
            }
        }

        private async void Events_ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            _logger.LogDetailAsync($"IP Port: [{e.IpPort}] Client Disconnected ").SafeFireAndForget();
            IsClientConnected = false;
            await Task.Delay(10, _cancellationToken);
            Mediator.GetInstance().OnTransmitStateChanged(this, false);
        }

        private async void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            ClientIpPort = e.IpPort;
            _logger.LogDetailAsync($"IP Port: [{e.IpPort}] Client Connected ").SafeFireAndForget();
            IsClientConnected = true;
            await Task.Delay(10, _cancellationToken);
            Mediator.GetInstance().OnTransmitStateChanged(this, true);
        }

        public async Task SendDataAsync(string value, bool isAck = false)
        {
            var counter = 0;
            _logger.LogDetailAsync($"Send Data Value: {value}").SafeFireAndForget();
            try
            {
                while (true)
                {
                    if (string.IsNullOrEmpty(_currentCommand))
                    {
                        // once a client has connected...

                        var command = new Put2LightCommand().GetCommand(value);
                        _currentCommand = SetCurrentCommand(value);
                        _logger.LogDetailAsync($"PROCESS DATA RECEIVED Put2LightCommand: {command}")
                            .SafeFireAndForget();

                        // NO ACK as of 03/07/2024
                        //  Will need ACK processing when push buttons are enabled

                        //if (isAck)
                        //{
                        //    command = new Put2LightCommand().GetAck(value);
                        //    _currentCommand = string.Empty;
                        //    _ = _logger.LogDetailAsync($"Send ACK: {command}{Environment.NewLine}");
                        //}
                        //else
                        //{
                        //    command = new Put2LightCommand().GetCommand(value);
                        //    _currentCommand = SetCurrentCommand(value);
                        //    _ = _logger.LogDetailAsync($"PROCESS DATA RECEIVED Put2LightCommand: {command}");
                        //}

                        if (_server.IsListening)
                        {
                            _logger.LogDetailAsync($"Server is Listening: TRUE").SafeFireAndForget();

                            if (IsClientConnected)
                            {
                                _logger.LogDetailAsync($"Client is Connected: TRUE  IP:PORT: {ClientIpPort}")
                                    .SafeFireAndForget();
                                await _server.SendAsync(ClientIpPort, command, _cancellationToken);

                                _logger.LogDetailAsync(
                                        $"SendData Command: {command.StringToByteArray().ByteArrayToHexString()}{Environment.NewLine}")
                                    .SafeFireAndForget();
                            }
                            else
                            {
                                _logger.LogDetailAsync($"Client is Connected: FALSE").SafeFireAndForget();
                            }
                        }
                        else
                        {
                            _logger.LogDetailAsync($"Server is Listening: FALSE").SafeFireAndForget();
                        }
                        break;
                    }
                    else
                    {
                        var now = DateTime.Now;
                        _logger.LogDetailAsync($"{now.Second}:{now.Millisecond}  PROCESS DATA RECEIVED _currentCommand Still has Value: {_currentCommand}.  New Command Value: {value}  -- Waiting 20MS").SafeFireAndForget();
                        await Task.Delay(10, _cancellationToken);
                        counter += 10;
                        if (counter >= _transmitDelay)
                        {
                            // Mediator.GetInstance().OnGeneralError(this, $"Display Number {_currentCommand} is not responding.");
                            _currentCommand = string.Empty;
                            now = DateTime.Now;
                            _logger.LogDetailAsync($"{now.Second}:{now.Millisecond}  PROCESS DATA RECEIVED _currentCommand Still has Value: {_currentCommand}. Breaking Out of Process.").SafeFireAndForget();
                            // break;
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                _logger.LogDetailAsync($"SendData SocketException: {ex.Message}").SafeFireAndForget();
                throw;
            }
            catch (IOException ex)
            {
                _logger.LogDetailAsync($"SendData IOException: {ex.Message}").SafeFireAndForget();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"SendData Exception: {ex.Message}").SafeFireAndForget();
                throw;
            }
            _logger.LogDetailAsync($"Send Data END").SafeFireAndForget();
        }

        private string SetCurrentCommand(string value)
        {
            var currentCommand = string.Empty;
            if (value.Length < 6) return currentCommand;

            var now = DateTime.Now;
            _logger.LogDetailAsync($"{now.Second}:{now.Millisecond}  PROCESS DATA RECEIVED SETCURRENTCOMMAND IN_VALUE: {value}").SafeFireAndForget();

            if (value.Substring(2, 2) == "14"
                || value.Substring(2, 2) == "27"
                || value.Substring(2, 2) == "10")
            {
                currentCommand = value.Substring(0, 4);
            }
            else
            {
                currentCommand = value.Substring(0, 6);
            }

            now = DateTime.Now;
            _logger.LogDetailAsync($"{now.Second}:{now.Millisecond}  PROCESS DATA RECEIVED SETCURRENTCOMMAND OUT_VALUE: {currentCommand}").SafeFireAndForget();
            return currentCommand;
        }

        private async Task SendAck(string cmd)
        {
            _logger.LogDetailAsync($"Send ACK: {cmd}").SafeFireAndForget();

            await SendDataAsync(cmd, true);
        }

        private async Task MonitorIsClientConnected()
        {
            // while (_server.IsListening)
            while (IsClientConnected)
            {
                // Simulate some work or condition to check the variable
                await Task.Delay(5000, _cancellationToken);
                Mediator.GetInstance().OnIsClientConnected(this, IsClientConnected);
            }
        }
    }
}
