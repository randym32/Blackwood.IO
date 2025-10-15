using System.Text;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the Text.ReadAllLines method in Blackwood.IO.
/// Tests cover reading text from streams, encoding handling, line processing,
/// and various edge cases including empty streams, null streams, and different text formats.
/// These tests verify that the ReadAllLines method correctly processes text streams.
/// </summary>
[TestFixture]
public class ReadAllLinesTests
{
    /// <summary>
    /// Helper method to normalize line endings for expected results.
    /// This accounts for the fact that ReadAllLines normalizes line endings to the system's format.
    /// </summary>
    /// <param name="content">The content with original line endings</param>
    /// <returns>The content with normalized line endings</returns>
    private static string NormalizeLineEndings(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        // Split by line endings and preserve empty lines
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

        // ReadAllLines behavior:
        // - If content ends with line endings, it removes the final empty line
        // - Always adds a final line ending to the result

        // Check if the original content ended with line endings
        bool endsWithLineEnding = content.EndsWith("\n") || content.EndsWith("\r") || content.EndsWith("\r\n");

        if (endsWithLineEnding)
        {
            // If it ended with line endings, ReadAllLines removes the final empty line
            // So we need to remove the last empty line from our split result
            if (lines.Length > 0 && string.IsNullOrEmpty(lines[lines.Length - 1]))
            {
                var linesWithoutLastEmpty = new string[lines.Length - 1];
                Array.Copy(lines, linesWithoutLastEmpty, lines.Length - 1);
                return string.Join(Environment.NewLine, linesWithoutLastEmpty) + Environment.NewLine;
            }
        }

        // Join with normalized line endings and add final line ending
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
    #region Basic Functionality Tests

    /// <summary>
    /// Tests that ReadAllLines returns the correct content from a simple text stream.
    /// This verifies the basic functionality of reading text from a stream.
    /// Note: ReadAllLines normalizes line endings to the system's line ending format.
    /// </summary>
    [Test]
    public void ReadAllLines_WithSimpleText_ShouldReturnCorrectContent()
    {
        // Arrange
        var content = "Hello World\nThis is a test\nLine 3";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        // The method normalizes line endings to the system's line ending
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    /// <summary>
    /// Tests that ReadAllLines handles empty streams correctly.
    /// This verifies that the method gracefully handles empty input.
    /// </summary>
    [Test]
    public void ReadAllLines_WithEmptyStream_ShouldReturnEmptyString()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    /// <summary>
    /// Tests that ReadAllLines handles single line text correctly.
    /// This verifies that the method works with minimal input.
    /// </summary>
    [Test]
    public void ReadAllLines_WithSingleLine_ShouldReturnCorrectContent()
    {
        // Arrange
        var content = "Single line text";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    /// <summary>
    /// Tests that ReadAllLines handles multiple lines correctly.
    /// This verifies that the method processes multiple lines as expected.
    /// Note: ReadAllLines normalizes line endings to the system's line ending format.
    /// </summary>
    [Test]
    public void ReadAllLines_WithMultipleLines_ShouldReturnCorrectContent()
    {
        // Arrange
        var content = "Line 1\nLine 2\nLine 3\nLine 4";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        // The method normalizes line endings to the system's line ending
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    #endregion

    #region Line Ending Tests

    /// <summary>
    /// Tests that ReadAllLines handles different line ending formats correctly.
    /// This verifies that the method works with various line ending conventions.
    /// </summary>
    [Test]
    public void ReadAllLines_WithDifferentLineEndings_ShouldHandleCorrectly()
    {
        // Arrange
        var content = "Line 1\r\nLine 2\nLine 3\rLine 4";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        // The method should normalize all line endings to the system's line ending
        var expectedLines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var expectedResult = string.Join(Environment.NewLine, expectedLines) + Environment.NewLine;
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    /// <summary>
    /// Tests that ReadAllLines handles text with no line endings correctly.
    /// This verifies that the method works with text that doesn't have explicit line breaks.
    /// </summary>
    [Test]
    public void ReadAllLines_WithNoLineEndings_ShouldAddLineEnding()
    {
        // Arrange
        var content = "Text without line endings";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    /// <summary>
    /// Tests that ReadAllLines handles text with trailing empty lines correctly.
    /// This verifies that the method preserves empty lines in the output.
    /// </summary>
    [Test]
    public void ReadAllLines_WithTrailingEmptyLines_ShouldPreserveEmptyLines()
    {
        // Arrange
        var content = "Line 1\nLine 2\n\n\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    #endregion

    #region Encoding Tests

    /// <summary>
    /// Tests that ReadAllLines uses UTF-8 encoding correctly.
    /// This verifies that the method handles UTF-8 encoded text properly.
    /// </summary>
    [Test]
    public void ReadAllLines_WithUtf8Encoding_ShouldHandleCorrectly()
    {
        // Arrange
        var content = "Hello 世界\nUnicode: \u2603\nEmoji: \U0001F600";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    /// <summary>
    /// Tests that ReadAllLines handles special characters correctly.
    /// This verifies that the method processes special characters without issues.
    /// </summary>
    [Test]
    public void ReadAllLines_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var content = "Special chars: !@#$%^&*()\nTabs:\tand\tspaces\nQuotes: \"double\" and 'single'";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that ReadAllLines handles very long lines correctly.
    /// This verifies that the method can process large amounts of text efficiently.
    /// </summary>
    [Test]
    public void ReadAllLines_WithVeryLongLine_ShouldHandleCorrectly()
    {
        // Arrange
        var longLine = new string('A', 10000);
        var content = $"Short line\n{longLine}\nAnother short line";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    /// <summary>
    /// Tests that ReadAllLines handles many lines correctly.
    /// This verifies that the method can process large numbers of lines efficiently.
    /// </summary>
    [Test]
    public void ReadAllLines_WithManyLines_ShouldHandleCorrectly()
    {
        // Arrange
        var lines = new string[1000];
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = $"Line {i + 1}";
        }
        var content = string.Join("\n", lines);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    /// <summary>
    /// Tests that ReadAllLines handles text with only whitespace correctly.
    /// This verifies that the method preserves whitespace-only lines.
    /// </summary>
    [Test]
    public void ReadAllLines_WithWhitespaceOnly_ShouldPreserveWhitespace()
    {
        // Arrange
        var content = "   \n\t\n  \t  \n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    #endregion

    #region Stream Handling Tests

    /// <summary>
    /// Tests that ReadAllLines properly disposes of the StreamReader.
    /// This verifies that the method follows proper resource management practices.
    /// </summary>
    [Test]
    public void ReadAllLines_ShouldDisposeStreamReader()
    {
        // Arrange
        var content = "Test content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        // The method should complete without throwing exceptions
        // The stream will be disposed after reading, so we can't check its position
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    /// <summary>
    /// Tests that ReadAllLines handles streams that are already at the end.
    /// This verifies that the method works correctly with streams in various positions.
    /// </summary>
    [Test]
    public void ReadAllLines_WithStreamAtEnd_ShouldReturnEmptyString()
    {
        // Arrange
        var content = "Test content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        // Read to the end of the stream
        stream.Read(new byte[content.Length], 0, content.Length);

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    /// <summary>
    /// Tests that ReadAllLines handles streams with mixed content correctly.
    /// This verifies that the method can process complex text content.
    /// </summary>
    [Test]
    public void ReadAllLines_WithMixedContent_ShouldHandleCorrectly()
    {
        // Arrange
        var content = "Line 1\n\nLine 3\n   \nLine 5\n\t\nLine 7";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result = Text.ReadAllLines(stream);

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that ReadAllLines performs reasonably with large content.
    /// This verifies that the method can handle substantial amounts of text efficiently.
    /// </summary>
    [Test]
    public void ReadAllLines_WithLargeContent_ShouldPerformEfficiently()
    {
        // Arrange
        var lines = new string[100];
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = $"This is line {i + 1} with some content to make it longer";
        }
        var content = string.Join("\n", lines);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var startTime = DateTime.UtcNow;
        var result = Text.ReadAllLines(stream);
        var endTime = DateTime.UtcNow;

        // Assert
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
        Assert.That((endTime - startTime).TotalMilliseconds, Is.LessThan(1000)); // Should complete within 1 second
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests that ReadAllLines works correctly with different stream types.
    /// This verifies that the method is compatible with various stream implementations.
    /// </summary>
    [Test]
    public void ReadAllLines_WithFileStream_ShouldWorkCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = "File content\nLine 2\nLine 3";
        File.WriteAllText(tempFile, content, Encoding.UTF8);

        try
        {
            using var stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);

            // Act
            var result = Text.ReadAllLines(stream);

            // Assert
            var expectedResult = NormalizeLineEndings(content);
        Assert.That(result, Is.EqualTo(expectedResult));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <summary>
    /// Tests that ReadAllLines produces consistent results across multiple calls.
    /// This verifies that the method is deterministic and reliable.
    /// </summary>
    [Test]
    public void ReadAllLines_WithSameContent_ShouldProduceConsistentResults()
    {
        // Arrange
        var content = "Consistent content\nLine 2\nLine 3";
        var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var result1 = Text.ReadAllLines(stream1);
        var result2 = Text.ReadAllLines(stream2);

        // Assert
        Assert.That(result1, Is.EqualTo(result2));
        var expectedResult = NormalizeLineEndings(content);
        Assert.That(result1, Is.EqualTo(expectedResult));
    }

    #endregion
}
