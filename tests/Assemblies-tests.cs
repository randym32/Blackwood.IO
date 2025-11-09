using System.Reflection;
using NUnit.Framework;
using Blackwood;

namespace Blackwood.IO.Tests;

/// <summary>
/// Tests for Util.AssembliesToSearch.
/// Verifies that the method returns assemblies in the correct order without duplicates.
/// </summary>
[TestFixture]
public class AssembliesTests
{
    /// <summary>
    /// Tests that AssembliesToSearch returns a non-empty collection.
    /// </summary>
    [Test]
    public void AssembliesToSearch_ReturnsNonEmptyCollection()
    {
        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        Assert.That(assemblies, Is.Not.Null);
        Assert.That(assemblies, Is.Not.Empty);
    }

    /// <summary>
    /// Tests that AssembliesToSearch includes the executing assembly.
    /// </summary>
    [Test]
    public void AssembliesToSearch_IncludesExecutingAssembly()
    {
        // Arrange
        var executingAssembly = Assembly.GetExecutingAssembly();

        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        Assert.That(assemblies, Does.Contain(executingAssembly));
    }

    /// <summary>
    /// Tests that AssembliesToSearch includes the entry assembly if available.
    /// </summary>
    [Test]
    public void AssembliesToSearch_IncludesEntryAssembly_IfAvailable()
    {
        // Arrange
        var entryAssembly = Assembly.GetEntryAssembly();

        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        if (entryAssembly != null)
        {
            Assert.That(assemblies, Does.Contain(entryAssembly));
        }
    }

    /// <summary>
    /// Tests that AssembliesToSearch includes loaded assemblies.
    /// </summary>
    [Test]
    public void AssembliesToSearch_IncludesLoadedAssemblies()
    {
        // Arrange
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var knownAssembly = typeof(string).Assembly; // System.Private.CoreLib

        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        Assert.That(assemblies, Does.Contain(knownAssembly));
    }

    /// <summary>
    /// Tests that AssembliesToSearch does not return duplicate assemblies.
    /// </summary>
    [Test]
    public void AssembliesToSearch_DoesNotReturnDuplicates()
    {
        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        var distinctAssemblies = assemblies.Distinct().ToList();
        Assert.That(assemblies.Count, Is.EqualTo(distinctAssemblies.Count), "Should not contain duplicates");
    }

    /// <summary>
    /// Tests that AssembliesToSearch returns entry assembly first if available.
    /// </summary>
    [Test]
    public void AssembliesToSearch_ReturnsEntryAssemblyFirst_IfAvailable()
    {
        // Arrange
        var entryAssembly = Assembly.GetEntryAssembly();

        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        if (entryAssembly != null)
        {
            Assert.That(assemblies[0], Is.EqualTo(entryAssembly));
        }
    }

    /// <summary>
    /// Tests that AssembliesToSearch returns executing assembly after entry assembly.
    /// </summary>
    [Test]
    public void AssembliesToSearch_ReturnsExecutingAssemblyAfterEntry()
    {
        // Arrange
        var entryAssembly = Assembly.GetEntryAssembly();
        // Note: Application.Assemblies() uses Assembly.GetExecutingAssembly()
        // which returns the assembly where Application.Assemblies() is defined (Blackwood.IO),
        // not the test assembly (Blackwood.IO.tests)
        var executingAssembly = typeof(Application).Assembly; // This is Blackwood.IO

        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        if (entryAssembly != null)
        {
            // Entry assembly should be first
            Assert.That(assemblies[0], Is.EqualTo(entryAssembly));
            // Executing assembly (where Application.Assemblies is defined) should be second
            Assert.That(assemblies[1], Is.EqualTo(executingAssembly),
                $"Expected executing assembly at index 1, but got {assemblies[1].GetName().Name} instead");
        }
        else
        {
            Assert.That(assemblies[0], Is.EqualTo(executingAssembly));
        }
    }

    /// <summary>
    /// Tests that AssembliesToSearch returns loaded assemblies in reverse order.
    /// </summary>
    [Test]
    public void AssembliesToSearch_ReturnsLoadedAssembliesInReverseOrder()
    {
        // Arrange
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var entryAssembly = Assembly.GetEntryAssembly();
        // Note: Application.Assemblies() uses Assembly.GetExecutingAssembly()
        // which returns the assembly where Application.Assemblies() is defined (Blackwood.IO),
        // not the test assembly (Blackwood.IO.tests)
        var executingAssembly = typeof(Application).Assembly; // This is Blackwood.IO

        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        // Skip entry and executing assemblies
        var startIndex = entryAssembly != null ? 2 : 1;
        var returnedLoadedAssemblies = assemblies.Skip(startIndex).ToList();
        var expectedLoadedAssemblies = loadedAssemblies.Reverse<Assembly>()
            .Where(a => a != entryAssembly && a != executingAssembly)
            .ToList();

        // Check that the order matches (most recent first)
        for (int i = 0; i < Math.Min(returnedLoadedAssemblies.Count, expectedLoadedAssemblies.Count); i++)
        {
            Assert.That(returnedLoadedAssemblies[i], Is.EqualTo(expectedLoadedAssemblies[i]));
        }
    }

    /// <summary>
    /// Tests that AssembliesToSearch returns all assemblies without null values.
    /// </summary>
    [Test]
    public void AssembliesToSearch_ReturnsNoNullAssemblies()
    {
        // Act
        var assemblies = Application.Assemblies().ToList();

        // Assert
        Assert.That(assemblies, Does.Not.Contain(null));
        Assert.That(assemblies.All(a => a != null), Is.True);
    }
}

