using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Ferro
{
    public class MetadataExchange
    { 
        // ourExtCode refers to the value we have associated with ut_metadata
        // theirExtCode refers to the value they have associated with ut_metadata
        public void RequestMetadata(NetworkStream stream, TcpClient connection, byte ourExtCode, byte theirExtCode)
        {
            if (!connection.Connected)
            {
                throw new Exception("Disconnected from peer after handshake.");
            }

            // Request the first piece.
            var initialRequest = ConstructMessage(ourExtCode, 0, 0);
            Console.WriteLine("Sending request for first metadata piece: " + initialRequest.ToHuman());
            stream.Write(initialRequest);

            while (true) {
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

                Console.WriteLine($"Got message with length {theirLength}.");
                var peerResponse = stream.ReadBytes(theirLength);
                var responseTypeId = peerResponse[0];
                switch (responseTypeId)
                {  
                    case 20:
                        Console.WriteLine("It's an extension message! Hurrah!");
                        // TODO: handle it, then return the result so we stop reading shit

                        var extensionId = peerResponse[1];

                        if (extensionId == theirExtCode) {
                            Console.WriteLine("It's a metadata exchange message!");
                            var data = peerResponse.Slice(2);
                            long dictSize;
                            var dict = Bencoding.DecodeFirst(data, out dictSize);
                            var postDict = data.Slice((Int32) dictSize);

                            Console.WriteLine($"Got BEP-9 {Bencoding.ToHuman(Bencoding.Encode(dict))} followed by {postDict.Length} bytes of data.");
                            HandleIncomingPiece(postDict);

                        } else {
                            Console.WriteLine($"Warning: it's an unexpected message type, ID {extensionId}.");
                        }

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

        private void HandleIncomingPiece(byte[] data)
        {
            Console.WriteLine($"Handling a piece of metadata, of length {data.Length}");
            dynamic piece = Bencoding.Decode(data);

            foreach (KeyValuePair<byte[], object> item in piece)
            {
                Console.WriteLine($"{item.Key.FromASCII()} : {item.Value.ToString()}");
            }
        }

        // For messages with msgType 0 (request) and 2 (reject)
        private static byte[] ConstructMessage(int ourExtCode, int msgType, int piece)
        {
            var messageDict = new Dictionary<byte[], object>();
            messageDict["msg_type".ToASCII()] = (Int64)msgType;
            messageDict["piece".ToASCII()] = (Int64)piece;
            var encodedMsg = Bencoding.Encode(messageDict);

            var length = (encodedMsg.Length + 2).EncodeBytes();

            var message = new byte[encodedMsg.Length + 6];
            length.CopyTo(message, 0);
            message[4] = 20;
            message[5] = (byte)ourExtCode;
            encodedMsg.CopyTo(message, 6);

            return message;
        }

        // For messages with msgType 1 (data)
        private static byte[] ConstructMessage(int ourExtCode, int msgType, int piece, int length)
        {
            return new byte[4];
        }
    }
}
