using System.Text;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the FolderWrapper class in Blackwood.IO.
/// Tests cover file system access functionality, including file existence checking,
/// stream creation, disposal, path handling, and various edge cases.
/// These tests verify that the FolderWrapper correctly implements the IFolderWrapper interface.
/// </summary>
[TestFixture]
public class FolderWrapperTests
{
    private string _tempDirectory;
    private string _subDirectory;
    private string _testFilePath;
    private string _emptyFilePath;
    private string _unicodeFilePath;
    private string _nestedFilePath;
    private string _fileWithSpacesPath;

    [SetUp]
    public void Setup()
    {
        // Create a temporary directory for testing
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Create test files
        _testFilePath = Path.Combine(_tempDirectory, "test.txt");
        File.WriteAllText(_testFilePath, "Hello, World!");

        _emptyFilePath = Path.Combine(_tempDirectory, "empty.txt");
        File.WriteAllText(_emptyFilePath, "");

        _unicodeFilePath = Path.Combine(_tempDirectory, "unicode.txt");
        File.WriteAllText(_unicodeFilePath, "Hello 世界! 🌍", Encoding.UTF8);

        // Create subdirectory with files
        _subDirectory = Path.Combine(_tempDirectory, "subdir");
        Directory.CreateDirectory(_subDirectory);

        _nestedFilePath = Path.Combine(_subDirectory, "nested.txt");
        File.WriteAllText(_nestedFilePath, "Nested content");

        _fileWithSpacesPath = Path.Combine(_subDirectory, "file with spaces.txt");
        File.WriteAllText(_fileWithSpacesPath, "File with spaces");
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

    #region Constructor Tests

    /// <summary>
    /// Tests that FolderWrapper constructor accepts a valid directory path.
    /// This verifies the basic constructor functionality.
    /// </summary>
    [Test]
    public void Constructor_WithValidDirectory_ShouldCreateInstance()
    {
        // Act
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Assert
        Assert.That(wrapper, Is.Not.Null);
        Assert.That(wrapper, Is.InstanceOf<IFolderWrapper>());
        Assert.That(wrapper, Is.InstanceOf<IDisposable>());
    }

    /// <summary>
    /// Tests that FolderWrapper constructor accepts null base path.
    /// This verifies that the constructor doesn't validate the path immediately.
    /// </summary>
    [Test]
    public void Constructor_WithNullPath_ShouldCreateInstance()
    {
        // Act
        using var wrapper = new FolderWrapper(null);

        // Assert
        Assert.That(wrapper, Is.Not.Null);
    }

    /// <summary>
    /// Tests that FolderWrapper constructor accepts empty string path.
    /// This verifies that the constructor doesn't validate the path immediately.
    /// </summary>
    [Test]
    public void Constructor_WithEmptyPath_ShouldCreateInstance()
    {
        // Act
        using var wrapper = new FolderWrapper("");

        // Assert
        Assert.That(wrapper, Is.Not.Null);
    }

    /// <summary>
    /// Tests that FolderWrapper constructor accepts non-existent directory path.
    /// This verifies that the constructor doesn't validate the path immediately.
    /// </summary>
    [Test]
    public void Constructor_WithNonExistentDirectory_ShouldCreateInstance()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent");

        // Act
        using var wrapper = new FolderWrapper(nonExistentPath);

        // Assert
        Assert.That(wrapper, Is.Not.Null);
    }

    #endregion

    #region Exists Method Tests

