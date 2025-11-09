using Blackwood;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ZipOperationsExample;

/// <summary>
/// Example program demonstrating ZIP file operations using Blackwood.IO.
/// This example shows how to use ZipWrapper for reading files from ZIP
/// archives and standard .NET ZIP operations for creating archives and
/// listing files.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ZIP Operations Example");
        Console.WriteLine("======================");
        Console.WriteLine();

        // ====================================================================
        // Example 1: Creating a ZIP File
        // ====================================================================
        // First, we'll create a test ZIP file with some sample files.
        // In a real application, you might be working with existing ZIP files.
        Console.WriteLine("1. Creating a ZIP File:");
        Console.WriteLine("------------------------");

        // Set up paths for the test ZIP file and source folder
        string testZipPath = Path.Combine(Path.GetTempPath(), "test.zip");
        string testFolder = Path.Combine(Path.GetTempPath(), "ZipTest");

        try
        {
            // Create a test folder structure with files
            // This simulates a directory structure that we'll compress
            Directory.CreateDirectory(testFolder);
            Directory.CreateDirectory(Path.Combine(testFolder, "subfolder"));

            // Create some test files with content
            File.WriteAllText(Path.Combine(testFolder, "file1.txt"), "Content of file 1");
            File.WriteAllText(Path.Combine(testFolder, "file2.txt"), "Content of file 2");
            File.WriteAllText(Path.Combine(testFolder, "subfolder", "file3.txt"), "Content of file 3");

            // Create a ZIP file from the directory
            // ZipFile.CreateFromDirectory creates a ZIP archive containing
            // all files and subdirectories from the source directory
            if (File.Exists(testZipPath))
            {
                File.Delete(testZipPath);
            }
            ZipFile.CreateFromDirectory(testFolder, testZipPath);
            Console.WriteLine($"Created ZIP file: {testZipPath}");
            Console.WriteLine();

            // ====================================================================
            // Example 2: Using ZipWrapper to Access Files
            // ====================================================================
            // ZipWrapper provides a consistent interface for reading files
            // from ZIP archives, similar to how FolderWrapper works for folders.
            // This allows you to use the same code for both ZIP archives and
            // regular folders.
            Console.WriteLine("2. Accessing Files with ZipWrapper:");
            Console.WriteLine("-----------------------------------");

            // Create a ZipWrapper instance for the ZIP file
            // ZipWrapper opens the ZIP file and provides access to its contents
            var zipWrapper = new ZipWrapper(testZipPath);

            // List files in ZIP using ZipArchive directly
            // Note: ZipWrapper doesn't have GetFiles() - use ZipArchive.Entries
            // for listing files. ZipWrapper is designed for reading individual
            // files through a consistent interface, not for enumeration.
            Console.WriteLine("Files in ZIP:");
            // Open the ZIP file for reading to enumerate entries
            using (var zipArchive = ZipFile.OpenRead(testZipPath))
            {
                // Iterate through all entries in the ZIP archive
                // Each entry represents a file or directory in the archive
                foreach (var entry in zipArchive.Entries)
                {
                    // Display the full path of each entry
                    Console.WriteLine($"  - {entry.FullName}");
                }
            }
            Console.WriteLine();

            // ====================================================================
            // Example 3: Reading Files from ZIP
            // ====================================================================
            // ZipWrapper.Stream() returns a Stream that you can use to read
            // files from the ZIP archive. This provides a consistent interface
            // for reading from ZIP archives, similar to FolderWrapper.
            Console.WriteLine("3. Reading Files from ZIP:");
            Console.WriteLine("---------------------------");

            Console.WriteLine("Reading file1.txt from ZIP:");
            // Get a stream for the file using ZipWrapper
            // The Stream method returns null if the file doesn't exist
            // The stream is a MemoryStream that contains the decompressed file
            // data, positioned at the beginning
            using var stream = zipWrapper.Stream("file1.txt");
            if (stream != null)
            {
                // Create a StreamReader to read text from the stream
                // Specify UTF-8 encoding to ensure proper text reading
                using var reader = new StreamReader(stream, Encoding.UTF8);

                // Read all content from the file
                string content = reader.ReadToEnd();
                Console.WriteLine($"Content: {content}");
            }
            else
            {
                Console.WriteLine("File not found");
            }
            Console.WriteLine();

            // ====================================================================
            // Example 4: Checking File Existence
            // ====================================================================
            // ZipWrapper.Exists() allows you to check if a file exists in the
            // ZIP archive without opening it. This is useful for conditional
            // logic based on file presence.
            Console.WriteLine("4. Checking File Existence:");
            Console.WriteLine("----------------------------");

            // Check if files exist in the ZIP archive
            // Exists() returns true if the file is found, false otherwise
            Console.WriteLine($"file1.txt exists: {zipWrapper.Exists("file1.txt")}");
            Console.WriteLine($"nonexistent.txt exists: {zipWrapper.Exists("nonexistent.txt")}");
        }
        catch (Exception ex)
        {
            // Handle any errors that might occur during ZIP operations
            // Common errors include: file not found, corrupted ZIP, permission
            // issues, or disk space problems
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Clean up: delete the temporary test files and folder
            // This ensures we don't leave test files on the system
            if (File.Exists(testZipPath))
            {
                File.Delete(testZipPath);
            }
            if (Directory.Exists(testFolder))
            {
                Directory.Delete(testFolder, true);
            }
        }
    }
}

