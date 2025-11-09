using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blackwood;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the RunProgram functionality in Blackwood.IO.
/// Tests cover the Util.RunCommand method, process execution, output streaming,
/// error handling, and various command scenarios.
/// These tests verify that background program execution works correctly.
/// </summary>
[TestFixture]
public class RunProgramTests
{
    private string? _tempDirectory;
    private string _testScriptPath;

    /// <summary>
    /// Sets up the test environment before each test.
    /// Creates a temporary directory and test script for each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Create a temporary directory for test files
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Create a test script path
        _testScriptPath = Path.Combine(_tempDirectory, "testscript.bat");
    }

    /// <summary>
    /// Cleans up the test environment after each test.
    /// Removes the temporary directory and all test files.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Tests that RunCommand method executes a simple command successfully.
    /// This verifies basic command execution functionality.
    /// </summary>
    [Test]
    public async Task RunCommand_WithSimpleCommand_ShouldExecuteSuccessfully()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo Hello World";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(line => line.Contains("Hello World")), Is.True);
    }

    /// <summary>
    /// Tests that RunCommand method handles null parameters gracefully.
    /// This verifies that null parameters are handled without crashing.
    /// </summary>
    [Test]
    public async Task RunCommand_WithNullParameters_ShouldThrowException()
    {
        // Act & Assert - Should throw exception for null command
        Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
        {
            var results = new List<string>();
            await foreach (var line in Util.RunCommand(null, null, null))
            {
                results.Add(line);
            }
        });
    }

    /// <summary>
    /// Tests that RunCommand method executes a batch script successfully.
    /// This verifies script execution functionality.
    /// </summary>
    [Test]
    public async Task RunCommand_WithBatchScript_ShouldExecuteSuccessfully()
    {
        // Arrange
        var scriptContent = "@echo off\necho Test Output\necho Another Line";
        File.WriteAllText(_testScriptPath, scriptContent);

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(_testScriptPath))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(line => line.Contains("Test Output")), Is.True);
        Assert.That(results.Any(line => line.Contains("Another Line")), Is.True);
    }

    /// <summary>
    /// Tests that RunCommand method handles commands with arguments correctly.
    /// This verifies argument passing functionality.
    /// </summary>
    [Test]
    public async Task RunCommand_WithArguments_ShouldPassArgumentsCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo Argument Test";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(line => line.Contains("Argument Test")), Is.True);
    }

    /// <summary>
    /// Tests that RunCommand method handles commands with verbs correctly.
    /// This verifies verb-based execution functionality.
    /// </summary>
    [Test]
    public async Task RunCommand_WithVerb_ShouldExecuteWithVerb()
    {
        // Arrange
        var command = "notepad.exe";
        var verb = "open";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, verb, null))
        {
            results.Add(line);
        }

        // Assert - Notepad might not produce output, but should not crash
        Assert.That(results, Is.Not.Null);
    }

    /// <summary>
    /// Tests that RunCommand method handles non-existent commands gracefully.
    /// This verifies error handling for invalid commands.
    /// </summary>
    [Test]
    public async Task RunCommand_WithNonExistentCommand_ShouldThrowException()
    {
        // Arrange
        var nonExistentCommand = "NonExistentCommand12345";

        // Act & Assert - Should throw exception for non-existent command
        Assert.ThrowsAsync<Win32Exception>(async () =>
        {
            var results = new List<string>();
            await foreach (var line in Util.RunCommand(nonExistentCommand))
            {
                results.Add(line);
            }
        });
    }

    /// <summary>
    /// Tests that RunCommand method handles empty output correctly.
    /// This verifies behavior when commands produce no output.
    /// </summary>
    [Test]
    public async Task RunCommand_WithEmptyOutput_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo. > nul";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Null);
        // Empty output should result in empty collection
        Assert.That(results, Is.Empty);
    }

    /// <summary>
    /// Tests that RunCommand method handles multiple lines of output correctly.
    /// This verifies multi-line output processing.
    /// </summary>
    [Test]
    public async Task RunCommand_WithMultipleLines_ShouldProcessAllLines()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo Line 1 && echo Line 2 && echo Line 3";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(results.Any(line => line.Contains("Line 1")), Is.True);
        Assert.That(results.Any(line => line.Contains("Line 2")), Is.True);
        Assert.That(results.Any(line => line.Contains("Line 3")), Is.True);
    }

    /// <summary>
    /// Tests that RunCommand method handles long-running commands correctly.
    /// This verifies behavior with commands that take time to complete.
    /// </summary>
    [Test]
    public async Task RunCommand_WithLongRunningCommand_ShouldWaitForCompletion()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c ping 127.0.0.1 -n 1 > nul && echo Completed";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Null);
        // Should wait for the command to complete
        Assert.That(results.Any(line => line.Contains("Completed")), Is.True);
    }

    /// <summary>
    /// Tests that RunCommand method handles commands with special characters correctly.
    /// This verifies special character handling in arguments.
    /// </summary>
    [Test]
    public async Task RunCommand_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo Special: !@#$%^&*()";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(line => line.Contains("Special:")), Is.True);
    }

    /// <summary>
    /// Tests that RunCommand method handles commands with Unicode characters correctly.
    /// This verifies Unicode character handling in output.
    /// </summary>
    [Test]
    public async Task RunCommand_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo Unicode: 你好世界";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(line => line.Contains("Unicode:")), Is.True);
    }

    /// <summary>
    /// Tests that RunCommand method handles concurrent executions correctly.
    /// This verifies thread safety and concurrent execution.
    /// </summary>
    [Test]
    public async Task RunCommand_WithConcurrentExecutions_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo Concurrent Test";

        // Act
        var tasks = new List<Task<List<string>>>();
        for (int? i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var results = new List<string>();
                await foreach (var line in Util.RunCommand(command, null, arguments))
                {
                    results.Add(line);
                }
                return results;
            }));
        }

        var allResults = await Task.WhenAll(tasks);

        // Assert
        Assert.That(allResults, Is.Not.Null);
        Assert.That(allResults.Length, Is.EqualTo(3));
        foreach (var results in allResults)
        {
            Assert.That(results, Is.Not.Empty);
            Assert.That(results.Any(line => line.Contains("Concurrent Test")), Is.True);
        }
    }

    /// <summary>
    /// Tests that RunCommand method handles commands that produce error output correctly.
    /// This verifies error output handling.
    /// </summary>
    [Test]
    public async Task RunCommand_WithErrorOutput_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo Error Test 2>&1";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Null);
        // Should handle error output gracefully
        Assert.That(results, Is.Not.Empty);
    }

    /// <summary>
    /// Tests that RunCommand method handles commands with large output correctly.
    /// This verifies large output processing.
    /// </summary>
    [Test]
    public async Task RunCommand_WithLargeOutput_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c for /l %i in (1,1,100) do @echo Line %i";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(100));
        Assert.That(results.Any(line => line.Contains("Line 1")), Is.True);
        Assert.That(results.Any(line => line.Contains("Line 100")), Is.True);
    }

    /// <summary>
    /// Tests that RunCommand method handles commands with different encodings correctly.
    /// This verifies encoding handling in output.
    /// </summary>
    [Test]
    public async Task RunCommand_WithDifferentEncodings_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c chcp 65001 && echo UTF-8 Test";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Null);
        // Should handle different encodings gracefully
        Assert.That(results, Is.Not.Empty);
    }

    /// <summary>
    /// Tests that RunCommand method handles commands with environment variables correctly.
    /// This verifies environment variable handling.
    /// </summary>
    [Test]
    public async Task RunCommand_WithEnvironmentVariables_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c echo %TEMP%";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Empty);
        // Should produce some output when echoing environment variable
        Assert.That(results.Count, Is.GreaterThan(0));
    }

    /// <summary>
    /// Tests that RunCommand method handles commands with working directory changes correctly.
    /// This verifies working directory handling.
    /// </summary>
    [Test]
    public async Task RunCommand_WithWorkingDirectory_ShouldHandleCorrectly()
    {
        // Arrange
        var command = "cmd";
        var arguments = "/c cd /d " + _tempDirectory + " && dir";

        // Act
        var results = new List<string>();
        await foreach (var line in Util.RunCommand(command, null, arguments))
        {
            results.Add(line);
        }

        // Assert
        Assert.That(results, Is.Not.Null);
        // Should handle working directory changes gracefully
        Assert.That(results, Is.Not.Empty);
    }
}
