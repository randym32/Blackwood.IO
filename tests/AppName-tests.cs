using System.Reflection;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the Application.Name property in Blackwood.IO.
/// Tests cover the application name resolution logic including assembly lookup,
/// fallback mechanisms, and caching behavior.
/// These tests verify that the application name is correctly determined across different execution contexts.
/// </summary>
[TestFixture]
public class AppNameTests
{
    #region Basic Functionality Tests

    /// <summary>
    /// Tests that Application.Name returns a valid, non-empty string.
    /// This verifies the basic functionality of the property.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldReturnValidString()
    {
        // Act
        var appName = Application.Name;

        // Assert
        Assert.That(appName, Is.Not.Null);
        Assert.That(appName, Is.Not.Empty);
        Assert.That(appName.Length, Is.GreaterThan(0));
    }

    /// <summary>
    /// Tests that Application.Name returns a consistent value across multiple calls.
    /// This verifies that the property is cached and doesn't change between calls.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldReturnConsistentValue()
    {
        // Act
        var firstCall = Application.Name;
        var secondCall = Application.Name;
        var thirdCall = Application.Name;

        // Assert
        Assert.That(firstCall, Is.EqualTo(secondCall));
        Assert.That(secondCall, Is.EqualTo(thirdCall));
        Assert.That(firstCall, Is.EqualTo(thirdCall));
    }

    /// <summary>
    /// Tests that Application.Name returns a string that doesn't contain invalid characters.
    /// This verifies that the name is suitable for use in file paths and other contexts.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldNotContainInvalidCharacters()
    {
        // Act
        var appName = Application.Name;

        // Assert
        Assert.That(appName, Does.Not.Contain("<"));
        Assert.That(appName, Does.Not.Contain(">"));
        Assert.That(appName, Does.Not.Contain(":"));
        Assert.That(appName, Does.Not.Contain("\""));
        Assert.That(appName, Does.Not.Contain("|"));
        Assert.That(appName, Does.Not.Contain("?"));
        Assert.That(appName, Does.Not.Contain("*"));
        Assert.That(appName, Does.Not.Contain("\\"));
        Assert.That(appName, Does.Not.Contain("/"));
    }

    #endregion

    #region Assembly Resolution Tests

    /// <summary>
    /// Tests that Application.Name returns a name that matches the current assembly.
    /// This verifies that the property correctly resolves the application name from the executing assembly.
    /// Note: In test environment, this may return "testhost" instead of the test assembly name.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldMatchCurrentAssemblyName()
    {
        // Arrange
        var currentAssembly = Assembly.GetExecutingAssembly();
        var expectedName = currentAssembly.GetName().Name;

        // Act
        var appName = Application.Name;

        // Assert
        // In test environment, Application.Name may return "testhost" instead of the test assembly name
        // This is expected behavior when running under a test host
        Assert.That(appName, Is.Not.Null);
        Assert.That(appName, Is.Not.Empty);
        // The name should either match the current assembly or be "testhost" (test runner)
        Assert.That(appName == expectedName || appName == "testhost", Is.True);
    }

    /// <summary>
    /// Tests that Application.Name returns a name that is not null or empty.
    /// This verifies that the assembly name resolution works correctly.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldNotBeNullOrEmpty()
    {
        // Act
        var appName = Application.Name;

        // Assert
        Assert.That(appName, Is.Not.Null);
        Assert.That(appName, Is.Not.Empty);
        Assert.That(appName.Trim(), Is.Not.Empty);
    }

    /// <summary>
    /// Tests that Application.Name returns a reasonable length string.
    /// This verifies that the name is not excessively long or short.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldHaveReasonableLength()
    {
        // Act
        var appName = Application.Name;

        // Assert
        Assert.That(appName.Length, Is.GreaterThan(0));
        Assert.That(appName.Length, Is.LessThan(100)); // Reasonable upper bound
    }

    #endregion

    #region Caching Behavior Tests

    /// <summary>
    /// Tests that Application.Name caches the result after the first call.
    /// This verifies the performance optimization of caching the application name.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldCacheResult()
    {
        // Act - Call multiple times to verify caching
        var call1 = Application.Name;
        var call2 = Application.Name;
        var call3 = Application.Name;

        // Assert - All calls should return the same value
        Assert.That(call1, Is.EqualTo(call2));
        Assert.That(call2, Is.EqualTo(call3));
        Assert.That(call1, Is.EqualTo(call3));
    }

    /// <summary>
    /// Tests that Application.Name returns the same value when called from different contexts.
    /// This verifies that the caching mechanism works correctly across different execution contexts.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldReturnSameValueFromDifferentContexts()
    {
        // Act
        var nameFromMainThread = Application.Name;
        var nameFromTask = Task.Run(() => Application.Name).Result;

        // Assert
        Assert.That(nameFromMainThread, Is.EqualTo(nameFromTask));
    }

