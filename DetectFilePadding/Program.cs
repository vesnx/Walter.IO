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
                    Console.WriteLine($"Padding {file.Name}: {padding:N0} bytes of padding ({percentagePadding:N2}%)");
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