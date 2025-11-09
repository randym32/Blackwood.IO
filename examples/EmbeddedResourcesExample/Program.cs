using Blackwood;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace EmbeddedResourcesExample;

/// <summary>
/// Example program demonstrating how to scan assemblies for embedded resources
/// and load them using Blackwood.IO.
///
/// This example shows:
/// 1. How to create an EmbeddedResources instance for a specific assembly
/// 2. How to check if a resource exists
/// 3. How to load a resource from a stream
/// 4. How to scan multiple assemblies to find a resource
/// 5. How to combine embedded resources with text processing
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Embedded Resources Example");
        Console.WriteLine("===========================");
        Console.WriteLine();

        // ====================================================================
        // Example 1: Loading a Resource from a Specific Assembly
        // ====================================================================
        // The EmbeddedResources class allows you to access embedded resources
        // from a specific assembly. Resources are embedded at build time and
        // become part of the assembly.
        Console.WriteLine("1. Loading Resource from Specific Assembly:");
        Console.WriteLine("-------------------------------------------");

        // Get the current executing assembly
        // In a real application, you might want to use a specific assembly
        var assembly = Assembly.GetExecutingAssembly();
        Console.WriteLine($"Searching in assembly: {assembly.GetName().Name}");

        // Create an EmbeddedResources instance for this assembly
        // This will search for resources embedded in the assembly
        var embeddedResources = new EmbeddedResources(assembly);

        // Example resource paths (these would need to be embedded in the project)
        // To embed a resource, add it to your .csproj file:
        // <ItemGroup>
        //   <EmbeddedResource Include="Resources\config.txt" />
        // </ItemGroup>
        string[] exampleResources = {
            "Resources/config.txt",
            "Templates/welcome.txt",
            "Data/default.json"
        };

        // Try to find and load each resource
        foreach (string resourcePath in exampleResources)
        {
            // Check if the resource exists in the assembly
            if (embeddedResources.Exists(resourcePath))
            {
                Console.WriteLine($"  Found: {resourcePath}");

                // Get a stream for the resource
                using var stream = embeddedResources.Stream(resourcePath);
                if (stream != null)
                {
                    // Read the resource content
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    string content = reader.ReadToEnd();
                    Console.WriteLine($"  Content: {content.Substring(0, Math.Min(50, content.Length))}...");
                }
            }
            else
            {
                Console.WriteLine($"  Not found: {resourcePath}");
            }
        }
        Console.WriteLine();

        // ====================================================================
        // Example 2: Scanning Multiple Assemblies
        // ====================================================================
        // Application.Assemblies() returns assemblies in priority order:
        // 1. Entry assembly (main application)
        // 2. Executing assembly (where code is running)
        // 3. All loaded assemblies (most recent first)
        // This allows you to search for a resource across multiple assemblies.
        Console.WriteLine("2. Scanning Multiple Assemblies:");
        Console.WriteLine("--------------------------------");

        string resourceToFind = "Resources/config.txt";
        Stream? foundResource = null;
        Assembly? foundAssembly = null;

        // Iterate through all available assemblies in priority order
        foreach (Assembly asm in Application.Assemblies())
        {
            Console.WriteLine($"  Checking assembly: {asm.GetName().Name}");

            // Create an EmbeddedResources instance for this assembly
            var resources = new EmbeddedResources(asm);

            // Check if the resource exists in this assembly
            if (resources.Exists(resourceToFind))
            {
                Console.WriteLine($"    ✓ Found resource: {resourceToFind}");

                // Get the resource stream
                foundResource = resources.Stream(resourceToFind);
                if (foundResource != null)
                {
                    foundAssembly = asm;
                    break; // Found it, stop searching
                }
            }
            else
            {
                Console.WriteLine($"    ✗ Resource not found");
            }
        }

        // Use the found resource
        if (foundResource != null && foundAssembly != null)
        {
            Console.WriteLine($"  Using resource from: {foundAssembly.GetName().Name}");
            // Note: In a real application, you would read and use the resource here
            foundResource.Dispose();
        }
        else
        {
            Console.WriteLine($"  Resource '{resourceToFind}' not found in any assembly");
        }
        Console.WriteLine();

        // ====================================================================
        // Example 3: Combining with Text Processing
        // ====================================================================
        // You can combine embedded resources with text processing utilities
        // to load templates and substitute variables.
        Console.WriteLine("3. Combining with Text Processing:");
        Console.WriteLine("-----------------------------------");

        // Example: Load an embedded template and substitute variables
        var currentAssembly = Assembly.GetExecutingAssembly();
        var templateResources = new EmbeddedResources(currentAssembly);

        string templatePath = "Templates/welcome.txt";

        if (templateResources.Exists(templatePath))
        {
            using var stream = templateResources.Stream(templatePath);
            if (stream != null)
            {
                // Read the template using Text.ReadAllLines
                // This preserves line breaks and handles encoding
                string template = Text.ReadAllLines(stream);
                Console.WriteLine($"Template loaded:");
                Console.WriteLine(template);
                Console.WriteLine();

                // Substitute variables in the template
                var variables = new Dictionary<string, object>
                {
                    ["name"] = "Alice",
                    ["version"] = "1.0.0",
                    ["date"] = DateTime.Now.ToString("yyyy-MM-dd")
                };

                string result = Text.SubstituteVars(template, variables);
                Console.WriteLine("After variable substitution:");
                Console.WriteLine(result);
            }
        }
        else
        {
            Console.WriteLine($"Template '{templatePath}' not found");
            Console.WriteLine("To use this example, embed a template file in your project.");
        }
        Console.WriteLine();

        // ====================================================================
        // Example 4: Resource Naming Convention
        // ====================================================================
        // Embedded resources follow a specific naming convention:
        // {AssemblyName}.{FolderPath}.{FileName}
        // Path separators (/, \) are converted to dots (.)
        Console.WriteLine("4. Resource Naming Convention:");
        Console.WriteLine("-------------------------------");
        Console.WriteLine("Embedded resources are named using the pattern:");
        Console.WriteLine("  {AssemblyName}.{FolderPath}.{FileName}");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine($"  Assembly: {assembly.GetName().Name}");
        Console.WriteLine($"  Path: Resources/config.txt");
        Console.WriteLine($"  Resource Name: {assembly.GetName().Name}.Resources.config.txt");
        Console.WriteLine();
        Console.WriteLine($"  Path: Templates/welcome.txt");
        Console.WriteLine($"  Resource Name: {assembly.GetName().Name}.Templates.welcome.txt");
        Console.WriteLine();
        Console.WriteLine("Note: Path separators (/, \\) are converted to dots (.)");
        Console.WriteLine();

        // ====================================================================
        // Example 5: Listing All Resources in an Assembly
        // ====================================================================
        // You can enumerate all manifest resources in an assembly
        Console.WriteLine("5. Listing All Resources in Assembly:");
        Console.WriteLine("-------------------------------------");

        var assemblyResources = assembly.GetManifestResourceNames();
        if (assemblyResources.Length > 0)
        {
            Console.WriteLine($"Found {assemblyResources.Length} embedded resource(s):");
            foreach (string resourceName in assemblyResources)
            {
                Console.WriteLine($"  - {resourceName}");
            }
        }
        else
        {
            Console.WriteLine("No embedded resources found in this assembly.");
            Console.WriteLine("To embed resources, add them to your .csproj file:");
            Console.WriteLine("  <ItemGroup>");
            Console.WriteLine("    <EmbeddedResource Include=\"Resources\\config.txt\" />");
            Console.WriteLine("  </ItemGroup>");
        }
        Console.WriteLine();
    }
}