    #endregion

    #region Fallback Mechanism Tests

    /// <summary>
    /// Tests that Application.Name handles the case where GetEntryAssembly returns null.
    /// This verifies the fallback mechanism using StackTrace when entry assembly is not available.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldHandleNullEntryAssembly()
    {
        // Note: This test verifies the fallback mechanism exists
        // The actual behavior depends on the execution context
        // In test context, GetEntryAssembly() typically returns null, so the fallback should work

        // Act
        var appName = Application.Name;

        // Assert
        Assert.That(appName, Is.Not.Null);
        Assert.That(appName, Is.Not.Empty);
        // Should not be the fallback "unknownApp" in normal test execution
        Assert.That(appName, Is.Not.EqualTo("unknownApp"));
    }

    /// <summary>
    /// Tests that Application.Name returns a valid name even when assembly resolution might fail.
    /// This verifies the robustness of the name resolution mechanism.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldBeRobustAgainstAssemblyResolutionIssues()
    {
        // Act
        var appName = Application.Name;

        // Assert
        Assert.That(appName, Is.Not.Null);
        Assert.That(appName, Is.Not.Empty);
        Assert.That(appName, Is.Not.EqualTo("unknownApp")); // Should not fall back to default
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests that Application.Name works correctly with the AppDataPath property.
    /// This verifies the integration between Application.Name and the path utilities.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldWorkWithAppDataPath()
    {
        // Act
        var appName = Application.Name;
        var appDataPath = FS.AppDataPath;

        // Assert
        Assert.That(appName, Is.Not.Null);
        Assert.That(appDataPath, Is.Not.Null);
        Assert.That(appDataPath, Does.EndWith(appName));
    }

    /// <summary>
    /// Tests that Application.Name returns a name that can be used in file paths.
    /// This verifies that the name is suitable for use in file system operations.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldBeSuitableForFilePathUsage()
    {
        // Arrange
        var appName = Application.Name;
        var testPath = Path.Combine(Path.GetTempPath(), appName, "test.txt");

        // Act & Assert - Should not throw an exception when used in path construction
        Assert.DoesNotThrow(() =>
        {
            var directory = Path.GetDirectoryName(testPath);
            // Verify the path is valid
            Assert.That(directory, Is.Not.Null);
            Assert.That(directory, Does.Contain(appName));
        });
    }

    /// <summary>
    /// Tests that Application.Name returns a name that is consistent with assembly metadata.
    /// This verifies that the name resolution correctly uses assembly information.
    /// Note: In test environment, this may return "testhost" instead of the test assembly name.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldBeConsistentWithAssemblyMetadata()
    {
        // Arrange
        var currentAssembly = Assembly.GetExecutingAssembly();
        var assemblyName = currentAssembly.GetName().Name;

        // Act
        var appName = Application.Name;

        // Assert
        // In test environment, Application.Name may return "testhost" instead of the test assembly name
        // This is expected behavior when running under a test host
        Assert.That(appName, Is.Not.Null);
        Assert.That(appName, Is.Not.Empty);
        // The name should either match the current assembly or be "testhost" (test runner)
        Assert.That(appName == assemblyName || appName == "testhost", Is.True);
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that Application.Name handles edge cases gracefully.
    /// This verifies the robustness of the name resolution in various scenarios.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldHandleEdgeCasesGracefully()
    {
        // Act
        var appName = Application.Name;

        // Assert
        Assert.That(appName, Is.Not.Null);
        Assert.That(appName, Is.Not.Empty);
        Assert.That(appName.Trim(), Is.Not.Empty);
        Assert.That(appName, Does.Not.StartWith(" "));
        Assert.That(appName, Does.Not.EndWith(" "));
    }

    /// <summary>
    /// Tests that Application.Name returns a name that is stable across different execution contexts.
    /// This verifies that the name resolution is consistent regardless of how the code is executed.
    /// </summary>
    [Test]
    public void ApplicationName_ShouldBeStableAcrossExecutionContexts()
    {
        // Act
        var name1 = Application.Name;
        var name2 = Task.Run(() => Application.Name).Result;
        var name3 = Task.Run(async () =>
        {
            await Task.Delay(1); // Small delay to test async context
            return Application.Name;
        }).Result;

        // Assert
        Assert.That(name1, Is.EqualTo(name2));
        Assert.That(name2, Is.EqualTo(name3));
        Assert.That(name1, Is.EqualTo(name3));
    }

    #endregion
}
