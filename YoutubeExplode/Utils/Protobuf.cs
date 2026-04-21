using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeExplode.Utils;

internal static class Protobuf
{
    // Deserializes a protobuf-encoded map<string, string> payload into a dictionary.
    // Each top-level LEN field (wire type 2) is treated as a map entry submessage where
    // field 1 is the string key and field 2 is the string value.
    public static IReadOnlyDictionary<string, string?> Deserialize(byte[] data)
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);

        var i = 0;
        while (i < data.Length)
        {
            if (!TryReadVarint(data, ref i, out var outerTag))
                break;

            // Only process LEN-encoded fields (wire type 2) as map entries
            if ((outerTag & 0x7) != 2)
                break;

            if (!TryReadVarint(data, ref i, out var entryLen))
                break;

            var entryEnd = i + (int)entryLen;
            if (entryEnd > data.Length)
                break;

            // Parse the map entry submessage: field 1 = key (string), field 2 = value (string)
            string? key = null,
                value = null;
            var j = i;
            while (j < entryEnd)
            {
                if (!TryReadVarint(data, ref j, out var fieldTag))
                    break;

                // Only handle LEN-encoded (string) fields
                if ((fieldTag & 0x7) != 2)
                    break;

                var fieldNum = (int)(fieldTag >> 3);

                if (!TryReadVarint(data, ref j, out var strLen))
                    break;
                if (j + (int)strLen > data.Length)
                    break;

                var str = Encoding.UTF8.GetString(data, j, (int)strLen);
                j += (int)strLen;

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
}
