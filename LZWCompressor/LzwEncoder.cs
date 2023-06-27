using System.Collections;

namespace LZMCompressor;

public class LzwEncoder
{
    private const int MinBytesToWrite = 1 << 12;
    private const int BufferSize = 1 << 16;

    private readonly int maxDictionarySize;
    private readonly BitArrayWriter buffer;
    private int bitsPerCode = 9;

    public LzwEncoder(int maxDictionarySize)
    {
        this.maxDictionarySize = maxDictionarySize;
        buffer = new BitArrayWriter(BufferSize);
    }

    private int MinBitsPerCode => 1 << bitsPerCode;

    public void Encode(StreamReader reader, BinaryWriter writer)
    {
        var d = InitializeDictionary();
        var w = string.Empty;
        
        while (reader.Peek() >= 0)
        {
            var currentSymbol = (char) reader.Read();
            var wa = w + currentSymbol;
            if (d.ContainsKey(wa))
            {
                w = wa;
            }
            else
            {
                WriteCodeToBuffer(d[w]);
                WriteFromBufferIfNecessary(writer);
                if (d.Count <= maxDictionarySize)
                {
                    d.Add(wa, d.Count);
                }
                w = currentSymbol.ToString();
            }
            if (d.Count > MinBitsPerCode)
                bitsPerCode++;
        }
        
        if (w != string.Empty)
        {
            WriteCodeToBuffer(d[w]);
        }
        buffer.PadLastByteWithLeadingZeros();
        WriteFromBufferIfNecessary(writer, true);
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
            Length = bitsPerCode
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