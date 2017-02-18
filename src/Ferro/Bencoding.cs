using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Ferro  {
    
    partial class Bencoding {
        // Creates a human-readable formatting of the bencoded data.
        public static string ToHuman(IList<byte> data) {
            var value = Bencoding.Decode(data);
            var result = new StringBuilder();
            toHuman(value, result, 0, 2);
            return result.ToString();
        }


        static void toHuman(object value, StringBuilder result, int indentLevel, int indent) {
            if (value is IList<byte>) {
                var s = (IList<byte>) value;
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

    public class ByteListComparer<T> : IComparer<T>, IEqualityComparer<T> where T : IList<byte> {
        // Leiconographic ordering of byte arrays.
        public int Compare(T x, T y) {
            for (var i = 0;; i++) {
                if (i >= x.Count) {
                    if (i >= y.Count) {
                        return 0; // they are equal
                    } else {
                        return -1; // y contains additional items
                    }
                } else if (i >= y.Count) {
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

        public int GetHashCode(T x) {
            int hashCode = x.Count;
            foreach (var value in x) {
                hashCode += value.GetHashCode();
            }
            return hashCode;
        }

        public bool Equals(T x, T y) {
            return Compare(x, y) == 0;
        }

        // Static instance that we can always use, since there's no state.
        public static readonly ByteListComparer<T> Instance;

        static ByteListComparer() {
            Instance = new ByteListComparer<T>();
        }

        public static bool Ascending(T x, T y) {
            return Instance.Compare(x, y) < 0;
        }

        public static bool Equal(T x, T y) {
            return Instance.Compare(x, y) == 0;
        }

        public static bool Descending(T x, T y) {
            return Instance.Compare(x, y) > 0;
        }
    }
}