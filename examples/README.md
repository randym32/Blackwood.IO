# Blackwood.IO Examples

This folder contains example projects demonstrating the features of Blackwood.IO.

## Example Projects

### MRUCacheExample
Demonstrates MRU (Most Recently Used) cache:
- Creating a cache with capacity limits
- Adding and retrieving items
- Automatic eviction of least recently used items

### TextProcessingExample
Demonstrates text processing utilities:
- Variable substitution in text templates
- Reading all lines from streams and files

### FileOperationsExample
Demonstrates file and folder operations:
- Using AppPaths and AppName for path management
- Using FolderWrapper for file operations
- Reading and writing files

### ZipOperationsExample
Demonstrates ZIP file operations:
- Creating ZIP files
- Using ZipWrapper to access files within ZIP archives
- Reading files from ZIP archives

### BackgroundSaveExample
Demonstrates background file saving:
- Non-blocking asynchronous file operations
- Background save operations

### RunCommandExample
Demonstrates running external commands:
- Running commands and streaming output
- Processing command output line by line

### EmbeddedResourcesExample
Demonstrates scanning assemblies for embedded resources:
- Loading resources from specific assemblies
- Scanning multiple assemblies to find resources
- Combining embedded resources with text processing
- Understanding resource naming conventions

## Building the Examples

### From Visual Studio
1. Open `Examples.sln` in Visual Studio
2. Select the example project you want to run
3. Build and run the project

### From Command Line
```bash
cd examples
dotnet build Examples.sln
dotnet run --project MRUCacheExample/MRUCacheExample.csproj
```

## Running Individual Examples

Each example can be run independently:

```bash
# Run MRUCacheExample
dotnet run --project MRUCacheExample/MRUCacheExample.csproj

# Run TextProcessingExample
dotnet run --project TextProcessingExample/TextProcessingExample.csproj

# Run FileOperationsExample
dotnet run --project FileOperationsExample/FileOperationsExample.csproj

# Run ZipOperationsExample
dotnet run --project ZipOperationsExample/ZipOperationsExample.csproj

# Run BackgroundSaveExample
dotnet run --project BackgroundSaveExample/BackgroundSaveExample.csproj

# Run RunCommandExample
dotnet run --project RunCommandExample/RunCommandExample.csproj

# Run EmbeddedResourcesExample
dotnet run --project EmbeddedResourcesExample/EmbeddedResourcesExample.csproj
```

## Requirements

- .NET 9.0 or later
- Blackwood.IO library (referenced as a project reference)

