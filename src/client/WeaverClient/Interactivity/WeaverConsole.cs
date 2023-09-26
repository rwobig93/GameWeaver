using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WeaverClient.Interactivity;

public static class WeaverConsole
{
    public static Process? Process { get; internal set; }
    
    public static StreamWriter? Writer { get; internal set; }
    
    public static StreamReader? Reader { get; internal set; }

    public static void StartConsole()
    {
        if (Process is not null)
            return;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };
        }
        
        Process?.Start();

        // To send output to the console
        Writer = Process?.StandardInput;
        Writer?.WriteLine("echo Hello, Console!");

        // To read input from the console
        Reader = Process?.StandardOutput;
        var consoleOutput = Reader?.ReadToEnd();
            
        // Output the console output to your application's console
        Console.WriteLine("Console Output:");
        Console.WriteLine(consoleOutput);
    }

    public static void StopConsole()
    {
        Process?.Close();
    }
}