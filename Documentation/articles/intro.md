# Introduction to Blackwood.IO

**Blackwood.IO** is a .NET library providing utilities for file operations,
caching, and lite text processing:


- **File Operations**: File handling with atomic operations and background processing
- **Caching**: In-memory caching with LRU (Least Recently Used) eviction policies
- **Path Management**: Path resolution and manipulation utilities
- **Resource Management**: Embedded resource handling and ZIP file operations
- **Text Processing**: Variable substitution and text manipulation


## Getting Started

### Installation

Blackwood.IO is available as a NuGet package:

```bash
dotnet add package Blackwood.IO
```

### Basic Usage

#### Application Information

Here's how to get information about your application and its environment using Blackwood.IO:


```csharp
using Blackwood;
using System.Reflection;

// Path to application data files
string path = Application.AppDataPath;
```

#### File Operations

Saving files asynchronously in the background is helpful for scenarios like
autosaving, storing user preferences, logging, and similar tasks.

```csharp
using Blackwood;

// Background file save (non-blocking)
Util.Save("path/to/file.txt", stream => {
    // Write your data to the stream
    stream.Write(data);
});
```

#### Caching

Caches are used to reduce loading of frequently used remote resources (and even
local files).

```csharp
using Blackwood;

// Create a cache with capacity limit
var cache = new Cache<string, MyData>(maxCapacity: 1000);

// Add items to cache
cache.Add("key1", myData);

// Retrieve items
if (cache.TryGet("key1", out MyData data)) {
    // Use the cached data
}
```

#### Text Processing

Variable substitution in strings can be done with:

```csharp
using Blackwood;

// Variable substitution
var variables = new Dictionary<string, object> {
    {"name", "John"},
    {"version", "1.0.0"}
};

string result = Text.SubstituteVars("Hello {{name}}, version {{version}}!", variables);
// Result: "Hello John, version 1.0.0!"
```

## Next Steps

- [API Reference](../api/index.md) - API documentation
- [Contributing](../contributing/CONTRIBUTING.md) - How to contribute to the project

## Support

- **Documentation**: Guides and API reference
- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/randym32/Blackwood.IO/issues)
- **Community**: Join discussions and get help from the community
