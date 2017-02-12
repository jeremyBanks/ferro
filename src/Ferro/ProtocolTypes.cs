using System;

namespace Ferro {
    public interface BencodingTypes {}
    public class Bytes : BencodingTypes {

    }
    public class Dictionary : System.Collections.Generic.Dictionary<Bytes, BencodingTypes>, BencodingTypes {

    }
    public class List : System.Collections.Generic.List<BencodingTypes>, BencodingTypes {

    }
    public struct Integer {
        readonly UInt64 value;
    }

    

    struct BTID {
        private byte[] data;

        public BTID(byte[] data) {
            this.data = new byte[20];
            if (data.Length != 20) {
                throw new Exception($"BTID data length must be 20, was {data.Length}");
            }
            data.CopyTo(this.data, 0);
        }

        public BTID(string s) : this(s.FromHex()) {}

        public static BTID operator ^(BTID left, BTID right) {
            return new BTID(new byte[]{
                (byte)(left.data[0] ^ right.data[0]),
                (byte)(left.data[1] ^ right.data[1]),
                (byte)(left.data[2] ^ right.data[2]),
                (byte)(left.data[3] ^ right.data[3]),
                (byte)(left.data[4] ^ right.data[4]),
                (byte)(left.data[5] ^ right.data[5]),
                (byte)(left.data[6] ^ right.data[6]),
                (byte)(left.data[7] ^ right.data[7]),
                (byte)(left.data[8] ^ right.data[8]),
                (byte)(left.data[9] ^ right.data[9]),
                (byte)(left.data[10] ^ right.data[10]),
                (byte)(left.data[11] ^ right.data[11]),
                (byte)(left.data[12] ^ right.data[12]),
                (byte)(left.data[13] ^ right.data[13]),
                (byte)(left.data[14] ^ right.data[14]),
                (byte)(left.data[15] ^ right.data[15]),
                (byte)(left.data[16] ^ right.data[16]),
                (byte)(left.data[17] ^ right.data[17]),
                (byte)(left.data[18] ^ right.data[18]),
                (byte)(left.data[19] ^ right.data[19]),
                (byte)(left.data[20] ^ right.data[20])
            });
        }

        public override string ToString() {
            return ToArray().ToHex();
        }

        public byte[] ToArray() {
            var result = new byte[20];
            data.CopyTo(result, 0);
            return result;
        }
    }
}