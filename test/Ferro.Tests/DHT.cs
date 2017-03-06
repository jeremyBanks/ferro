using Xunit;

namespace Ferro.UnitTests
{
    public class DHTTests
    {
        [Fact]
        public void TestIncrementToken() {
            Assert.Equal(
                new byte[1]{ 1 },
                DHT.Client.IncrementToken(new byte[0]));

            Assert.Equal(
                new byte[1]{ 6 },
                DHT.Client.IncrementToken(new byte[1]{ 5 }));

            Assert.Equal(
                new byte[2]{ 0xFF, 0x03 },
                DHT.Client.IncrementToken(new byte[2]{ 0xFF, 0x02 }));

            Assert.Equal(
                new byte[3]{ 0xFF, 0xFF, 0x03 },
                DHT.Client.IncrementToken(new byte[3]{ 0xFF, 0xFF, 0x02 }));

            Assert.Equal(
                new byte[6]{ 0xFF, 0x03, 0x00, 0x00, 0x00, 0x00 },
                DHT.Client.IncrementToken(new byte[6]{ 0xFF, 0x02, 0xFF, 0xFF, 0xFF, 0xFF }));

            Assert.Equal(
                new byte[3]{ 0xFF, 0x03, 0x00 },
                DHT.Client.IncrementToken(new byte[3]{ 0xFF, 0x02, 0xFF }));

            Assert.Equal(
                new byte[4]{ 0x01, 0x00, 0x00, 0x00 },
                DHT.Client.IncrementToken(new byte[3]{ 0xFF, 0xFF, 0xFF }));
        }
    }
}