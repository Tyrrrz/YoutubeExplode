using System;
using System.IO;
using System.Text;

namespace YoutubeExplode.Utils
{
    internal class ProtoBuilder
    {
        private MemoryStream _byteBuffer;

        public ProtoBuilder()
        {
            _byteBuffer = new MemoryStream();
        }

        public byte[] ToBytes()
        {
            return _byteBuffer.ToArray();
        }

        public string ToUrlencodedBase64()
        {
            var b64 = Convert
                .ToBase64String(ToBytes())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
            return Uri.EscapeDataString(b64);
        }

        private void WriteVarint(long val)
        {
            if (val == 0)
            {
                _byteBuffer.WriteByte(0);
            }
            else
            {
                long v = val;
                while (v != 0)
                {
                    byte b = (byte)(v & 0x7F);
                    v >>= 7;

                    if (v != 0)
                    {
                        b |= 0x80;
                    }
                    _byteBuffer.WriteByte(b);
                }
            }
        }

        private void Field(int field, byte wire)
        {
            long fbits = ((long)field) << 3;
            long wbits = ((long)wire) & 0x07;
            long val = fbits | wbits;
            WriteVarint(val);
        }

        public void Varint(int field, long val)
        {
            Field(field, 0);
            WriteVarint(val);
        }

        public void String(int field, string str)
        {
            var strBytes = Encoding.UTF8.GetBytes(str);
            Bytes(field, strBytes);
        }

        public void Bytes(int field, byte[] bytes)
        {
            Field(field, 2);
            WriteVarint(bytes.Length);
            _byteBuffer.Write(bytes, 0, bytes.Length);
        }
    }
}
