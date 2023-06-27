using System.Collections;
using System.Text;

namespace LZMCompressor;

public class LzwDecoder
{
    private const int BufferSize = 1 << 16;

    private readonly int dictionarySize;
    private BitArrayReader buffer;
    private int bitsPerCode = 9;

    public LzwDecoder(int dictionarySize)
    {
        this.dictionarySize = dictionarySize;
        buffer = new BitArrayReader(BufferSize);
    }

    private int MinBitsPerCode => 1 << bitsPerCode;

    public void Decode(byte[] input, StreamWriter writer)
    {
        var d = InitializeDictionary();
        buffer = new BitArrayReader(input);
        var w = string.Empty;
        var v = string.Empty;
        while (buffer.ReadOffset < buffer.BufferSize)
        {
            int code;
            try
            {
                code = buffer.ReadBitsToInt32(bitsPerCode);
            }
            catch
            {
                break;
            }
            if (d.ContainsKey(code))
            {
                v = d[code];
            }
            else
            {
                v = w + w[0];
            }
            writer.Write(v);
            var toAdd = w + v[0];
            if (toAdd.Length > 1)
                d.Add(d.Count, w + v[0]);
            w = v;
            
            if (d.Count >= MinBitsPerCode)
                bitsPerCode++;
        }
    }

    public void Decode(List<int> encoded, StreamWriter writer)
    {
        var d = InitializeDictionary();
        var w = string.Empty;
        var v = string.Empty;
        foreach (var code in encoded)
        {
            if (d.ContainsKey(code))
            {
                v = d[code];
            }
            else
            {
                v = w + w[0];
            }
            writer.Write(v);
            var toAdd = w + v[0];
            if (toAdd.Length > 1)
                d.Add(d.Count, w + v[0]);
            w = v;
        }
    }

    private void ReadToBufferIfNecessary(BinaryReader reader)
    {
        
    }

    private static Dictionary<int, string> InitializeDictionary()
    {
        var d = new Dictionary<int, string>();
        for (var i = (char) byte.MinValue; i <= byte.MaxValue; i++)
        {
            d.Add(i, i.ToString());
        }

        return d;
    }
}