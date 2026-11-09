using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace WtcPractice;

public static class PngReader
{
    private const int SignatureLength = 8;
    private const int ChunkOverhead = 12;
    private const int IndexedColorType = 3;

    public static byte[] ReadRgba(byte[] file, out int width, out int height)
    {
        width = 0;
        height = 0;
        int depth = 0;
        byte[] palette = null;
        byte[] alpha = Array.Empty<byte>();
        using MemoryStream compressed = new();

        for (int position = SignatureLength; position < file.Length;)
        {
            int length = BinaryPrimitives.ReadInt32BigEndian(file.AsSpan(position));
            string type = Encoding.ASCII.GetString(file, position + 4, 4);
            int data = position + 8;

            if (type == "IHDR")
            {
                width = BinaryPrimitives.ReadInt32BigEndian(file.AsSpan(data));
                height = BinaryPrimitives.ReadInt32BigEndian(file.AsSpan(data + 4));
                depth = file[data + 8];

                if (file[data + 9] != IndexedColorType)
                    throw new NotSupportedException("Only indexed PNGs are supported");
            }
            else if (type == "PLTE")
            {
                palette = file.AsSpan(data, length).ToArray();
            }
            else if (type == "tRNS")
            {
                alpha = file.AsSpan(data, length).ToArray();
            }
            else if (type == "IDAT")
            {
                compressed.Write(file, data, length);
            }

            position += length + ChunkOverhead;
        }

        int stride = (width * depth + 7) / 8;
        byte[] lines = Inflate(compressed.ToArray(), (stride + 1) * height);
        Unfilter(lines, stride, height);
        return ToRgba(lines, stride, width, height, depth, palette, alpha);
    }

    private static byte[] Inflate(byte[] compressed, int size)
    {
        using ZLibStream stream = new(new MemoryStream(compressed), CompressionMode.Decompress);
        using MemoryStream result = new(size);
        stream.CopyTo(result);
        return result.ToArray();
    }

    private static void Unfilter(byte[] lines, int stride, int height)
    {
        for (int y = 0; y < height; y++)
        {
            int line = y * (stride + 1);
            byte filter = lines[line];
            int start = line + 1;

            for (int x = 0; x < stride; x++)
            {
                int left = x > 0 ? lines[start + x - 1] : 0;
                int up = y > 0 ? lines[start + x - stride - 1] : 0;
                int upLeft = x > 0 && y > 0 ? lines[start + x - stride - 2] : 0;
                lines[start + x] += Prediction(filter, left, up, upLeft);
            }
        }
    }

    private static byte Prediction(byte filter, int left, int up, int upLeft)
    {
        return filter switch
        {
            0 => 0,
            1 => (byte)left,
            2 => (byte)up,
            3 => (byte)((left + up) / 2),
            4 => Paeth(left, up, upLeft),
            _ => throw new NotSupportedException($"Unknown PNG filter {filter}"),
        };
    }

    private static byte Paeth(int left, int up, int upLeft)
    {
        int estimate = left + up - upLeft;
        int toLeft = Math.Abs(estimate - left);
        int toUp = Math.Abs(estimate - up);
        int toUpLeft = Math.Abs(estimate - upLeft);

        if (toLeft <= toUp && toLeft <= toUpLeft)
            return (byte)left;

        return (byte)(toUp <= toUpLeft ? up : upLeft);
    }

    private static byte[] ToRgba(byte[] lines, int stride, int width, int height, int depth, byte[] palette, byte[] alpha)
    {
        byte[] rgba = new byte[width * height * 4];
        int mask = (1 << depth) - 1;

        for (int y = 0; y < height; y++)
        {
            int start = y * (stride + 1) + 1;

            for (int x = 0; x < width; x++)
            {
                int bit = x * depth;
                int index = (lines[start + bit / 8] >> (8 - depth - bit % 8)) & mask;
                int pixel = (y * width + x) * 4;

                rgba[pixel] = palette[index * 3];
                rgba[pixel + 1] = palette[index * 3 + 1];
                rgba[pixel + 2] = palette[index * 3 + 2];
                rgba[pixel + 3] = index < alpha.Length ? alpha[index] : (byte)255;
            }
        }

        return rgba;
    }
}
