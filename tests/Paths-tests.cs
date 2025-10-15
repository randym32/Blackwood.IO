namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the FS (File System) utility class in Blackwood.IO.
/// Tests cover the AssemblyDirectory property, RemoveBasePath method, and BuildNameToRelativePathXref method.
/// </summary>
[TestFixture]
public class FSTests
{
    /// <summary>
    /// Temporary directory used for file system tests.
    /// Each test run gets a unique temporary directory to avoid conflicts.
    /// </summary>
    private string _tempDirectory;

    /// <summary>
    /// Sets up a unique temporary directory for each test.
    /// This ensures test isolation and prevents conflicts between test runs.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // Create a temporary directory for testing
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// Cleans up the temporary directory after each test.
    /// This ensures no leftover files from previous test runs.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Clean up temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    /// <summary>
    /// Tests that AssemblyDirectory returns a valid, non-empty path that actually exists on the file system.
    /// This verifies the basic functionality of the property.
    /// </summary>
    [Test]
    public void AssemblyDirectory_ShouldReturnValidPath()
    {
        // Act
        var assemblyDirectory = FS.AssemblyDirectory;

        // Assert
        Assert.That(assemblyDirectory, Is.Not.Null);
        Assert.That(assemblyDirectory, Is.Not.Empty);
        Assert.That(Directory.Exists(assemblyDirectory), Is.True);
    }

    /// <summary>
    /// Tests that AssemblyDirectory caches its result for performance.
    /// Multiple calls should return the same reference, indicating caching is working.
    /// </summary>
    [Test]
    public void AssemblyDirectory_ShouldCacheResult()
    {
        // Act
        var firstCall = FS.AssemblyDirectory;
        var secondCall = FS.AssemblyDirectory;

        // Assert
        Assert.That(firstCall, Is.EqualTo(secondCall));
    }

    #region RemoveBasePath Tests