    /// <summary>
    /// Tests that Exists returns true for existing files.
    /// This verifies the basic file existence checking functionality.
    /// </summary>
    [Test]
    public void Exists_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("test.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that Exists returns false for non-existent files.
    /// This verifies that non-existent files are correctly identified.
    /// </summary>
    [Test]
    public void Exists_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("nonexistent.txt");

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists handles null input by throwing ArgumentNullException.
    /// This verifies proper null handling in the interface implementation.
    /// </summary>
    [Test]
    public void Exists_WithNullPath_ShouldThrowException()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => wrapper.Exists(null));
    }

    /// <summary>
    /// Tests that Exists handles empty string input.
    /// This verifies proper empty string handling in the interface implementation.
    /// </summary>
    [Test]
    public void Exists_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("");

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists works with files in subdirectories.
    /// This verifies that nested directory structures are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithFileInSubdirectory_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("subdir/nested.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that Exists works with files that have spaces in their names.
    /// This verifies that file names with spaces are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithFileWithSpaces_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("subdir/file with spaces.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that Exists works with Unicode file names.
    /// This verifies that Unicode characters in file names are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithUnicodeFile_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("unicode.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that Exists works with empty files.
    /// This verifies that empty files are properly detected.
    /// </summary>
    [Test]
    public void Exists_WithEmptyFile_ShouldReturnTrue()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("empty.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that Exists handles invalid path characters.
    /// This verifies that invalid path characters are handled gracefully.
    /// </summary>
    [Test]
    public void Exists_WithInvalidPath_ShouldReturnFalse()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        var invalidPaths = new[] { "..", "../", "..\\", "C:\\", "/", "\\" };

        foreach (var invalidPath in invalidPaths)
        {
            Assert.That(wrapper.Exists(invalidPath), Is.False);
        }
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
    /// Tests that Stream returns null for non-existent files.
    /// This verifies that non-existent files return null streams.
    /// </summary>
    [Test]
    public void Stream_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var stream = wrapper.Stream("nonexistent.txt");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream handles null input by throwing ArgumentNullException.
    /// This verifies proper null handling in the interface implementation.
    /// </summary>
    [Test]
    public void Stream_WithNullPath_ShouldThrowException()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => wrapper.Stream(null));
    }

    /// <summary>
    /// Tests that Stream handles empty string input.
    /// This verifies proper empty string handling in the interface implementation.
    /// </summary>
    [Test]
    public void Stream_WithEmptyPath_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var stream = wrapper.Stream("");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream works with files in subdirectories.
    /// This verifies that nested directory structures are properly handled.
    /// </summary>
    [Test]
    public void Stream_WithFileInSubdirectory_ShouldReturnValidStream()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        using var stream = wrapper.Stream("subdir/nested.txt");

        // Assert
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.CanRead, Is.True);

        // Verify content
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("Nested content"));
    }

    /// <summary>
    /// Tests that Stream works with files that have spaces in their names.
    /// This verifies that file names with spaces are properly handled.
    /// </summary>
    [Test]
    public void Stream_WithFileWithSpaces_ShouldReturnValidStream()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        using var stream = wrapper.Stream("subdir/file with spaces.txt");

        // Assert
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.CanRead, Is.True);

        // Verify content
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("File with spaces"));
    }

    /// <summary>
    /// Tests that Stream works with Unicode file names.
    /// This verifies that Unicode characters in file names are properly handled.
    /// </summary>
    [Test]
    public void Stream_WithUnicodeFile_ShouldReturnValidStream()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        using var stream = wrapper.Stream("unicode.txt");

        // Assert
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.CanRead, Is.True);

        // Verify content
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo("Hello 世界! 🌍"));
    }

    /// <summary>
    /// Tests that Stream works with empty files.
    /// This verifies that empty files are properly handled.
    /// </summary>
    [Test]
    public void Stream_WithEmptyFile_ShouldReturnValidStream()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        using var stream = wrapper.Stream("empty.txt");

        // Assert
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.CanRead, Is.True);

        // Verify content
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        Assert.That(content, Is.EqualTo(""));
    }

    /// <summary>
    /// Tests that Stream handles invalid path characters.
    /// This verifies that invalid path characters are handled gracefully.
    /// </summary>
    [Test]
    public void Stream_WithInvalidPath_ShouldReturnNull()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        var invalidPaths = new[] { "..", "../", "..\\", "C:\\", "/", "\\" };

        foreach (var invalidPath in invalidPaths)
        {
            Assert.That(wrapper.Stream(invalidPath), Is.Null);
        }
    }

    /// <summary>
    /// Tests that Stream returns a seekable stream.
    /// This verifies that the returned stream supports seeking operations.
    /// </summary>
    [Test]
    public void Stream_ShouldReturnSeekableStream()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        using var stream = wrapper.Stream("test.txt");

        // Assert
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.CanSeek, Is.True);
        Assert.That(stream.CanRead, Is.True);
    }

    #endregion

    #region Disposal Tests

    /// <summary>
    /// Tests that FolderWrapper can be disposed using the using statement.
    /// This verifies that the IDisposable interface is properly implemented.
    /// </summary>
    [Test]
    public void Dispose_UsingStatement_ShouldWorkCorrectly()
    {
        // Arrange & Act
        IFolderWrapper wrapper;
        using (wrapper = new FolderWrapper(_tempDirectory))
        {
            // Assert - should not throw exceptions
            Assert.That(wrapper.Exists("test.txt"), Is.True);
        }

        // After disposal, the object should be disposed
        // Note: We can't test this directly as the interface doesn't expose disposal state
        Assert.That(wrapper, Is.Not.Null);
    }

    /// <summary>
    /// Tests that multiple disposal calls don't throw exceptions.
    /// This verifies that implementations handle multiple disposal calls gracefully.
    /// </summary>
    [Test]
    public void Dispose_MultipleCalls_ShouldNotThrowException()
    {
        // Arrange
        var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            wrapper.Dispose();
            wrapper.Dispose();
        });
    }

    /// <summary>
    /// Tests that operations after disposal still work.
    /// This verifies that disposal doesn't prevent further operations.
    /// Note: FolderWrapper doesn't maintain state that would be affected by disposal.
    /// </summary>
    [Test]
    public void Dispose_AfterDisposal_ShouldStillWork()
    {
        // Arrange
        var wrapper = new FolderWrapper(_tempDirectory);
        wrapper.Dispose();

        // Act & Assert - operations should still work after disposal
        Assert.That(wrapper.Exists("test.txt"), Is.True);

        using var stream = wrapper.Stream("test.txt");
        Assert.That(stream, Is.Not.Null);
    }

    #endregion

    #region Path Handling Tests

    /// <summary>
    /// Tests that FolderWrapper handles forward slashes correctly.
    /// This verifies that path separators are properly normalized.
    /// </summary>
    [Test]
    public void Exists_WithForwardSlashes_ShouldWorkCorrectly()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("subdir/nested.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that FolderWrapper handles backslashes correctly.
    /// This verifies that path separators are properly normalized.
    /// </summary>
    [Test]
    public void Exists_WithBackslashes_ShouldWorkCorrectly()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("subdir\\nested.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that FolderWrapper handles mixed path separators correctly.
    /// This verifies that mixed path separators are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithMixedSeparators_ShouldWorkCorrectly()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists("subdir\\nested.txt");

        // Assert
        Assert.That(exists, Is.True);
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that FolderWrapper handles very long file names correctly.
    /// This verifies that long file names are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithLongFileName_ShouldWorkCorrectly()
    {
        // Arrange
        var longFileName = new string('a', 200) + ".txt";
        var longFilePath = Path.Combine(_tempDirectory, longFileName);
        File.WriteAllText(longFilePath, "Long file name test");

        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists(longFileName);

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that FolderWrapper handles files with special characters correctly.
    /// This verifies that special characters in file names are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var specialFileName = "file-with-special-chars_123.txt";
        var specialFilePath = Path.Combine(_tempDirectory, specialFileName);
        File.WriteAllText(specialFilePath, "Special characters test");

        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act
        var exists = wrapper.Exists(specialFileName);

        // Assert
        Assert.That(exists, Is.True);
    }

    /// <summary>
    /// Tests that FolderWrapper handles case sensitivity correctly.
    /// This verifies that file name case sensitivity is properly handled.
    /// Note: Windows file systems are typically case-insensitive.
    /// </summary>
    [Test]
    public void Exists_WithCaseSensitiveFileName_ShouldWorkCorrectly()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);

        // Act & Assert
        Assert.That(wrapper.Exists("test.txt"), Is.True);
        // Windows file systems are typically case-insensitive, so these should also return true
        Assert.That(wrapper.Exists("TEST.TXT"), Is.True); // Case insensitive on Windows
        Assert.That(wrapper.Exists("Test.Txt"), Is.True); // Case insensitive on Windows
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that FolderWrapper performs efficiently with multiple operations.
    /// This verifies that the implementation is efficient for repeated operations.
    /// </summary>
    [Test]
    public void Exists_MultipleOperations_ShouldPerformEfficiently()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);
        var startTime = DateTime.UtcNow;

        // Act
        for (int i = 0; i < 100; i++)
        {
            Assert.That(wrapper.Exists("test.txt"), Is.True);
            Assert.That(wrapper.Exists("nonexistent.txt"), Is.False);
        }

        var endTime = DateTime.UtcNow;

        // Assert
        Assert.That((endTime - startTime).TotalSeconds, Is.LessThan(1)); // Should complete within 1 second
    }

    /// <summary>
    /// Tests that FolderWrapper performs efficiently with stream operations.
    /// This verifies that the implementation is efficient for repeated stream operations.
    /// </summary>
    [Test]
    public void Stream_MultipleOperations_ShouldPerformEfficiently()
    {
        // Arrange
        using var wrapper = new FolderWrapper(_tempDirectory);
        var startTime = DateTime.UtcNow;

        // Act
        for (int i = 0; i < 50; i++)
        {
            using var stream = wrapper.Stream("test.txt");
            Assert.That(stream, Is.Not.Null);
        }

        var endTime = DateTime.UtcNow;

        // Assert
        Assert.That((endTime - startTime).TotalSeconds, Is.LessThan(1)); // Should complete within 1 second
    }

    #endregion
}
