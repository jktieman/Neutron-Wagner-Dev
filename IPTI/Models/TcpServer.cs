using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using AlliedLogger;
using EthernetTransmitter;
using AsyncAwaitBestPractices;
using NeutronCore.Extensions;
using NeutronEvents;

namespace IPTI.Models
{
    public class TcpServer
    {
        // Propagates cancellation notifications to all CancellationToken objects created from it.
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private int _transmitDelay;
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        
        private TcpListener _listener;
        // Dictionary to store the clients connected to the server.
        // Key: Client's remote endpoint.
        // Value: Metadata object containing information about the client.
        private ConcurrentDictionary<string, Metadata> _connectedClients = new ConcurrentDictionary<string, Metadata>();
        
        private IDynamicLogger _logger;
        private bool _monitorTransmitter = true;



        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="ipAddress">The IP address of the TCP server.</param>
        /// <param name="port">The port number of the TCP server.</param>
        /// <param name="transmitDelay">The delay in milliseconds between transmissions.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided IP address is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the provided IP address is not in a correct format.</exception>
        public TcpServer(string ipAddress, int port, int transmitDelay)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new ArgumentNullException(nameof(ipAddress), @"IP address cannot be null or empty.");
            }
            if (!IPAddress.TryParse(ipAddress, out _ipAddress))
            {
                throw new FormatException(@"IP address is not in a correct format.");
            }

            _port = port;
            _transmitDelay = transmitDelay;
            // Initialize the CancellationTokenSource object.
            _tokenSource = new CancellationTokenSource();
            Init();
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("TcpServer");
            _listener = new TcpListener(_ipAddress, _port);
            _token = _tokenSource.Token;
            _listener.Start();

