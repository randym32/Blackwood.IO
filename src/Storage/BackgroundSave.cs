// Copyright (c) 2020-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Blackwood;

/// <summary>
/// This is a helper class to provide a variety of utility procedures.
/// </summary>
/// <remarks>
/// In a more perfect world, these helpers would organize themselves, or at
/// least lend to a more appropriate home. Until that day arrives, this class
/// serves as a humble repository for the miscellaneous functions that refuse
/// to fit neatly elsewhere.
/// </remarks>
public static partial class Util
{
    /// <summary>
    /// This delegate will be called in the background to allow saving a file
    /// without causing the UI to experience an slowdown or stutter.
    /// </summary>
    /// <param name="stream">The file that was created in the background.</param>
    public delegate void dWriteBackground(FileStream stream);

    /// <summary>
    /// This is used to lock and prevent race conditions when multiple saves
    /// target the same file path simultaneously. Uses per-file locking to allow
    /// concurrent saves to different files while serializing saves to the same file.
    /// </summary>
    static readonly ConcurrentDictionary<string, object> fileLocks = new();

    /// <summary>
    /// Gets or creates a lock object for a specific file path.
    /// </summary>
    /// <param name="path">The file path to get a lock for.</param>
    /// <returns>A lock object for the specified path.</returns>
    static object GetFileLock(string path)
    {
        return fileLocks.GetOrAdd(path, _ => new object());
    }

    /// <summary>
    /// Saves the file to the given path.
    /// This is done in the background by first writing to a temporary file,
    /// then moving to the target.
    /// </summary>
    /// <param name="path">The path to store the data.</param>
    /// <param name="writeBackground">The callback to populate the data in the file.</param>
    public static void Save(string path, dWriteBackground writeBackground)
    {
        // Validate input parameters
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (writeBackground == null)
            throw new ArgumentNullException(nameof(writeBackground));

        // Capture the path in a local variable to ensure proper closure
        var targetPath = path;

        // Have this run in the background, so that it doesn't cause the UI
        // to lag
        _ = ThreadPool.QueueUserWorkItem((object? s) =>
            {
                // Get a per-file lock to ensure thread safety for concurrent saves
                // to the same file path. This allows different files to be saved
                // concurrently while serializing saves to the same file.
                var fileLock = GetFileLock(targetPath);

                lock (fileLock)
                {
                    // Now that this background task is running we can save the
                    // state data to a file.
                    string? tempName = null;

                    try
                    {
                        // Create a temporary file to write to.  If there is an
                        // exception crash, the main file won't be corrupted.  Once we
                        // have finished writing to the file, we can replace the
                        // previous version of the file with the new.
                        tempName = Path.GetTempFileName();

                        // Create the temporary file and populate it.
                        using FileStream fs = File.Create(tempName);

                        // If I use an async/await fs.Write() here in the main
                        // thread, it hangs and never returns.  So that's why I
                        // backgrounded the writes.
                        //
                        // Call the call back to commit the data to the file.
                        writeBackground(fs);
                        fs.Close();

                        // Ensure the directory for the target path exists
                        var directory = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // Check to see if the destination already exists,
                        // if it does exist, replace the file; otherwise
                        // Move the temporary file into place
                        if (File.Exists(targetPath))
                            File.Replace(tempName, targetPath, targetPath + ".bak");
                        else
                            File.Move(tempName, targetPath);
                    }
                    catch (Exception ex)
                    {
                        // There was a problem, clean up the temporary file if it was created
                        if (!string.IsNullOrEmpty(tempName) && File.Exists(tempName))
                        {
                            try
                            {
                                File.Delete(tempName);
                            }
                            catch
                            {
                                // Ignore cleanup errors
                            }
                        }

                        // Log the exception for debugging (in a real application, you might want to use a proper logger)
                        System.Diagnostics.Debug.WriteLine($"Background save failed for {targetPath}: {ex.Message}");
                    }
                }
            });
    }
}
