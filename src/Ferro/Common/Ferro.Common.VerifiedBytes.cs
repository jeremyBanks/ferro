using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ferro.Common {
    public static class CommonPieceSizes {
        public static Int32 BEP9_METADATA = 16384;
        public static Int32 BEP9_SUBPIECE = 16384;
    }

    // An array of bytes verified by a single SHA-1 hash, but consisting of
    // potentially several pieces of some fixed size. This could be used to
    // represent a data piece or info dictionary from a torrent, as being
    // provided by another peer. (This is not meant to handle data from
    // multiple untrusted sources, only one.) None of these methods duplicate
    // arrays unnecessarily, so consumers and providers must avoid mutating
    // them.
    public class VerifiedBytes {
        // The SHA-1 hash digest again which the data will be verified.
        public readonly byte[] Digest;

        // The total length of the data.
        public readonly Int32 Length;
        // The length of each piece of the data except for the final one,
        // which may be shorter.
        public readonly Int32 PieceLength;
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

        public VerifiedBytes(byte[] digest, Int32 length, Int32 pieceLength) {
            if (length < 0 || pieceLength < 0) {
                throw new ArgumentOutOfRangeException("Lengths must be non-negative.");
            }
            if (digest == null || digest.Length != 20) {
                throw new ArgumentException($"Expected digest of length 20, was {digest?.Length}.");
            }

            Digest = digest;
            Length = length;
            PieceLength = pieceLength;

            pieces = new byte[PieceCount][];
            piecesOutstanding = PieceCount;

            if (Length == 0) {
                finalizePieces();
            }
        }

        // Alternate constructor for known data with a known hash.
        public static VerifiedBytes From(byte[] data, byte[] digest, Int32 pieceLength) {
            var that = new VerifiedBytes(digest, data.Length, pieceLength);
            that.ProvideData(data);
            return that;
        } 

        // Alternate constructor for known data without a known hash.
        public static VerifiedBytes FromUnverified(byte[] data, Int32 pieceLength) {
            byte[] digest;
            using (var sha1 = SHA1.Create()) {
                digest = sha1.ComputeHash(data);
            }
            return From(data, digest, pieceLength);
        }

        // Provides all of the data at once.
        public void ProvideData(byte[] data) {
            // Verify that the data 
            if (data.Length != Length) {
                throw new ArgumentException($"Data has length {data.Length} but {Length} was expected.");
            }

            // Verify that no pieces were already provided, which we could be duplicating.
            if (pieces == null) {
                throw new VerifiedBytesStateException("All pieces have already been provided.");
            }
            var knownPieceCount = 0;
            foreach (var piece in pieces) {
                if (piece != null) {
                    knownPieceCount++;
                }
            }
            if (knownPieceCount > 0) {
                throw new VerifiedBytesStateException($"{knownPieceCount} pieces have already been provided.");
            }

            pieces = null;
            finalizeData(data);
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
                throw new VerifiedBytesStateException("All pieces have already been provided.");
            }
            if (pieces[index] != null) {
                throw new VerifiedBytesStateException($"Piece {index} has already been provided.");
            }

            pieces[index] = piece;
            piecesOutstanding -= 1;

            if (piecesOutstanding == 0) {
                finalizePieces();
            }
        }

        public byte[] GetPiece(Int32 index) {
            if (pieces != null) {
                throw new VerifiedBytesStateException("Not all data has been provided.");
            }

            // Verify that the piece and index are valid in general.
            if (index < 0 || index >= PieceCount ) {
                throw new ArgumentOutOfRangeException($"Index {index} out of range.");
            }

            var data = Result.Result;
            var length = (ExtraPieceLength > 0 && index == PieceCount - 1) ? ExtraPieceLength : PieceLength;
            var piece = new byte[length];
            Buffer.BlockCopy(data, PieceLength * index, piece, 0, length);
            return piece;
        }

        public byte[] GetData() {
            if (pieces != null) {
                throw new VerifiedBytesStateException("Not all data has been provided.");
            }

            return Result.Result;
        }

        private void finalizePieces() {
            if (pieces == null) {
                throw new VerifiedBytesStateException("Already finalized!");
            }

            var resultValue = new byte[Length];

            for (Int32 i = 0; i < PieceCount; i++) {
                Buffer.BlockCopy(pieces[i], 0, resultValue, PieceLength * i, pieces[i].Length);
            }

            pieces = null;
            finalizeData(resultValue);
        }

        private void finalizeData(byte[] data) {
            if (pieces != null) {
                throw new VerifiedBytesStateException("Expected pieces to already be null.");
            }

            byte[] actualDigest;
            using (var sha1 = SHA1.Create()) {
                actualDigest = sha1.ComputeHash(data);
            }

            if (Digest.SequenceEqual(actualDigest)) {
                resultSource.SetResult(data);
            } else {
                resultSource.SetException(new BytesVerificationException(
                    $"Verification failed: expected {Digest.ToHex()}, got {actualDigest.ToHex()}."));
            }
        }
    }

    public class VerifiedBytesStateException : Exception {
        public VerifiedBytesStateException(string message) : base(message) {} }

    public class BytesVerificationException : Exception {
        public BytesVerificationException(string message) : base(message) {} }
}
