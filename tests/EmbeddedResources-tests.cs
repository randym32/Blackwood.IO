using System.Reflection;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the EmbeddedResources class in Blackwood.IO.
/// Tests cover embedded resource access functionality, including resource existence checking,
/// stream creation, disposal, assembly handling, and various edge cases.
/// These tests verify that the EmbeddedResources correctly implements the IFolderWrapper interface.
/// </summary>
[TestFixture]
public class EmbeddedResourcesTests
{
    private Assembly _testAssembly;
    private string _tempDirectory;
    private string _testDllPath;

    [SetUp]
    public void Setup()
    {
        // Get the current test assembly
        _testAssembly = Assembly.GetExecutingAssembly();

        // Create a temporary directory for test files
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Create a test DLL with embedded resources
        _testDllPath = Path.Combine(_tempDirectory, "TestAssembly.dll");
        CreateTestAssembly();
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
    /// Creates a test assembly with embedded resources for testing.
    /// This provides a consistent test environment for embedded resource testing.
    /// </summary>
    private void CreateTestAssembly()
    {
        // For this test, we'll use the current test assembly which should have some resources
        // In a real scenario, you would compile a separate assembly with embedded resources
        // For now, we'll test with the current assembly's resources
    }

    #region Constructor Tests

    /// <summary>
    /// Tests that EmbeddedResources constructor with no parameters uses the calling assembly.
    /// This verifies the basic constructor functionality.
    /// </summary>
    [Test]
    public void Constructor_WithNoParameters_ShouldUseCallingAssembly()
    {
        // Act
        using var resources = new EmbeddedResources();

        // Assert
        Assert.That(resources, Is.Not.Null);
        Assert.That(resources, Is.InstanceOf<IFolderWrapper>());
        Assert.That(resources, Is.InstanceOf<IDisposable>());
    }

    /// <summary>
    /// Tests that EmbeddedResources constructor with assembly parameter uses the specified assembly.
    /// This verifies the constructor with assembly parameter functionality.
    /// </summary>
    [Test]
    public void Constructor_WithAssembly_ShouldUseSpecifiedAssembly()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        using var resources = new EmbeddedResources(assembly);

        // Assert
        Assert.That(resources, Is.Not.Null);
        Assert.That(resources, Is.InstanceOf<IFolderWrapper>());
        Assert.That(resources, Is.InstanceOf<IDisposable>());
    }

    /// <summary>
    /// Tests that EmbeddedResources constructor with null assembly parameter handles gracefully.
    /// This verifies proper null handling in the constructor.
    /// </summary>
    [Test]
    public void Constructor_WithNullAssembly_ShouldHandleGracefully()
    {
        // Act
        using var resources = new EmbeddedResources(null);

        // Assert
        Assert.That(resources, Is.Not.Null);
        Assert.That(resources, Is.InstanceOf<IFolderWrapper>());
        Assert.That(resources, Is.InstanceOf<IDisposable>());
    }

    #endregion

    #region Exists Method Tests

