using System;
using System.Collections.Generic;
using System.Text;

using Ditto.Common;
using Microsoft.Extensions.Logging;

namespace Ditto.Serialization  {
    // Utility functions for bencoded data.
    public partial class Bencoding {
        static ILogger logger { get; } = GlobalLogger.CreateLogger<Bencoding>();

        // Returns a new Dictionary that can be directly bencoded.
        public static Dictionary<byte[], object> Dict() {
            return new Dictionary<byte[], object>(ByteArrayComparer.Instance);
        }

        // Creates a human-readable formatting of the bencoded data.
        public static string ToHuman(byte[] data) {
            var value = Bencoding.decode(data);
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

                foreach (var (k, v) in d) {
                    for (var i = 0; i < indentLevel + indent; i++) result.Append(" ");
                    toHuman(k, result, indentLevel + indent, indent);
                    result.Append(": ");
                    toHuman(v, result, indentLevel + indent, indent);
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
}