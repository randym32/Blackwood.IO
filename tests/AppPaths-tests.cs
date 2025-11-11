using System.Reflection;
using Blackwood.Base;

namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the AppPaths functionality in Blackwood.IO.
/// Tests cover the CommonApplicationDataPath, AppDataPath, and ExeFilePath properties.
/// These tests verify that the application path utilities work correctly across different environments.
/// </summary>
[TestFixture]
public class AppPathsTests
{
    #region CommonApplicationDataPath Tests

    /// <summary>
    /// Tests that CommonApplicationDataPath returns a valid, non-empty path.
    /// This verifies that the property correctly retrieves the common application data folder.
    /// </summary>
    [Test]
    public void CommonApplicationDataPath_ShouldReturnValidPath()
    {
        // Act
        var commonAppDataPath = FS.CommonApplicationDataPath;

        // Assert
        Assert.That(commonAppDataPath, Is.Not.Null);
        Assert.That(commonAppDataPath, Is.Not.Empty);
        Assert.That(Directory.Exists(commonAppDataPath), Is.True);
    }

    /// <summary>
    /// Tests that CommonApplicationDataPath returns the same value as Environment.GetFolderPath.
    /// This verifies that the property correctly delegates to the Environment API.
    /// </summary>
    [Test]
    public void CommonApplicationDataPath_ShouldMatchEnvironmentGetFolderPath()
    {
        // Act
        var fsPath = FS.CommonApplicationDataPath;
        var environmentPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        // Assert
        Assert.That(fsPath, Is.EqualTo(environmentPath));
    }

    /// <summary>
    /// Tests that CommonApplicationDataPath returns a consistent value across multiple calls.
    /// This verifies that the property is stable and doesn't change between calls.
    /// </summary>
    [Test]
    public void CommonApplicationDataPath_ShouldReturnConsistentValue()
    {
        // Act
        var firstCall = FS.CommonApplicationDataPath;
        var secondCall = FS.CommonApplicationDataPath;

        // Assert
        Assert.That(firstCall, Is.EqualTo(secondCall));
    }

    #endregion

    #region AppDataPath Tests

    /// <summary>
    /// Tests that AppDataPath returns a valid, non-empty path.
    /// This verifies that the property correctly constructs the application data path.
    /// </summary>
    [Test]
    public void AppDataPath_ShouldReturnValidPath()
    {
        // Act
        var appDataPath = FS.AppDataPath;

        // Assert
        Assert.That(appDataPath, Is.Not.Null);
        Assert.That(appDataPath, Is.Not.Empty);
    }

    /// <summary>
    /// Tests that AppDataPath includes the application name in the path.
    /// This verifies that the path is constructed using the Application.Name property.
    /// </summary>
    [Test]
    public void AppDataPath_ShouldIncludeApplicationName()
    {
        // Act
        var appDataPath = FS.AppDataPath;
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Application.Name);

        // Assert
        Assert.That(appDataPath, Is.EqualTo(expectedPath));
    }

    /// <summary>
    /// Tests that AppDataPath returns a consistent value across multiple calls.
    /// This verifies that the property is stable and doesn't change between calls.
    /// </summary>
    [Test]
    public void AppDataPath_ShouldReturnConsistentValue()
    {
        // Act
        var firstCall = FS.AppDataPath;
        var secondCall = FS.AppDataPath;

        // Assert
        Assert.That(firstCall, Is.EqualTo(secondCall));
    }

    /// <summary>
    /// Tests that AppDataPath is based on ApplicationData folder.
    /// This verifies that the path uses the correct base folder for application data.
    /// </summary>
    [Test]
    public void AppDataPath_ShouldBeBasedOnLocalApplicationData()
    {
        // Act
        var appDataPath = FS.AppDataPath;
        var appDataPathBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Assert
        Assert.That(appDataPath, Does.StartWith(appDataPathBase));
    }

    #endregion

    #region ExeFilePath Tests

    /// <summary>
    /// Tests that ExeFilePath returns a valid, non-empty path.
    /// This verifies that the property correctly retrieves the executable file path.
    /// </summary>
    [Test]
    public void ExeFilePath_ShouldReturnValidPath()
    {
        // Act
        var exeFilePath = FS.ExeFilePath;

        // Assert
        Assert.That(exeFilePath, Is.Not.Null);
        Assert.That(exeFilePath, Is.Not.Empty);
        Assert.That(Directory.Exists(exeFilePath), Is.True);
    }

    /// <summary>
    /// Tests that ExeFilePath returns the same value as Assembly.GetExecutingAssembly().Location directory.
    /// This verifies that the property correctly delegates to the Assembly API.
    /// </summary>
    [Test]
    public void ExeFilePath_ShouldMatchAssemblyLocationDirectory()
    {
        // Act
        var fsPath = FS.ExeFilePath;
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // Assert
        Assert.That(fsPath, Is.EqualTo(assemblyPath));
    }

    /// <summary>
    /// Tests that ExeFilePath returns a consistent value across multiple calls.
    /// This verifies that the property is stable and doesn't change between calls.
    /// </summary>
    [Test]
    public void ExeFilePath_ShouldReturnConsistentValue()
    {
        // Act
        var firstCall = FS.ExeFilePath;
        var secondCall = FS.ExeFilePath;

        // Assert
        Assert.That(firstCall, Is.EqualTo(secondCall));
    }

    /// <summary>
    /// Tests that ExeFilePath contains the current assembly's directory.
    /// This verifies that the path points to the correct location of the executing assembly.
    /// </summary>
    [Test]
    public void ExeFilePath_ShouldContainCurrentAssemblyDirectory()
    {
        // Act
        var exeFilePath = FS.ExeFilePath;
        var currentAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        var currentAssemblyDirectory = Path.GetDirectoryName(currentAssemblyLocation);

        // Assert
        Assert.That(exeFilePath, Is.EqualTo(currentAssemblyDirectory));
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests that all path properties return valid paths and are consistent.
    /// This verifies that the entire AppPaths functionality works together correctly.
    /// </summary>
    [Test]
    public void AllPathProperties_ShouldReturnValidPaths()
    {
        // Act
        var commonAppDataPath = FS.CommonApplicationDataPath;
        var appDataPath = FS.AppDataPath;
        var exeFilePath = FS.ExeFilePath;

        // Assert
        Assert.That(commonAppDataPath, Is.Not.Null.And.Not.Empty);
        Assert.That(appDataPath, Is.Not.Null.And.Not.Empty);
        Assert.That(exeFilePath, Is.Not.Null.And.Not.Empty);

        // Verify that paths are different (they should point to different locations)
        Assert.That(commonAppDataPath, Is.Not.EqualTo(appDataPath));
        Assert.That(commonAppDataPath, Is.Not.EqualTo(exeFilePath));
        Assert.That(appDataPath, Is.Not.EqualTo(exeFilePath));
    }

    /// <summary>
    /// Tests that AppDataPath is properly constructed with the application name.
    /// This verifies the integration between AppDataPath and Application.Name.
    /// </summary>
    [Test]
    public void AppDataPath_ShouldBeProperlyConstructedWithApplicationName()
    {
        // Arrange
        var expectedBasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var applicationName = Application.Name;

        // Act
        var appDataPath = FS.AppDataPath;

        // Assert
        Assert.That(appDataPath, Does.StartWith(expectedBasePath));
        Assert.That(appDataPath, Does.EndWith(applicationName));
        Assert.That(appDataPath, Is.EqualTo(Path.Combine(expectedBasePath, applicationName)));
    }

    #endregion
}
