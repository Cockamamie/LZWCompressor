namespace LZMCompressor;

public class LzwDecoder
{
    // private const int BytesToRead = 1 << 12;
    // private const int BufferSize = 1 << 16;

    private readonly int maxDictionarySize;
    private BitArrayReader buffer;
    private int bitsPerCode = 9;

    public LzwDecoder(int maxDictionarySize)
    {
        this.maxDictionarySize = maxDictionarySize;
        //buffer = new BitArrayReader(BufferSize);
    }

    private int MinBitsPerCode => 1 << bitsPerCode;

    public void Decode(byte[] input, StreamWriter writer)
    {
        var d = InitializeDictionary();
        buffer = new BitArrayReader(input);
        var w = string.Empty;
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

            var v = d.ContainsKey(code) ? d[code] : w + w[0];
            writer.Write(v);
            var toAdd = w + v[0];
            if (toAdd.Length > 1 && d.Count <= maxDictionarySize)
                d.Add(d.Count, w + v[0]);
            w = v;
            
            if (d.Count >= MinBitsPerCode)
                bitsPerCode++;
        }
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