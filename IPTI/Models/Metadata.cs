using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IPTI.Models
{
    public class Metadata
    {
        public TcpClient TcpClient { get; set; }
        public NetworkStream NetworkStream { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken Token { get; set; }
        public object SendLock { get; set; }

        public Metadata(TcpClient client)
        {
            TcpClient = client;
            NetworkStream = TcpClient.GetStream();
            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
            SendLock = new object();
        }

        public void Dispose()
        {
            if (NetworkStream != null)
            {
                NetworkStream.Close();
                NetworkStream.Dispose();
            }

            TokenSource.Cancel();

            if (TcpClient != null)
            {
                TcpClient.Close();
                TcpClient.Dispose();
            }
        }
    }
}
