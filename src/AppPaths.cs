// Copyright (c) 2020-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.IO;

namespace Blackwood;

public static partial class FS
{
    /// <summary>
    /// The path to the application's data.
    /// </summary>
    /// <value>
    /// The path to the application's data.
    /// </value>
    public static string CommonApplicationDataPath =>
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);


    /// <summary>
    /// The path to the application's data.
    /// </summary>
    /// <value>
    /// The path to the application's data.
    /// </value>
    public static string AppLocalDataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Base.Application.Name??".");

    /// <summary>
    /// The path to the application's data, such as preferences.
    /// </summary>
    /// <value>
    /// The path to the application's data.
    /// </value>
    public static string AppDataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Base.Application.Name ?? ".");


    /// <summary>
    /// The path to the executable.
    /// </summary>
    /// <value>
    /// The path to the executable.
    /// </value>
    public static string? ExeFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
}