            Task.Run(() => AcceptConnections(_token), _token);
           // Task.Run( () => MonitorTransmitter(_token), _token);
        }

        public TcpListener Listener
        {
            get { return _listener; }
            private set { _listener = value; }
        }

        public void DisposeServer()
        {
            try
            {
                _monitorTransmitter = false;
                
                if (_connectedClients != null && _connectedClients.Count > 0)
                {
                    foreach (var curr in _connectedClients)
                    {
                        _logger.LogDetailAsync("Disconnecting " + curr.Key).SafeFireAndForget();
                        curr.Value.Dispose();
                    }
                }

                _tokenSource.Cancel();
                _tokenSource.Dispose();

                if (_listener != null && _listener.Server != null)
                {
                    _listener.Server.Close();
                    _listener.Server.Dispose();
                }

                if (_listener != null) _listener.Stop();
            }
            catch (Exception e)
            {
                _logger.LogDetailAsync(
                    Environment.NewLine +
                    "Dispose Exception:" +
                    Environment.NewLine +
                    e.ToString() +
                    Environment.NewLine).SafeFireAndForget();
            }
        }

        public List<string> ListClients()
        {
            _logger.LogDetailAsync("Clients: " + _connectedClients.Count).SafeFireAndForget();
            var clients = new List<string>();

            foreach (var curr in _connectedClients)
            {
                clients.Add(curr.Key);
                _logger.LogDetailAsync("  " + curr.Key).SafeFireAndForget();
            }

            return clients;

        }

        public void SendData(string value)
        {
            try
            {

                // ListClients();
                _logger.LogDetailAsync($"Value: {value} ").SafeFireAndForget();

                // if there are any _clients
                // return the first _client
                var key = _connectedClients.Keys.FirstOrDefault();
                if (string.IsNullOrEmpty(key)) return;
                _logger.LogDetailAsync($"Key Value: {key} ").SafeFireAndForget();
                var md = _connectedClients[key];

                var command = new Put2LightCommand().GetCommand(value);
                // _currentCommand = SetCurrentCommand(value);
                _logger.LogDetailAsync($"Command: {command} ").SafeFireAndForget();
                var dataBytes = Encoding.UTF8.GetBytes(command);

                lock (md.SendLock)
                {
                    if (!md.NetworkStream.CanWrite)
                    {
                        md.NetworkStream = md.TcpClient.GetStream();
                    }
                    md.NetworkStream.WriteAsync(dataBytes, 0, dataBytes.Length, _token);
                    md.NetworkStream.FlushAsync(_token);
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }
        }

        public void RemoveClient()
        {
            ListClients();
            _logger.LogDetailAsync("Client: ").SafeFireAndForget();
            var key = Console.ReadLine();
            if (String.IsNullOrEmpty(key)) return;
            var md = _connectedClients[key];

            _logger.LogDetailAsync(md.TcpClient.Client.RemoteEndPoint.ToString()).SafeFireAndForget();
            md.Dispose();
        }

        public async Task AcceptConnections(CancellationToken token)
        {
            _logger.LogDetailAsync($"Accept Connection Start: {DateTime.Now.Millisecond}").SafeFireAndForget();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _logger.LogDetailAsync($"Accept Connection LOOP TIME: {DateTime.Now.Millisecond}").SafeFireAndForget();
                    var client = await _listener.AcceptTcpClientAsync();
                    _logger.LogDetailAsync($"Got a Client: {DateTime.Now.Millisecond}").SafeFireAndForget();
                    var md = new Metadata(client);
                    
                    _connectedClients.TryAdd(client.Client.RemoteEndPoint.ToString(), md);

                    Mediator.GetInstance().OnIsClientConnected(this, true);
                    _logger.LogDetailAsync($"Got a Client: {client.Client.RemoteEndPoint}").SafeFireAndForget();


                    await Task.Run(() => DataReceiver(md), md.Token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }
        }

        public async Task DataReceiver(Metadata md)
        {
            var header = $"[ {md.TcpClient.Client.RemoteEndPoint} ]";
            _logger.LogDetailAsync(header + " Data Receiver Started!").SafeFireAndForget();

            try
            {
                while (true)
                {
                    _logger.LogDetailAsync($"LOOP TIME: {DateTime.Now.Millisecond}").SafeFireAndForget();

                    if (!IsClientConnected(md.TcpClient))
                    {
                        _logger.LogDetailAsync(header + " client no longer connected").SafeFireAndForget();
                        break;
                    }

                    lock (md.SendLock)
                    {
                        if (_token.IsCancellationRequested)
                        {
                            _logger.LogDetailAsync(header + " cancellation requested").SafeFireAndForget();
                            break;
                        }
                    }

                    var data = await DataReadAsync(md.TcpClient, _token);

                    if (data == null || data.Length < 1)
                    {
                        await Task.Delay(30, _token);
                        continue;
                    }
                    _logger.LogDetailAsync($"Byte Data as String: {Encoding.UTF8.GetString(data)}").SafeFireAndForget();
                    
                    //ProcessDataReceived(data);

                }
            }
            catch (Exception e)
            {
                _logger.LogDetailAsync(
                    Environment.NewLine +
                    header +
                    " DataReceiver Exception: " +
                    Environment.NewLine +
                    e.ToString() +
                    Environment.NewLine).SafeFireAndForget();
            }

            _logger.LogDetailAsync(header + @" data receiver terminating").SafeFireAndForget();

            md.Dispose();
        }

        private void ProcessDataReceived(byte[] data)
        {
            _logger.LogDetailAsync($"Byte Data as String: {Encoding.UTF8.GetString(data)}").SafeFireAndForget();
        }

        public async Task<byte[]> DataReadAsync(TcpClient client, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var stream = client.GetStream();
            if (!stream.CanRead || !stream.DataAvailable) return null;
            var buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                if (read <= 0)
                {
                    throw new SocketException();
                }
                return ms.ToArray();
            }
        }


        //public async Task<byte[]> DataReadAsync(TcpClient client, CancellationToken token)
        //{
        //    if (token.IsCancellationRequested) throw new OperationCanceledException();

        //    var stream = client.GetStream();
        //    if (!stream.CanRead) return null;
        //    if (!stream.DataAvailable) return null;

        //    var buffer = new byte[1024];
        //    using (var ms = new MemoryStream())
        //    {
        //        while (true)
        //        {
        //            var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);
        //            if (read > 0)
        //            {
        //                ms.Write(buffer, 0, read);
        //                return ms.ToArray();
        //            }
        //            else
        //            {
        //                throw new SocketException();
        //            }
        //        }
        //    }
        //}


        private async Task MonitorTransmitter(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                var key = _connectedClients.Keys.FirstOrDefault();
                if (string.IsNullOrEmpty(key)) return;
                _logger.LogDetailAsync($"Key Value: {key} ").SafeFireAndForget();
                var md = _connectedClients[key];
                var ready = IsClientConnected(md.TcpClient);
                Mediator.GetInstance().OnIsClientConnected(this, ready);
               await Task.Delay(100, token);
            }
        }

        public bool IsClientConnected(TcpClient client)
        {
            var result = false;
            try
            {
                if (client.Connected)
                {
                    _logger.LogDetailAsync($"IsClientConnected Client: {client.Client.LocalEndPoint.ToStringOrEmpty()}").SafeFireAndForget();
                    if ((client.Client.Poll(0, SelectMode.SelectWrite)) && (!client.Client.Poll(0, SelectMode.SelectError)))
                    {
                        var buffer = new byte[1];
                        if (client.Client.Receive(buffer, SocketFlags.Peek) == 0)
                        {
                            result = false;
                        }
                        else
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {

                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }

            return result;
        }
    }
}
