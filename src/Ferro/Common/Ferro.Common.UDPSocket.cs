using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Ferro.Common
{
    // Dumb async wrapper over the UDP Socket interface.
    class UDPSocket : IDisposable {
        readonly IPEndPoint localEndPoint;
        private Socket dotnetSocket;

        ILogger Logger { get; } = ApplicationLogging.CreateLogger<UDPSocket>();

        public UDPSocket(IPEndPoint localEndPoint) {
            this.localEndPoint = localEndPoint;
            dotnetSocket = new Socket(
                AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            dotnetSocket.Bind(localEndPoint);
        }

        public void SendTo(byte[] data, IPEndPoint remoteEndPoint) {
            var sent = dotnetSocket.SendTo(data, remoteEndPoint);
            if (sent != data.Length) {
                throw new Exception($"Failed to send all data to {remoteEndPoint}.");
            }
        }

        public Task<ReceivedPacket> ReceiveAsync() {
            var taskSource = new TaskCompletionSource<ReceivedPacket>();
            
            // I'd recycle but would have thread-safety concerns.
            byte[] buffer = new byte[65507];

            SocketAsyncEventArgs responseHandling = new SocketAsyncEventArgs();
            responseHandling.SetBuffer(buffer, 0, buffer.Length);
            responseHandling.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            
            Action onReceived = () => {
                if (responseHandling.SocketError != SocketError.Success) {
                    Logger.LogWarning($"UDP: Got socket error when trying to read packet: {responseHandling.SocketError}");
                    Task.Run(async () => {
                        // try again, really hackily
                        taskSource.TrySetResult(await ReceiveAsync());
                    });
                }

                var data = new byte[responseHandling.BytesTransferred];
                Array.Copy(buffer, 0, data, 0, responseHandling.BytesTransferred);
                
                taskSource.SetResult(new ReceivedPacket {
                    Data = data,
                    Source = (IPEndPoint) responseHandling.RemoteEndPoint
                });
            };

            responseHandling.Completed += (_1, _2) => onReceived();

            var willRaiseEvent = dotnetSocket.ReceiveFromAsync(responseHandling);
            if (!willRaiseEvent) {
                onReceived();
            }

            return taskSource.Task;
        }

        public class ReceivedPacket {
            public byte[] Data;
            public IPEndPoint Source;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                dotnetSocket.Dispose();

                disposedValue = true;
            }
        }

        ~UDPSocket() {
          Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}