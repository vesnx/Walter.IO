# WALTER
Introducing the WALTER Framework: Workable Algorithms for Location-aware Transmission, Encryption Response. Designed for modern developers, WALTER is a groundbreaking suite of NuGet packages crafted for excellence in .NET Standard 2.0, 2.1, Core 3.1, and .NET 6, 7, 8, as well as C++ environments. Emphasizing 100% AoT support and reflection-free operations, this framework is the epitome of performance and stability.

Whether you're tackling networking, encryption, or secure communication, WALTER offers unparalleled efficiency and precision in processing, making it an essential tool for developers who prioritize speed and memory management in their applications.

# About the Walter.IO Package

Basically there a 2 main interfaces in the Nuget package

- IDiskGuard
- FileInfo extensions



## IDiskGuard
The IDiskGuard monitors and alerts if there are any uncontrolled disk activities where data is altered.

### Features

- **Real-Time Disk Activity Monitoring**: `IDiskGuard` actively watches specified directories for changes, including file creations, modifications, and deletions.
- **Event-Driven Alerts**: Triggers events when unauthorized disk manipulations are detected, allowing for immediate response.
- **Flexible Monitoring**: Supports monitoring specific directories with customizable file filters and the option to include subdirectories.
- **Easy Integration**: While demonstrated with `IDiskGuard` is designed for broad applicability across various types of applications.

### Getting Started

The bellow sample show you one of many ways that you can integrate and configure IDiskGuard.

```c#

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Walter.BOM;
using Walter.IO;

namespace YourNamespace
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<DiskMonitorService>();
                    services.AddSecureDiskMonitor();
                });
    }

    public class DiskMonitorService : BackgroundService
    {
        private readonly IDiskGuard _diskGuard;

        public DiskMonitorService(IDiskGuard diskGuard)
        {
            _diskGuard = diskGuard;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var monitoredDirectory = @"path\to\monitored\directory";

            // Start monitoring the directory
            _diskGuard.StartMonitoring(new DirectoryInfo(monitoredDirectory), "*.*", true);
            _diskGuard.OnDiskManipulation += DiskGuard_OnDiskManipulation;

            stoppingToken.Register(() => 
            {
                // Cleanup code when the service is stopping
                _diskGuard.StopMonitoring(new DirectoryInfo(monitoredDirectory), "*.*");
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                // The service will keep running, monitoring disk activities.
                // Implement any additional periodic checks or maintenance tasks here.

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private void DiskGuard_OnDiskManipulation(object sender, DiskManipulationEventArgs e)
        {
            // Example padding check for manipulated files
            var fileInfo = new FileInfo(e.Violation.Path);
            if (fileInfo.Exists && fileInfo.CountPaddingBytesChunked() > 0)
            {
                // Handle detection of significant padding in the manipulated file
                Console.WriteLine($"Unauthorized manipulation with padding detected on file: {e.Violation.Path}");
            }
            // Implement additional handling logic for detected disk manipulations
        }
    }
}

```

### Key Components:
- Program: Sets up the host builder and configures services, registering DiskMonitorService as a hosted service and adding IDiskGuard to the DI container.
- DiskMonitorService: A background service that starts disk monitoring on application start and keeps running until the application shuts down. It listens for IDiskGuard's OnDiskManipulation event to detect and respond to unauthorized disk manipulations, including checking for significant padding in altered files.

### Steps to Use
- Replace "path\\to\\monitored\\directory" with the actual path of the directory you want to monitor.
- Implement any additional logic you require in the DiskGuard_OnDiskManipulation method, especially for handling detected unauthorized disk manipulations and padding analysis.

This setup allows IDiskGuard to continuously monitor specified directories in the background, making it an efficient solution for detecting and responding to potential security threats or unauthorized changes in real-time, without requiring direct interaction with user-facing components like controllers.


## FileInfo Extension Methods Overview

The Walter.IO package enriches your application with powerful FileInfo extension methods, designed to enhance file analysis and security measures. Among these, two standout methods provide essential capabilities for detecting potential security threats and ensuring data integrity:

### CountPaddingBytesChunked()
This method efficiently calculates the number of padding bytes present at the end of a file. By leveraging chunked reading strategies, it optimizes performance and minimizes memory usage, making it exceptionally well-suited for processing large files. This capability is crucial for analyzing file structures and detecting anomalies that could signify data corruption or tampering.

#### Key Benefits:

- Performance-Optimized: Tailored for large files with minimal performance impact.
- Security Analysis: Aids in the identification of file tampering or corruption.

### ContainsMoreThanOrEqualTo(percentage)
Evaluates whether the padding within a file constitutes a specified percentage (or more) of the file's total size. This method is instrumental in uncovering files that may have been deliberately modified to circumvent malware detection mechanisms through the introduction of excessive padding. Such modifications can alter a file's hash signature, enabling it to evade traditional detection techniques based on known signatures.

#### Key Benefits:

- Malware Detection: Helps identify files potentially altered to bypass hash-based malware detection.
- Customizable Threshold: Allows specification of the padding percentage threshold for targeted analysis.

### Practical Applications
Detecting excessive padding in files is more than just a matter of maintaining data integrity. It's a critical security practice. Malicious actors often manipulate file padding to change hash signatures, thereby evading detection by antivirus programs and intrusion detection systems. By employing the ContainsMoreThanOrEqualTo method, developers and security professionals can pinpoint files with unusual padding patterns, flagging them for further investigation or automated responses.

### Example Usage:

