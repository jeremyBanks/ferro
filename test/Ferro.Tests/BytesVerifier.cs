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
            var expectedDigest = "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d".FromHex();
            var verifier = new BytesVerifier(expectedDigest, 5, 3);

            // Piece indicies need to be in range.
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                verifier.ProvidePiece(-1, "hel".ToASCII());
            });
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                verifier.ProvidePiece(2, "hel".ToASCII());
            });

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

        [Fact]
        public async void TestTwoUnevenPiecesInvalid() {
            var expectedDigest = "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d".FromHex();
            var verifier = new BytesVerifier(expectedDigest, 5, 3);

            verifier.ProvidePiece(1, "l!".ToASCII());

            // Each piece can only be provided once, even it would fix it.
            Assert.Throws<BytesVerifierStateException>(() => {
                verifier.ProvidePiece(1, "l!".ToASCII());
            });

            verifier.ProvidePiece(0, "hel".ToASCII());

            await Assert.ThrowsAsync<BytesVerificationException>(async () => {
                var resultValue = await verifier.Result;
            });
        }
    }
}
