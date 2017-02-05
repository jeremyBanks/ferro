using System;
using System.Collections.Generic;
using Xunit;


namespace Ferro.UnitTests
{
    // Plug in your exception classes (for invalid values) here.
    using serializationException = Ferro.SerializationException;
    using deserializationException = Ferro.DeserializationException;

    public class BencodingTests
    {
        // Plug in your encoding and decoding functions here.
        static object deserialize(byte[] bytes) {
            return BencodeDeserializer.Deserialize(bytes);
        }

        static byte[] serialize(object value) {
            return BencodeSerializer.Serialize(value);
        }

        // Asserts that deserializing and reserializing doesn't modify a value.
        public void AssertRoundTrip(byte[] bytes) {
            return; // TODO: remove me
            Assert.Equal(bytes, serialize(deserialize(bytes)));
        }

        [Fact]
        public void PositiveIntegerFromValue() 
        {
            Int64 value = 12345;
            var encoded = serialize(value);
            Assert.Equal("i12345e".ToASCII(), encoded);
            AssertRoundTrip(encoded);
        }

        [Fact]
        public void PositiveIntegerFromBytes() 
        {
            var bytes = "i13e".ToASCII();
            var result = deserialize(bytes);
            Assert.Equal((Int64) 13, result);
            AssertRoundTrip(bytes);
        }

        [Fact]
        public void NegativeIntegerFromBytes()
        {
            var bytes = "i-3153e".ToASCII();
            var result = deserialize(bytes);
            Assert.Equal((Int64) (-3153), result);
            AssertRoundTrip(bytes);
        }

        [Fact]
        public void NegativeIntegerFromValue()
        {
            Int64 value = -9876;
            var encoded = serialize(value);
            Assert.Equal("i-9876e".ToASCII(), encoded);
            AssertRoundTrip(encoded);
        }

        [Fact]
        public void ZeroIntegerFromBytes()
        {
            var value = "i0e".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(Int64), result.GetType());
            var typedResult = (Int64) result;
            Assert.Equal(0, typedResult);
            AssertRoundTrip(value);
        }

