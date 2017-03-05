using System;
using System.Collections.Generic;
using System.Text;

namespace Ferro  {
    partial class Bencoding {
        public static Dictionary<byte[], object> Dict() {
            return new Dictionary<byte[], object>(ByteArrayComparer.Instance);
        }

        // Creates a human-readable formatting of the bencoded data.
        public static string ToHuman(byte[] data) {
            var value = Bencoding.Decode(data);
            var result = new StringBuilder();
            toHuman(value, result, 0, 2);
            return result.ToString();
        }

        static void toHuman(object value, StringBuilder result, int indentLevel, int indent) {
            if (value is byte[]) {
                var s = (byte[]) value;
                result.Append("\"");
                foreach (var c in s) {
                    if (c >= ' ' && c <= '~' && c != '"') {
                        result.Append((char) c);
                    } else {
                        result.Append('?');
                    }
                }
                result.Append("\"");
            } else if (value is Int64) {
                var i = (Int64) value;
                result.Append(i.ToString());
            } else if (value is Dictionary<byte[], object>) {
                var d = (Dictionary<byte[], object>) value;
                result.Append("{\n");

                foreach (var item in d) {
                    for (var i = 0; i < indentLevel + indent; i++) result.Append(" ");
                    toHuman(item.Key, result, indentLevel + indent, indent);
                    result.Append(": ");
                    toHuman(item.Value, result, indentLevel + indent, indent);
                    result.Append('\n');
                }

                for (var i = 0; i < indentLevel; i++) result.Append(" ");
                result.Append("}");
            } else if (value is List<object>) {
                var l = (List<object>) value;
                result.Append("[\n");

                foreach (var item in l) {
                    for (var i = 0; i < indentLevel + indent; i++) result.Append(" ");
                    toHuman(item, result, indentLevel + indent, indent);
                    result.Append('\n');
                }

                for (var i = 0; i < indentLevel; i++) result.Append(" ");
                result.Append("]");
            } else {
                throw new Exception($"impossible value type in toHuman {value.GetType()}");
            }
        }
    }

    class ByteArrayComparer : IEqualityComparer<byte[]>, IComparer<byte[]> {

        // Leiconographic ordering of byte arrays.
        public int Compare(byte[] x, byte[] y) {
            if (x == null || y == null)  {
                throw new Exception("null is not ordered relative to byte arrays.");
            }
            if (ReferenceEquals(x, y)) {
                return 0;
            }
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

        public bool Equals(byte[] x, byte[] y) {
            if (x == null || y == null) {
                return x == null && y == null;
            }
            return Compare(x, y) == 0;
        }

        public int GetHashCode(byte[] x) {
            return x.Length;
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