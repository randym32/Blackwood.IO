# Examples

This article provides examples using Blackwood.IO for common tasks.

## Table of Contents

- [Background file save](#background-file-save)
- [Application paths](#application-paths)
- [Simple file IO](#simple-file-io)
- [MRU cache](#mru-cache)
- [Variable substitution](#variable-substitution)
- [Read all lines](#read-all-lines)
- [Read from ZIP](#read-from-zip)


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
written to by the delegate.  When it has successfully been written, the file is
renamed to target file name -- with the old file being renamed first


## Application paths

The following section demonstrates retrieving common application paths:


```csharp
using Blackwood;

string dataPath = FS.AppDataPath;
Console.WriteLine($"data path: {dataPath}");
```

## Simple file IO

Some files are easiest to read in as a big string, or write out as a string.

```csharp
using Blackwood;

var folder = new FolderWrapper(FS.AppDataPath);
folder.WriteAllText("config.json", "{\"enabled\":true}");
string config = folder.ReadAllText("config.json");
```

## MRU cache

Use MRUCache to store and retrieve items. The cache will automatically remove
the least recently used entries when capacity is exceeded.


```csharp
using Blackwood;

// Create an MRU cache that holds up to 256 assets,
// where the key is a string and the value is a byte array (file content)
var cache = new MRUCache<string, byte[]>(capacity: 256);

// GetAsset will load the asset from disk if it's not cached,
// or return the cached value if it is present.
byte[] GetAsset(string key)
{
	// Try to get the data from cache
	if (cache.TryGetValue(key, out var data))
		return data; // Return cached asset if available

	// If not cached, load the file content from disk
	var loaded = System.IO.File.ReadAllBytes(key);

	// Add the loaded asset to the cache
	cache[key] = loaded;

	// Return the newly loaded data
	return loaded;
}
```

## Variable substitution

You can perform variable substitution in strings using the `Text.SubstituteVars`
method, passing in a template and a dictionary of variables.


```csharp
using Blackwood;

// Define a template string with variable placeholders
var template = "Hello, {user}! Build {build} succeeded.";

// Create a dictionary mapping placeholder names to values
var variables = new Dictionary<string, object>
{
	["user"] = "Alice", // The user's name
	["build"] = 42,     // The build number
};

// Perform variable substitution using the SubstituteVars utility,
// replacing {user} and {build} in the template with their values.
string message = SubstituteVars.Substitute(template, variables);

// The resulting message will be: "Hello, Alice! Build 42 succeeded."
```

## Read all lines

This method allows you to easily read all lines from a file.


```csharp
using Blackwood;

// The following example reads all lines from a text file using UTF-8 encoding.

foreach (var line in ReadAllLines.FromFile("notes.txt", System.Text.Encoding.UTF8))
{
	Console.WriteLine(line);
}
```

## Read from ZIP

The ZipWrapper class eases reading files directly from ZIP archives without extraction.


```csharp
using Blackwood;

// Open the 'assets.zip' archive for reading.
var zip = new ZipWrapper("assets.zip");

// Check if the file 'images/logo.png' exists inside the ZIP archive.
if (zip.Exists("images/logo.png"))
{
	// Open a read-only stream for the file within the ZIP.
	using var stream = zip.Stream("images/logo.png");
	// (You can now read from 'stream' as needed.)
}
```

