// Copyright (c) 2020-2021 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;

namespace Blackwood
{
/// <summary>
/// This class holds a variety of helper utilities to modify text.
/// </summary>
public static partial class Text
{
    /// <summary>
    /// Substitute in the variable references with the values that are in the
    /// tableau.
    /// </summary>
    /// <param name="sourceText">The original text string</param>
    /// <param name="tableau">The table of values for variables</param>
    /// <returns>The potentially modified string</returns>
    /// <remarks>This is not particularly effecient but it isn't intended to be
    /// used in an environment like that</remarks>
    public static string SubstituteVars(string sourceText, IDictionary<string, object>? tableau)
    {
        // Check that the passed table has contents
        if (null == tableau)
            return sourceText;

        // Substitute in each of the parameters that are in the tableau.
        foreach (var kv in tableau)
            sourceText = sourceText.Replace("{{" + kv.Key + "}}", kv.Value.ToString());

        // Return the potentially updated string
        return sourceText;
    }
}
}
