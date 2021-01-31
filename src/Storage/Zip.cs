using System.IO;
using System.IO.Compression;

namespace Blackwood
{
static partial class Util
{
    /// <summary>
    /// Compresses the array
    /// </summary>
    /// <param name="array">The array to compress</param>
    /// <param name="offset">The offset in the array to start</param>
    /// <param name="count">The number of bytes to compress</param>
    /// <returns>The compressed bytes</returns>
    public static byte[] Zip(byte[] array, int offset, int count)
    {
        // Create a memory stream to hold the bytes
        using var memoryStream = new MemoryStream();
        // Compress the bytes to memory stream
        using (var zip = new GZipStream(memoryStream, CompressionMode.Compress, false))
        {
            zip.Write(array, offset, count);
            zip.Flush();
        }

        // Return the results
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Decompresses the array
    /// </summary>
    /// <param name="zippedBuffer">The buffer of compressed data</param>
    /// <returns>The decompressed data</returns>
    public static byte[] Unzip(byte[] zippedBuffer)
    {
        // Create a memory stream to hold the input bytes
        using var memoryStream = new MemoryStream(zippedBuffer);
        // Decompress the bytes from memory stream
        using var zip = new GZipStream(memoryStream, CompressionMode.Decompress, false);
        // put them in the following
        // TODO: add max capacity to handle zipbombs
        using var decompressed = new MemoryStream();
        zip.CopyTo(decompressed);
        return decompressed.ToArray();
    }
}
}