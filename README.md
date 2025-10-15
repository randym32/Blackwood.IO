# Blackwood.IO

A collection of C# utilities commonly needed across many projects. This library provides cross-platform utilities without Windows-specific dependencies.

## Features

- Cross-platform compatibility (.NET 8)
- Modern C# language features (C# 12)
- Utility classes for common development tasks
- No Windows Forms or Windows-specific dependencies

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022 (Windows) or Visual Studio Code (cross-platform)

### Development Tools

**Windows:**
- [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/vs/community/) (free for personal use)
- [Visual Studio Code](https://code.visualstudio.com/) with C# extension

**Cross-platform:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
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

// Example usage of utilities
var cache = new MRUCache<string, object>(100);
var textProcessor = new TextProcessor();
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

