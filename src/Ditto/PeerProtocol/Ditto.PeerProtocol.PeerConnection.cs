﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using Ditto.Common;

namespace Ditto.PeerProtocol
{
    public class PeerConnection
    {
        readonly private Int32 myPort = 6881;
        readonly private IPAddress myIpAddress;
        readonly private byte[] peerId = new byte[20].FillRandom();

        private bool extensionsEnabled = false;
        private bool theirExtensionsEnabled = false;

        ILogger logger { get; } = GlobalLogger.CreateLogger<PeerConnection>();

        public PeerConnection(IPAddress ipAddress)
        {
            myIpAddress = ipAddress;
            "-FR0001-".ToASCII().CopyTo(peerId, 0);
        }

        public void InitiateHandshake(IPAddress peerIP, Int32 peerPort, byte[] infoHash)
        {
            logger.LogInformation("Our peer id: " + peerId.ToHuman());
            var fixedHeader = new byte[20];
            fixedHeader[0] = (byte) 19;
            "BitTorrent protocol".ToASCII().CopyTo(fixedHeader, 1);

            var bufferBitfield = new byte[8];
            bufferBitfield[5] = (byte) 16;
            extensionsEnabled = true;

            TcpClient connection = new TcpClient();
            connection.ConnectAsync(peerIP, peerPort).Wait();

            if (!connection.Connected)
            {
                throw new Exception("Failed to connect to peer.");
            }

            var initialHandshake = new byte[68];
            fixedHeader.CopyTo(initialHandshake, 0);
            bufferBitfield.CopyTo(initialHandshake, fixedHeader.Length);
            infoHash.CopyTo(initialHandshake, fixedHeader.Length + bufferBitfield.Length);
            peerId.CopyTo(initialHandshake, fixedHeader.Length + bufferBitfield.Length + infoHash.Length);

            logger.LogInformation(LoggingEvents.HANDSHAKE_OUTGOING, "Sending our handshake to " + peerIP + ":" + peerPort);
            using (var stream = connection.GetStream())
            {
                stream.Write(initialHandshake);

                logger.LogInformation(LoggingEvents.HANDSHAKE_INCOMING, "Received response from peer.");
                
                var theirFixedHeader = stream.ReadBytes(20);
                if (!theirFixedHeader.SequenceEqual(fixedHeader))
                {
                    throw new Exception("Peer failed to return fixed header; aborting connection.");
                }

                var theirBuffer = stream.ReadBytes(8);
                if (theirBuffer[5] == 16)
                {
                    theirExtensionsEnabled = true;
                }

                var theirInfoHash = stream.ReadBytes(20);
                logger.LogInformation(LoggingEvents.HANDSHAKE_INCOMING, "Peer's infohash is: " + theirInfoHash.ToHuman());
                if (!theirInfoHash.SequenceEqual(infoHash))
                {
                    throw new Exception("Peer failed to return a matching infohash; aborting connection.");
                }

                var theirPeerId = stream.ReadBytes(20);
                logger.LogInformation(LoggingEvents.HANDSHAKE_INCOMING, "The peer's ID is " + theirPeerId.ToHuman());

                if (extensionsEnabled && theirExtensionsEnabled)
                {
                    var theirExtensionHeader = GetPeerExtensionHeader(stream);
                    var decodedExtensionHeader = Bencoding.DecodeDict(theirExtensionHeader);
                    var theirExtensions = decodedExtensionHeader.GetDict("m");

                    logger.LogInformation(LoggingEvents.EXTENSION_HEADER_IN, "Peer's extension header:" + Environment.NewLine + Bencoding.ToHuman(theirExtensionHeader));

                    var extensionDict = GenerateExtentionDict();
                    var extensionHeader = new byte[extensionDict.Length + 6];
                    var lengthPrefix = (extensionDict.Length + 2).EncodeBytes();
                    Array.Copy(lengthPrefix, extensionHeader, 4);
                    extensionHeader[4] = 20;
                    extensionHeader[5] = 0;
                    extensionDict.CopyTo(extensionHeader, 6);
                    stream.Write(extensionHeader);

                    logger.LogInformation(LoggingEvents.EXTENSION_HEADER_OUT, "Sending our extension header: " + Environment.NewLine + Bencoding.ToHuman(extensionDict));

                    // Send interested message
                    stream.Write(1.EncodeBytes());
                    stream.Write(new byte[1]{2});
                    logger.LogInformation(LoggingEvents.PEER_PROTOCOL_MSG, "Sent interested message.");

                    if (theirExtensions.ContainsKey("ut_metadata")) {
                        logger.LogInformation(LoggingEvents.METADATA_EXCHANGE, "They also support metadata exchange. Lets try that.");
                        var theirMetadataExtensionId = (byte) theirExtensions.Get("ut_metadata");

                        var metadata = new MetadataExchange(decodedExtensionHeader.Get("metadata_size"));
                        metadata.RequestMetadata(stream, connection, 2, theirMetadataExtensionId, infoHash);
                    }
                }
            }
        }

        private byte[] GetPeerExtensionHeader(NetworkStream stream)
        {
            var lengthPrefix = stream.ReadBytes(4);
            var length = lengthPrefix.Decode32BitInteger();

            var extensionResponse = stream.ReadBytes(length);
            if (extensionResponse[0] != 20)
            {
                stream.Dispose();
                throw new Exception("Unexpected payload in handshake extension; Aborting.");
            }
            if (extensionResponse[1] != 0)
            {
                stream.Dispose();
                throw new Exception("Unexpected extended message id; Aborting.");
            }

            var theirExtensionDict = new byte[length - 2];
            Array.Copy(extensionResponse, 2, theirExtensionDict, 0, length - 2);

            return theirExtensionDict;
        }

        private byte[] GenerateExtentionDict()
        {
            var extensionDict = new Dictionary<byte[], object>();
            var supportedExtensions = new Dictionary<byte[], object>();
            
            supportedExtensions.Set("ut_metadata", 2);
            extensionDict.Set("m", supportedExtensions);
            // metadata_size is unnecessary if we are requesting. If we're providing metadata, we should add this. 
            // extensionDict.Set("metadata_size", 0);
            extensionDict.Set("p", myPort);
            extensionDict.Set("v", "Ditto 0.1.0");

            return Bencoding.Encode(extensionDict);
        }
    }
}