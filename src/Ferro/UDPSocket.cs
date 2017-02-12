using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ferro
{
    // Dumb async wrapper over the UDP Socket interface.
    class UDPSocket {
        readonly IPEndPoint localEndPoint;
        private Socket dotnetSocket;

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
                    throw new Exception($"Got socket error {responseHandling.SocketError}");
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
    }
}