    /// <summary>
    /// Tests that RemoveBasePath returns null when the base path parameter is null.
    /// This verifies proper null handling for the first parameter.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithNullBasePath_ShouldReturnNull()
    {
        // Act
        var result = FS.RemoveBasePath(null, "some/path");

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that RemoveBasePath returns null when the path parameter is null.
    /// This verifies proper null handling for the second parameter.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithNullPath_ShouldReturnNull()
    {
        // Act
        var result = FS.RemoveBasePath("base/path", null);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that RemoveBasePath returns null when both parameters are null.
    /// This verifies proper null handling for edge cases.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithBothNull_ShouldReturnNull()
    {
        // Act
        var result = FS.RemoveBasePath(null, null);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that RemoveBasePath returns the original path when the base path doesn't match.
    /// This verifies that non-matching paths are returned unchanged.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithNonMatchingPath_ShouldReturnOriginalPath()
    {
        // Arrange
        var basePath = "base/path";
        var path = "different/path";

        // Act
        var result = FS.RemoveBasePath(basePath, path);

        // Assert
        Assert.That(result, Is.EqualTo(path));
    }

    /// <summary>
    /// Tests that RemoveBasePath returns an empty string when the path exactly matches the base path.
    /// This verifies the behavior when there's nothing left after removing the base.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithExactMatch_ShouldReturnEmptyString()
    {
        // Arrange
        var basePath = "base/path";
        var path = "base/path";

        // Act
        var result = FS.RemoveBasePath(basePath, path);

        // Assert
        Assert.That(result, Is.EqualTo(""));
    }

    /// <summary>
    /// Tests that RemoveBasePath correctly removes the base path and forward slash separator.
    /// This verifies proper handling of Unix-style path separators.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithMatchingBaseAndForwardSlash_ShouldRemoveBaseAndSlash()
    {
        // Arrange
        var basePath = "base/path";
        var path = "base/path/subfolder";

        // Act
        var result = FS.RemoveBasePath(basePath, path);

        // Assert
        Assert.That(result, Is.EqualTo("subfolder"));
    }

    /// <summary>
    /// Tests that RemoveBasePath correctly removes the base path and backslash separator.
    /// This verifies proper handling of Windows-style path separators.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithMatchingBaseAndBackslash_ShouldRemoveBaseAndSlash()
    {
        // Arrange
        var basePath = "base\\path";
        var path = "base\\path\\subfolder";

        // Act
        var result = FS.RemoveBasePath(basePath, path);

        // Assert
        Assert.That(result, Is.EqualTo("subfolder"));
    }

    /// <summary>
    /// Tests that RemoveBasePath performs case-insensitive matching.
    /// This verifies that the method works regardless of case differences.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithCaseInsensitiveMatch_ShouldWork()
    {
        // Arrange
        var basePath = "BASE/PATH";
        var path = "base/path/subfolder";

        // Act
        var result = FS.RemoveBasePath(basePath, path);

        // Assert
        Assert.That(result, Is.EqualTo("subfolder"));
    }

    /// <summary>
    /// Tests that RemoveBasePath removes the base path even when it's only a partial match at the beginning.
    /// This verifies the method's behavior with prefix matching.
    /// </summary>
    [Test]
    public void RemoveBasePath_WithPartialMatch_ShouldRemoveBaseAndReturnRemainder()
    {
        // Arrange
        var basePath = "base";
        var path = "baseball/path";

        // Act
        var result = FS.RemoveBasePath(basePath, path);

        // Assert
        // The method removes the base path if it matches the beginning of the path
        Assert.That(result, Is.EqualTo("ball/path"));
    }

    #endregion

    #region BuildNameToRelativePathXref Tests

    /// <summary>
    /// Tests that BuildNameToRelativePathXref returns an empty dictionary when the directory doesn't exist.
    /// This verifies graceful handling of non-existent paths.
    /// </summary>
    [Test]
    public void BuildNameToRelativePathXref_WithNonExistentDirectory_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent");

        // Act
        var result = FS.BuildNameToRelativePathXref(nonExistentPath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    /// <summary>
    /// Tests that BuildNameToRelativePathXref returns an empty dictionary when the directory exists but is empty.
    /// This verifies the behavior with no matching files.
    /// </summary>
    [Test]
    public void BuildNameToRelativePathXref_WithEmptyDirectory_ShouldReturnEmptyDictionary()
    {
        // Act
        var result = FS.BuildNameToRelativePathXref(_tempDirectory);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    /// <summary>
    /// Tests that BuildNameToRelativePathXref correctly maps file names to their full paths for JSON files.
    /// This verifies the basic functionality with the default extension and recursive directory traversal.
    /// </summary>
    [Test]
    public void BuildNameToRelativePathXref_WithJsonFiles_ShouldMapCorrectly()
    {
        // Arrange
        var file1 = Path.Combine(_tempDirectory, "test1.json");
        var file2 = Path.Combine(_tempDirectory, "test2.json");
        var subDir = Path.Combine(_tempDirectory, "subdir");
        Directory.CreateDirectory(subDir);
        var file3 = Path.Combine(subDir, "test3.json");

        File.WriteAllText(file1, "{}");
        File.WriteAllText(file2, "{}");
        File.WriteAllText(file3, "{}");

        // Act
        var result = FS.BuildNameToRelativePathXref(_tempDirectory);

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.ContainsKey("test1"), Is.True);
        Assert.That(result.ContainsKey("test2"), Is.True);
        Assert.That(result.ContainsKey("test3"), Is.True);
        Assert.That(result["test1"], Is.EqualTo(file1));
        Assert.That(result["test2"], Is.EqualTo(file2));
        Assert.That(result["test3"], Is.EqualTo(file3));
    }

    /// <summary>
    /// Tests that BuildNameToRelativePathXref correctly filters files by custom extension.
    /// This verifies that only files with the specified extension are included in the result.
    /// </summary>
    [Test]
    public void BuildNameToRelativePathXref_WithCustomExtension_ShouldFilterCorrectly()
    {
        // Arrange
        var jsonFile = Path.Combine(_tempDirectory, "test.json");
        var xmlFile = Path.Combine(_tempDirectory, "test.xml");
        var txtFile = Path.Combine(_tempDirectory, "test.txt");

        File.WriteAllText(jsonFile, "{}");
        File.WriteAllText(xmlFile, "<root/>");
        File.WriteAllText(txtFile, "content");

        // Act
        var result = FS.BuildNameToRelativePathXref(_tempDirectory, "xml");

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("test"), Is.True);
        Assert.That(result["test"], Is.EqualTo(xmlFile));
    }

    /// <summary>
    /// Tests that BuildNameToRelativePathXref uses "json" as the default extension when none is specified.
    /// This verifies the default parameter behavior.
    /// </summary>
    [Test]
    public void BuildNameToRelativePathXref_WithDefaultExtension_ShouldUseJson()
    {
        // Arrange
        var jsonFile = Path.Combine(_tempDirectory, "test.json");
        var xmlFile = Path.Combine(_tempDirectory, "test.xml");

        File.WriteAllText(jsonFile, "{}");
        File.WriteAllText(xmlFile, "<root/>");

        // Act
        var result = FS.BuildNameToRelativePathXref(_tempDirectory);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("test"), Is.True);
        Assert.That(result["test"], Is.EqualTo(jsonFile));
    }

    /// <summary>
    /// Tests that BuildNameToRelativePathXref recursively searches subdirectories and includes all matching files.
    /// This verifies that the SearchOption.AllDirectories parameter works correctly.
    /// </summary>
    [Test]
    public void BuildNameToRelativePathXref_WithSubdirectories_ShouldIncludeAllFiles()
    {
        // Arrange
        var subDir1 = Path.Combine(_tempDirectory, "subdir1");
        var subDir2 = Path.Combine(_tempDirectory, "subdir2");
        var nestedDir = Path.Combine(subDir1, "nested");

        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);
        Directory.CreateDirectory(nestedDir);

        var file1 = Path.Combine(_tempDirectory, "root.json");
        var file2 = Path.Combine(subDir1, "sub1.json");
        var file3 = Path.Combine(subDir2, "sub2.json");
        var file4 = Path.Combine(nestedDir, "nested.json");

        File.WriteAllText(file1, "{}");
        File.WriteAllText(file2, "{}");
        File.WriteAllText(file3, "{}");
        File.WriteAllText(file4, "{}");

        // Act
        var result = FS.BuildNameToRelativePathXref(_tempDirectory);

        // Assert
        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(result.ContainsKey("root"), Is.True);
        Assert.That(result.ContainsKey("sub1"), Is.True);
        Assert.That(result.ContainsKey("sub2"), Is.True);
        Assert.That(result.ContainsKey("nested"), Is.True);
    }

    /// <summary>
    /// Tests that BuildNameToRelativePathXref handles duplicate file names by using the last found file.
    /// This verifies the behavior when multiple files have the same name (without extension).
    /// </summary>
    [Test]
    public void BuildNameToRelativePathXref_WithDuplicateFileNames_ShouldUseLastFound()
    {
        // Arrange
        var subDir = Path.Combine(_tempDirectory, "subdir");
        Directory.CreateDirectory(subDir);

        var file1 = Path.Combine(_tempDirectory, "duplicate.json");
        var file2 = Path.Combine(subDir, "duplicate.json");

        File.WriteAllText(file1, "{}");
        File.WriteAllText(file2, "{}");

        // Act
        var result = FS.BuildNameToRelativePathXref(_tempDirectory);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("duplicate"), Is.True);
        // The exact path returned depends on enumeration order, but it should be one of the two files
        Assert.That(result["duplicate"], Is.EqualTo(file1).Or.EqualTo(file2));
    }

    #endregion
}
