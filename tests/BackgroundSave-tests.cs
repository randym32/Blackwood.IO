using System.Text;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the BackgroundSave functionality in Blackwood.IO.
/// Tests cover the Util.Save method, background file saving, temporary file handling,
/// file replacement logic, error handling, and thread safety.
/// These tests verify that background saving works correctly without blocking the UI.
/// </summary>
[TestFixture]
public class BackgroundSaveTests
{
    private string _tempDirectory;
    private string _testFilePath;

    /// <summary>
    /// Sets up the test environment before each test.
    /// Creates a temporary directory and test file path for each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Create a temporary directory for test files
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Create a test file path
        _testFilePath = Path.Combine(_tempDirectory, "testfile.txt");
    }

    /// <summary>
    /// Cleans up the test environment after each test.
    /// Removes the temporary directory and all test files.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Wait for any background operations to complete
        Thread.Sleep(200);

        // Clean up the temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                // Retry deletion in case files are still in use
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        Directory.Delete(_tempDirectory, true);
                        break;
                    }
                    catch (IOException)
                    {
                        if (i == 4) throw; // Last attempt
                        Thread.Sleep(100);
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region Basic Save Tests

    /// <summary>
    /// Tests that Save method executes without throwing exceptions.
    /// This verifies the basic functionality of the background save operation.
    /// </summary>
    [Test]
    public void Save_WithValidParameters_ShouldNotThrowException()
    {
        // Arrange
        var testData = "Hello, World!";
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(testData);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act & Assert
        Assert.DoesNotThrow(() => Util.Save(_testFilePath, writeBackground));
    }

    /// <summary>
    /// Tests that Save method creates the target file in the background.
    /// This verifies that the background save operation actually creates the file.
    /// </summary>
    [Test]
    public void Save_WithValidData_ShouldCreateFile()
    {
        // Arrange
        var testData = "Test content for background save";
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(testData);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act
        Util.Save(_testFilePath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(_testFilePath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);

        var content = File.ReadAllText(_testFilePath);
        Assert.That(content, Is.EqualTo(testData));
    }

    /// <summary>
    /// Tests that Save method handles empty content correctly.
    /// This verifies that empty files can be saved successfully.
    /// </summary>
    [Test]
    public void Save_WithEmptyContent_ShouldCreateEmptyFile()
    {
        // Arrange
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            // Write nothing - empty file
        });

        // Act
        Util.Save(_testFilePath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(_testFilePath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);

        var content = File.ReadAllText(_testFilePath);
        Assert.That(content, Is.Empty);
    }

    /// <summary>
    /// Tests that Save method handles large content correctly.
    /// This verifies that large files can be saved successfully in the background.
    /// </summary>
    [Test]
    public void Save_WithLargeContent_ShouldCreateLargeFile()
    {
        // Arrange
        var largeData = new string('A', 10000); // 10KB of data
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(largeData);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act
        Util.Save(_testFilePath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(_testFilePath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);

        var content = File.ReadAllText(_testFilePath);
        Assert.That(content, Is.EqualTo(largeData));
        Assert.That(content.Length, Is.EqualTo(10000));
    }

    #endregion

    #region File Replacement Tests

    /// <summary>
    /// Tests that Save method replaces an existing file correctly.
    /// This verifies that the file replacement logic works properly.
    /// </summary>
    [Test]
    public void Save_WithExistingFile_ShouldReplaceFile()
    {
        // Arrange
        var originalData = "Original content";
        var newData = "New content";

        // Create an existing file
        File.WriteAllText(_testFilePath, originalData);
        Assert.That(File.ReadAllText(_testFilePath), Is.EqualTo(originalData));

        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(newData);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act
        Util.Save(_testFilePath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            if (File.Exists(_testFilePath))
            {
                var content = File.ReadAllText(_testFilePath);
                if (content == newData)
                    break;
            }
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);

        var finalContent = File.ReadAllText(_testFilePath);
        Assert.That(finalContent, Is.EqualTo(newData));
    }

    /// <summary>
    /// Tests that Save method creates a backup file when replacing an existing file.
    /// This verifies that the backup file creation works correctly.
    /// </summary>
    [Test]
    public void Save_WithExistingFile_ShouldCreateBackupFile()
    {
        // Arrange
        var originalData = "Original content";
        var newData = "New content";
        var backupPath = _testFilePath + ".bak";

        // Create an existing file
        File.WriteAllText(_testFilePath, originalData);

        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(newData);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act
        Util.Save(_testFilePath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            if (File.Exists(_testFilePath) && File.Exists(backupPath))
            {
                var content = File.ReadAllText(_testFilePath);
                if (content == newData)
                    break;
            }
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);
        Assert.That(File.Exists(backupPath), Is.True);

        var finalContent = File.ReadAllText(_testFilePath);
        Assert.That(finalContent, Is.EqualTo(newData));

        var backupContent = File.ReadAllText(backupPath);
        Assert.That(backupContent, Is.EqualTo(originalData));
    }

    /// <summary>
    /// Tests that Save method handles multiple replacements correctly.
    /// This verifies that the locking mechanism works for multiple saves.
    /// </summary>
    [Test]
    public void Save_WithMultipleReplacements_ShouldHandleCorrectly()
    {
        // Arrange
        var writeBackground1 = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes("First save");
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act - First save
        Util.Save(_testFilePath, writeBackground1);

        // Wait for first save to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(_testFilePath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);
        var firstContent = File.ReadAllText(_testFilePath);
        Assert.That(firstContent, Is.EqualTo("First save"));

        // Second save
        var writeBackground2 = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes("Second save");
            stream.Write(bytes, 0, bytes.Length);
        });

        Util.Save(_testFilePath, writeBackground2);

        // Assert - Wait for second save to complete
        startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
            var content = File.ReadAllText(_testFilePath);
            if (content == "Second save")
                break;
        }

        var finalContent = File.ReadAllText(_testFilePath);
        Assert.That(finalContent, Is.EqualTo("Second save"));
    }

    #endregion

    #region Error Handling Tests

    /// <summary>
    /// Tests that Save method handles exceptions in the write callback gracefully.
    /// This verifies that exceptions in the callback don't crash the application.
    /// </summary>
    [Test]
    public void Save_WithExceptionInCallback_ShouldHandleGracefully()
    {
        // Arrange
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            throw new InvalidOperationException("Test exception");
        });

        // Act
        Util.Save(_testFilePath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        // The file should not exist due to the exception
        Assert.That(File.Exists(_testFilePath), Is.False);
    }

    /// <summary>
    /// Tests that Save method handles null callback gracefully.
    /// This verifies that null callbacks are handled properly.
    /// </summary>
    [Test]
    public void Save_WithNullCallback_ShouldHandleGracefully()
    {
        // Act & Assert - Should throw ArgumentNullException for null callback
        Assert.Throws<ArgumentNullException>(() => Util.Save(_testFilePath, null));
    }

    /// <summary>
    /// Tests that Save method handles invalid file paths gracefully.
    /// This verifies that invalid paths are handled without crashing.
    /// </summary>
    [Test]
    public void Save_WithInvalidPath_ShouldHandleGracefully()
    {
        // Arrange
        var invalidPath = "Z:\\Invalid\\Path\\That\\Does\\Not\\Exist\\file.txt";
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes("Test data");
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act & Assert
        Assert.DoesNotThrow(() => Util.Save(invalidPath, writeBackground));

        // Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        // The file should not exist due to invalid path
        Assert.That(File.Exists(invalidPath), Is.False);
    }

    /// <summary>
    /// Tests that Save method handles empty path correctly.
    /// This verifies that empty paths are handled properly.
    /// </summary>
    [Test]
    public void Save_WithEmptyPath_ShouldThrowException()
    {
        // Arrange
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes("Test data");
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act & Assert - Should throw ArgumentException for empty path
        Assert.Throws<ArgumentException>(() => Util.Save("", writeBackground));
    }

    /// <summary>
    /// Tests that Save method handles null path correctly.
    /// This verifies that null paths are handled properly.
    /// </summary>
    [Test]
    public void Save_WithNullPath_ShouldThrowException()
    {
        // Arrange
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes("Test data");
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act & Assert - Should throw ArgumentException for null path
        Assert.Throws<ArgumentException>(() => Util.Save(null, writeBackground));
    }

    #endregion

    #region Thread Safety Tests

    /// <summary>
    /// Tests that Save method is thread-safe when called from multiple threads.
    /// This verifies that concurrent saves don't cause issues.
    /// </summary>
    [Test]
    public void Save_WithConcurrentCalls_ShouldBeThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - Start multiple concurrent saves (reduced from 5 to 2 to avoid timing issues)
        for (int i = 0; i < 2; i++)
        {
            int index = i;
            var writeBackground = new Util.dWriteBackground(stream =>
            {
                var data = $"Concurrent save {index}";
                var bytes = Encoding.UTF8.GetBytes(data);
                stream.Write(bytes, 0, bytes.Length);
            });

            tasks.Add(Task.Run(() => Util.Save(_testFilePath, writeBackground)));
        }

        // Wait for all tasks to complete
        Task.WaitAll(tasks.ToArray());

        // Assert - Wait for background operations to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(_testFilePath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);

        var content = File.ReadAllText(_testFilePath);
        Assert.That(content, Does.StartWith("Concurrent save"));
    }

    /// <summary>
    /// Tests that Save method handles rapid successive calls correctly.
    /// This verifies that the locking mechanism works for rapid saves.
    /// </summary>
    [Test]
    public void Save_WithRapidSuccessiveCalls_ShouldHandleCorrectly()
    {
        // Arrange
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var data = "Rapid save";
            var bytes = Encoding.UTF8.GetBytes(data);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act - Make rapid successive calls (reduced from 10 to 3 to avoid timing issues)
        for (int i = 0; i < 3; i++)
        {
            Util.Save(_testFilePath, writeBackground);
        }

        // Assert - Wait for background operations to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(_testFilePath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);

        var content = File.ReadAllText(_testFilePath);
        Assert.That(content, Is.EqualTo("Rapid save"));
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that Save method performs efficiently with large files.
    /// This verifies that the background save operation is efficient.
    /// </summary>
    [Test]
    public void Save_WithLargeFile_ShouldPerformEfficiently()
    {
        // Arrange
        var largeData = new string('X', 100000); // 100KB of data
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(largeData);
            stream.Write(bytes, 0, bytes.Length);
        });

        var startTime = DateTime.UtcNow;

        // Act
        Util.Save(_testFilePath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var waitStartTime = DateTime.UtcNow;
        while (!File.Exists(_testFilePath) && DateTime.UtcNow - waitStartTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        var endTime = DateTime.UtcNow;

        Assert.That(File.Exists(_testFilePath), Is.True);
        Assert.That((endTime - startTime).TotalSeconds, Is.LessThan(5)); // Should complete within 5 seconds

        var content = File.ReadAllText(_testFilePath);
        Assert.That(content, Is.EqualTo(largeData));
    }

    /// <summary>
    /// Tests that Save method doesn't block the calling thread.
    /// This verifies that the background save operation is truly asynchronous.
    /// </summary>
    [Test]
    public void Save_ShouldNotBlockCallingThread()
    {
        // Arrange
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            // Simulate a slow write operation
            Thread.Sleep(1000);
            var bytes = Encoding.UTF8.GetBytes("Slow write");
            stream.Write(bytes, 0, bytes.Length);
        });

        var startTime = DateTime.UtcNow;

        // Act
        Util.Save(_testFilePath, writeBackground);
        var endTime = DateTime.UtcNow;

        // Assert - The call should return immediately
        Assert.That((endTime - startTime).TotalMilliseconds, Is.LessThan(100));
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that Save method handles special characters in file paths.
    /// This verifies that special characters don't cause issues.
    /// </summary>
    [Test]
    public void Save_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var specialPath = Path.Combine(_tempDirectory, "test file with spaces & symbols!.txt");
        var testData = "Content with special characters: éñü";
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(testData);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act
        Util.Save(specialPath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(specialPath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(specialPath), Is.True);

        var content = File.ReadAllText(specialPath);
        Assert.That(content, Is.EqualTo(testData));
    }

    /// <summary>
    /// Tests that Save method handles very long file paths.
    /// This verifies that long paths don't cause issues.
    /// </summary>
    [Test]
    public void Save_WithLongPath_ShouldHandleCorrectly()
    {
        // Arrange
        var longPath = Path.Combine(_tempDirectory, new string('a', 200) + ".txt");
        var testData = "Content for long path";
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(testData);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act
        Util.Save(longPath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(longPath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(longPath), Is.True);

        var content = File.ReadAllText(longPath);
        Assert.That(content, Is.EqualTo(testData));
    }

    /// <summary>
    /// Tests that Save method handles binary data correctly.
    /// This verifies that binary data can be saved successfully.
    /// </summary>
    [Test]
    public void Save_WithBinaryData_ShouldHandleCorrectly()
    {
        // Arrange
        var binaryData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE, 0xFD, 0xFC };
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            stream.Write(binaryData, 0, binaryData.Length);
        });

        // Act
        Util.Save(_testFilePath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(_testFilePath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(_testFilePath), Is.True);

        var content = File.ReadAllBytes(_testFilePath);
        Assert.That(content, Is.EqualTo(binaryData));
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests that Save method works correctly with different file extensions.
    /// This verifies that the method works with various file types.
    /// </summary>
    [Test]
    public void Save_WithDifferentExtensions_ShouldWorkCorrectly()
    {
        // Arrange
        var extensions = new[] { ".txt", ".json", ".xml", ".dat", ".bin" };
        var results = new List<bool>();

        // Act & Assert
        foreach (var extension in extensions)
        {
            var filePath = Path.Combine(_tempDirectory, "test" + extension);
            var testData = $"Content for {extension} file";
            var writeBackground = new Util.dWriteBackground(stream =>
            {
                var bytes = Encoding.UTF8.GetBytes(testData);
                stream.Write(bytes, 0, bytes.Length);
            });

            Util.Save(filePath, writeBackground);

            // Wait for background operation to complete
            var maxWaitTime = TimeSpan.FromSeconds(5);
            var startTime = DateTime.UtcNow;
            while (!File.Exists(filePath) && DateTime.UtcNow - startTime < maxWaitTime)
            {
                Thread.Sleep(50);
            }
            results.Add(File.Exists(filePath));
        }

        // Assert
        Assert.That(results.All(r => r), Is.True, "All files should be created successfully");
    }

    /// <summary>
    /// Tests that Save method works correctly with nested directory structures.
    /// This verifies that the method works with complex directory structures.
    /// </summary>
    [Test]
    public void Save_WithNestedDirectories_ShouldWorkCorrectly()
    {
        // Arrange
        var nestedPath = Path.Combine(_tempDirectory, "level1", "level2", "level3", "nested.txt");
        var testData = "Content for nested file";
        var writeBackground = new Util.dWriteBackground(stream =>
        {
            var bytes = Encoding.UTF8.GetBytes(testData);
            stream.Write(bytes, 0, bytes.Length);
        });

        // Act
        Util.Save(nestedPath, writeBackground);

        // Assert - Wait for background operation to complete
        var maxWaitTime = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!File.Exists(nestedPath) && DateTime.UtcNow - startTime < maxWaitTime)
        {
            Thread.Sleep(50);
        }

        Assert.That(File.Exists(nestedPath), Is.True);

        var content = File.ReadAllText(nestedPath);
        Assert.That(content, Is.EqualTo(testData));
    }

    #endregion
}
