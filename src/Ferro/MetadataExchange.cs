using System;
using System.Collections.Generic;
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

            while (true) {
                // var initialRequest = ConstructMessage(extCode, 0, 0);
                // Console.WriteLine("Sending message: " + initialRequest.ToHuman());
                // stream.Write(initialRequest);

                Int32 theirLength = 0;
                // Read lengths until we get a non-zero (non-keepalive) length.
                while (theirLength == 0) {
                    var theirPrefix = stream.ReadBytes(4);
                    theirLength = theirPrefix.Decode32BitInteger();
                    if (theirLength == 0) {
                        Console.WriteLine("Got keepalive. Let's send our own!");
                        stream.Write(new byte[4]);
                    }
                }

                Console.WriteLine("Their length: " + theirLength);
                var peerResponse = stream.ReadBytes(theirLength);
                var responseTypeId = peerResponse[0];
                switch (responseTypeId)
                {  
                    case 20:
                        Console.WriteLine("It's an extension message! Hurrah!");
                        // TODO: handle it, then return the result so we stop reading shit
                    break;

                    case 0:
                        Console.WriteLine("It's a choke message! :(");
                    break;

                    case 1:
                        Console.WriteLine("It's an unchoke message! :D");
                    break;

                    case 2:
                        Console.WriteLine("It's an interested message! <3");
                    break;

                    case 4:
                        Console.WriteLine("It's a not interested message! </3");
                    break;

                    default:
                        Console.WriteLine($"Unexpected message type {responseTypeId}; aborting.");
                    break;
                }
            }
        }


        // For messages with msgType 0 (request) and 2 (reject)
        public static byte[] ConstructMessage(int extCode, int msgType, int piece)
        {
            var messageDict = new Dictionary<byte[], object>();
            messageDict["msg_type".ToASCII()] = (Int64)msgType;
            messageDict["piece".ToASCII()] = (Int64)piece;
            var encodedMsg = Bencoding.Encode(messageDict);

            var length = (encodedMsg.Length + 2).EncodeBytes();
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
