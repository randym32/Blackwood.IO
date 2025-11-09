# Blackwood.IO

A collection of C# utilities commonly needed across many projects. This library provides cross-platform utilities without Windows-specific dependencies.

## Features

- Cross-platform compatibility (.NET 9)
- Modern C# language features (latest C#)
- Utility classes for common development tasks
- No Windows Forms or Windows-specific dependencies

## Getting Started

### Prerequisites

- .NET 9 SDK or later
- Visual Studio 2022 (Windows) or Visual Studio Code (cross-platform)

### Development Tools

**Windows:**
- [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/vs/community/) (free for personal use)
- [Visual Studio Code](https://code.visualstudio.com/) with C# extension

**Cross-platform:**
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio Code](https://code.visualstudio.com/) with C# extension


## Installation

### NuGet Package
```bash
dotnet add package Blackwood.IO
```

Or via Package Manager Console:
```
Install-Package Blackwood.IO
```

### Manual Installation
1. Clone the repository: `git clone https://github.com/randym32/Blackwood.IO.git`
2. Build the solution: `dotnet build`
3. Reference the assembly in your project

## Usage

```csharp
using Blackwood;
using System.Collections.Generic;
using System.IO;

// MRUCache
var cache = new MRUCache<string, int>(100);
cache["answer"] = 42;
int value = cache["answer"]; // 42

// Substitute variables in text
var result = Text.SubstituteVars("Hello {{name}}!", new Dictionary<string, object> { ["name"] = "World" });

// Read all lines from a stream
using var stream = File.OpenRead("./README.md");
string text = Text.ReadAllLines(stream);

// Run a command and stream its output
await foreach (var line in Util.RunCommand(toRun: "dotnet", arguments: "--info"))
{
    Console.WriteLine(line);
}
```

## Documentation

- [API Documentation](https://randym32.github.io/Blackwood.IO)
- [GitHub Repository](https://github.com/randym32/Blackwood.IO)
- [NuGet Package](https://www.nuget.org/packages/Blackwood.IO/)

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details on how to:

- Report issues
- Submit pull requests
- Follow our coding standards

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- [GitHub Issues](https://github.com/randym32/Blackwood.IO/issues) - Report bugs and request features
- [GitHub Discussions](https://github.com/randym32/Blackwood.IO/discussions) - Ask questions and discuss ideas

