using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ferro.Common {
    // An array of bytes verified by a single SHA-1 hash, but consisting of
    // potentially several pieces of some fixed size. This could be used to
    // represent a data piece or info dictionary from a torrent, as being
    // provided by another peer. (This is not meant to handle data from
    // multiple untrusted sources, only one.)
    public class BytesVerifier {
        // The SHA-1 hash digest again which the data will be verified.
        readonly byte[] Sha1Digest;

        // The total length of the data.
        readonly Int32 Length;
        // The length of each piece of the data except for the final one,
        // which may be shorter.
        readonly Int32 PieceLength;
        public Int32 FullPieceCount => Length / PieceLength;
        public Int32 ExtraPieceLength => Length % PieceLength;
        public Int32 PieceCount => FullPieceCount + (ExtraPieceLength > 0 ? 1 : 0);

        // The number of pieces that have not been provided.
        protected Int32 piecesOutstanding;

        // An array of the unverified pieces. Pieces that have not yet been
        // provided will be null, and the entire thing will be nullified once
        // all of the data has been completed and verified.
        protected byte[][] pieces;

        // The task will complete with the entire verified byte array,
        // or with an exception if the provided data fails validation.
        protected TaskCompletionSource<byte[]> resultSource = new TaskCompletionSource<byte[]>();
        public Task<byte[]> Result => resultSource.Task;

        public BytesVerifier(byte[] sha1Digest, Int32 length, Int32 pieceLength) {
            if (length < 0 || pieceLength < 0) {
                throw new ArgumentOutOfRangeException("Lengths must be non-negative.");
            }
            if (sha1Digest == null || sha1Digest.Length != 20) {
                throw new ArgumentException($"Expected digest of length 20, was {sha1Digest?.Length}.");
            }

            Sha1Digest = sha1Digest;
            Length = length;
            PieceLength = pieceLength;

            pieces = new byte[PieceCount][];
            piecesOutstanding = PieceCount;

            if (Length == 0) {
                finalizeResult();
            }
        }

        // Provides the contents of a given piece.
        // We won't copy it, so the piece we're given must not be modified.
        // This will throw if called multiple times for the same piece index.
        public void ProvidePiece(Int32 index, byte[] piece) {
            // Verify that the piece and index are valid in general.
            if (index < 0 || index >= PieceCount ) {
                throw new ArgumentOutOfRangeException($"Index {index} out of range.");
            }
            if (piece == null) {
                throw new ArgumentException("Piece is null.");
            }
            if (index < FullPieceCount && piece.Length != PieceLength) {
                throw new ArgumentException($"Expected piece {index} to have length {PieceLength}, was {piece.Length}.");
            }
            if (index == PieceCount - 1 && ExtraPieceLength > 0 && piece.Length != ExtraPieceLength) {
                throw new ArgumentException($"Expected final piece {index} to have length {ExtraPieceLength}, was {piece.Length}.");
            }

            // Verify that the piece and index are actually valid given the current state.
            if (pieces == null) {
                throw new BytesVerifierStateException("All pieces have already been provided.");
            }
            if (pieces[index] != null) {
                throw new BytesVerifierStateException($"Piece {index} has already been provided.");
            }

            pieces[index] = piece;
            piecesOutstanding -= 1;

            if (piecesOutstanding == 0) {
                finalizeResult();
            }
        }

        private void finalizeResult() {
            if (pieces == null) {
                throw new BytesVerifierStateException("Already finalized!");
            }

            var resultValue = new byte[Length];

            for (Int32 i = 0; i < PieceCount; i++) {
                Buffer.BlockCopy(
                    pieces[i],
                    0,
                    resultValue,
                    PieceLength * i,
                    pieces[i].Length);
            }

            pieces = null;

            byte[] actualDigest;
            using (var sha1 = SHA1.Create()) {
                actualDigest = sha1.ComputeHash(resultValue);
            }

            if (Sha1Digest.SequenceEqual(actualDigest)) {
                resultSource.SetResult(resultValue);
            } else {
                resultSource.SetException(new BytesVerificationException(
                    $"Verification failed: expected {Sha1Digest.ToHex()}, got {actualDigest.ToHex()}."));
            }
        }
    }

    public class BytesVerifierStateException : Exception {
        public BytesVerifierStateException(string message) : base(message) {} }

    public class BytesVerificationException : Exception {
        public BytesVerificationException(string message) : base(message) {} }
}
