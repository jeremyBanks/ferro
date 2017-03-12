using Xunit;

using Ditto.Common;
using System;

namespace Ditto.UnitTests
{
    public class VerifiedBytesTests
    {
        [Fact]
        public async void ZeroLengthValid() {
            var expectedDigest = "da39a3ee5e6b4b0d3255bfef95601890afd80709".FromHex();
            var verified = new VerifiedBytes(expectedDigest, 0, 1024);
            var resultValue = await verified.Result;

            Assert.Equal(new byte[0], resultValue);
            Assert.Equal(new byte[0], verified.GetData());

            // No possible indices are valid.
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                verified.ProvidePiece(0, new byte[0]);
            });
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                verified.ProvidePiece(0, new byte[0]);
            Assert.Equal("".ToASCII(), verified.GetPiece(0));
            });
        }

        [Fact]
        public async void ZeroLengthInvalid() {
            var expectedDigest = "0000000000000000000000000000000000000000".FromHex();
            var verified = new VerifiedBytes(expectedDigest, 0, 1024);

            await Assert.ThrowsAsync<BytesVerificationException>(async () => {
                var resultValue = await verified.Result;
            });
        }

        [Fact]
        public async void TwoUnevenPiecesValid() {
            var expectedDigest = "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d".FromHex();
            var verified = new VerifiedBytes(expectedDigest, 5, 3);

            // Piece indicies need to be in range.
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                verified.ProvidePiece(-1, "hel".ToASCII());
            });
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                verified.ProvidePiece(2, "hel".ToASCII());
            });

            // Pieces need to be of the expected size.
            Assert.Throws<ArgumentException>(() => {
                verified.ProvidePiece(0, "lo".ToASCII());
            });
            Assert.Throws<ArgumentException>(() => {
                verified.ProvidePiece(1, "hel".ToASCII());
            });

            verified.ProvidePiece(1, "lo".ToASCII());
            verified.ProvidePiece(0, "hel".ToASCII());

            // Each piece can only be provided once.
            Assert.Throws<VerifiedBytesStateException>(() => {
                verified.ProvidePiece(0, "hel".ToASCII());
            });

            var resultValue = await verified.Result;
            Assert.Equal("hello".ToASCII(), resultValue);
        }

        [Fact]
        public async void TwoUnevenPiecesInvalid() {
            var expectedDigest = "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d".FromHex();
            var verified = new VerifiedBytes(expectedDigest, 5, 3);

            verified.ProvidePiece(1, "l!".ToASCII());

            // Each piece can only be provided once, even it would fix it.
            Assert.Throws<VerifiedBytesStateException>(() => {
                verified.ProvidePiece(1, "l!".ToASCII());
            });

            verified.ProvidePiece(0, "hel".ToASCII());

            await Assert.ThrowsAsync<BytesVerificationException>(async () => {
                var resultValue = await verified.Result;
            });
        }

        [Fact]
        public async void TwoUnevenPiecesTogetherValid() {
            var expectedDigest = "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d".FromHex();
            var verified = new VerifiedBytes(expectedDigest, 5, 3);
            verified.ProvideData("hello".ToASCII());

            var resultValue = await verified.Result;
            Assert.Equal("hello".ToASCII(), resultValue);

            Assert.Equal("hel".ToASCII(), verified.GetPiece(0));
            Assert.Equal("lo".ToASCII(), verified.GetPiece(1));
            Assert.Equal("hello".ToASCII(), verified.GetData());
        }

        [Fact]
        public async void TwoUnevenPiecesTogetherInvalid() {
            var expectedDigest = "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d".FromHex();
            var verified = new VerifiedBytes(expectedDigest, 5, 3);
            verified.ProvideData("hell!".ToASCII());

            // Data can only be provided once, even it would fix it.
            Assert.Throws<VerifiedBytesStateException>(() => {
                verified.ProvideData("hello".ToASCII());
            });

            await Assert.ThrowsAsync<BytesVerificationException>(async () => {
                var resultValue = await verified.Result;
            });
        }

        [Fact]
        public async void TwoUnevenPiecesFromUnverified() {
            var expectedDigest = "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d".FromHex();
            var verified = VerifiedBytes.FromUnverified("hello".ToASCII(), 3);

            Assert.Equal(expectedDigest, verified.Digest);

            var resultValue = await verified.Result;
            Assert.Equal("hello".ToASCII(), resultValue);

            // Data can only be provided once.
            Assert.Throws<VerifiedBytesStateException>(() => {
                verified.ProvideData("hello".ToASCII());
            });
            Assert.Throws<VerifiedBytesStateException>(() => {
                verified.ProvidePiece(0, "hel".ToASCII());
            });
        }

        [Fact]
        public async void TwoUnevenPiecesValidFromVerified() {
            var expectedDigest = "aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d".FromHex();
            var verified = VerifiedBytes.From("hello".ToASCII(), expectedDigest, 3);

            Assert.Equal(expectedDigest, verified.Digest);

            var resultValue = await verified.Result;
            Assert.Equal("hello".ToASCII(), resultValue);

            // Data can only be provided once.
            Assert.Throws<VerifiedBytesStateException>(() => {
                verified.ProvideData("hello".ToASCII());
            });
            Assert.Throws<VerifiedBytesStateException>(() => {
                verified.ProvidePiece(0, "hel".ToASCII());
            });
        }

        [Fact]
        public async void TwoUnevenPiecesInvalidFromVerified() {
            var expectedDigest = "0000000000000000000000000000000000000000".FromHex();
            var verified = VerifiedBytes.From("hello".ToASCII(), expectedDigest, 3);
            
            await Assert.ThrowsAsync<BytesVerificationException>(async () => {
                await verified.Result;
            });
        }
    }
}
