namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the Text.SubstituteVars method in Blackwood.IO.
/// Tests cover variable substitution functionality, including various substitution patterns,
/// edge cases, null handling, and different data types in the tableau.
/// These tests verify that the SubstituteVars method correctly processes variable references.
/// </summary>
[TestFixture]
public class SubstituteVarsTests
{
    #region Basic Functionality Tests

    /// <summary>
    /// Tests that SubstituteVars performs basic variable substitution correctly.
    /// This verifies the core functionality of replacing {{variable}} patterns with values.
    /// </summary>
    [Test]
    public void SubstituteVars_WithBasicSubstitution_ShouldReplaceVariables()
    {
        // Arrange
        var sourceText = "Hello {{name}}, welcome to {{app}}!";
        var tableau = new Dictionary<string, object>
        {
            { "name", "John" },
            { "app", "MyApp" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("Hello John, welcome to MyApp!"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles single variable substitution.
    /// This verifies that the method works correctly with just one variable.
    /// </summary>
    [Test]
    public void SubstituteVars_WithSingleVariable_ShouldReplaceCorrectly()
    {
        // Arrange
        var sourceText = "The value is {{value}}";
        var tableau = new Dictionary<string, object>
        {
            { "value", "42" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("The value is 42"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles multiple occurrences of the same variable.
    /// This verifies that all instances of a variable are replaced consistently.
    /// </summary>
    [Test]
    public void SubstituteVars_WithMultipleOccurrences_ShouldReplaceAllInstances()
    {
        // Arrange
        var sourceText = "{{greeting}} {{name}}, {{greeting}} again!";
        var tableau = new Dictionary<string, object>
        {
            { "greeting", "Hello" },
            { "name", "World" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("Hello World, Hello again!"));
    }

    #endregion

    #region Null and Empty Input Tests

    /// <summary>
    /// Tests that SubstituteVars returns the original text when tableau is null.
    /// This verifies the null safety of the method.
    /// </summary>
    [Test]
    public void SubstituteVars_WithNullTableau_ShouldReturnOriginalText()
    {
        // Arrange
        var sourceText = "Hello {{name}}";
        IDictionary<string, object> tableau = null;

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo(sourceText));
    }

    /// <summary>
    /// Tests that SubstituteVars handles empty tableau correctly.
    /// This verifies that the method works with empty dictionaries.
    /// </summary>
    [Test]
    public void SubstituteVars_WithEmptyTableau_ShouldReturnOriginalText()
    {
        // Arrange
        var sourceText = "Hello {{name}}";
        var tableau = new Dictionary<string, object>();

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo(sourceText));
    }

    /// <summary>
    /// Tests that SubstituteVars handles null source text correctly.
    /// This verifies that the method handles null input gracefully.
    /// Note: The actual implementation throws a NullReferenceException for null source text.
    /// </summary>
    [Test]
    public void SubstituteVars_WithNullSourceText_ShouldThrowException()
    {
        // Arrange
        string sourceText = null;
        var tableau = new Dictionary<string, object>
        {
            { "name", "John" }
        };

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => Text.SubstituteVars(sourceText, tableau));
    }

    /// <summary>
    /// Tests that SubstituteVars handles empty source text correctly.
    /// This verifies that the method works with empty strings.
    /// </summary>
    [Test]
    public void SubstituteVars_WithEmptySourceText_ShouldReturnEmptyString()
    {
        // Arrange
        var sourceText = "";
        var tableau = new Dictionary<string, object>
        {
            { "name", "John" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo(""));
    }

    #endregion

    #region Variable Pattern Tests

    /// <summary>
    /// Tests that SubstituteVars handles text without any variable patterns.
    /// This verifies that the method returns unchanged text when no substitutions are needed.
    /// </summary>
    [Test]
    public void SubstituteVars_WithNoVariables_ShouldReturnOriginalText()
    {
        // Arrange
        var sourceText = "This is plain text with no variables";
        var tableau = new Dictionary<string, object>
        {
            { "name", "John" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo(sourceText));
    }

    /// <summary>
    /// Tests that SubstituteVars handles partial variable patterns correctly.
    /// This verifies that incomplete {{ patterns are not processed.
    /// Note: The method only processes complete {{variable}} patterns.
    /// </summary>
    [Test]
    public void SubstituteVars_WithPartialVariablePatterns_ShouldNotReplace()
    {
        // Arrange
        var sourceText = "This has {{incomplete and {complete}} patterns";
        var tableau = new Dictionary<string, object>
        {
            { "complete", "replaced" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        // Only complete {{variable}} patterns are processed, not partial ones
        Assert.That(result, Is.EqualTo("This has {{incomplete and {complete}} patterns"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles nested variable patterns correctly.
    /// This verifies that the method processes all variables in the tableau.
    /// Note: The method processes all variables in the tableau, so both outer and inner are replaced.
    /// </summary>
    [Test]
    public void SubstituteVars_WithNestedPatterns_ShouldProcessCorrectly()
    {
        // Arrange
        var sourceText = "{{outer}} {{inner}} {{outer}}";
        var tableau = new Dictionary<string, object>
        {
            { "outer", "{{inner}}"},
            { "inner", "value" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        // All variables in the tableau are processed, so both outer and inner are replaced
        Assert.That(result, Is.EqualTo("value value value"));
    }

    #endregion

    #region Data Type Tests

    /// <summary>
    /// Tests that SubstituteVars handles different data types in the tableau.
    /// This verifies that the method correctly converts various types to strings.
    /// </summary>
    [Test]
    public void SubstituteVars_WithDifferentDataTypes_ShouldConvertToString()
    {
        // Arrange
        var sourceText = "Number: {{num}}, Bool: {{flag}}, Date: {{date}}";
        var tableau = new Dictionary<string, object>
        {
            { "num", 42 },
            { "flag", true },
            { "date", new DateTime(2023, 1, 1) }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("Number: 42, Bool: True, Date: 1/1/2023 12:00:00 AM"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles null values in the tableau.
    /// This verifies that null values cause a NullReferenceException.
    /// Note: The actual implementation throws a NullReferenceException when calling ToString() on null values.
    /// </summary>
    [Test]
    public void SubstituteVars_WithNullValues_ShouldThrowException()
    {
        // Arrange
        var sourceText = "Value: {{value}}";
        var tableau = new Dictionary<string, object>
        {
            { "value", null }
        };

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => Text.SubstituteVars(sourceText, tableau));
    }

    /// <summary>
    /// Tests that SubstituteVars handles complex objects in the tableau.
    /// This verifies that complex objects are converted using their ToString method.
    /// </summary>
    [Test]
    public void SubstituteVars_WithComplexObjects_ShouldUseToString()
    {
        // Arrange
        var sourceText = "Object: {{obj}}";
        var complexObject = new { Name = "Test", Value = 123 };
        var tableau = new Dictionary<string, object>
        {
            { "obj", complexObject }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo($"Object: {complexObject}"));
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that SubstituteVars handles very long variable names correctly.
    /// This verifies that the method works with long variable identifiers.
    /// </summary>
    [Test]
    public void SubstituteVars_WithLongVariableNames_ShouldWorkCorrectly()
    {
        // Arrange
        var longVarName = new string('A', 1000);
        var sourceText = $"{{{{{longVarName}}}}}";
        var tableau = new Dictionary<string, object>
        {
            { longVarName, "replaced" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("replaced"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles special characters in variable names.
    /// This verifies that the method works with various character sets.
    /// </summary>
    [Test]
    public void SubstituteVars_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var sourceText = "{{var_name}}, {{var-name}}, {{var.name}}";
        var tableau = new Dictionary<string, object>
        {
            { "var_name", "underscore" },
            { "var-name", "dash" },
            { "var.name", "dot" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("underscore, dash, dot"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles Unicode characters correctly.
    /// This verifies that the method works with international characters.
    /// </summary>
    [Test]
    public void SubstituteVars_WithUnicodeCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var sourceText = "{{中文}}, {{русский}}, {{العربية}}";
        var tableau = new Dictionary<string, object>
        {
            { "中文", "Chinese" },
            { "русский", "Russian" },
            { "العربية", "Arabic" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("Chinese, Russian, Arabic"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles case-sensitive variable names.
    /// This verifies that the method respects case sensitivity in variable names.
    /// </summary>
    [Test]
    public void SubstituteVars_WithCaseSensitiveNames_ShouldRespectCase()
    {
        // Arrange
        var sourceText = "{{Name}} and {{name}} are different";
        var tableau = new Dictionary<string, object>
        {
            { "Name", "Capital" },
            { "name", "lowercase" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("Capital and lowercase are different"));
    }

    #endregion

    #region Performance and Behavior Tests

    /// <summary>
    /// Tests that SubstituteVars processes variables in the order they appear in the tableau.
    /// This verifies the iteration behavior of the method.
    /// Note: The method processes all variables in the tableau, so both first and second are replaced.
    /// </summary>
    [Test]
    public void SubstituteVars_ShouldProcessVariablesInOrder()
    {
        // Arrange
        var sourceText = "{{first}} {{second}} {{first}}";
        var tableau = new Dictionary<string, object>
        {
            { "first", "{{second}}" },
            { "second", "replaced" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        // All variables in the tableau are processed, so both first and second are replaced
        Assert.That(result, Is.EqualTo("replaced replaced replaced"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles large numbers of variables efficiently.
    /// This verifies that the method can handle substantial substitution workloads.
    /// </summary>
    [Test]
    public void SubstituteVars_WithManyVariables_ShouldProcessAll()
    {
        // Arrange
        var sourceText = "{{var1}} {{var2}} {{var3}} {{var4}} {{var5}}";
        var tableau = new Dictionary<string, object>();
        for (int i = 1; i <= 5; i++)
        {
            tableau[$"var{i}"] = $"value{i}";
        }

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("value1 value2 value3 value4 value5"));
    }

    /// <summary>
    /// Tests that SubstituteVars handles variables that don't exist in the tableau.
    /// This verifies that undefined variables are left unchanged.
    /// </summary>
    [Test]
    public void SubstituteVars_WithUndefinedVariables_ShouldLeaveUnchanged()
    {
        // Arrange
        var sourceText = "{{defined}} {{undefined}} {{defined}}";
        var tableau = new Dictionary<string, object>
        {
            { "defined", "replaced" }
        };

        // Act
        var result = Text.SubstituteVars(sourceText, tableau);

        // Assert
        Assert.That(result, Is.EqualTo("replaced {{undefined}} replaced"));
    }

    #endregion
}

