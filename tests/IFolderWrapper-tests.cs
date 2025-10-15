using System.Text;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the IFolderWrapper interface in Blackwood.IO.
/// Tests cover the interface contract and expected behavior for implementations,
/// including file existence checking, stream creation, disposal, and various edge cases.
/// These tests verify that implementations correctly follow the interface contract.
/// </summary>
[TestFixture]
public class IFolderWrapperTests
{
    private string _tempDirectory;
    private string _testZipPath;

    [SetUp]
    public void Setup()
    {
        // Create a temporary directory for testing
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Create test files
        File.WriteAllText(Path.Combine(_tempDirectory, "test.txt"), "Hello, World!");
        File.WriteAllText(Path.Combine(_tempDirectory, "empty.txt"), "");
        File.WriteAllText(Path.Combine(_tempDirectory, "unicode.txt"), "Hello 世界! 🌍");

        // Create subdirectory with files
        var subDir = Path.Combine(_tempDirectory, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "nested.txt"), "Nested content");
        File.WriteAllText(Path.Combine(subDir, "file with spaces.txt"), "File with spaces");

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
                    System.Threading.Thread.Sleep(100);
                    try
                    {
                        Directory.Delete(_tempDirectory, true);
                        break;
                    }
                    catch (IOException)
                    {
                        if (i == 2) throw; // Re-throw on final attempt
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a test ZIP file with various content for testing.
    /// This provides a consistent test environment for ZIP-based implementations.
    /// </summary>
    private void CreateTestZipFile()
    {
        using var fileStream = new FileStream(_testZipPath, FileMode.Create);
        using var archive = new System.IO.Compression.ZipArchive(fileStream, System.IO.Compression.ZipArchiveMode.Create);

        // Add test files to ZIP
        var testFile = archive.CreateEntry("test.txt");
        using (var entryStream = testFile.Open())
        using (var writer = new StreamWriter(entryStream))
        {
            writer.Write("Hello, World!");
        }

        var emptyFile = archive.CreateEntry("empty.txt");
        using (var entryStream = emptyFile.Open())
        {
            // Empty file
        }

        var unicodeFile = archive.CreateEntry("unicode.txt");
        using (var entryStream = unicodeFile.Open())
        using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
        {
            writer.Write("Hello 世界! 🌍");
        }

        var nestedFile = archive.CreateEntry("subdir/nested.txt");
        using (var entryStream = nestedFile.Open())
        using (var writer = new StreamWriter(entryStream))
        {
            writer.Write("Nested content");
        }
    }

    #region Interface Contract Tests

    /// <summary>
    /// Tests that FolderWrapper correctly implements the IFolderWrapper interface.
    /// This verifies that the implementation follows the interface contract.
    /// </summary>
    [Test]
    public void FolderWrapper_ShouldImplementIFolderWrapper()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        Assert.That(wrapper, Is.InstanceOf<IFolderWrapper>());
        Assert.That(wrapper, Is.InstanceOf<IDisposable>());
    }

