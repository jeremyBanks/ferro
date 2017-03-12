using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ditto.Common;

namespace Ditto.Serialization  {
    // Utilities for mapping between bencoding structures and other data types.
    public static partial class Bencoding {
        public static T bToType<T>(object bData) where T : new() {
            var props = typeof(T).GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fields = typeof(T).GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            Console.WriteLine($"I see {fields.Length} fields and {props.Length} props for {typeof(T)}.");

            // NOTE: This logic is not yet considering list or dict types, just classes.
            var bDict = (Dictionary<byte[], object>) bData;

            var keysSeen = new HashSet<byte[]>(ByteArrayComparer.Instance);

            var result = new T();

            foreach (var field in fields) {
                Console.WriteLine("see field:" + field.Name);
                var bAttrs = field.GetCustomAttributes(typeof(BencodableAttribute)).ToList();
                if (bAttrs.Count == 1) {
                    var attr = (BencodableAttribute) bAttrs[0];
                    var key = attr.Key;
                    Console.WriteLine("MAPPABLE!!! " + key);
                    if (keysSeen.Contains(key)) {
                        throw new BencodingTypeMappingException(
                            $"[Bencodable(\"{key}\")] specified for multiple properties.");
                    } else {
                        keysSeen.Add(key);

                        var type = field.FieldType;
                        var bValue = bDict[key];
                        object value;
                        if (type == typeof(byte[])) {
                            value = (byte[]) bValue;
                        } else if (type == typeof(string)) {
                            value = ((string) bValue).ToASCII();
                        } else {
                            throw new BencodingTypeMappingException($"Can't map field of type {type}.");
                        }
                        field.SetValue(result, value);
                    }
                } else if (bAttrs.Count > 1) {
                    throw new BencodingTypeMappingException(
                        $"Expected zero or none [Bencodable] attributes, got {bAttrs.Count}.");
                }
            }

            foreach (var prop in props) {
                Console.WriteLine("see prop:" + prop.Name);
                var bAttrs = prop.GetCustomAttributes(typeof(BencodableAttribute)).ToList();
                if (bAttrs.Count == 1) {
                    var attr = (BencodableAttribute) bAttrs[0];
                    var key = attr.Key;
                    Console.WriteLine("MAPPABLE!!! " + key);
                    if (keysSeen.Contains(key)) {
                        throw new BencodingTypeMappingException(
                            $"[Bencodable(\"{key}\")] specified for multiple properties.");
                    } else {
                        keysSeen.Add(key);

                        var type = prop.PropertyType;
                        var bValue = bDict[key];
                        object value;
                        if (type == typeof(byte[])) {
                            value = (byte[]) bValue;
                        } else if (type == typeof(string)) {
                            value = ((byte[]) bValue).FromASCII();
                        } else {
                            throw new BencodingTypeMappingException($"Can't map prop of type {type}.");
                        }
                        prop.SetValue(result, value);
                    }
                } else if (bAttrs.Count > 1) {
                    throw new BencodingTypeMappingException(
                        $"Expected zero or none [Bencodable] attributes, got {bAttrs.Count}.");
                }
            }

            return result;
        }
    }

    public class BencodableAttribute : Attribute {
        public readonly byte[] Key;

        public BencodableAttribute(byte[] key) {
            Key = key;
        }

        public BencodableAttribute(string key) :
            this(key.ToUTF8()) {}
    }

    public class BencodingTypeMappingException : Exception {
        public BencodingTypeMappingException(string message) :
            base(message) {}
    }
}