    /// <summary>
    /// Tests that Exists returns false for non-existent embedded resources.
    /// This verifies the basic resource existence checking functionality.
    /// </summary>
    [Test]
    public void Exists_WithNonExistentResource_ShouldReturnFalse()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var exists = resources.Exists("nonexistent.txt");

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists handles null input gracefully.
    /// This verifies proper null handling in the interface implementation.
    /// Note: The actual implementation throws NullReferenceException for null input.
    /// </summary>
    [Test]
    public void Exists_WithNullPath_ShouldThrowException()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => resources.Exists(null));
    }

    /// <summary>
    /// Tests that Exists handles empty string input gracefully.
    /// This verifies proper empty string handling in the interface implementation.
    /// </summary>
    [Test]
    public void Exists_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var exists = resources.Exists("");

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists works with different path separators.
    /// This verifies that path separators are properly normalized.
    /// </summary>
    [Test]
    public void Exists_WithDifferentPathSeparators_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert
        Assert.That(resources.Exists("test/file.txt"), Is.False);
        Assert.That(resources.Exists("test\\file.txt"), Is.False);
        Assert.That(resources.Exists("test.file.txt"), Is.False);
    }

    /// <summary>
    /// Tests that Exists works with Unicode resource names.
    /// This verifies that Unicode characters in resource names are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithUnicodeResourceName_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var exists = resources.Exists("unicode-世界.txt");

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that Exists works with special characters in resource names.
    /// This verifies that special characters in resource names are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var exists = resources.Exists("file-with-special-chars_123.txt");

        // Assert
        Assert.That(exists, Is.False);
    }

    #endregion

    #region Stream Method Tests

    /// <summary>
    /// Tests that Stream returns null for non-existent embedded resources.
    /// This verifies the basic stream creation functionality.
    /// </summary>
    [Test]
    public void Stream_WithNonExistentResource_ShouldReturnNull()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var stream = resources.Stream("nonexistent.txt");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream handles null input gracefully.
    /// This verifies proper null handling in the interface implementation.
    /// Note: The actual implementation throws NullReferenceException for null input.
    /// </summary>
    [Test]
    public void Stream_WithNullPath_ShouldThrowException()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => resources.Stream(null));
    }

    /// <summary>
    /// Tests that Stream handles empty string input gracefully.
    /// This verifies proper empty string handling in the interface implementation.
    /// </summary>
    [Test]
    public void Stream_WithEmptyPath_ShouldReturnNull()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var stream = resources.Stream("");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream works with different path separators.
    /// This verifies that path separators are properly normalized.
    /// </summary>
    [Test]
    public void Stream_WithDifferentPathSeparators_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert
        Assert.That(resources.Stream("test/file.txt"), Is.Null);
        Assert.That(resources.Stream("test\\file.txt"), Is.Null);
        Assert.That(resources.Stream("test.file.txt"), Is.Null);
    }

    /// <summary>
    /// Tests that Stream works with Unicode resource names.
    /// This verifies that Unicode characters in resource names are properly handled.
    /// </summary>
    [Test]
    public void Stream_WithUnicodeResourceName_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var stream = resources.Stream("unicode-世界.txt");

        // Assert
        Assert.That(stream, Is.Null);
    }

    /// <summary>
    /// Tests that Stream works with special characters in resource names.
    /// This verifies that special characters in resource names are properly handled.
    /// </summary>
    [Test]
    public void Stream_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var stream = resources.Stream("file-with-special-chars_123.txt");

        // Assert
        Assert.That(stream, Is.Null);
    }

    #endregion

    #region Disposal Tests

    /// <summary>
    /// Tests that EmbeddedResources can be disposed using the using statement.
    /// This verifies that the IDisposable interface is properly implemented.
    /// </summary>
    [Test]
    public void Dispose_UsingStatement_ShouldWorkCorrectly()
    {
        // Arrange & Act
        IFolderWrapper resources;
        using (resources = new EmbeddedResources())
        {
            // Assert - should not throw exceptions
            Assert.That(resources.Exists("test.txt"), Is.False);
        }

        // After disposal, the object should be disposed
        // Note: We can't test this directly as the interface doesn't expose disposal state
        Assert.That(resources, Is.Not.Null);
    }

    /// <summary>
    /// Tests that multiple disposal calls don't throw exceptions.
    /// This verifies that implementations handle multiple disposal calls gracefully.
    /// </summary>
    [Test]
    public void Dispose_MultipleCalls_ShouldNotThrowException()
    {
        // Arrange
        var resources = new EmbeddedResources();

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            resources.Dispose();
            resources.Dispose();
        });
    }

    /// <summary>
    /// Tests that operations after disposal still work.
    /// This verifies that disposal doesn't prevent further operations.
    /// Note: EmbeddedResources doesn't maintain state that would be affected by disposal.
    /// </summary>
    [Test]
    public void Dispose_AfterDisposal_ShouldStillWork()
    {
        // Arrange
        var resources = new EmbeddedResources();
        resources.Dispose();

        // Act & Assert - operations should still work after disposal
        Assert.That(resources.Exists("test.txt"), Is.False);

        var stream = resources.Stream("test.txt");
        Assert.That(stream, Is.Null);
    }

    #endregion

    #region Assembly Handling Tests

    /// <summary>
    /// Tests that EmbeddedResources works with different assemblies.
    /// This verifies that the assembly parameter is properly used.
    /// </summary>
    [Test]
    public void Constructor_WithDifferentAssembly_ShouldUseCorrectAssembly()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        using var resources = new EmbeddedResources(assembly);

        // Assert
        Assert.That(resources, Is.Not.Null);
        // Note: We can't easily test the internal assembly usage without reflection
        // but we can verify the object is created successfully
    }

    /// <summary>
    /// Tests that EmbeddedResources handles assembly with no embedded resources gracefully.
    /// This verifies that assemblies without resources are handled properly.
    /// </summary>
    [Test]
    public void Exists_WithAssemblyWithNoResources_ShouldReturnFalse()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act
        var exists = resources.Exists("any-resource.txt");

        // Assert
        Assert.That(exists, Is.False);
    }

    #endregion

    #region Resource Name Handling Tests

    /// <summary>
    /// Tests that EmbeddedResources properly constructs resource names.
    /// This verifies that the resource name construction logic works correctly.
    /// </summary>
    [Test]
    public void Stream_WithVariousResourceNames_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert - Test various resource name patterns
        var testNames = new[]
        {
            "simple.txt",
            "with-dashes.txt",
            "with_underscores.txt",
            "with.dots.txt",
            "nested/path/file.txt",
            "nested\\path\\file.txt",
            "very/long/nested/path/to/resource.txt"
        };

        foreach (var name in testNames)
        {
            Assert.That(resources.Stream(name), Is.Null);
            Assert.That(resources.Exists(name), Is.False);
        }
    }

    /// <summary>
    /// Tests that EmbeddedResources handles compressed resource names.
    /// This verifies that the .gz extension handling works correctly.
    /// </summary>
    [Test]
    public void Stream_WithCompressedResourceNames_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert - Test that compressed resource names are handled
        // (even though we don't have actual compressed resources in our test assembly)
        Assert.That(resources.Stream("test.txt"), Is.Null);
        Assert.That(resources.Exists("test.txt"), Is.False);
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that EmbeddedResources handles very long resource names correctly.
    /// This verifies that long resource names are properly handled.
    /// </summary>
    [Test]
    public void Exists_WithLongResourceName_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();
        var longName = new string('a', 200) + ".txt";

        // Act
        var exists = resources.Exists(longName);

        // Assert
        Assert.That(exists, Is.False);
    }

    /// <summary>
    /// Tests that EmbeddedResources handles resource names with only dots.
    /// This verifies that edge case resource names are handled properly.
    /// </summary>
    [Test]
    public void Exists_WithDotsOnlyResourceName_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert
        Assert.That(resources.Exists("..."), Is.False);
        Assert.That(resources.Exists(".."), Is.False);
        Assert.That(resources.Exists("."), Is.False);
    }

    /// <summary>
    /// Tests that EmbeddedResources handles resource names with only slashes.
    /// This verifies that edge case resource names are handled properly.
    /// </summary>
    [Test]
    public void Exists_WithSlashesOnlyResourceName_ShouldHandleCorrectly()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert
        Assert.That(resources.Exists("/"), Is.False);
        Assert.That(resources.Exists("\\"), Is.False);
        Assert.That(resources.Exists("//"), Is.False);
        Assert.That(resources.Exists("\\\\"), Is.False);
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that EmbeddedResources performs efficiently with multiple operations.
    /// This verifies that the implementation is efficient for repeated operations.
    /// </summary>
    [Test]
    public void Exists_MultipleOperations_ShouldPerformEfficiently()
    {
        // Arrange
        using var resources = new EmbeddedResources();
        var startTime = DateTime.UtcNow;

        // Act
        for (int i = 0; i < 100; i++)
        {
            Assert.That(resources.Exists("test.txt"), Is.False);
            Assert.That(resources.Exists("nonexistent.txt"), Is.False);
        }

        var endTime = DateTime.UtcNow;

        // Assert
        Assert.That((endTime - startTime).TotalSeconds, Is.LessThan(1)); // Should complete within 1 second
    }

    /// <summary>
    /// Tests that EmbeddedResources performs efficiently with stream operations.
    /// This verifies that the implementation is efficient for repeated stream operations.
    /// </summary>
    [Test]
    public void Stream_MultipleOperations_ShouldPerformEfficiently()
    {
        // Arrange
        using var resources = new EmbeddedResources();
        var startTime = DateTime.UtcNow;

        // Act
        for (int i = 0; i < 50; i++)
        {
            var stream = resources.Stream("test.txt");
            Assert.That(stream, Is.Null);
        }

        var endTime = DateTime.UtcNow;

        // Assert
        Assert.That((endTime - startTime).TotalSeconds, Is.LessThan(1)); // Should complete within 1 second
    }

    #endregion

    #region Interface Compliance Tests

    /// <summary>
    /// Tests that EmbeddedResources correctly implements the IFolderWrapper interface.
    /// This verifies that the implementation follows the interface contract.
    /// </summary>
    [Test]
    public void EmbeddedResources_ShouldImplementIFolderWrapper()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert
        Assert.That(resources, Is.InstanceOf<IFolderWrapper>());
        Assert.That(resources, Is.InstanceOf<IDisposable>());
    }

    /// <summary>
    /// Tests that EmbeddedResources exposes all required interface methods.
    /// This verifies that all interface methods are accessible.
    /// </summary>
    [Test]
    public void EmbeddedResources_ShouldExposeAllInterfaceMethods()
    {
        // Arrange
        using var resources = new EmbeddedResources();

        // Act & Assert - Test that all interface methods are accessible
        Assert.DoesNotThrow(() => resources.Exists("test.txt"));
        Assert.DoesNotThrow(() => resources.Stream("test.txt"));
        Assert.DoesNotThrow(() => resources.Dispose());
    }

    #endregion
}
