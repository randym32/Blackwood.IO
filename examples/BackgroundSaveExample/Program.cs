using System.Threading;
using Blackwood;

namespace BackgroundSaveExample;

/// <summary>
/// Example program demonstrating background file saving using Blackwood.IO.
/// This example shows how to use Util.Save for non-blocking file operations
/// that run asynchronously in the background, allowing your application to
/// continue processing while files are being written.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Background Save Example");
        Console.WriteLine("=======================");
        Console.WriteLine();

        // ====================================================================
        // Example: Background File Saving
        // ====================================================================
        // Util.Save provides a non-blocking way to save files. Instead of
        // blocking the current thread while the file is written, it performs
        // the save operation in the background, allowing your application to
        // continue processing other tasks.
        Console.WriteLine("Background File Saving:");
        Console.WriteLine("----------------------");

        // Set up the path for the test file
        // In a real application, this would be your actual file path
        string testFile = Path.Combine(Path.GetTempPath(), "background_save_test.txt");

        try
        {
            // Prepare the data to be saved
            // This could be any data: configuration, user input, logs, etc.
            Console.WriteLine("Saving data in the background...");
            string data = "This is data that will be saved in the background.\n" +
                         "The save operation is non-blocking and happens asynchronously.";

            // Use Util.Save to save the file in the background
            // Util.Save takes two parameters:
            //   1. The file path where the data should be saved
            //   2. An action that receives a Stream and writes data to it
            // The save operation runs asynchronously and doesn't block the
            // current thread. This is useful for saving large files or when
            // you want to keep the UI responsive during save operations.
            Util.Save(testFile, stream =>
            {
                // The stream parameter is a FileStream opened for writing
                // You can use any StreamWriter or write directly to the stream
                using var writer = new StreamWriter(stream);

                // Write the data to the file
                // This operation happens in the background thread
                writer.Write(data);
            });

            // Note: At this point, the save operation has been initiated but
            // may not be complete yet. The file write happens asynchronously
            // in a background thread.
            Console.WriteLine("Background save initiated (may still be in progress)");
            Console.WriteLine();

            // Wait a moment to ensure the file write operation completes
            // In a real application, you might use proper synchronization
            // mechanisms (like Task.Wait or await) or check for completion
            // through other means. For this example, a short sleep is used
            // to demonstrate that the save operation completes.
            Thread.Sleep(100);

            // ====================================================================
            // Verification: Check if File Was Written
            // ====================================================================
            // After waiting, verify that the file was successfully written
            Console.WriteLine("Verifying file was saved:");
            Console.WriteLine("------------------------");

            // Check if the file exists
            if (File.Exists(testFile))
            {
                // Read the file content to verify it was written correctly
                Console.WriteLine("File was successfully saved:");
                string content = File.ReadAllText(testFile);
                Console.WriteLine($"Content:\n{content}");
            }
            else
            {
                // If the file doesn't exist, the save operation may have failed
                // or may still be in progress (though unlikely after the sleep)
                Console.WriteLine("File was not found");
            }
        }
        catch (Exception ex)
        {
            // Handle any errors that might occur during the save operation
            // Common errors include: permission issues, disk space problems,
            // invalid file paths, or I/O errors
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Clean up: delete the temporary test file
            // This ensures we don't leave test files on the system
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }
}