Detecting a file with excessive padding could be as straightforward as:
```c#
FileInfo suspiciousFile = new FileInfo("path/to/suspect/file.exe");

// Check if the file has padding exceeding 30% of its total size
bool isAltered = suspiciousFile.ContainsMoreThanOrEqualTo(30);

if (isAltered)
{
    Console.WriteLine($"File: {suspiciousFile.Name} may have been altered to evade malware detection.");
}

```
By integrating these FileInfo extensions into your security protocols, you can enhance your application's resilience against sophisticated cyber threats and ensure a higher level of data integrity and reliability.


### Comparing File Padding Changes Between Scans

This example demonstrates how to monitor files for changes in padding, which could indicate tampering or unauthorized alterations aimed at evading detection.
```c#
using System;
using System.Collections.Generic;
using System.IO;
using Walter.IO;

public class PaddingMonitor
{
    private Dictionary<string, long> _lastScanResults = new Dictionary<string, long>();

    public void ScanDirectoryForPaddingChanges(string directoryPath)
    {
        DirectoryInfo directory = new DirectoryInfo(directoryPath);
        FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            long currentPadding = file.CountPaddingBytesChunked();
            if (_lastScanResults.TryGetValue(file.FullName, out long lastPadding))
            {
                if (currentPadding != lastPadding)
                {
                    Console.WriteLine($"Padding change detected in {file.Name}. Previous: {lastPadding}, Current: {currentPadding}");
                }
            }
            else
            {
                Console.WriteLine($"New file detected: {file.Name} with {currentPadding} padding bytes.");
            }

            // Update the last scan results
            _lastScanResults[file.FullName] = currentPadding;
        }
    }
}

```
Instantiate `PaddingMonitor` and call ScanDirectoryForPaddingChanges periodically or upon specific triggers to check for padding changes in the monitored directory.

### File Comparison Extension: FileIsEqualToIgnoringPadding

The `FileIsEqualToIgnoringPadding` extension method is an invaluable tool in the `Walter.IO` package designed to enhance file integrity checks by comparing the binary content of two files, ignoring any trailing padding. This method is particularly useful for identifying files that have been altered to evade hash-based detection mechanisms by adding or modifying padding.

#### Use Case

In cybersecurity, verifying the integrity of files is crucial, especially for system-critical executables like `powershell.exe`. Malicious actors might duplicate and slightly alter such executables (e.g., by adding padding) to bypass security checks. The `FileIsEqualToIgnoringPadding` method enables developers to detect such tampering by comparing the core content of files, excluding padding variations.

### Example: Comparing PowerShell Executable

This example demonstrates how to use `FileIsEqualToIgnoringPadding` to compare a downloaded file with the system's `powershell.exe`, focusing on their content while ignoring padding differences.

```csharp
using System;
using System.IO;
using Walter.IO;

class Program
{
    static void Main(string[] args)
    {
    
        FileInfo originalPowerShell = new FileInfo(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe");
        FileInfo downloadedFile = new FileInfo(@"path\to\downloaded\file.exe"); // Update with the actual file path

        // Compare the original PowerShell executable with a downloaded file, ignoring padding
        bool areFilesEqual = originalPowerShell.FileIsEqualToIgnoringPadding(downloadedFile);

        if (areFilesEqual)
        {
            Console.WriteLine("The downloaded file is identical to powershell.exe, ignoring padding.");
        }
        else
        {
            Console.WriteLine("The downloaded file differs from powershell.exe or does not exist.");
        }
    }
}


### Yuu can use this method to see if malware is using hash signature changes in order to evaid malware detection signatures

```c#
using Walter.IO;

public static partial class Program
{
    public static void Main(string[] args)
    {
        // Define the path to the folder you want to scan. Here, using the Downloads folder as an example.
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        // Ensure the folder exists
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("The specified folder does not exist.");
            return;
        }
        // Create a "poison" file with a specific amount of padding
        var poisonFilePath = Path.Combine(folderPath, $"sample_{DateTime.Now:yy-MM-dd-HH-mm-ss-fff}.bin");
        long totalFileSizeInBytes = 1024 * 1024; // 1 MB total size
        int paddingPercentage = 20; // Target 20% of the file size as padding
        FileWithPaddingCreator.CreateFileWithPadding(poisonFilePath, totalFileSizeInBytes, paddingPercentage);

        // Get all files in the folder
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();

        
        

        Console.WriteLine("Scanning files for significant padding...");

        // Use Parallel.ForEach for efficient multi-threaded processing
        Parallel.ForEach(files, (file) =>
        {
            long fileSize = file.Length;
            if (fileSize == 0) return; // Skip empty files

            // Calculate the padding threshold based on the file size (e.g., 20% of the file size)


            // Use the extension method to count padding bytes
            long paddingBytesCount = file.CountPaddingBytesChunked();
            var padding = file.CountPaddingBytes();


            if (padding > 0)
            {
                decimal percentagePadding = (decimal)padding / file.Length * 100; // Correct percentage calculation
                if (paddingBytesCount > 0)
                {
                    Console.WriteLine($"Padding {file.Name} ({file.Age()} age): {padding:N0} bytes of padding ({percentagePadding:N2}%)");
                }
            }

        });
        if(File.Exists(poisonFilePath)) 
        {
            File.Delete(poisonFilePath);
        }
        Console.WriteLine("Scanning complete.");

#if !DEBUG
        Console.WriteLine("Press any to to exit..");
        Console.ReadKey();
#endif
    }
}
```