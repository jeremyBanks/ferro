using System;
using System.Reflection;
using Ditto.Common;

namespace Ditto.Serialization  {
    // Utilities for mapping between bencoding structures and other data types.
    public static partial class Bencoding {
        public static T bToType<T>(object bData) where T : new() {
            var props = typeof(T).GetType().GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Console.WriteLine($"I see {props.Length} props.");

            return new T();
        }
    }

    public class BencodableAttribute : Attribute {
        public BencodableAttribute(string key) :
            this(key.ToUTF8()) {}

        public BencodableAttribute(byte[] key) {

        }
    }
}