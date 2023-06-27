using System.Collections;

namespace LZMCompressor;

public class BitArrayReader
{
    private readonly BitArray buffer;

    public BitArrayReader(int size)
    {
        buffer = new BitArray(new byte[size]);
    }

    public BitArrayReader(byte[] data)
    {
        buffer = new BitArray(data);
    }

    public BitArrayReader(BitArray bits)
    {
        buffer = bits;
        
    }

    public int WriteOffset { get; private set; }
    public int ReadOffset { get; private set; }
    public int BufferSize => buffer.Count;

    public int ReadBitsToInt32(int bitsCount)
    {
        if (bitsCount > 1 << 5)
            throw new ArgumentException("", nameof(bitsCount));
        
        var readToOffset = ReadOffset + bitsCount;
        var bits = new BitArray(bitsCount);
        var index = 0;
        
        while (ReadOffset < readToOffset)
        {
            bits.Set(index, buffer[ReadOffset]);
            index++;
            ReadOffset++;
        }
        
        var intArray = new int[1];
        bits.CopyTo(intArray, 0);
        return intArray.First();
    }

    public BitArray ReadBits(int count)
    {
        var readToOffset = ReadOffset + count;
        var bits = new BitArray(count);
        var index = 0;
        
        while (ReadOffset < readToOffset)
        {
            bits.Set(index, buffer[ReadOffset]);
            index++;
            ReadOffset++;
        }

        return bits;
    }

    private static IEnumerable<byte> ConvertToBytes(BitArray bits)
    {
        byte result = 0;

        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i])
                result |= (byte)(1 << i);
            if ((i + 1) % 8 != 0)
                continue;
            yield return result;
            result = 0;
        }
    }
}