using Blackwood;
using System.Collections.Generic;
using System.IO;

namespace TextProcessingExample;

/// <summary>
/// Example program demonstrating text processing utilities in Blackwood.IO.
/// This example shows how to use variable substitution and reading text from streams.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Text Processing Example");
        Console.WriteLine("=======================");
        Console.WriteLine();

        // ====================================================================
        // Example 1: Variable Substitution
        // ====================================================================
        // The Text.SubstituteVars method allows you to replace placeholders
        // in a template string with actual values from a dictionary.
        // Placeholders use double curly braces: {{variableName}}
        Console.WriteLine("1. Variable Substitution:");

        // Define a template string with placeholders
        // Placeholders are marked with double curly braces: {{name}}, {{score}}, etc.
        string template = "Hello {{name}}! Your score is {{score}} and you are {{status}}.";

        // Create a dictionary containing the variable values
        // The keys must match the placeholder names (without the curly braces)
        var variables = new Dictionary<string, object>
        {
            ["name"] = "Alice",      // Will replace {{name}}
            ["score"] = 95,          // Will replace {{score}}
            ["status"] = "excellent"  // Will replace {{status}}
        };

        // Call Text.SubstituteVars to replace all placeholders with their values
        // This method iterates through the dictionary and replaces each {{key}} with its value
        string result = Text.SubstituteVars(template, variables);

        Console.WriteLine($"Template: {template}");
        Console.WriteLine($"Result: {result}");
        Console.WriteLine();

        // ====================================================================
        // Example 2: Reading All Lines from a Memory Stream
        // ====================================================================
        // The Text.ReadAllLines method reads all text from a stream and returns
        // it as a single string with line breaks preserved.
        Console.WriteLine("2. Reading All Lines from Stream:");

        // Create sample content with multiple lines
        // \n represents a newline character
        string testContent = "Line 1\nLine 2\nLine 3\nLine 4";

        // Convert the string to bytes using UTF-8 encoding
        // This simulates reading from a file or network stream
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent));

        // Read all lines from the stream
        // Text.ReadAllLines reads the entire stream and returns it as a string
        // It automatically handles UTF-8 encoding and preserves line breaks
        string allLines = Text.ReadAllLines(stream);

        Console.WriteLine($"Content from stream:\n{allLines}");
        Console.WriteLine();

        // ====================================================================
        // Example 3: Reading All Lines from a File
        // ====================================================================
        // This example demonstrates reading text from an actual file on disk.
        // Text.ReadAllLines works with any Stream, including FileStream.
        Console.WriteLine("3. Reading from File:");

        string testFile = "test.txt";
        try
        {
            // Create a test file with some content
            // Each \n creates a new line in the file
            File.WriteAllText(testFile, "First line\nSecond line\nThird line");

            // Open the file for reading
            // File.OpenRead returns a FileStream that can be used with Text.ReadAllLines
            using var fileStream = File.OpenRead(testFile);

            // Read all lines from the file stream
            // This reads the entire file content into a single string
            string fileContent = Text.ReadAllLines(fileStream);

            Console.WriteLine($"Content from file:\n{fileContent}");

            // Clean up: delete the temporary test file
            File.Delete(testFile);
        }
        catch (Exception ex)
        {
            // Handle any errors that might occur (e.g., file permissions, disk full)
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

