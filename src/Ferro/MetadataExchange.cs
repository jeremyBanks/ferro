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
            if (!connection.Connected)
            {
                throw new Exception("Disconnected from peer after handshake.");
            }

            var request = new Dictionary<byte[], object>();
            request["msg_type".ToASCII()] = (Int64) 0; // 0 here indicates an initial request
            request["piece".ToASCII()] = (Int64) 1;
            var encodedRequest = Bencoding.Encode(request);
            Console.WriteLine("request: ");
            Console.WriteLine(Bencoding.ToHuman(encodedRequest));

            var message = new byte[encodedRequest.Length + 6];
            var lengthPrefix = BitConverter.GetBytes(encodedRequest.Length + 2);
            Array.Reverse(lengthPrefix); // must be big-endian
            Array.Copy(lengthPrefix, message, 4);
            message[4] = 20;
            message[5] = 2;
            encodedRequest.CopyTo(message, 6);
            stream.Write(message);

            var responseLengthPrefix = new byte[4];
            stream.Read(responseLengthPrefix, 0, 4);
            var length = responseLengthPrefix.Decode32BitInteger();

            var response = new byte[length + 2]; // each response is a piece of up to 16kb
            stream.Read(response, 0, length + 2);

            if (response[0] != 20)
            {
                stream.Dispose();
                throw new Exception("Unexpected payload; Aborting.");
            }
            // will handle response[1] once we can reliably grab the peer's identifier for the ut_metadata protocol

            var responseBody = new byte[length];
            Array.Copy(response, 2, responseBody, 0, length);
            var decodedResponse = Bencoding.Decode(responseBody);
            Console.WriteLine(Bencoding.ToHuman(responseBody));
        }
    }
}
