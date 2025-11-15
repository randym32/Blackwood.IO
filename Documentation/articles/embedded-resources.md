# Working with Embedded Resources

Embedded resources are like that friend who always has everything you need in
their backpack, except this friend is compiled into your executable and can't
forget things at home.  Embedded resources are files that are compiled directly
into your assembly, making them part of your application's executable.  Use them
for including configuration files, templates, default data, or other assets that
should always be available with your application.


## Overview

Blackwood.IO provides utilities to load embedded resources.
The library includes:


- **`EmbeddedResources`**: A class for accessing resources embedded in a specific assembly

## How Embedded Resources Work

The resources embedded in a assembly follow a specificname follows a specific pattern:

```
{AssemblyName}.{FolderPath}.{FileName}
```

For example, if your assembly is named `MyApp` and you embed a file at
`Resources/config.json`, the resource name would be:
```
MyApp.Resources.config.json
```

### Resource Naming

When embedding resources, keep in mind:

- **Path Separators**: Use forward slashes (`/`) or backslashes (`\`) in the
  path - they will be converted to dots (`.`) in the resource name
- **Assembly Name**: The resource name always starts with the assembly name
- **Case Sensitivity**: Resource names are case-sensitive

Example resource paths:
- `Resources/config.json` → `MyApp.Resources.config.json`
- `Data\default.txt` → `MyApp.Data.default.txt`
- `config.json` → `MyApp.config.json`

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


## Compressed Resources

`EmbeddedResources` automatically handles compressed resources. If a resource is
stored as a `.gz` file, it is decompressed transparently when accessed. This
allows you to reduce the size of your assembly while maintaining easy access to
resources at runtime.

### How Compression Works

The `EmbeddedResources` class first tries to find a compressed version of a
resource (with a `.gz` extension), and if not found, falls back to the
uncompressed version. This means you can use the same code to access both
compressed and uncompressed resources - no code changes are needed.

### Benefits of Compression

- **Smaller Assembly Size**: Compressed resources can significantly reduce the
  size of your assembly, especially for text-based resources like JSON, XML, or
  templates
- **Faster Deployment**: Smaller assemblies deploy faster
- **Transparent Access**: No code changes needed - `EmbeddedResources` handles
  decompression automatically
- **Backward Compatible**: If a compressed version isn't found, the class falls
  back to the uncompressed version

### Resource Naming for Compressed Resources

When embedding compressed resources, the resource name follows this pattern:

```
{AssemblyName}.{Path}.{FileName}.gz
```

For example:
- `Resources/config.json` → `MyApp.Resources.config.json.gz`
- `Data\default.txt` → `MyApp.Data.default.txt.gz`

The `EmbeddedResources` class looks for the `.gz` version first, then falls
back to the uncompressed version if not found.

### Setting Up Compression During Build

To compress resources during build, you can configure MSBuild to compress files
before embedding them. This is done by adding a custom MSBuild target to your
`.csproj` file.

Here's a complete example of how to set up resource compression:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Your project properties -->

  <!-- Resources to be compressed during build -->
  <ItemGroup>
    <ResourceToCompress Include="Resources\sample.txt" />
    <ResourceToCompress Include="Resources\config.json" />
  </ItemGroup>

  <!-- Target to compress resources before embedding -->
  <Target Name="CompressResources" BeforeTargets="EmbeddedResource">
    <Message Text="Compressing resources for embedding..." Importance="normal" />

    <ItemGroup>
      <_CompressedResources Include="@(ResourceToCompress->'%(Identity).gz')">
        <OriginalFile>%(Identity)</OriginalFile>
      </_CompressedResources>
    </ItemGroup>

    <!-- Compress each resource file using GZip -->
    <CompressResourceFile
      SourceFiles="@(ResourceToCompress)"
      DestinationFiles="@(_CompressedResources)" />
  </Target>

  <!-- Custom task to compress files using GZip -->
  <UsingTask TaskName="CompressResourceFile" TaskFactory="RoslynCodeTaskFactory" AssemblyFile="$(MSBuildToolsPath)\Microsoft.Build.Tasks.Core.dll">
    <ParameterGroup>
      <SourceFiles ParameterType="Microsoft.Build.Framework.ITaskItem[]" Required="true" />
      <DestinationFiles ParameterType="Microsoft.Build.Framework.ITaskItem[]" Required="true" />
    </ParameterGroup>
    <Task>
      <Using Namespace="System" />
      <Using Namespace="System.IO" />
      <Using Namespace="System.IO.Compression" />
      <Code Type="Fragment" Language="cs">
        <![CDATA[
        for (int i = 0; i < SourceFiles.Length; i++)
        {
          var sourcePath = SourceFiles[i].GetMetadata("FullPath");
          var destPath = DestinationFiles[i].GetMetadata("FullPath");

          using (var inputStream = File.OpenRead(sourcePath))
          using (var outputStream = File.Create(destPath))
          using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
          {
            inputStream.CopyTo(gzipStream);
          }
        }
        ]]>
      </Code>
    </Task>
  </UsingTask>

  <!-- Embed the compressed resources -->
  <!-- Note: LogicalName must match the pattern: AssemblyName.Path.FileName.gz -->
  <!-- Path separators (/, \) need to be replaced with dots -->
  <ItemGroup>
    <_CompressedResourceFiles Include="@(ResourceToCompress->'%(Identity).gz')" />
  </ItemGroup>

  <!-- Set LogicalName correctly with path separators replaced by dots -->
  <Target Name="SetCompressedResourceLogicalNames" BeforeTargets="EmbeddedResource">
    <ItemGroup>
      <EmbeddedResource Include="@(_CompressedResourceFiles)">
        <LogicalName>$(AssemblyName).$([System.String]::Copy('%(Identity)').Replace('\', '.').Replace('/', '.'))</LogicalName>
      </EmbeddedResource>
    </ItemGroup>
  </Target>
</Project>
```

### Using Compressed Resources

Once you've configured compression during build, using compressed resources is
transparent - no code changes are needed. The `EmbeddedResources` class
automatically detects and decompresses `.gz` resources:

```csharp
using Blackwood;
using System.IO;
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();
var embeddedResources = new EmbeddedResources(assembly);

// This will automatically decompress if the resource is stored as .gz
if (embeddedResources.Exists("Resources/config.json"))
{
    using var stream = embeddedResources.Stream("Resources/config.json");
    if (stream != null)
    {
        // The stream contains the decompressed content
        using var reader = new StreamReader(stream);
        string content = reader.ReadToEnd();
        Console.WriteLine(content);
    }
}
```

## Best Practices

1. **Organize Resources**: Use folders to organize your embedded resources (e.g.,
   `Resources/`, `Templates/`, `Data/`)

2. **Error Handling**: Always check if a resource exists before trying to access
   it, and handle null streams

3. **Resource Cleanup**: Use `using` statements to ensure streams are properly
   disposed

4. **Resource Size**: For large resources, consider using compression (`.gz`)
   to reduce assembly size. See the [Compressed Resources](#compressed-resources)
   section for details on setting up compression during build

## Common Use Cases

- **Configuration Files**: Embed default configuration that can be overridden by user settings
- **Templates**: Include text or HTML templates that can be customized with variable substitution
- **Default Data**: Provide default datasets or lookup tables
- **Localization**: Embed translation files for different languages
- **Icons and Images**: Include small icons or images directly in the assembly

## Next Steps

- [API Reference](../api/index.md) - API documentation
- [Examples](../articles/examples.md) - Examples of using embedded resources
- See the `CompressedResourcesExample` project in the examples folder for a
  complete working example of compressing resources during build

