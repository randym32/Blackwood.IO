using Blackwood;
using System.IO;
using System.Text;

namespace FileOperationsExample;

/// <summary>
/// Example program demonstrating file operations using Blackwood.IO.
/// This example shows how to use FolderWrapper for reading files and
/// standard .NET file operations for writing files.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("File Operations Example");
        Console.WriteLine("========================");
        Console.WriteLine();

        // ====================================================================
        // Example 1: Getting Application Paths
        // ====================================================================
        // Blackwood.IO provides utilities to get standard application paths
        // and the application name. These are useful for storing application
        // data in the correct location for each platform.
        Console.WriteLine("1. Application Paths:");
        Console.WriteLine("----------------------");

        // Get the application name from the entry assembly
        // Application.Name returns the name of the entry assembly, or falls
        // back to the executing assembly if no entry assembly is available
        string appNameValue = Application.Name ?? "UnknownApp";

        // Get the application data path
        // FS.AppDataPath returns the path to the LocalApplicationData folder
        // combined with the application name (e.g., %LOCALAPPDATA%\MyApp)
        string dataPath = FS.AppDataPath;

        Console.WriteLine($"Application Name: {appNameValue}");
        Console.WriteLine($"Data Path: {dataPath}");
        Console.WriteLine();

        // ====================================================================
        // Example 2: Using FolderWrapper for File Operations
        // ====================================================================
        // FolderWrapper provides a consistent interface for reading
        // files, similar to how ZipWrapper works for ZIP archives. This allows
        // you to use the same code for both folder and archive access.
        Console.WriteLine("2. File Operations with FolderWrapper:");
        Console.WriteLine("--------------------------------------");

        // Create a temporary folder for testing
        // In a real application, you would use FS.AppDataPath or another
        // standard location
        string testFolder = Path.Combine(Path.GetTempPath(), "BlackwoodIOExample");
        Directory.CreateDirectory(testFolder);

        // Create a FolderWrapper instance
        // FolderWrapper provides a consistent interface for accessing files
        // within a folder, similar to how ZipWrapper works for ZIP archives
        var folder = new FolderWrapper(testFolder);

        try
        {
            // Write text to a file using standard .NET file operations
            // Note: FolderWrapper doesn't have WriteAllText - use File.WriteAllText
            // for writing files. FolderWrapper is primarily for reading files
            // through a consistent interface.
            string testFile = "example.txt";
            string content = "Hello, World!\nThis is a test file.";

            Console.WriteLine($"Writing to {testFile}:");
            // Combine the test folder path with the file name to get the full path
            string fullPath = Path.Combine(testFolder, testFile);
            // Write the content to the file
            File.WriteAllText(fullPath, content);
            Console.WriteLine("File written successfully");
            Console.WriteLine();

            // Read text from a file using FolderWrapper.Stream()
            // FolderWrapper.Stream() returns a Stream that you can use to read
            // the file. This is useful when you want a consistent interface for
            // reading from folders, ZIP archives, or embedded resources.
            Console.WriteLine($"Reading from {testFile}:");

            // Get a stream for the file using FolderWrapper
            // The Stream method returns null if the file doesn't exist
            using var stream = folder.Stream(testFile);
            if (stream != null)
            {
                // Create a StreamReader to read text from the stream
                // Specify UTF-8 encoding to ensure proper text reading
                using var reader = new StreamReader(stream, Encoding.UTF8);

                // Read all content from the file
                string readContent = reader.ReadToEnd();
                Console.WriteLine($"Content:\n{readContent}");
            }
            else
            {
                Console.WriteLine("File not found");
            }
            Console.WriteLine();

            // Check if file exists using FolderWrapper.Exists()
            // This is useful when you want to check for file existence through
            // a consistent interface (works with FolderWrapper, ZipWrapper, etc.)
            Console.WriteLine($"File exists: {folder.Exists(testFile)}");
            Console.WriteLine();

            // List files in folder using standard .NET directory operations
            // Note: FolderWrapper doesn't have GetFiles() - use Directory.GetFiles()
            // for listing files. FolderWrapper is designed for reading individual
            // files, not for directory enumeration.
            Console.WriteLine("Files in folder:");
            // Get all files in the test folder
            var files = Directory.GetFiles(testFolder);
            foreach (var file in files)
            {
                // Display just the filename (not the full path)
                Console.WriteLine($"  - {Path.GetFileName(file)}");
            }
        }
        finally
        {
            // Clean up: delete the temporary test folder
            // This ensures we don't leave test files on the system
            if (Directory.Exists(testFolder))
            {
                Directory.Delete(testFolder, true);
            }
        }
    }
}

