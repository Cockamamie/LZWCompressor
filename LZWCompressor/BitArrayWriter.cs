using System.Collections;

namespace LZMCompressor;

public class BitArrayWriter
{
    private readonly BitArray buffer;

    public BitArrayWriter(int size)
    {
        buffer = new BitArray(new byte[size]);
    }

    public int Offset { get; private set; }

    public void Write(BitArray bits)
    {
        foreach (bool bit in bits)
        {
            buffer.Set(Offset, bit);
            Offset++;
        }
    }

    public void PadLastByteWithLeadingZeros()
    {
        while (Offset % 8 != 0)
        {
            buffer.Set(Offset, false);
            Offset++;
        }
    }

    public void LeftJustNonIntegerByte()
    {
        for (var i = Offset / 8; i < Offset; i++)
        {
            buffer.Set(i % 8, buffer[i]);
        }

        Offset %= 8;
    }

    public IEnumerable<byte> ConvertToBytes()
    {
        var bytesCount = Offset / 8;
        var integerBytesBitArray = new BitArray(buffer);
        integerBytesBitArray.Length = bytesCount * 8;
        var res = new byte[bytesCount];
        integerBytesBitArray.CopyTo(res, 0);
        return res;
    }
}