# Getting Started with Blackwood.IO

This guide will help you get up and running with Blackwood.IO,
a .NET library that provides utilities for file operations,
caching, and system integration.

## Prerequisites

- **.NET 8.0 or later** - The library targets .NET 8.0+
- **Visual Studio 2022** (Windows) or **VS Code** with C# extension (cross-platform)

## Installation

### Using Package Manager Console

```powershell
Install-Package Blackwood.IO
```

### Using .NET CLI

```bash
dotnet add package Blackwood.IO
```

### Using PackageReference

Add the following to your `.csproj` file:

```xml
<PackageReference Include="Blackwood.IO" Version="2.0.0" />
```

## Quick Start

### 1. Basic File Operations

```csharp
using Blackwood;

// Get application paths
string dataPath = FS.AppDataPath; // Path to application data directory

// Create a folder wrapper for file operations
var folder = new FolderWrapper(dataPath);

// Write a file with atomic operation
string content = "Hello, World!";
folder.WriteAllText("greeting.txt", content);

// Read the file back
string readContent = folder.ReadAllText("greeting.txt");
Console.WriteLine(readContent); // Output: Hello, World!
```

### 2. Caching with MRU Cache

```csharp
using Blackwood;

// Create an MRU cache with a maximum of 100 items
var cache = new MRUCache<string, string>(100);

// Add items to cache
cache["user1"] = "John Doe";
cache["user2"] = "Jane Smith";

// Retrieve items
string user1 = cache["user1"]; // Returns "John Doe"

// The cache automatically manages memory by removing least recently used items
```

### 3. Path Management

Leverage the built-in path management to
use standard paths instead of hardcoded ones for

- **`Application.Name`**: Get application name from assembly
- **`FS.AppDataPath`**: Get path to application data directory
- **`FS.ExeFilePath`**: Get path to executable directory
- **`FS.AssemblyDirectory`**: Get path of the executing assembly

```csharp
using Blackwood;

// Get application name and paths
string applicationName = Application.Name;
string dataPath = FS.AppDataPath;
string exePath = FS.ExeFilePath;

Console.WriteLine($"Application: {applicationName}");
Console.WriteLine($"Data Path: {dataPath}");
Console.WriteLine($"Executable Path: {exePath}");
```


### 4. Text Processing

- **`SubstituteVars`**: Variable substitution in text templates
- **`ReadAllLines`**: Enhanced text file reading with encoding support


```csharp
using Blackwood;

// Substitute variables in text
string template = "Hello, {{name}}! Your score is {{score}}.";
var variables = new Dictionary<string, object>
{
    ["name"] = "Alice",
    ["score"] = 95
};

string result = Text.SubstituteVars(template, variables);
Console.WriteLine(result); // Output: Hello, Alice! Your score is 95.
```

## Core Components

### File Operations

- **`FolderWrapper`**: Provides atomic file operations and background processing
- **`IFolderWrapper`**: Interface for folder operations (implemented by `FolderWrapper` and `ZipWrapper`)
- **`ZipWrapper`**: Access files within ZIP archives

### Caching

- **`MRUCache<TKey, TValue>`**: Most Recently Used cache with configurable capacity
- **`CacheItem<T>`**: Individual cache items with key-value pairs


## Common Use Cases

### Application Configuration

```csharp
using Blackwood;

public class AppConfig
{
    private readonly FolderWrapper _configFolder;

    public AppConfig()
    {
        // Use FS.AppDataPath to get the application data directory
        string configPath = FS.AppDataPath;
        _configFolder = new FolderWrapper(configPath);
    }

    public void SaveConfig(string configJson)
    {
        _configFolder.WriteAllText("appsettings.json", configJson);
    }

    public string LoadConfig()
    {
        return _configFolder.ReadAllText("appsettings.json");
    }
}
```

### Data Caching

```csharp
using Blackwood;

public class DataService
{
    private readonly MRUCache<string, object> _cache;

    public DataService()
    {
        _cache = new MRUCache<string, object>(1000);
    }

    public async Task<T> GetDataAsync<T>(string key, Func<Task<T>> dataLoader)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return (T)cachedValue;
        }

        var data = await dataLoader();
        _cache[key] = data;
        return data;
    }
}
```

## Next Steps

- [API Reference](../api/index.md) - Explore the complete API documentation

