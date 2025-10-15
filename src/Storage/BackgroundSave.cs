// Copyright © 2020-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.
using System;
using System.IO;
using System.Threading;

namespace Blackwood;

/// <summary>
/// This is a helper class to provide a variety of utility procedures.
/// </summary>
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
    /// target the same file path simultaneously.
    /// </summary>
    static readonly object saveLock = "";

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

        // Have this run in the background, so that it doesn't cause the UI
        // to lag
        _ = ThreadPool.QueueUserWorkItem((object s) =>
            {
                // Now that this background task is running we can save the
                // state data to a file.
                var tempFileCreated = false;
                string tempName = null;

                // Create a temporary file to write to.  If there is an
                // exception crash, the main file won't be corrupted.  Once we
                // have finished writing to the file, we can replace the
                // previous version of the file with the new.
                lock(saveLock)
                    tempName = Path.GetTempFileName();

                try
                {
                    // Create the temporary file and populate it.
                    using (var fs = File.Create(tempName))
                    {
                        tempFileCreated = true;

                        // If I use an async/await fs.Write() here in the main
                        // thread, it hangs and never returns.  So that's why I
                        // backgrounded the writes.
                        //
                        // Call the call back to commit the data to the file.
                        writeBackground(fs);
                    }

                    // Ensure the directory for the target path exists
                    var directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Rename the temporary file to become the main file
                    // Use a lock to prevent queued up saves from racing with each
                    // other
                    lock (saveLock)
                    {
                        // Check to see if the destination already exists,
                        // if it does exist, replace the file; otherwise
                        // Move the temporary file into place
                        if (File.Exists(path))
                            File.Replace(tempName, path, path + ".bak");
                        else
                            File.Move(tempName, path);
                    }
                }
                catch
                {
                    // There was a problem, clean up the temporary file if it was created
                    if (tempFileCreated && !string.IsNullOrEmpty(tempName) && File.Exists(tempName))
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
                }
            });
    }
}
