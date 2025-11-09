# Working with Embedded Resources

Embedded resources are like that friend who always has everything you need in
their backpack, except this friend is compiled into your executable and can't
forget things at home.  Embedded resources are files that are compiled directly
into your assembly, making them part of your application's executable.  Use them
for including configuration files, templates, default data, or other assets that
should always be available with your application.


## Overview

Blackwood.IO provides utilities to scan assemblies and load embedded resources.
The library includes:

- **`Application.Assemblies()`**: Returns a list of assemblies to search.
- **`EmbeddedResources`**: A class that implements `IFolderWrapper` for accessing
  embedded resources from a specific assembly

## How Embedded Resources Work

When you embed a file in your assembly, it becomes a manifest resource.  The
resource name follows a specific pattern:

```
{AssemblyName}.{FolderPath}.{FileName}
```

For example, if your assembly is named `MyApp` and you embed a file at
`Resources/config.json`, the resource name would be:
```
MyApp.Resources.config.json
```

## Scanning Assemblies

The `Application.Assemblies()` method returns assemblies in a specific order:

1. **Entry Assembly** (if available) - Your main application assembly
2. **Executing Assembly** - The assembly where the code is running
3. **All Loaded Assemblies** - Other assemblies loaded in the application
   domain, in reverse order (most recent first)

This ordering ensures that resources in your main application are found before
resources in referenced libraries.

## Loading Embedded Resources

### Basic Usage

To load an embedded resource from a specific assembly:

```csharp
using Blackwood;
using System.IO;
using System.Reflection;

// Create an EmbeddedResources instance for a specific assembly
var assembly = Assembly.GetExecutingAssembly();
var embeddedResources = new EmbeddedResources(assembly);

// Check if a resource exists
if (embeddedResources.Exists("Resources/config.json"))
{
    // Get a stream for the resource
    using var stream = embeddedResources.Stream("Resources/config.json");
    if (stream != null)
    {
        // Read the resource content
        using var reader = new StreamReader(stream);
        string content = reader.ReadToEnd();
        Console.WriteLine(content);
    }
}
```

You can also create an `EmbeddedResources` instance without specifying
an assembly. In this case, it uses the calling assembly (the assembly
where the code is executed):

```csharp
// Uses the calling assembly automatically
var embeddedResources = new EmbeddedResources();

// Check and load a resource
if (embeddedResources.Exists("Data/default.txt"))
{
    using var stream = embeddedResources.Stream("Data/default.txt");
    if (stream != null)
    {
        using var reader = new StreamReader(stream);
        string content = reader.ReadToEnd();
        // Process the content...
    }
}
```

The `Stream` method returns a `Stream` that you can use to read the
resource data. The stream is positioned at the beginning, so you can
read from it immediately. Remember to dispose of the stream when you're
done with it, either by using a `using` statement or by calling `Dispose()`
explicitly.

### Scanning Multiple Assemblies

The `Application.Assemblies()` method returns assemblies in priority order, so
resources in your main application are checked before resources in referenced
libraries. This means you can override default resources from libraries by
embedding your own version in your main application assembly.


To search for a single resource across multiple assemblies:

```csharp
using Blackwood;
using System.IO;
using System.Reflection;

// The path to the resource you're searching for in each assembly
string resourcePath = "Resources/config.json";
Stream? resourceStream = null;

// Iterate over all assemblies returned by Application.Assemblies()
// Assemblies are searched in priority order (entry, executing, then others)
foreach (Assembly assembly in Application.Assemblies())
{
    // Create an EmbeddedResources instance for the current assembly
    var embeddedResources = new EmbeddedResources(assembly);

    // Check if the resource exists in this assembly
    if (embeddedResources.Exists(resourcePath))
    {
        // If found, get a stream for the resource
        resourceStream = embeddedResources.Stream(resourcePath);
        if (resourceStream != null)
        {
            // Log which assembly contained the resource
            Console.WriteLine($"Found resource in assembly: {assembly.GetName().Name}");
            break; // Stop searching after first match
        }
    }
}

// If the resource stream was found, use it
if (resourceStream != null)
{
    // Use a StreamReader to read the contents of the resource
    using var reader = new StreamReader(resourceStream);

    // Print the content of the resource if found, and indicate which assembly provided it.
    string content = reader.ReadToEnd();
    Console.WriteLine(content);
}
```


