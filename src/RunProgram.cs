// Copyright (c) 2020-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Diagnostics;

namespace Blackwood;

public static partial class Util
{
    /// <summary>
    /// This is used to run the program or verb-based commands in the background.
    /// </summary>
    /// <param name="toRun">The path to the executable or command to run.</param>
    /// <param name="verb">The verb to use to run the program.</param>
    /// <param name="arguments">The command line arguments</param>
    /// <returns>An enumeration of the text lines that the process emitted</returns>
    public static async IAsyncEnumerable<string> RunCommand(string toRun=null, string verb=null, string arguments=null)
    {
        // Fill out the paper work on the program to run.  Or verb
        var process = new Process
        {
            EnableRaisingEvents = false,
            StartInfo =
            {
                FileName               = toRun ?? string.Empty,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
                Verb                   = verb ?? string.Empty,
                Arguments              = arguments ?? string.Empty
            }
        };
        // Start the program up
        process.Start();
        var stdOut = process.StandardOutput;

        // Fetch each of the lines from the process and pass them on
        string line;
        while ((line = await stdOut.ReadLineAsync()) != null)
        {
            // Otherwise send the line we read back
            yield return line;
        }

        // We have reached the end of the file, indicating that all of
        // the data that would be sent by this program has reached.
        // Wait for the program to finish and exit
        process.WaitForExit();
    }
}

