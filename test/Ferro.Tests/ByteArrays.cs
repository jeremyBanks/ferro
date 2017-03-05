using Xunit;


namespace Ferro.UnitTests
{
    public class ByteArrayTests
    {
        [Fact]
        public void TestSlice() {
            var input = "abcdefghij".ToASCII();

            // Verify that the default behaviour is to create a copy.
            Assert.Equal(input, input.Slice());
            Assert.NotSame(input, input.Slice());

            // Nonnegative starting indicies only.
            Assert.Equal("abcdefghij".ToASCII(), input.Slice(start: 0));
            Assert.Equal("bcdefghij".ToASCII(), input.Slice(start: 1));
            Assert.Equal("defghij".ToASCII(), input.Slice(start: 3));
            Assert.Equal("j".ToASCII(), input.Slice(start: 9));
            Assert.Equal("".ToASCII(), input.Slice(start: 10));
            Assert.Equal("".ToASCII(), input.Slice(start: 999));

            // Nonnegative ending indicies only
            Assert.Equal("".ToASCII(), input.Slice(end: 0));
            Assert.Equal("a".ToASCII(), input.Slice(end: 1));
            Assert.Equal("abc".ToASCII(), input.Slice(end: 3));
            Assert.Equal("abcdefghi".ToASCII(), input.Slice(end: 9));
            Assert.Equal("abcdefghij".ToASCII(), input.Slice(end: 10));
            Assert.Equal("abcdefghij".ToASCII(), input.Slice(end: 999));

            // Negative starting indicies only.
            Assert.Equal("j".ToASCII(), input.Slice(start: -1));
            Assert.Equal("hij".ToASCII(), input.Slice(start: -3));
            Assert.Equal("abcdefghij".ToASCII(), input.Slice(start: -10));
            Assert.Equal("abcdefghij".ToASCII(), input.Slice(start: -999));

            // Negative ending indicies only.
            Assert.Equal("abcdefghi".ToASCII(), input.Slice(end: -1));
            Assert.Equal("abcdefg".ToASCII(), input.Slice(end: -3));
            Assert.Equal("".ToASCII(), input.Slice(end: -10));
            Assert.Equal("".ToASCII(), input.Slice(end: -999));

            // Both indicies, all over!
            Assert.Equal("abcdefghij".ToASCII(), input.Slice(0, 10));
            Assert.Equal("cdef".ToASCII(), input.Slice(2, 6));
            Assert.Equal("cdef".ToASCII(), input.Slice(-8, -4));
            Assert.Equal("f".ToASCII(), input.Slice(5, -4));
            Assert.Equal("".ToASCII(), input.Slice(-4, -8));
            Assert.Equal("ab".ToASCII(), input.Slice(-999, -8));
            Assert.Equal("cde".ToASCII(), input.Slice(-8, 5));
        }
    }
}
