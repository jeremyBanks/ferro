using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ditto.Common;
using Microsoft.Extensions.Logging;

namespace Ditto.Serialization  {
    // Utilities for mapping between bencoding structures and other data types.
    public partial class Bencoding {
        public static T bToType<T>(object bData) where T : new() {
            var targetType = typeof(T);

            var props = targetType.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fields = targetType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            logger.BeginScope($"mapping to {targetType}");
            logger.LogDebug($"found {fields.Length} fields and {props.Length}");

            // NOTE: This logic is not yet considering list or dict types, just classes.
            var bDict = (Dictionary<byte[], object>) bData;

            var keysSeen = new HashSet<byte[]>(ByteArrayComparer.Instance);

            var result = new T();

            foreach (var field in fields) {
                var bAttrs = field.GetCustomAttributes(typeof(BencodableAttribute)).ToList();
                if (bAttrs.Count == 1) {
                    var attr = (BencodableAttribute) bAttrs[0];
                    var key = attr.Key;
                    logger.LogDebug($"found mappable prop: {key.ToHuman()} to {field.Name}:{field.FieldType}");
                    if (keysSeen.Contains(key)) {
                        throw new BencodingTypeMappingException(
                            $"[Bencodable(\"{key}\")] specified for multiple props/fields.");
                    } else {
                        keysSeen.Add(key);

                        if (!bDict.ContainsKey(key)) {
                            continue;
                        }

                        var type = field.FieldType;
                        var bValue = bDict[key];
                        object value;
                        if (type == typeof(byte[])) {
                            value = (byte[]) bValue;
                        } else if (type == typeof(string)) {
                            value = ((byte[]) bValue).FromASCII();
                        } else {
                            logger.LogError($"Can't map field of type {type}.");;
                            continue;
                        }
                        field.SetValue(result, value);
                    }
                } else if (bAttrs.Count > 1) {
                    throw new BencodingTypeMappingException(
                        $"Expected zero or none [Bencodable] attributes, got {bAttrs.Count}.");
                }
            }

            foreach (var prop in props) {
                var bAttrs = prop.GetCustomAttributes(typeof(BencodableAttribute)).ToList();
                if (bAttrs.Count == 1) {
                    var attr = (BencodableAttribute) bAttrs[0];
                    var key = attr.Key;
                    logger.LogDebug($"found mappable prop: {key.ToHuman()} to {prop.Name}:{prop.PropertyType}");
                    if (keysSeen.Contains(key)) {
                        throw new BencodingTypeMappingException(
                            $"[Bencodable(\"{key}\")] specified for multiple props/fields.");
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