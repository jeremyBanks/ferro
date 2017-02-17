﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Ferro
{
    public class PeerConnector
    {
        // We probably want to keep this private and potentially constant
        // This is the port we'll be listening on
        private Int32 myPort = 6881;
        private IPAddress myIpAddress;
        private byte[] fixedHeader = new byte[20];
        private byte[] zeroBuffer = new byte[8];
        private byte[] peerId = new byte[20];

        public PeerConnector(IPAddress ipAddress)
        {
            fixedHeader[0] = Convert.ToByte(19);
            "BitTorrent protocol".ToASCII().CopyTo(fixedHeader, 1);
            myIpAddress = ipAddress;
            peerId.FillRandom();
        }

        public void Handshake(IPAddress peerIP, Int32 peerPort, byte[] infoHash)
        {
            TcpListener connection = new TcpListener(myIpAddress, myPort);
            connection.Start();

            TcpClient client = new TcpClient();
            client.ConnectAsync(peerIP, peerPort).Wait();

            if (!client.Connected)
            {
                throw new Exception("Failed to connect to peer.");
            }

            Console.WriteLine("Connected to peer.");
            

            // Put all of our handshake data into a byte array
            byte[] handshake = new byte[68];
            fixedHeader.CopyTo(handshake, 0);
            zeroBuffer.CopyTo(handshake, fixedHeader.Length);
            infoHash.CopyTo(handshake, fixedHeader.Length + zeroBuffer.Length);
            peerId.CopyTo(handshake, fixedHeader.Length + zeroBuffer.Length + infoHash.Length);

            Console.WriteLine(handshake.FromASCII());

            NetworkStream stream = client.GetStream();
            stream.Write(handshake);

            if (!stream.CanRead)
            {
                throw new Exception("Unable to read from the network stream.");
            }

            byte[] response = new byte[256];
            stream.Read(response, 0, response.Length);
            Console.WriteLine(response.FromASCII());

            // we probably want to get rid of this in the future, when there's a proceding action
            connection.Stop();
        }
    }
}
