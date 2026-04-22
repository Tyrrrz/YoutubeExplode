using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeExplode.Utils;

internal static class Protobuf
{
    private static bool TryReadVarint(byte[] data, ref int i, out ulong value)
    {
        value = 0;
        var shift = 0;
        while (i < data.Length)
        {
            var b = data[i++];
            value |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0)
                return true;
            shift += 7;
            if (shift >= 64)
                break;
        }
        return false;
    }

    private static bool IsLenField(ulong tag) => (tag & 0x7) == 2;

    private static bool TryReadString(byte[] data, ref int i, out string? value)
    {
        value = null;
        if (!TryReadVarint(data, ref i, out var strLen))
            return false;
        if (i + (int)strLen > data.Length)
            return false;
        value = Encoding.UTF8.GetString(data, i, (int)strLen);
        i += (int)strLen;
        return true;
    }

    // Deserializes a protobuf-encoded map<string, string> payload into a dictionary.
    // Each top-level LEN field (wire type 2) is treated as a map entry submessage where
    // field 1 is the string key and field 2 is the string value.
    // Returns null if the data cannot be parsed.
    public static IReadOnlyDictionary<string, string?>? TryDeserialize(byte[] data)
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);

        var i = 0;
        while (i < data.Length)
        {
            if (!TryReadVarint(data, ref i, out var outerTag))
                return null;

            // Only process LEN-encoded fields (wire type 2) as map entries
            if (!IsLenField(outerTag))
                return null;

            if (!TryReadVarint(data, ref i, out var entryLen))
                return null;

            var entryEnd = i + (int)entryLen;
            if (entryEnd > data.Length)
                return null;

            // Parse the map entry submessage: field 1 = key (string), field 2 = value (string)
            string? key = null,
                value = null;
            var j = i;
            while (j < entryEnd)
            {
                if (!TryReadVarint(data, ref j, out var fieldTag))
                    break;

                // Only handle LEN-encoded (string) fields
                if (!IsLenField(fieldTag))
                    break;

                var fieldNum = (int)(fieldTag >> 3);

                if (!TryReadString(data, ref j, out var str))
                    break;

                if (fieldNum == 1)
                    key = str;
                else if (fieldNum == 2)
                    value = str;
            }

            if (key is not null)
                result[key] = value;

            i = entryEnd;
        }

        return result;
    }

    // Decodes a base64-encoded protobuf map<string, string> payload into a dictionary.
    // Returns null if the string is not valid base64 or cannot be parsed.
    public static IReadOnlyDictionary<string, string?>? TryDeserialize(string base64)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64);
            return TryDeserialize(bytes);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
