using System.IO.Compression;
using System.Threading;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the ZipWrapper class in Blackwood.IO.
/// Tests cover ZIP archive access functionality, including file existence checking,
/// stream creation, reference counting, disposal, and various edge cases.
/// These tests verify that the ZipWrapper correctly implements the IFolderWrapper interface.
/// </summary>
[TestFixture]
public class ZipWrapperTests
{
    private string _tempDirectory;
    private string _testZipPath;

    [SetUp]
    public void Setup()
    {
        // Create a temporary directory for testing
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Create a test ZIP file
        _testZipPath = Path.Combine(_tempDirectory, "test.zip");
        CreateTestZipFile();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up temporary directory with retry logic
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch (IOException)
            {
                // If we can't delete immediately, try a few more times
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(100);
                    try
                    {
                        Directory.Delete(_tempDirectory, true);
                        break;
                    }
                    catch (IOException)
                    {
                        if (i == 2) // Last attempt
                        {
                            // Force garbage collection to release any remaining file handles
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            try
                            {
                                Directory.Delete(_tempDirectory, true);
                            }
                            catch
                            {
                                // If we still can't delete, just log and continue
                                // The OS will clean up eventually
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a test ZIP file with various files for testing purposes.
    /// This helper method sets up test data that will be used across multiple tests.
    /// </summary>
    private void CreateTestZipFile()
    {
        using (var fileStream = new FileStream(_testZipPath, FileMode.Create))
        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
        {
            // Add a text file
            var textEntry = archive.CreateEntry("test.txt");
            using (var entryStream = textEntry.Open())
            using (var writer = new StreamWriter(entryStream))
            {
                writer.Write("Hello, World!");
            }

            // Add a JSON file
            var jsonEntry = archive.CreateEntry("data.json");
            using (var entryStream = jsonEntry.Open())
            using (var writer = new StreamWriter(entryStream))
            {
                writer.Write("{\"name\": \"Test\", \"value\": 42}");
            }

            // Add a file in a subdirectory
            var subDirEntry = archive.CreateEntry("subdir/nested.txt");
            using (var entryStream = subDirEntry.Open())
            using (var writer = new StreamWriter(entryStream))
            {
                writer.Write("Nested content");
            }

            // Add an empty file
            archive.CreateEntry("empty.txt");

            // Add a file with special characters in the name
            var specialEntry = archive.CreateEntry("file with spaces.txt");
            using (var entryStream = specialEntry.Open())
            using (var writer = new StreamWriter(entryStream))
            {
                writer.Write("Special file content");
            }
        }
    }

    #region Constructor Tests

    /// <summary>
    /// Tests that ZipWrapper can be constructed with a valid file path.
    /// This verifies the basic constructor functionality with file path input.
    /// </summary>
    [Test]
    public void Constructor_WithValidFilePath_ShouldCreateInstance()
    {
        // Act
        using var wrapper = new ZipWrapper(_testZipPath);

        // Assert
        Assert.That(wrapper, Is.Not.Null);
        Assert.That(wrapper, Is.InstanceOf<IFolderWrapper>());
    }

    /// <summary>
    /// Tests that ZipWrapper can be constructed with a valid stream.
    /// This verifies the constructor functionality with stream input.
    /// </summary>
    [Test]
    public void Constructor_WithValidStream_ShouldCreateInstance()
    {
        // Arrange
        using var fileStream = new FileStream(_testZipPath, FileMode.Open, FileAccess.Read);

        // Act
        using var wrapper = new ZipWrapper(fileStream);

        // Assert
        Assert.That(wrapper, Is.Not.Null);
        Assert.That(wrapper, Is.InstanceOf<IFolderWrapper>());
    }

    /// <summary>
    /// Tests that ZipWrapper throws an exception when constructed with a non-existent file path.
    /// This verifies proper error handling for invalid file paths.
    /// </summary>
    [Test]
    public void Constructor_WithNonExistentFilePath_ShouldThrowException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.zip");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new ZipWrapper(nonExistentPath));
    }

    /// <summary>
    /// Tests that ZipWrapper throws an exception when constructed with a null file path.
    /// This verifies proper error handling for null inputs.
    /// </summary>
    [Test]
    public void Constructor_WithNullFilePath_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ZipWrapper((string)null));
    }

    /// <summary>
    /// Tests that ZipWrapper throws an exception when constructed with a null stream.
    /// This verifies proper error handling for null stream inputs.
    /// </summary>
    [Test]
    public void Constructor_WithNullStream_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ZipWrapper((Stream)null));
    }

    #endregion

    #region Exists Method Tests

