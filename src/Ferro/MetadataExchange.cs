using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Ferro
{
    public class MetadataExchange
    {
        public void SendInitialRequest(NetworkStream stream, TcpClient connection)
        {
            if (!stream.CanWrite || !connection.Connected)
            {
                throw new Exception("Disconnected from peer after handshake.");
            }

            var request = new Dictionary<byte[], object>();
            request["msg_type".ToASCII()] = (Int64) 0; // 0 here indicates an initial request
            request["piece".ToASCII()] = (Int64) 0;
            var encodedRequest = Bencoding.Encode(request);

            var message = new byte[encodedRequest.Length + 6];
            var lengthPrefix = BitConverter.GetBytes(encodedRequest.Length);
            Array.Reverse(lengthPrefix); // must be big-endian
            Array.Copy(lengthPrefix, message, 4);
            message[4] = 20;
            message[5] = 1;
            lengthPrefix.CopyTo(message, 6);
            stream.Write(message);

            var response = new byte[16384]; // each response is a piece of up to 16kb
            stream.Read(response, 0, 16384);

            var decodedResponse = Bencoding.Decode(response);
            Console.WriteLine(Bencoding.ToHuman(response));
        }
    }
}
