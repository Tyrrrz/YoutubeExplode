using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace YoutubeExplode.Utils;

internal class ProtoBuilder
{
    private readonly MemoryStream _byteBuffer = new();

    private void WriteNumber(long val)
    {
        if (val == 0)
        {
            _byteBuffer.WriteByte(0);
            return;
        }

        var v = val;
        while (v != 0)
        {
            var b = (byte)(v & 0x7F);
            v >>= 7;

            if (v != 0)
                b |= 0x80;

            _byteBuffer.WriteByte(b);
        }
    }

    private void WriteField(int field, byte wire)
    {
        var fBits = (long)field << 3;
        var wBits = (long)wire & 0x07;
        var val = fBits | wBits;

        WriteNumber(val);
    }

    public ProtoBuilder AddNumber(int field, long val)
    {
        WriteField(field, 0);
        WriteNumber(val);

        return this;
    }

    public ProtoBuilder AddString(int field, string str) =>
        AddBytes(field, Encoding.UTF8.GetBytes(str));

    public ProtoBuilder AddBytes(int field, byte[] bytes)
    {
        WriteField(field, 2);
        WriteNumber(bytes.Length);
        _byteBuffer.Write(bytes, 0, bytes.Length);

        return this;
    }

    public byte[] ToBytes() => _byteBuffer.ToArray();

    public string ToUrlEncodedBase64() =>
        Uri.EscapeDataString(
            Convert.ToBase64String(ToBytes()).Replace('+', '-').Replace('/', '_').TrimEnd('=')
        );

    [ExcludeFromCodeCoverage]
    public override string ToString() => ToUrlEncodedBase64();
}