    /// <summary>
    /// Tests that ZipWrapper correctly implements the IFolderWrapper interface.
    /// This verifies that the implementation follows the interface contract.
    /// </summary>
    [Test]
    public void ZipWrapper_ShouldImplementIFolderWrapper()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act & Assert
        Assert.That(wrapper, Is.InstanceOf<IFolderWrapper>());
        Assert.That(wrapper, Is.InstanceOf<IDisposable>());
    }

    /// <summary>
    /// Tests that EmbeddedResources correctly implements the IFolderWrapper interface.
    /// This verifies that the implementation follows the interface contract.
    /// </summary>
    [Test]
    public void EmbeddedResources_ShouldImplementIFolderWrapper()
    {
        // Arrange
        using var wrapper = new EmbeddedResources();

        // Act & Assert
        Assert.That(wrapper, Is.InstanceOf<IFolderWrapper>());
        Assert.That(wrapper, Is.InstanceOf<IDisposable>());
    }

    #endregion

    #region Exists Method Tests

    /// <summary>
    /// Tests that Exists returns true for existing files in FolderWrapper.
    /// This verifies the basic file existence checking functionality.
    /// </summary>
    [Test]
    public void FolderWrapper_Exists_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("test.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that Exists returns false for non-existent files in FolderWrapper.
    /// This verifies that non-existent files are correctly identified.
    /// </summary>
    [Test]
    public void FolderWrapper_Exists_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("nonexistent.txt");

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists returns true for existing files in ZipWrapper.
    /// This verifies the basic file existence checking functionality in ZIP archives.
    /// </summary>
    [Test]
    public void ZipWrapper_Exists_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var exists = wrapper.Exists("test.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that Exists returns false for non-existent files in ZipWrapper.
    /// This verifies that non-existent files are correctly identified in ZIP archives.
    /// </summary>
    [Test]
    public void ZipWrapper_Exists_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var exists = wrapper.Exists("nonexistent.txt");

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists handles null input gracefully in FolderWrapper.
    /// This verifies proper null handling in the interface implementation.
    /// Note: The actual implementation throws ArgumentNullException for null input.
    /// </summary>
    [Test]
    public void FolderWrapper_Exists_WithNullPath_ShouldThrowException()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => wrapper.Exists(null));
    }

    /// <summary>
    /// Tests that Exists handles null input gracefully in ZipWrapper.
    /// This verifies proper null handling in the interface implementation.
    /// </summary>
    [Test]
    public void ZipWrapper_Exists_WithNullPath_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var exists = wrapper.Exists(null);

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists handles empty string input gracefully in FolderWrapper.
    /// This verifies proper empty string handling in the interface implementation.
    /// </summary>
    [Test]
    public void FolderWrapper_Exists_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("");

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists handles empty string input gracefully in ZipWrapper.
    /// This verifies proper empty string handling in the interface implementation.
    /// </summary>
    [Test]
    public void ZipWrapper_Exists_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var exists = wrapper.Exists("");

        // Assert
        Assert.That(exists, Is.False);
    }

    #endregion

    #region Stream Method Tests

    /// <summary>
    /// Tests that Stream returns a valid stream for existing files in FolderWrapper.
    /// This verifies the basic stream creation functionality.
    /// </summary>
    [Test]
    public void FolderWrapper_Stream_WithExistingFile_ShouldReturnValidStream()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        using var stream = wrapper.Stream("test.txt");

        // Assert
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.CanRead, Is.True);

        // Verify content
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("Hello, World!"));
    }

    /// <summary>
    /// Tests that Stream returns null for non-existent files in FolderWrapper.
    /// This verifies that non-existent files return null streams.
    /// </summary>
    [Test]
    public void FolderWrapper_Stream_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var stream = wrapper.Stream("nonexistent.txt");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream returns a valid stream for existing files in ZipWrapper.
    /// This verifies the basic stream creation functionality in ZIP archives.
    /// </summary>
    [Test]
    public void ZipWrapper_Stream_WithExistingFile_ShouldReturnValidStream()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        using var stream = wrapper.Stream("test.txt");

        // Assert
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.CanRead, Is.True);

        // Verify content
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("Hello, World!"));
    }

    /// <summary>
    /// Tests that Stream returns null for non-existent files in ZipWrapper.
    /// This verifies that non-existent files return null streams in ZIP archives.
    /// </summary>
    [Test]
    public void ZipWrapper_Stream_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var stream = wrapper.Stream("nonexistent.txt");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream handles null input gracefully in FolderWrapper.
    /// This verifies proper null handling in the interface implementation.
    /// Note: The actual implementation throws ArgumentNullException for null input.
    /// </summary>
    [Test]
    public void FolderWrapper_Stream_WithNullPath_ShouldThrowException()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => wrapper.Stream(null));
    }

    /// <summary>
    /// Tests that Stream handles null input gracefully in ZipWrapper.
    /// This verifies proper null handling in the interface implementation.
    /// </summary>
    [Test]
    public void ZipWrapper_Stream_WithNullPath_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var stream = wrapper.Stream(null);

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream handles empty string input gracefully in FolderWrapper.
    /// This verifies proper empty string handling in the interface implementation.
    /// </summary>
    [Test]
    public void FolderWrapper_Stream_WithEmptyPath_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var stream = wrapper.Stream("");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream handles empty string input gracefully in ZipWrapper.
    /// This verifies proper empty string handling in the interface implementation.
    /// </summary>
    [Test]
    public void ZipWrapper_Stream_WithEmptyPath_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new ZipWrapper(_testZipPath);

        // Act
        var stream = wrapper.Stream("");

        // Assert
        Assert.That(stream, Is.Null);
    }

    #endregion

    #region Disposal Tests

    /// <summary>
    /// Tests that implementations can be disposed using the using statement.
    /// This verifies that the IDisposable interface is properly implemented.
    /// </summary>
    [Test]
    public void IFolderWrapper_UsingStatement_ShouldWorkCorrectly()
    {
        // Arrange & Act
        IFolderWrapper folderWrapper;
        IFolderWrapper zipWrapper;

        using (folderWrapper = new FolderWrapper(_tempDirectory))
        using (zipWrapper = new ZipWrapper(_testZipPath))
        {
            // Assert - should not throw exceptions
            Assert.That(folderWrapper.Exists("test.txt"), Is.True);
            Assert.That(zipWrapper.Exists("test.txt"), Is.True);
        }

        // After disposal, the objects should be disposed
        // Note: We can't test this directly as the interface doesn't expose disposal state
        Assert.That(folderWrapper, Is.Not.Null);
        Assert.That(zipWrapper, Is.Not.Null);
    }

    /// <summary>
    /// Tests that multiple disposal calls don't throw exceptions.
    /// This verifies that implementations handle multiple disposal calls gracefully.
    /// </summary>
    [Test]
    public void IFolderWrapper_MultipleDisposal_ShouldNotThrowException()
    {
        // Arrange
        var folderWrapper = new FolderWrapper(_tempDirectory);
        var zipWrapper = new ZipWrapper(_testZipPath);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            folderWrapper.Dispose();
            folderWrapper.Dispose();
            zipWrapper.Dispose();
            zipWrapper.Dispose();
        });
    }

    #endregion

    #region Unicode and Special Characters Tests

    /// <summary>
    /// Tests that implementations handle Unicode file names correctly.
    /// This verifies that Unicode characters in file names are properly supported.
    /// </summary>
    [Test]
    public void IFolderWrapper_WithUnicodeFileNames_ShouldHandleCorrectly()
    {
        // Arrange
        using var folderWrapper = new FolderWrapper(_tempDirectory);
        using var zipWrapper = new ZipWrapper(_testZipPath);

        // Act & Assert
        Assert.That(folderWrapper.Exists("unicode.txt"), Is.True);
        Assert.That(zipWrapper.Exists("unicode.txt"), Is.True);

        // Verify content
        using var folderStream = folderWrapper.Stream("unicode.txt");
        using var zipStream = zipWrapper.Stream("unicode.txt");

        Assert.That(folderStream, Is.Not.Null);
        Assert.That(zipStream, Is.Not.Null);

        using var folderReader = new StreamReader(folderStream);
        using var zipReader = new StreamReader(zipStream);

        var folderContent = folderReader.ReadToEnd();
        var zipContent = zipReader.ReadToEnd();

        Assert.That(folderContent, Is.EqualTo("Hello 世界! 🌍"));
        Assert.That(zipContent, Is.EqualTo("Hello 世界! 🌍"));
    }

    /// <summary>
    /// Tests that implementations handle files with spaces in names correctly.
    /// This verifies that file names with spaces are properly supported.
    /// </summary>
    [Test]
    public void IFolderWrapper_WithSpacesInFileNames_ShouldHandleCorrectly()
    {
        // Arrange
        using var folderWrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        Assert.That(folderWrapper.Exists("subdir/file with spaces.txt"), Is.True);

        // Verify content
        using var stream = folderWrapper.Stream("subdir/file with spaces.txt");
        Assert.That(stream, Is.Not.Null);

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("File with spaces"));
    }

    #endregion

    #region Empty Files Tests

    /// <summary>
    /// Tests that implementations handle empty files correctly.
    /// This verifies that empty files are properly supported.
    /// </summary>
    [Test]
    public void IFolderWrapper_WithEmptyFiles_ShouldHandleCorrectly()
    {
        // Arrange
        using var folderWrapper = new FolderWrapper(_tempDirectory);
        using var zipWrapper = new ZipWrapper(_testZipPath);

        // Act & Assert
        Assert.That(folderWrapper.Exists("empty.txt"), Is.True);
        Assert.That(zipWrapper.Exists("empty.txt"), Is.True);

        // Verify content
        using var folderStream = folderWrapper.Stream("empty.txt");
        using var zipStream = zipWrapper.Stream("empty.txt");

        Assert.That(folderStream, Is.Not.Null);
        Assert.That(zipStream, Is.Not.Null);

        using var folderReader = new StreamReader(folderStream);
        using var zipReader = new StreamReader(zipStream);

        var folderContent = folderReader.ReadToEnd();
        var zipContent = zipReader.ReadToEnd();

        Assert.That(folderContent, Is.EqualTo(""));
        Assert.That(zipContent, Is.EqualTo(""));
    }

    #endregion

    #region Nested Directory Tests

    /// <summary>
    /// Tests that implementations handle nested directory structures correctly.
    /// This verifies that files in subdirectories are properly accessible.
    /// </summary>
    [Test]
    public void IFolderWrapper_WithNestedDirectories_ShouldHandleCorrectly()
    {
        // Arrange
        using var folderWrapper = new FolderWrapper(_tempDirectory);
        using var zipWrapper = new ZipWrapper(_testZipPath);

        // Act & Assert
        Assert.That(folderWrapper.Exists("subdir/nested.txt"), Is.True);
        Assert.That(zipWrapper.Exists("subdir/nested.txt"), Is.True);

        // Verify content
        using var folderStream = folderWrapper.Stream("subdir/nested.txt");
        using var zipStream = zipWrapper.Stream("subdir/nested.txt");

        Assert.That(folderStream, Is.Not.Null);
        Assert.That(zipStream, Is.Not.Null);

        using var folderReader = new StreamReader(folderStream);
        using var zipReader = new StreamReader(zipStream);

        var folderContent = folderReader.ReadToEnd();
        var zipContent = zipReader.ReadToEnd();

        Assert.That(folderContent, Is.EqualTo("Nested content"));
        Assert.That(zipContent, Is.EqualTo("Nested content"));
    }

    #endregion

    #region Interface Consistency Tests

    /// <summary>
    /// Tests that all implementations behave consistently for the same operations.
    /// This verifies that the interface contract is consistently implemented.
    /// </summary>
    [Test]
    public void IFolderWrapper_AllImplementations_ShouldBehaveConsistently()
    {
        // Arrange
        using var folderWrapper = new FolderWrapper(_tempDirectory);
        using var zipWrapper = new ZipWrapper(_testZipPath);

        // Act & Assert - Test existence checking
        Assert.That(folderWrapper.Exists("test.txt"), Is.EqualTo(zipWrapper.Exists("test.txt")));
        Assert.That(folderWrapper.Exists("nonexistent.txt"), Is.EqualTo(zipWrapper.Exists("nonexistent.txt")));
        // Note: FolderWrapper throws ArgumentNullException for null input, while ZipWrapper returns false
        // So we test them separately
        Assert.That(zipWrapper.Exists(null), Is.False);
        Assert.That(folderWrapper.Exists(""), Is.EqualTo(zipWrapper.Exists("")));

        // Test stream creation for existing files
        using var folderStream = folderWrapper.Stream("test.txt");
        using var zipStream = zipWrapper.Stream("test.txt");

        Assert.That(folderStream != null, Is.EqualTo(zipStream != null));

        if (folderStream != null && zipStream != null)
        {
            using var folderReader = new StreamReader(folderStream);
            using var zipReader = new StreamReader(zipStream);

            var folderContent = folderReader.ReadToEnd();
            var zipContent = zipReader.ReadToEnd();

            Assert.That(folderContent, Is.EqualTo(zipContent));
        }
    }

    #endregion

    #region Error Handling Tests

    /// <summary>
    /// Tests that implementations handle invalid paths gracefully.
    /// This verifies that invalid path characters are handled properly.
    /// </summary>
    [Test]
    public void IFolderWrapper_WithInvalidPaths_ShouldHandleGracefully()
    {
        // Arrange
        using var folderWrapper = new FolderWrapper(_tempDirectory);
        using var zipWrapper = new ZipWrapper(_testZipPath);

        // Act & Assert
        var invalidPaths = new[] { "..", "../", "..\\", "C:\\", "/", "\\" };

        foreach (var invalidPath in invalidPaths)
        {
            Assert.That(folderWrapper.Exists(invalidPath), Is.False);
            Assert.That(zipWrapper.Exists(invalidPath), Is.False);
            Assert.That(folderWrapper.Stream(invalidPath), Is.Null);
            Assert.That(zipWrapper.Stream(invalidPath), Is.Null);
        }
    }

    #endregion
}