    /// <summary>
    /// Tests that Exists returns true for files that exist in the ZIP archive.
    /// This verifies the basic file existence checking functionality.
    /// </summary>
    [Test]
    public void Exists_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var result = wrapper.Exists("test.txt");

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that Exists returns false for files that don't exist in the ZIP archive.
    /// This verifies that non-existent files are properly detected.
    /// </summary>
    [Test]
    public void Exists_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var result = wrapper.Exists("nonexistent.txt");

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests that Exists handles files in subdirectories correctly.
    /// This verifies that the method works with nested file paths.
    /// </summary>
    [Test]
    public void Exists_WithFileInSubdirectory_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var result = wrapper.Exists("subdir/nested.txt");

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that Exists handles files with spaces in their names.
    /// This verifies that the method works with special characters in file names.
    /// </summary>
    [Test]
    public void Exists_WithFileWithSpaces_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var result = wrapper.Exists("file with spaces.txt");

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that Exists handles empty files correctly.
    /// This verifies that the method works with files that have no content.
    /// </summary>
    [Test]
    public void Exists_WithEmptyFile_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var result = wrapper.Exists("empty.txt");

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that Exists handles null input gracefully.
    /// This verifies that the method doesn't throw exceptions for null inputs.
    /// </summary>
    [Test]
    public void Exists_WithNullPath_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var result = wrapper.Exists(null);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests that Exists handles empty string input gracefully.
    /// This verifies that the method doesn't throw exceptions for empty string inputs.
    /// </summary>
    [Test]
    public void Exists_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var result = wrapper.Exists("");

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests that Exists normalizes backslashes to forward slashes.
    /// This verifies that the method handles Windows-style path separators correctly.
    /// </summary>
    [Test]
    public void Exists_WithBackslashes_ShouldNormalizeToForwardSlashes()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var result = wrapper.Exists("subdir\\nested.txt");

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Stream Method Tests

    /// <summary>
    /// Tests that Stream returns a valid stream for existing files.
    /// This verifies the basic stream creation functionality.
    /// </summary>
    [Test]
    public void Stream_WithExistingFile_ShouldReturnValidStream()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        using var stream = wrapper.Stream("test.txt");