        [Fact]
        public void LargePositiveIntegerFromBytes()
        {
            var value = "i42897244160e".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(Int64), result.GetType());
            var typedResult = (Int64) result;
            Assert.Equal(42897244160, typedResult);
            AssertRoundTrip(value);
        }

        [Fact]
        public void InvalidLeadingZerosPositiveIntegerFromBytes()
        {
            var value = "i05e".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidMultipleMinusInIntegerFromBytes()
        {
            var value = "i--33e".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidNonInitialMinusInIntegerFromBytes()
        {
            var value = "i3-3e".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidLeadingZeroesNegativeIntegerFromBytes()
        {
            var value = "i-03e".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidNegativeZeroIntegerFromBytes()
        {
            var value = "i-0e".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidEmptyIntegerFromBytes()
        {
            var value = "ie".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void UnsupportedGiganticIntegerFromBytes()
        {
            var value = (
                "i" +
                "123456789012345678901234567890123456789012345678901234567890" +
                "123456789012345678901234567890123456789012345678901234567890" +
                "123456789012345678901234567890123456789012345678901234567890" +
                "123456789012345678901234567890123456789012345678901234567890" +
                "e").ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidLeadingCharacterFromBytes()
        {
            var value = "z0e".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void StringFromBytes()
        {
            var value = "5:hello".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(byte[]), result.GetType());
            var typedResult = (byte[]) result;
            Assert.Equal("hello".ToASCII(), typedResult);
            AssertRoundTrip(value);
        }

        [Fact]
        public void StringFromValue()
        {
            var value = "hello world".ToASCII();
            var encoded = serialize(value);
            Assert.Equal("11:hello world".ToASCII(), encoded);
            AssertRoundTrip(encoded);
        }

        [Fact]
        public void EmptyStringFromValue()
        {
            var value = "0:".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(byte[]), result.GetType());
            var typedResult = (byte[]) result;
            Assert.Equal(new byte[]{}, typedResult);
            AssertRoundTrip(value);
        }

        [Fact]
        public void InvalidLeadingZeroesStringLengthFromBytes()
        {
            var value = "05:hello".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidNegativeStringLengthFromBytes()
        {
            var value = "-5:hello".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidStringLongerThanDataFromBytes()
        {
            var value = "50:hello".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidStringLongerThanPossibleFromBytes()
        {
            var value =
                "987654332198765433219876543321987654332198765433219876543321:hello".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void EmptyListFromBytes()
        {
            var value = "le".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(List<object>), result.GetType());
            var typedResult = (List<object>) result;
            Assert.Equal(0, typedResult.Count);
            AssertRoundTrip(value);
        }

        [Fact]
        public void IntegerListFromBytes()
        {
            var value = "li1ei2ei3ee".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(List<object>), result.GetType());
            AssertRoundTrip(value);
        }

        [Fact]
        public void StringAndIntegerListFromValue()
        {
            var value = new List<object> { (Int64) 1234, "hello".ToASCII(), (Int64) (-5678), "world".ToASCII() };
            var encoded = serialize(value);
            Assert.Equal("li1234e5:helloi-5678e5:worlde".ToASCII(), encoded);
            AssertRoundTrip(encoded);
        }

        [Fact]
        public void EmptyDictionaryFromBytes()
        {
            var value = "de".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(Dictionary<byte[], object>), result.GetType());
            var typedResult = (Dictionary<byte[], object>) result;
            Assert.Equal(0, typedResult.Count);
            AssertRoundTrip(value);
        }

        [Fact]
        public void IntegerDictionaryFromValue()
        {
            var value = "d1:1i2e1:3i4ee".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(Dictionary<byte[], object>), result.GetType());
            AssertRoundTrip(value);
        }

        [Fact]
        public void IntegerListDictionaryFromBytes()
        {
            var value = new Dictionary<byte[], object> {
                {"hello".ToASCII(), (Int64) 1234},
                {"world".ToASCII(), (Int64) (-5678)}
            };
            var encoded = serialize(value);
            Assert.Equal("d5:helloi1234e5:worldi-5678ee".ToASCII(), encoded);
            AssertRoundTrip(encoded);
        }

        [Fact]
        public void NestedListDictionaryFromValue()
        {
            var value = "d1:1li2ee1:3li4eee".ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(Dictionary<byte[], object>), result.GetType());
            AssertRoundTrip(value);
        }

        [Fact]
        public void InvalidTwoKeyOneValueDictionaryFromBytes()
        {
            var value = new Dictionary<byte[], object> {
                {
                    "hello".ToASCII(),
                    new List<object> {
                        new Dictionary<byte[], object> {
                            {"world".ToASCII(), (Int64) 102436}
                        }
                    }
                }
            };
            var encoded = serialize(value);
            Console.WriteLine(encoded.FromASCII());
            Assert.Equal("d5:hellold5:worldi102436eeee".ToASCII(), encoded);
            AssertRoundTrip(encoded);
        }

        [Fact]
        public void InvalidMisorderedKeysDictionaryFromBytes()
        {
            var value = "d1:1i2e1:3e".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidDuplicateKeysDictionaryFromBytes()
        {
            var value = "d1:3i4e1:1i2ee".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void DictionaryFromMisorderedDictionaryFromValue()
        {
            var value = "d1:1i2e1:1i2ee".ToASCII();
            Assert.Throws<deserializationException>(() => deserialize(value));
        }

        [Fact]
        public void InvalidIntegerKeyedDictionaryFromBytes()
        {
            var value = new Dictionary<byte[], object> {
                {"zzz".ToASCII(), "last".ToASCII()},
                {"zz".ToASCII(), "dictionary".ToASCII()},
                {"zza".ToASCII(), "whose".ToASCII()},
                {"".ToASCII(), "disordered".ToASCII()},
                {"a".ToASCII(), "with".ToASCII()},
                {"az".ToASCII(), "keys".ToASCII()},
                {"azb".ToASCII(), "invalid".ToASCII()},
                {"aza".ToASCII(), "two".ToASCII()}
            };
            var encoded = serialize(value);
            Assert.Equal(
                ("d0:10:disordered1:a4:with2:az4:keys3:aza3:two3:azb" + 
                    "7:invalid2:zz10:dictionary3:zza5:whose3:zzz4:laste").ToASCII(),
                encoded);
            AssertRoundTrip(encoded);
        }

        [Fact]
        public void TorrentLikeFromBytes()
        {
            var value = (
                "d8:announce35:udp://tracker.openbittorrent.com:8013:announce-list" +
                "ll35:udp://tracker.openbittorrent.com:80el33:udp://tracker.opentrackr.org:1337ee" +
                "4:infod6:lengthi7e4:name7:example12:piece lengthi7e6:pieces20:0I0')s000000v0-0o0?0" + 
                "4:salt3:200e8:url-listl57:https://mgnt.ca/123456fc77d23aca05a8b58066bb55fe06c72f8e/" + 
                "56:http://mgnt.ca/123456fc77d23aca05a8b58066bb55fe06c72f8e/ee").ToASCII();
            var result = deserialize(value);
            Assert.Equal(typeof(Dictionary<byte[], object>), result.GetType());
            AssertRoundTrip(value);
        }
    }
}
