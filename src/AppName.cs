// Copyright © 2020-2021 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.Diagnostics;

namespace Blackwood
{
/// <summary>
/// This is a utility class to provide some information about the application
/// </summary>
public static partial class Application
{
    #region Application Name
    /// <summary>
    /// The application name cached for reuse
    /// </summary>
    static string _appName;

    /// <summary>
    /// The application name.
    /// </summary>
    /// <value>
    /// The application name.
    /// </value>
    public static string Name
    {
        get
        {
            // See if we already looked up the name of the application
            if (null != _appName)
                return _appName;
            try
            {
                // Try to look up the real assembly for the application
                var assembly = System.Reflection.Assembly.GetEntryAssembly();
                if (null == assembly)
                {
                    var frames = new StackTrace().GetFrames();
                    assembly = frames[frames.Length - 1].GetMethod().Module.Assembly;
                }

                // use the name from the assembly
                return _appName = assembly.GetName().Name;
            }
            catch (Exception)
            { }

            // There was an error, fall back to a default name
            return "unknownApp";
        }
    }
    #endregion
}
}