// Copyright (c) 2020-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.
using System;
using System.IO;

namespace Blackwood;


/// <summary>
/// This is an interface to allow access to resources within a folder or archive
/// </summary>
/// <remarks>
/// This interface provides a way to access resources within a folder or archive.
/// In a more perfect world, all containers would present themselves with the
/// same polite interface, but until that day arrives, this abstraction serves
/// as a diplomatic protocol for accessing resources regardless of their chosen
/// domicile.
///
/// It is implemented by siblings:
/// - <see cref="EmbeddedResources"/> which can access resources within an
///   assembly,
/// - <see cref="FolderWrapper"/> which can access resources within a folder, and
/// - <see cref="ZipWrapper"/> which can access resources within a compressed
///   archive.
/// </remarks>
public interface IFolderWrapper:IDisposable
{
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="relativePath">The name of file within the wrapper</param>
    /// <returns>true if the file exists within the wrapper, false otherwise</returns>
    bool Exists(string relativePath);

    /// <summary>
    /// This creates a stream for the given resources within the container
    /// </summary>
    /// <param name="relativePath">The name of file within the container</param>
    /// <returns>A stream that can be used to access the file data</returns>
    Stream? Stream(string relativePath);
}

