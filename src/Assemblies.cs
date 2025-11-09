// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blackwood;

public static partial class Application
{
    /// <summary>
    /// Returns a list of assemblies to search for embedded resources.
    /// </summary>
    /// <returns>A list of assemblies to search for embedded resources.</returns>
    public static IEnumerable<Assembly> Assemblies()
    {
        // Keep track of the assemblies that have been searched.
        HashSet<Assembly> searchedAssemblies = [];

        // Start with the application's entry assembly if available (WinForms/WPF), else executing
        var asm = Assembly.GetEntryAssembly();
        if (asm != null && searchedAssemblies.Add(asm))
            yield return asm;
        asm = Assembly.GetExecutingAssembly();
        if (asm != null && searchedAssemblies.Add(asm))
            yield return asm;

        // Next all of the loaded assemblies, in dependency order, most recent first.
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var loadedAsm in loadedAssemblies.Reverse())
        {
            if (searchedAssemblies.Add(loadedAsm))
                yield return loadedAsm;
        }
    }

}
