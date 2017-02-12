namespace Ferro
{
    class Examples {
        public static void Main()
        {
            var dht = new DHTClient();
            dht.Ping().Wait();
        }
    }
}
