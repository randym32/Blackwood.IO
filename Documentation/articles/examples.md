# Examples

This article provides examples using Blackwood.IO for common tasks.

## Table of Contents

- [Background file save](#background-file-save)
- [Application paths](#application-paths)
- [Atomic file I/O](#atomic-file-io)
- [MRU cache](#mru-cache)


## Background file save

Use `Util.Save` to save data to a file asynchronously without blocking your
application's UI or main thread.

```csharp
using Blackwood;

await Util.Save("data/output.txt", stream =>
{
	var bytes = System.Text.Encoding.UTF8.GetBytes("Hello from background save!\n");
	stream.Write(bytes, 0, bytes.Length);
});
```

In this example, we're demonstrating how to use the `Util.Save` method to write
data to a file asynchronously.  In the background, a _temporary_ file is created,
written to by the delegate.  When it has successfully be written, the file is
renamed to target file name -- with the old file being renamed first


## Application paths

The following section demonstrates retrieving common application paths:


```csharp
using Blackwood;

string appName = Application.Name;
string dataPath = Application.AppDataPath;
Console.WriteLine($"{appName} data path: {dataPath}");
```

## Simple file IO

Some files are easiest to read in as a big string, or write out as a string.

```csharp
using Blackwood;

var folder = new FolderWrapper(Application.AppDataPath);
folder.WriteAllText("config.json", "{\"enabled\":true}");
string config = folder.ReadAllText("config.json");
```

## MRU cache

```csharp
using Blackwood;

var cache = new MRUCache<string, byte[]>(capacity: 256);

byte[] GetAsset(string key)
{
	if (cache.TryGetValue(key, out var data))
		return data;

	var loaded = System.IO.File.ReadAllBytes(key);
	cache[key] = loaded;
	return loaded;
}
```

## Variable substitution

```csharp
using Blackwood;

var template = "Hello, {user}! Build {build} succeeded.";
var variables = new Dictionary<string, object>
{
	["user"] = "Alice",
	["build"] = 42,
};

string message = SubstituteVars.Substitute(template, variables);
```

## Read all lines

```csharp
using Blackwood;

foreach (var line in ReadAllLines.FromFile("notes.txt", System.Text.Encoding.UTF8))
{
	Console.WriteLine(line);
}
```

## Read from ZIP

```csharp
using Blackwood;

var zip = new ZipWrapper("assets.zip");
if (zip.Exists("images/logo.png"))
{
	using var stream = zip.OpenRead("images/logo.png");
}
```
