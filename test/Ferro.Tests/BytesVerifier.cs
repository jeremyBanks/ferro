using Xunit;

using Ferro.Common;
using System;

namespace Ferro.UnitTests
{
    public class BytesVerifierTests
    {
        [Fact]
        public async void TestZeroLengthValid() {
            var expectedDigest = "da39a3ee5e6b4b0d3255bfef95601890afd80709".FromHex();
            var verifier = new BytesVerifier(expectedDigest, 0, 1024);
            var resultValue = await verifier.Result;

            Assert.Equal(new byte[0], resultValue);

            // No possible indices are valid.
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                verifier.ProvidePiece(0, new byte[0]);
            });
        }

        [Fact]
        public async void TestZeroLengthInvalid() {
            var expectedDigest = "0000000000000000000000000000000000000000".FromHex();
            var verifier = new BytesVerifier(expectedDigest, 0, 1024);

            await Assert.ThrowsAsync<BytesVerificationException>(async () => {
                var resultValue = await verifier.Result;
            });
        }

        [Fact]
        public async void TestTwoUnevenPiecesValid() {
            var expectedDigest = "a94a8fe5ccb19ba61c4c0873d391e987982fbbd3".FromHex();
            var verifier = new BytesVerifier(expectedDigest, 5, 3);

            // Pieces need to be of the expected size.
            Assert.Throws<ArgumentException>(() => {
                verifier.ProvidePiece(0, "lo".ToASCII());
            });
            Assert.Throws<ArgumentException>(() => {
                verifier.ProvidePiece(1, "hel".ToASCII());
            });

            verifier.ProvidePiece(1, "lo".ToASCII());
            verifier.ProvidePiece(0, "hel".ToASCII());

            // Each piece can only be provided once.
            Assert.Throws<BytesVerifierStateException>(() => {
                verifier.ProvidePiece(0, "hel".ToASCII());
            });

            var resultValue = await verifier.Result;
            Assert.Equal("hello".ToASCII(), resultValue);
        }
    }
}
