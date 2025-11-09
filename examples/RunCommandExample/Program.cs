using Blackwood;

namespace RunCommandExample;

/// <summary>
/// Example program demonstrating command execution using Blackwood.IO.
/// This example shows how to use Util.RunCommand to execute external commands
/// and stream their output line by line asynchronously.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Run Command Example");
        Console.WriteLine("===================");
        Console.WriteLine();

        // ====================================================================
        // Example 1: Running a Simple Command
        // ====================================================================
        // Util.RunCommand executes an external command and streams its output
        // line by line asynchronously. This is useful for running system commands,
        // scripts, or other executables and processing their output in real-time.
        Console.WriteLine("1. Running a Simple Command:");
        Console.WriteLine("---------------------------");

        // Run a command and stream its output
        // Util.RunCommand returns an IAsyncEnumerable<string> that yields each
        // line of output from the command as it becomes available. This allows
        // you to process output in real-time without waiting for the command to
        // complete.
        Console.WriteLine("Running 'dotnet --version' command:");
        Console.WriteLine("-----------------------------------");

        // Execute the command and iterate over its output lines
        // The command runs in the background, and each line is yielded as it
        // is produced by the process. The method waits for the process to
        // complete before finishing.
        await foreach (var line in Util.RunCommand(toRun: "dotnet", arguments: "--version"))
        {
            // Process each line of output as it arrives
            // In this example, we simply print each line
            Console.WriteLine(line);
        }
        Console.WriteLine();

        // ====================================================================
        // Example 2: Processing Command Output with Limits
        // ====================================================================
        // You can process command output incrementally and stop early if needed.
        // This is useful for commands that produce large amounts of output or
        // when you only need a portion of the output.
        Console.WriteLine("2. Processing Command Output with Limits:");
        Console.WriteLine("----------------------------------------");

        // Run another command that produces more output
        // This example demonstrates how to limit the amount of output processed
        Console.WriteLine("Running 'dotnet --info' command (first 10 lines):");
        Console.WriteLine("--------------------------------------------------");

        // Track the number of lines processed
        int lineCount = 0;

        // Execute the command and process only the first 10 lines
        // The command continues running in the background, but we break
        // out of the loop after processing the desired number of lines
        await foreach (var line in Util.RunCommand(toRun: "dotnet", arguments: "--info"))
        {
            // Process each line of output
            Console.WriteLine(line);

            // Increment the line counter
            lineCount++;

            // Stop processing after 10 lines
            // Note: The process continues running, but we stop reading its output
            if (lineCount >= 10)
            {
                Console.WriteLine("... (truncated)");
                break;
            }
        }

        // Note: The process may still be running after we break out of the loop.
        // In a production application, you might want to handle process cleanup
        // or cancellation if you need to stop the command early.
    }
}

