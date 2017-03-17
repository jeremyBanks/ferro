using System;
using System.Collections.Generic;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using Ditto.Common;

namespace Ditto
{
    public class MetadataExchange
    {
        Int64 MetadataLength;

        ILogger logger { get; } = GlobalLogger.CreateLogger<MetadataExchange>();

        public MetadataExchange(Int64 metadataLength)
        {
            MetadataLength = metadataLength;
        }

        // ourExtCode refers to the value we have associated with ut_metadata
        // theirExtCode refers to the value they have associated with ut_metadata
        public byte[] GetMetadata(NetworkStream stream, TcpClient connection, byte ourExtCode, byte theirExtCode, byte[] infohash)
        {
            if (!connection.Connected)
            {
                throw new Exception("Disconnected from peer after handshake.");
            }

            var currentPiece = 0;
            var info = new VerifiedBytes(infohash, (Int32) MetadataLength, CommonPieceSizes.BEP9_METADATA);

            using (logger.BeginScope($"Metadata request for {infohash.ToHex()} from {connection.Client.RemoteEndPoint}"))
            {
                // Request the first piece.
                var initialRequest = ConstructRequestMessage(ourExtCode, 0);
                logger.LogInformation(LoggingEvents.METADATA_REQUEST, "Sending request for first metadata piece: " + initialRequest.ToHuman());
                stream.Write(initialRequest);

                while (true)
                {
                    Int32 theirLength = 0;
                    // Read lengths until we get a non-zero (non-keepalive) length.
                    while (theirLength == 0)
                    {
                        var theirPrefix = stream.ReadBytes(4);
                        theirLength = theirPrefix.Decode32BitInteger();
                        if (theirLength == 0)
                        {
                            logger.LogInformation(LoggingEvents.PEER_PROTOCOL_MSG, "Got keepalive. Let's send our own!");
                            stream.Write(new byte[4]);
                        }
                    }

                    logger.LogInformation(LoggingEvents.PEER_PROTOCOL_MSG, $"Got message with length {theirLength}.");
                    var peerResponse = stream.ReadBytes(theirLength);
                    var responseTypeId = peerResponse[0];
                    switch (responseTypeId)
                    {
                        case 20:
                            logger.LogInformation(LoggingEvents.EXTENSION_MESSAGE, "It's an extension message! Hurrah!");
                            var extensionId = peerResponse[1];

                            if (extensionId == theirExtCode)
                            {
                                logger.LogInformation(LoggingEvents.METADATA_RESPONSE_INCOMING, "It's a metadata exchange message!");
                                var data = peerResponse.Slice(2);
                                var dict = Bencoding.DecodeFirstDict(data, out Int64 dictSize);
                                var postDict = data.Slice((Int32)dictSize); // This is the metadata itself -- a bencoded dictionary of utf8 strings

                                if (dict.GetInt("piece") != currentPiece)
                                {
                                    throw new Exception($"Expected piece {currentPiece}. Instead, received {dict.GetInt("piece")}");
                                }

                                logger.LogInformation(
                                    LoggingEvents.METADATA_RESPONSE_INCOMING,
                                    $"Got BEP-9 {Bencoding.ToHuman(Bencoding.Encode(dict))} followed by {postDict.Length} bytes of data.\nStoring...");

                                info.ProvidePiece(currentPiece, postDict);
                                currentPiece++;

                                if (currentPiece == info.PieceCount)
                                {
                                    byte[] combinedPieces;
                                    try {
                                        combinedPieces = info.Result.Result;
                                    } catch (BytesVerificationException ex) {
                                        logger.LogWarning(LoggingEvents.METADATA_FAILURE, "metadata verification failed!", ex);
                                        throw new MetadataException("Metadata verification failed -- try another peer.");
                                    }

                                    logger.LogInformation(LoggingEvents.DATA_STORAGE_ACTION, "metadata verified! saving...");
                                    DataHandler.SaveMetadata(combinedPieces);
                                    logger.LogInformation(LoggingEvents.DATA_STORAGE_ACTION, "metadata saved.");
                                    return combinedPieces;
                                }

                                var request = ConstructRequestMessage(ourExtCode, currentPiece);
                                logger.LogInformation(LoggingEvents.METADATA_REQUEST, "Requesting the next piece of metadata...");
                                stream.Write(request);
                            }
                            else
                            {
                                logger.LogWarning(LoggingEvents.PEER_UNEXPECTED_RESPONSE, $"It's an unexpected message type, ID {extensionId}.");
                            }

                            break;

                        case 0:
                            logger.LogInformation(LoggingEvents.PEER_PROTOCOL_MSG, "It's a choke message! :(");
                            break;

                        case 1:
                            logger.LogInformation(LoggingEvents.PEER_PROTOCOL_MSG, "It's an unchoke message! :D");
                            break;

                        case 2:
                            logger.LogInformation(LoggingEvents.PEER_PROTOCOL_MSG, "It's an interested message! <3");
                            break;

                        case 4:
                            logger.LogInformation(LoggingEvents.PEER_PROTOCOL_MSG, "It's a not interested message! </3");
                            break;

                        default:
                            logger.LogWarning(LoggingEvents.PEER_UNEXPECTED_RESPONSE, $"Unexpected message type {responseTypeId}; ignoring.");
                            break;
                    }
                }
            }
        }

        // For messages with msgType 0 (request) and 2 (reject)
        private static byte[] ConstructMessage(int ourExtCode, int msgType, int piece)
        {
            var messageDict = new Dictionary<byte[], object>();
            messageDict.Set("msg_type", msgType);
            messageDict.Set("piece", piece);
            var encodedMsg = Bencoding.Encode(messageDict);

            var length = (encodedMsg.Length + 2).EncodeBytes();

            var message = new byte[encodedMsg.Length + 6];
            length.CopyTo(message, 0);
            message[4] = 20;
            message[5] = (byte) ourExtCode;
            encodedMsg.CopyTo(message, 6);

            return message;
        }

        private static byte[] ConstructRequestMessage(int ourExtCode, int piece)
        {
            return ConstructMessage(ourExtCode, 0, piece);
        }

        private static byte[] ConstructRejectMessage(int ourExtCode, int piece)
        {
            return ConstructMessage(ourExtCode, 2, piece);
        }

        // For messages with msgType 1 (data)
        private static byte[] ConstructRejectMessage(int ourExtCode, int piece, int length)
        {
            return new byte[4];
        }
    }

    public class MetadataException : Exception
    {
        public MetadataException(string message) : base(message) { }
    }
}
