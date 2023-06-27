using System.Collections;
using System.Text;

namespace LZMCompressor;

public class LzwEncoder
{
    private const int MinBytesToWrite = 1 << 12;
    private const int BufferSize = 1 << 16;

    private readonly int dictionarySize;
    private readonly BitArrayWriter buffer;
    private int bitsForCode = 9;

    public LzwEncoder(int dictionarySize)
    {
        this.dictionarySize = dictionarySize;
        buffer = new BitArrayWriter(BufferSize);
    }

    private int MinSizeForCode => 1 << bitsForCode;

    public List<int> Encode(StreamReader reader, BinaryWriter writer)
    {
        var d = InitializeDictionary();
        // var sb = new StringBuilder();
        // var previousSubstring = string.Empty;
        var encoded = new List<int>();
        var w = string.Empty;
        
        while (reader.Peek() >= 0)
        {
            var currentSymbol = (char) reader.Read();
            if (d.Count == 512)
                ;
            var wa = w + currentSymbol;
            if (d.ContainsKey(wa))
            {
                w = wa;
            }
            else
            {
                encoded.Add(d[w]);
                WriteCodeToBuffer(d[w]);
                WriteFromBufferIfNecessary(writer);
                d.Add(wa, d.Count);
                w = currentSymbol.ToString();
            }
            // var currentSymbol = (char) reader.Read();
            // sb.Append(currentSymbol);
            // var currentSubstring = sb.ToString();
            // if (!d.ContainsKey(currentSubstring))
            // {
            //     encoded.Add(d[previousSubstring]);
            //     if (d.Count < dictionarySize)
            //     {
            //         d.Add(currentSubstring, d.Count);
            //     }
            //
            //     sb.Clear();
            //     sb.Append(currentSymbol);
            // }
            //
            // previousSubstring = currentSubstring;
            if (d.Count > MinSizeForCode)
                bitsForCode++;
        }

        // if (sb.Length > 0)
        //     encoded.Add(d[sb.ToString()]);
        if (w != string.Empty)
        {
            WriteCodeToBuffer(d[w]);
            encoded.Add(d[w]);
        }
        buffer.PadLastByteWithLeadingZeros();
        WriteFromBufferIfNecessary(writer, true);
        
        return encoded;
    }

    private void WriteFromBufferIfNecessary(BinaryWriter writer, bool writeAnySize = false)
    {
        if (buffer.Offset / 8 < MinBytesToWrite && !writeAnySize)
            return;
        var bytes = buffer.ConvertToBytes();
        WriteToStream(bytes, writer);
        buffer.LeftJustNonIntegerByte();
    }

    private static void WriteToStream(IEnumerable<byte> data, BinaryWriter writer)
    {
        foreach (var @byte in data)
        {
            writer.Write(@byte);
        }
    }

    private void WriteCodeToBuffer(int code)
    {
        var bits = new BitArray(new[] { code })
        {
            Length = bitsForCode
        };
        buffer.Write(bits);
    }

    private static Dictionary<string, int> InitializeDictionary()
    {
        var d = new Dictionary<string, int>();
        for (var i = (char) byte.MinValue; i <= byte.MaxValue; i++)
        {
            d.Add(i.ToString(), i);
        }

        return d;
    }
}