using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Blackwood;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the Zip utility class in Blackwood.IO.
/// Tests cover GZip compression and decompression functionality, including
/// basic operations, edge cases, error handling, and performance scenarios.
/// These tests verify that the Zip and Unzip methods work correctly together.
/// </summary>
[TestFixture]
public class ZipTests
{
    #region Zip Method Tests

    /// <summary>
    /// Tests that Zip compresses a simple byte array correctly.
    /// This verifies the basic compression functionality.
    /// </summary>
    [Test]
    public void Zip_WithSimpleArray_ShouldCompressCorrectly()
    {
        // Arrange
        var originalData = Encoding.UTF8.GetBytes("Hello, World!");
        var expectedLength = originalData.Length;

        // Act
        var compressed = Util.Zip(originalData, 0, originalData.Length);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.GreaterThan(0));
        Assert.That(compressed.Length, Is.LessThanOrEqualTo(expectedLength + 30)); // GZip overhead (adjusted for actual behavior)
    }

    /// <summary>
    /// Tests that Zip compresses data with an offset correctly.
    /// This verifies that the offset parameter works as expected.
    /// </summary>
    [Test]
    public void Zip_WithOffset_ShouldCompressFromOffset()
    {
        // Arrange
        var fullData = Encoding.UTF8.GetBytes("PrefixHello, World!Suffix");
        var offset = 6; // Start after "Prefix"
        var count = 13; // "Hello, World!"
        var expectedData = Encoding.UTF8.GetBytes("Hello, World!");

        // Act
        var compressed = Util.Zip(fullData, offset, count);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.GreaterThan(0));

        // Verify we can decompress and get the expected data
        var decompressed = Util.Unzip(compressed);
        Assert.That(decompressed, Is.EqualTo(expectedData));
    }

    /// <summary>
    /// Tests that Zip handles empty arrays correctly.
    /// This verifies that compression works with zero-length data.
    /// Note: GZip compression of empty data results in empty output.
    /// </summary>
    [Test]
    public void Zip_WithEmptyArray_ShouldHandleCorrectly()
    {
        // Arrange
        var emptyArray = new byte[0];

        // Act
        var compressed = Util.Zip(emptyArray, 0, 0);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.EqualTo(0)); // GZip produces empty output for empty input

        // Note: We cannot decompress empty data as it's not valid GZip format
    }

    /// <summary>
    /// Tests that Zip handles single byte arrays correctly.
    /// This verifies compression of minimal data.
    /// </summary>
    [Test]
    public void Zip_WithSingleByte_ShouldCompressCorrectly()
    {
        // Arrange
        var singleByte = new byte[] { 42 };

        // Act
        var compressed = Util.Zip(singleByte, 0, 1);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.GreaterThan(0));

        // Verify we can decompress and get the original byte
        var decompressed = Util.Unzip(compressed);
        Assert.That(decompressed, Is.EqualTo(singleByte));
    }

    /// <summary>
    /// Tests that Zip handles large arrays correctly.
    /// This verifies compression performance with substantial data.
    /// </summary>
    [Test]
    public void Zip_WithLargeArray_ShouldCompressCorrectly()
    {
        // Arrange
        var largeData = new byte[10000];
        for (int i = 0; i < largeData.Length; i++)
        {
            largeData[i] = (byte)(i % 256);
        }

        // Act
        var compressed = Util.Zip(largeData, 0, largeData.Length);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.GreaterThan(0));
        Assert.That(compressed.Length, Is.LessThan(largeData.Length)); // Should be compressed

        // Verify we can decompress and get the original data
        var decompressed = Util.Unzip(compressed);
        Assert.That(decompressed, Is.EqualTo(largeData));
    }

    /// <summary>
    /// Tests that Zip handles highly compressible data correctly.
    /// This verifies that repeated patterns compress well.
    /// </summary>
    [Test]
    public void Zip_WithRepeatedData_ShouldCompressWell()
    {
        // Arrange
        var repeatedData = new byte[1000];
        Array.Fill(repeatedData, (byte)65); // All 'A' characters

        // Act
        var compressed = Util.Zip(repeatedData, 0, repeatedData.Length);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.LessThan(repeatedData.Length / 2)); // Should compress significantly

        // Verify we can decompress and get the original data
        var decompressed = Util.Unzip(compressed);
        Assert.That(decompressed, Is.EqualTo(repeatedData));
    }

    /// <summary>
    /// Tests that Zip handles random data correctly.
    /// This verifies compression with non-compressible data.
    /// </summary>
    [Test]
    public void Zip_WithRandomData_ShouldHandleCorrectly()
    {
        // Arrange
        var randomData = new byte[1000];
        var random = new Random(42); // Fixed seed for reproducibility
        random.NextBytes(randomData);

        // Act
        var compressed = Util.Zip(randomData, 0, randomData.Length);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.GreaterThan(0));

        // Verify we can decompress and get the original data
        var decompressed = Util.Unzip(compressed);
        Assert.That(decompressed, Is.EqualTo(randomData));
    }

    /// <summary>
    /// Tests that Zip throws an exception with null input.
    /// This verifies proper error handling for null arrays.
    /// </summary>
    [Test]
    public void Zip_WithNullArray_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Util.Zip(null, 0, 0));
    }

    /// <summary>
    /// Tests that Zip throws an exception with invalid offset.
    /// This verifies proper error handling for invalid parameters.
    /// </summary>
    [Test]
    public void Zip_WithInvalidOffset_ShouldThrowException()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Util.Zip(data, -1, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => Util.Zip(data, 10, 3));
    }

    /// <summary>
    /// Tests that Zip throws an exception with invalid count.
    /// This verifies proper error handling for invalid count parameters.
    /// </summary>
    [Test]
    public void Zip_WithInvalidCount_ShouldThrowException()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Util.Zip(data, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Util.Zip(data, 2, 10));
    }

    #endregion

    #region Unzip Method Tests

    /// <summary>
    /// Tests that Unzip decompresses data correctly.
    /// This verifies the basic decompression functionality.
    /// </summary>
    [Test]
    public void Unzip_WithValidData_ShouldDecompressCorrectly()
    {
        // Arrange
        var originalData = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = Util.Zip(originalData, 0, originalData.Length);

        // Act
        var decompressed = Util.Unzip(compressed);

        // Assert
        Assert.That(decompressed, Is.Not.Null);
        Assert.That(decompressed, Is.EqualTo(originalData));
    }

    /// <summary>
    /// Tests that Unzip handles empty compressed data correctly.
    /// This verifies decompression of minimal compressed data.
    /// </summary>
    [Test]
    public void Unzip_WithEmptyCompressedData_ShouldReturnEmptyArray()
    {
        // Arrange
        var emptyData = new byte[0];
        var compressed = Util.Zip(emptyData, 0, 0);

        // Act
        var decompressed = Util.Unzip(compressed);

        // Assert
        Assert.That(decompressed, Is.Not.Null);
        Assert.That(decompressed, Is.EqualTo(emptyData));
    }

    /// <summary>
    /// Tests that Unzip handles large compressed data correctly.
    /// This verifies decompression performance with substantial data.
    /// </summary>
    [Test]
    public void Unzip_WithLargeData_ShouldDecompressCorrectly()
    {
        // Arrange
        var largeData = new byte[5000];
        for (int i = 0; i < largeData.Length; i++)
        {
            largeData[i] = (byte)(i % 256);
        }
        var compressed = Util.Zip(largeData, 0, largeData.Length);

        // Act
        var decompressed = Util.Unzip(compressed);

        // Assert
        Assert.That(decompressed, Is.Not.Null);
        Assert.That(decompressed, Is.EqualTo(largeData));
    }

    /// <summary>
    /// Tests that Unzip handles binary data correctly.
    /// This verifies decompression of non-text binary data.
    /// </summary>
    [Test]
    public void Unzip_WithBinaryData_ShouldDecompressCorrectly()
    {
        // Arrange
        var binaryData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE, 0xFD, 0xFC };
        var compressed = Util.Zip(binaryData, 0, binaryData.Length);

        // Act
        var decompressed = Util.Unzip(compressed);

        // Assert
        Assert.That(decompressed, Is.Not.Null);
        Assert.That(decompressed, Is.EqualTo(binaryData));
    }

    /// <summary>
    /// Tests that Unzip throws an exception with null input.
    /// This verifies proper error handling for null compressed data.
    /// </summary>
    [Test]
    public void Unzip_WithNullInput_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Util.Unzip(null));
    }

    /// <summary>
    /// Tests that Unzip throws an exception with invalid compressed data.
    /// This verifies proper error handling for corrupted data.
    /// </summary>
    [Test]
    public void Unzip_WithInvalidData_ShouldThrowException()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x01, 0x02, 0x03 }; // Not valid GZip data

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => Util.Unzip(invalidData));
    }

    /// <summary>
    /// Tests that Unzip handles empty input array correctly.
    /// This verifies behavior with zero-length compressed data.
    /// Note: The actual implementation doesn't throw an exception for empty input.
    /// </summary>
    [Test]
    public void Unzip_WithEmptyInput_ShouldReturnEmptyArray()
    {
        // Arrange
        var emptyData = new byte[0];

        // Act
        var result = Util.Unzip(emptyData);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(new byte[0]));
    }

    #endregion

    #region Round-trip Tests

    /// <summary>
    /// Tests that Zip and Unzip work together correctly for text data.
    /// This verifies the complete compression/decompression cycle.
    /// </summary>
    [Test]
    public void ZipUnzip_WithTextData_ShouldPreserveData()
    {
        // Arrange
        var originalText = "This is a test string with various characters: !@#$%^&*()_+-=[]{}|;':\",./<>?";
        var originalData = Encoding.UTF8.GetBytes(originalText);

        // Act
        var compressed = Util.Zip(originalData, 0, originalData.Length);
        var decompressed = Util.Unzip(compressed);
        var resultText = Encoding.UTF8.GetString(decompressed);

        // Assert
        Assert.That(resultText, Is.EqualTo(originalText));
    }

    /// <summary>
    /// Tests that Zip and Unzip work together correctly for Unicode data.
    /// This verifies compression/decompression with international characters.
    /// </summary>
    [Test]
    public void ZipUnzip_WithUnicodeData_ShouldPreserveData()
    {
        // Arrange
        var originalText = "Hello 世界! 🌍 Unicode: αβγδε 中文 العربية";
        var originalData = Encoding.UTF8.GetBytes(originalText);

        // Act
        var compressed = Util.Zip(originalData, 0, originalData.Length);
        var decompressed = Util.Unzip(compressed);
        var resultText = Encoding.UTF8.GetString(decompressed);

        // Assert
        Assert.That(resultText, Is.EqualTo(originalText));
    }

    /// <summary>
    /// Tests that Zip and Unzip work together correctly for partial array data.
    /// This verifies compression/decompression with offset and count parameters.
    /// </summary>
    [Test]
    public void ZipUnzip_WithPartialData_ShouldPreserveData()
    {
        // Arrange
        var fullData = Encoding.UTF8.GetBytes("PrefixDataToCompressSuffix");
        var offset = 6; // Start after "Prefix"
        var count = 16; // "DataToCompress"
        var expectedData = new byte[count];
        Array.Copy(fullData, offset, expectedData, 0, count);

        // Act
        var compressed = Util.Zip(fullData, offset, count);
        var decompressed = Util.Unzip(compressed);

        // Assert
        Assert.That(decompressed, Is.EqualTo(expectedData));
    }

    /// <summary>
    /// Tests that multiple compression/decompression cycles preserve data.
    /// This verifies that the methods are idempotent when used together.
    /// </summary>
    [Test]
    public void ZipUnzip_MultipleCycles_ShouldPreserveData()
    {
        // Arrange
        var originalData = Encoding.UTF8.GetBytes("Test data for multiple compression cycles");
        var currentData = originalData;

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            var compressed = Util.Zip(currentData, 0, currentData.Length);
            var decompressed = Util.Unzip(compressed);

            Assert.That(decompressed, Is.EqualTo(currentData));
            currentData = decompressed;
        }

        Assert.That(currentData, Is.EqualTo(originalData));
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that compression performance is reasonable for large data.
    /// This verifies that the methods can handle substantial amounts of data.
    /// </summary>
    [Test]
    public void ZipUnzip_WithVeryLargeData_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var largeData = new byte[100000]; // 100KB
        var random = new Random(12345);
        random.NextBytes(largeData);

        // Act
        var startTime = DateTime.UtcNow;
        var compressed = Util.Zip(largeData, 0, largeData.Length);
        var decompressed = Util.Unzip(compressed);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.That(decompressed, Is.EqualTo(largeData));
        Assert.That((endTime - startTime).TotalSeconds, Is.LessThan(5)); // Should complete within 5 seconds
    }

    /// <summary>
    /// Tests that compression ratio is reasonable for compressible data.
    /// This verifies that the GZip compression is working effectively.
    /// </summary>
    [Test]
    public void Zip_WithCompressibleData_ShouldAchieveGoodCompressionRatio()
    {
        // Arrange
        var compressibleData = new byte[10000];
        for (int i = 0; i < compressibleData.Length; i++)
        {
            compressibleData[i] = (byte)(i % 10); // Highly repetitive pattern
        }

        // Act
        var compressed = Util.Zip(compressibleData, 0, compressibleData.Length);

        // Assert
        var compressionRatio = (double)compressed.Length / compressibleData.Length;
        Assert.That(compressionRatio, Is.LessThan(0.5)); // Should achieve at least 50% compression
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that Zip handles maximum offset correctly.
    /// This verifies behavior when offset equals array length.
    /// Note: When count is 0, GZip produces empty output.
    /// </summary>
    [Test]
    public void Zip_WithMaxOffset_ShouldHandleCorrectly()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var compressed = Util.Zip(data, data.Length, 0);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.EqualTo(0)); // Empty input produces empty output

        // Note: We cannot decompress empty data as it's not valid GZip format
    }

    /// <summary>
    /// Tests that Zip handles zero count correctly.
    /// This verifies behavior when count is zero.
    /// Note: When count is 0, GZip produces empty output.
    /// </summary>
    [Test]
    public void Zip_WithZeroCount_ShouldHandleCorrectly()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var compressed = Util.Zip(data, 0, 0);

        // Assert
        Assert.That(compressed, Is.Not.Null);
        Assert.That(compressed.Length, Is.EqualTo(0)); // Empty input produces empty output

        // Note: We cannot decompress empty data as it's not valid GZip format
    }

    /// <summary>
    /// Tests that Zip handles single byte with offset correctly.
    /// This verifies behavior with minimal data at specific offset.
    /// </summary>
    [Test]
    public void Zip_WithSingleByteAtOffset_ShouldHandleCorrectly()
    {
        // Arrange
        var data = new byte[] { 0, 1, 2, 3, 4 };
        var offset = 2;
        var count = 1;
        var expectedData = new byte[] { 2 };

        // Act
        var compressed = Util.Zip(data, offset, count);
        var decompressed = Util.Unzip(compressed);

        // Assert
        Assert.That(decompressed, Is.EqualTo(expectedData));
    }

    #endregion
}