If you need to search through all assemblies without stopping at the
first match, you can collect all matching resources:

```csharp
// Collect all instances of a resource across all assemblies

// The relative path to the embedded resource you want to find
string resourcePath = "Resources/config.json";

// This list will hold tuples of (Assembly, Stream) for each match found
var foundResources = new List<(Assembly assembly, Stream stream)>();

// Iterate over every assembly returned by Application.Assemblies()
foreach (Assembly assembly in Application.Assemblies())
{
    // Create an EmbeddedResources wrapper for this assembly
    var embeddedResources = new EmbeddedResources(assembly);

    // Check if the resource exists in this assembly
    if (embeddedResources.Exists(resourcePath))
    {
        // Get a stream to the resource (if found)
        var stream = embeddedResources.Stream(resourcePath);

        // Only add to the list if the stream isn't null
        if (stream != null)
        {
            foundResources.Add((assembly, stream));
        }
    }
}

// Process all found resources
foreach (var (assembly, stream) in foundResources)
{
    // Log the assembly name that contains the resource
    Console.WriteLine($"Found in: {assembly.GetName().Name}");

    // TODO: Add your logic here to process the resource stream (e.g., read its contents)

    // Always dispose streams when done to free resources
    stream.Dispose();
}
```

### Using with Text Processing

You can combine embedded resources with text processing utilities:

```csharp
using Blackwood;
using System.IO;
using System.Reflection;

// Load an embedded template
var assembly = Assembly.GetExecutingAssembly();
var embeddedResources = new EmbeddedResources(assembly);

using var stream = embeddedResources.Stream("Templates/welcome.txt");
if (stream != null)
{
    // Read the template
    string template = Text.ReadAllLines(stream);

    // Substitute variables
    var variables = new Dictionary<string, object>
    {
        ["name"] = "Alice",
        ["version"] = "1.0.0"
    };

    string result = Text.SubstituteVars(template, variables);
    Console.WriteLine(result);
}
```

## Resource Naming

When embedding resources, keep in mind:

- **Path Separators**: Use forward slashes (`/`) or backslashes (`\`) in the
  path - they will be converted to dots (`.`) in the resource name
- **Assembly Name**: The resource name always starts with the assembly name
- **Case Sensitivity**: Resource names are case-sensitive

Example resource paths:
- `Resources/config.json` → `MyApp.Resources.config.json`
- `Data\default.txt` → `MyApp.Data.default.txt`
- `config.json` → `MyApp.config.json`

## Compressed Resources

`EmbeddedResources` handles compressed resources. If a resource is stored as a
`.gz` file, it is decompressed when accessed.

The class first tries to find a compressed version (`.gz`), and if not found,
falls back to the uncompressed version.

## Best Practices

1. **Organize Resources**: Use folders to organize your embedded resources (e.g.,
   `Resources/`, `Templates/`, `Data/`)

2. **Error Handling**: Always check if a resource exists before trying to access
   it, and handle null streams

3. **Resource Cleanup**: Use `using` statements to ensure streams are properly
   disposed

4. **Assembly Selection**: Use `Application.Assemblies()` to search in the
   correct order, or specify a specific assembly if you know where the resource
   is located

5. **Resource Size**: For large resources, consider using compression (`.gz`)
   to reduce assembly size

## Common Use Cases

- **Configuration Files**: Embed default configuration that can be overridden by user settings
- **Templates**: Include text or HTML templates that can be customized with variable substitution
- **Default Data**: Provide default datasets or lookup tables
- **Localization**: Embed translation files for different languages
- **Icons and Images**: Include small icons or images directly in the assembly

## Next Steps

- [API Reference](../api/index.md) - API documentation
- [Examples](../articles/examples.md) - Examples of using embedded resources

