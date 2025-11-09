// Copyright (c) 2020-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.
using System.IO;
using System.Text;

namespace Blackwood;

public static partial class Text
{
    /// <summary>
    /// Load all of the text lines from the stream
    /// </summary>
    /// <param name="stream">The text stream</param>
    /// <returns>The text of the file</returns>
    public static string ReadAllLines(Stream stream)
    {
        // This will read the file in and append it to a string buffer
        var SB = new StringBuilder();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string? line;
        // Read each of the lines
        while ((line = reader.ReadLine()) != null)
        {
            SB.AppendLine(line);
        }

        // return the result
        return SB.ToString();
    }
}
