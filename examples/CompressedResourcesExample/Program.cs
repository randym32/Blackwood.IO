using Blackwood;
using System.IO;
using System.Reflection;
using System.Text;

namespace CompressedResourcesExample;

/// <summary>
/// Example program demonstrating how Visual Studio compresses resources during build
/// and how to read them using Blackwood.IO's EmbeddedResources class.
///
/// This example shows:
/// 1. How to configure MSBuild to compress resources during build
/// 2. How EmbeddedResources automatically detects and decompresses .gz resources
/// 3. How to read compressed embedded resources transparently
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Compressed Resources Example");
        Console.WriteLine("=============================");
        Console.WriteLine();
        Console.WriteLine("This example demonstrates how Visual Studio can compress");
        Console.WriteLine("resources during build, and how Blackwood.IO automatically");
        Console.WriteLine("decompresses them at runtime.");
        Console.WriteLine();

        // Get the current assembly
        var assembly = Assembly.GetExecutingAssembly();
        Console.WriteLine($"Assembly: {assembly.GetName().Name}");
        Console.WriteLine();

        // Create an EmbeddedResources instance
        // This will automatically handle compressed resources (files with .gz extension)
        var embeddedResources = new EmbeddedResources(assembly);

        // ====================================================================
        // Example 1: Reading a Compressed Text File
        // ====================================================================
        Console.WriteLine("1. Reading Compressed Text File:");
        Console.WriteLine("---------------------------------");

        string textResourcePath = "Resources/sample.txt";

        if (embeddedResources.Exists(textResourcePath))
        {
            Console.WriteLine($"  Found: {textResourcePath}");

            using var stream = embeddedResources.Stream(textResourcePath);
            if (stream != null)
            {
                // Read the decompressed content
                using var reader = new StreamReader(stream, Encoding.UTF8);
                string content = reader.ReadToEnd();

                Console.WriteLine($"  Content length: {content.Length} characters");
                Console.WriteLine($"  First 100 characters:");
                Console.WriteLine($"  {content.Substring(0, Math.Min(100, content.Length))}...");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine($"  Not found: {textResourcePath}");
            Console.WriteLine("  Note: Make sure the resource is embedded and compressed during build.");
            Console.WriteLine();
        }

        // ====================================================================
        // Example 2: Reading a Compressed JSON File
        // ====================================================================
        Console.WriteLine("2. Reading Compressed JSON File:");
        Console.WriteLine("---------------------------------");

        string jsonResourcePath = "Resources/config.json";

        if (embeddedResources.Exists(jsonResourcePath))
        {
            Console.WriteLine($"  Found: {jsonResourcePath}");

            using var stream = embeddedResources.Stream(jsonResourcePath);
            if (stream != null)
            {
                // Read the decompressed JSON content
                using var reader = new StreamReader(stream, Encoding.UTF8);
                string jsonContent = reader.ReadToEnd();

                Console.WriteLine($"  JSON content length: {jsonContent.Length} characters");
                Console.WriteLine($"  JSON content:");
                Console.WriteLine(jsonContent);
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine($"  Not found: {jsonResourcePath}");
            Console.WriteLine();
        }

        // ====================================================================
        // Example 3: How Compression Works
        // ====================================================================
        Console.WriteLine("3. How Compression Works:");
        Console.WriteLine("-------------------------");
        Console.WriteLine("During build:");
        Console.WriteLine("  1. MSBuild compresses resource files using GZip");
        Console.WriteLine("  2. Compressed files (.gz) are embedded in the assembly");
        Console.WriteLine("  3. Original uncompressed files are NOT embedded");
        Console.WriteLine();
        Console.WriteLine("At runtime:");
        Console.WriteLine("  1. EmbeddedResources looks for resource with .gz extension");
        Console.WriteLine("  2. If found, automatically decompresses using GZipStream");
        Console.WriteLine("  3. Returns decompressed content transparently");
        Console.WriteLine("  4. If .gz version not found, falls back to uncompressed");
        Console.WriteLine();

        // ====================================================================
        // Example 4: Listing All Embedded Resources
        // ====================================================================
        Console.WriteLine("4. Listing All Embedded Resources:");
        Console.WriteLine("-----------------------------------");

        var resourceNames = assembly.GetManifestResourceNames();
        if (resourceNames.Length > 0)
        {
            Console.WriteLine($"Found {resourceNames.Length} embedded resource(s):");
            foreach (string resourceName in resourceNames)
            {
                Console.WriteLine($"  - {resourceName}");
                if (resourceName.EndsWith(".gz"))
                {
                    Console.WriteLine($"    (compressed)");
                }
            }
        }
        else
        {
            Console.WriteLine("No embedded resources found.");
            Console.WriteLine();
            Console.WriteLine("To embed compressed resources:");
            Console.WriteLine("  1. Add resource files to Resources/ folder");
            Console.WriteLine("  2. Configure .csproj to compress during build");
            Console.WriteLine("  3. Embed the compressed .gz files");
        }
        Console.WriteLine();

        // ====================================================================
        // Example 5: Benefits of Compression
        // ====================================================================
        Console.WriteLine("5. Benefits of Compression:");
        Console.WriteLine("---------------------------");
        Console.WriteLine("  • Smaller assembly size");
        Console.WriteLine("  • Faster deployment");
        Console.WriteLine("  • Transparent decompression at runtime");
        Console.WriteLine("  • No code changes needed - EmbeddedResources handles it");
        Console.WriteLine();
    }
}

