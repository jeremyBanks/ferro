using System.Collections.Generic;

namespace Ferro  {
    class ByteArrayComparer : IComparer<byte[]> {
        // Leiconographic ordering of byte arrays.
        public int Compare(byte[] x, byte[] y) {
            for (var i = 0;; i++) {
                if (i >= x.Length) {
                    if (i >= y.Length) {
                        return 0; // they are equal
                    } else {
                        return -1; // y contains additional items
                    }
                } else if (i >= y.Length) {
                    return +1; // x contains additional item
                }

                var xItem = x[i];
                var yItem = y[i];
                if (xItem > yItem) {
                    return +1; // x contains a greater item first
                } else if (yItem > xItem) {
                    return -1; // y contains a greater item first
                }
            }
        }

        // Static instance that we can always use, since there's no state.
        public static readonly ByteArrayComparer Instance;

        static ByteArrayComparer() {
            Instance = new ByteArrayComparer();
        }

        public static bool Ascending(byte[] x, byte[] y) {
            return Instance.Compare(x, y) < 0;
        }

        public static bool Equal(byte[] x, byte[] y) {
            return Instance.Compare(x, y) == 0;
        }

        public static bool Descending(byte[] x, byte[] y) {
            return Instance.Compare(x, y) > 0;
        }
    }
}