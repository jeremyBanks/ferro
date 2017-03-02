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
        // extCode refers to the value peer has associated with ut_metadata
        public void RequestMetadata(NetworkStream stream, TcpClient connection, int extCode)
        {
            if (!connection.Connected)
            {
                throw new Exception("Disconnected from peer after handshake.");
            }

            var initialRequest = ConstructMessage(extCode, 0, 0);
            stream.Write(initialRequest);

            var test = new byte[4];
            stream.Read(test, 0, 4);
            Console.WriteLine(test.ToHuman());
        }

        // For messages with msgType 0 (request) and 2 (reject)
        public static byte[] ConstructMessage(int extCode, int msgType, int piece)
        {
            var messageDict = new Dictionary<byte[], object>();
            messageDict["msg_type".ToASCII()] = (Int64)msgType;
            messageDict["piece".ToASCII()] = (Int64)piece;
            var encodedMsg = Bencoding.Encode(messageDict);

            var length = BitConverter.GetBytes(encodedMsg.Length + 2);
            Array.Reverse(length);

            var message = new byte[encodedMsg.Length + 6];
            length.CopyTo(message, 0);
            message[4] = 20;
            message[5] = (byte)extCode;
            encodedMsg.CopyTo(message, 6);

            return message;
        }

        // For messages with msgType 1 (data)
        public static byte[] ConstructMessage(int extCode, int msgType, int piece, int length)
        {
            return new byte[4];
        }
    }
}
