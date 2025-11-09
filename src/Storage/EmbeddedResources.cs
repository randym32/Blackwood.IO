// Copyright (c) 2021-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Blackwood;

/// <summary>
/// This is an interface to allow access to resources within a folder or archive
/// </summary>
public class EmbeddedResources : IFolderWrapper
{
    /// <summary>
    /// The assembly read resources from
    /// </summary>
    readonly Assembly assembly = Assembly.GetCallingAssembly();


    /// <summary>
    /// Load resources from embedded in the assembly resources.
    /// </summary>
    public EmbeddedResources()
    {
    }


    /// <summary>
    /// Load resources from embedded in the assembly resources.
    /// </summary>
    /// <param name="a">The assembly read resources from</param>
    public EmbeddedResources(Assembly a)
    {
        assembly = a;
    }

    /// <summary>
    /// Performs any needed clean up
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="relativePath">The name of file embedded in the assembly</param>
    /// <returns>true if the file exists embedded in the assembly, false otherwise</returns>
    public bool Exists(string relativePath)
    {
        using var s = Stream(relativePath);
        return s != null;
    }

    /// <summary>
    /// This creates a stream for the given resources embedded in the assembly
    /// </summary>
    /// <param name="relativePath">The name of file embedded in the assembly</param>
    /// <returns>null on error, otherwise a stream that can be used to access the file data</returns>
    public Stream? Stream(string relativePath)
    {
        // Convert to a name that we can look up
        var fileName = assembly.GetName().Name + "." + relativePath.Replace('/', '.').Replace('\\','.');
        // Try a compressed stream first
        var stream = assembly.GetManifestResourceStream(fileName+".gz");
        if (null != stream)
        {
            // Decompress the bytes from memory stream
            using var zip = new GZipStream(stream, CompressionMode.Decompress, false);
            // put them in the following
            // TODO: add max capacity to handle zipbombs
            var decompressed = new MemoryStream();
            zip.CopyTo(decompressed);
            return decompressed;
        }

        // Fall back assuming it was not compressed
        return assembly.GetManifestResourceStream(fileName);
    }
}