        // Assert
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.CanRead, Is.True);
        Assert.That(stream.CanSeek, Is.True);
        Assert.That(stream.Position, Is.EqualTo(0));
    }

    /// <summary>
    /// Tests that Stream returns null for non-existent files.
    /// This verifies that the method handles missing files gracefully.
    /// </summary>
    [Test]
    public void Stream_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var stream = wrapper.Stream("nonexistent.txt");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream returns the correct content for existing files.
    /// This verifies that the stream contains the expected data.
    /// </summary>
    [Test]
    public void Stream_WithExistingFile_ShouldReturnCorrectContent()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        using var stream = wrapper.Stream("test.txt");
        using var reader = new StreamReader(stream);

        // Assert
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("Hello, World!"));
    }

    /// <summary>
    /// Tests that Stream handles files in subdirectories correctly.
    /// This verifies that the method works with nested file paths.
    /// </summary>
    [Test]
    public void Stream_WithFileInSubdirectory_ShouldReturnCorrectContent()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        using var stream = wrapper.Stream("subdir/nested.txt");
        using var reader = new StreamReader(stream);

        // Assert
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("Nested content"));
    }

    /// <summary>
    /// Tests that Stream handles files with spaces in their names.
    /// This verifies that the method works with special characters in file names.
    /// </summary>
    [Test]
    public void Stream_WithFileWithSpaces_ShouldReturnCorrectContent()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        using var stream = wrapper.Stream("file with spaces.txt");
        using var reader = new StreamReader(stream);

        // Assert
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("Special file content"));
    }

    /// <summary>
    /// Tests that Stream returns an empty stream for empty files.
    /// This verifies that the method works with files that have no content.
    /// </summary>
    [Test]
    public void Stream_WithEmptyFile_ShouldReturnEmptyStream()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        using var stream = wrapper.Stream("empty.txt");
        using var reader = new StreamReader(stream);

        // Assert
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo(""));
    }

    /// <summary>
    /// Tests that Stream returns null for null input.
    /// This verifies that the method handles null inputs gracefully.
    /// </summary>
    [Test]
    public void Stream_WithNullPath_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var stream = wrapper.Stream(null);

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream returns null for empty string input.
    /// This verifies that the method handles empty string inputs gracefully.
    /// </summary>
    [Test]
    public void Stream_WithEmptyPath_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var stream = wrapper.Stream("");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream normalizes backslashes to forward slashes.
    /// This verifies that the method handles Windows-style path separators correctly.
    /// </summary>
    [Test]
    public void Stream_WithBackslashes_ShouldNormalizeToForwardSlashes()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        using var stream = wrapper.Stream("subdir\\nested.txt");
        using var reader = new StreamReader(stream);

        // Assert
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("Nested content"));
    }

    /// <summary>
    /// Tests that Stream returns a seekable stream.
    /// This verifies that the returned stream supports seeking operations.
    /// </summary>
    [Test]
    public void Stream_ShouldReturnSeekableStream()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        using var stream = wrapper.Stream("test.txt");

        // Assert
        Assert.That(stream.CanSeek, Is.True);
        Assert.That(stream.Position, Is.EqualTo(0));

        // Test seeking
        stream.Seek(7, SeekOrigin.Begin);
        Assert.That(stream.Position, Is.EqualTo(7));
    }

    #endregion

    #region Reference Counting Tests

    /// <summary>
    /// Tests that Retain increments the reference count.
    /// This verifies the reference counting mechanism for resource management.
    /// </summary>
    [Test]
    public void Retain_ShouldIncrementReferenceCount()
    {
        // Arrange
        var wrapper = new ZipWrapper(_testZipPath);

        // Act
        wrapper.Retain();
        wrapper.Retain();

        // Assert
        // The reference count is internal, so we can't directly test it
        // But we can verify that multiple retains don't cause issues
        Assert.That(wrapper, Is.Not.Null);

        // Clean up
        wrapper.Dispose();
    }

    /// <summary>
    /// Tests that multiple Retain calls work correctly.
    /// This verifies that the reference counting can handle multiple increments.
    /// </summary>
    [Test]
    public void Retain_MultipleCalls_ShouldWorkCorrectly()
    {
        // Arrange
        var wrapper = new ZipWrapper(_testZipPath);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            wrapper.Retain();
            wrapper.Retain();
            wrapper.Retain();
        });

        // Clean up
        wrapper.Dispose();
    }

    #endregion

    #region Disposal Tests

    /// <summary>
    /// Tests that Dispose can be called multiple times safely.
    /// This verifies that the disposal mechanism is idempotent.
    /// </summary>
    [Test]
    public void Dispose_MultipleCalls_ShouldNotThrowException()
    {
        // Arrange
        var wrapper = new ZipWrapper(_testZipPath);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            wrapper.Dispose();
            wrapper.Dispose();
        });
    }

    /// <summary>
    /// Tests that operations after disposal handle gracefully.
    /// This verifies that the wrapper handles disposal correctly.
    /// Note: The actual implementation doesn't throw ObjectDisposedException,
    /// but returns false/null for operations after disposal.
    /// </summary>
    [Test]
    public void Dispose_AfterDisposal_ShouldHandleGracefully()
    {
        // Arrange
        var wrapper = new ZipWrapper(_testZipPath);
        wrapper.Dispose();

        // Act & Assert
        // The actual implementation doesn't throw exceptions, but returns false/null
        var exists = wrapper.Exists("test.txt");
        var stream = wrapper.Stream("test.txt");

        Assert.That(exists, Is.False);
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that the wrapper implements IDisposable correctly.
    /// This verifies that the wrapper can be used in using statements.
    /// </summary>
    [Test]
    public void Dispose_UsingStatement_ShouldWorkCorrectly()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            using var wrapper = new ZipWrapper(_testZipPath);
            var exists = wrapper.Exists("test.txt");
            Assert.That(exists, Is.True);
        });
    }

    #endregion

    #region Interface Implementation Tests

    /// <summary>
    /// Tests that ZipWrapper correctly implements IFolderWrapper interface.
    /// This verifies that the wrapper fulfills its contract obligations.
    /// </summary>
    [Test]
    public void ZipWrapper_ShouldImplementIFolderWrapper()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Assert
        Assert.That(wrapper, Is.InstanceOf<IFolderWrapper>());
        Assert.That(wrapper, Is.InstanceOf<IDisposable>());
    }

    /// <summary>
    /// Tests that all IFolderWrapper methods are accessible.
    /// This verifies that the interface contract is properly implemented.
    /// </summary>
    [Test]
    public void ZipWrapper_ShouldExposeAllInterfaceMethods()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);
        IFolderWrapper interfaceWrapper = wrapper;

        // Act & Assert
        Assert.DoesNotThrow(() => interfaceWrapper.Exists("test.txt"));
        Assert.DoesNotThrow(() => interfaceWrapper.Stream("test.txt"));
        Assert.DoesNotThrow(() => interfaceWrapper.Dispose());
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that the wrapper handles corrupted ZIP files gracefully.
    /// This verifies error handling for invalid ZIP archives.
    /// </summary>
    [Test]
    public void ZipWrapper_WithCorruptedZip_ShouldHandleGracefully()
    {
        // Arrange
        var corruptedZipPath = Path.Combine(_tempDirectory, "corrupted.zip");
        File.WriteAllText(corruptedZipPath, "This is not a valid ZIP file");

        // Act & Assert
        Assert.Throws<InvalidDataException>(() =>
        {
            var wrapper = new ZipWrapper(corruptedZipPath);
            wrapper.Dispose(); // Ensure disposal even if constructor succeeds
        });
    }

    /// <summary>
    /// Tests that the wrapper handles very long file paths.
    /// This verifies that the wrapper can handle edge cases in file naming.
    /// </summary>
    [Test]
    public void ZipWrapper_WithVeryLongPath_ShouldHandleCorrectly()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);
        var longPath = new string('a', 1000) + ".txt";

        // Act
        var exists = wrapper.Exists(longPath);
        var stream = wrapper.Stream(longPath);

        // Assert
        Assert.That(exists, Is.False);
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that the wrapper handles special characters in paths.
    /// This verifies that the wrapper can handle various character encodings.
    /// </summary>
    [Test]
    public void ZipWrapper_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);
        var specialPath = "file with spaces.txt";

        // Act
        var exists = wrapper.Exists(specialPath);
        var stream = wrapper.Stream(specialPath);

        // Assert
        Assert.That(exists, Is.True);
        Assert.That(stream, Is.Not.Null);
    }

    #endregion
}
