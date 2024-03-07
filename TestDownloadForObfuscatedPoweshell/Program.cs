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
