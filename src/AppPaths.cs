using System;
using System.Reflection;

namespace Blackwood
{
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
    public static string AppDataPath => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Application.Name);

#if false
    /// <summary>
    /// The path to the application's data
    /// </summary>
    static string AppDataPath => Directory.GetParent(Application.LocalUserAppDataPath).FullName;
#endif

    /// <summary>
    /// The path to the executable.
    /// </summary>
    /// <value>
    /// The path to the executable.
    /// </value>
    public static string ExeFilePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
}
}
