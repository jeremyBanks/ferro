namespace Ferro
{
    class Examples {
        public static void Main()
        {
            dhtClient();
            tcpPeerProtocol();
        }

        static void dhtClient() {
            var dht = new DHTClient();
            dht.Ping().Wait();
        }

        static void tcpPeerProtocol() {
            // TODO
        }
    }
}
