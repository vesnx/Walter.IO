public static partial class Program
{
    class FileWithPaddingCreator
    {
        /// <summary>
        /// Creates a file with a specified amount of padding.
        /// </summary>
        /// <param name="filePath">The path where the file will be created.</param>
        /// <param name="totalSizeInBytes">The total size of the file, including content and padding.</param>
        /// <param name="paddingPercentage">The percentage of the total file size that should be padding.</param>
        public static void CreateFileWithPadding(string filePath, long totalSizeInBytes, int paddingPercentage)
        {
            if (paddingPercentage < 0 || paddingPercentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(paddingPercentage), "Padding percentage must be between 0 and 100.");
            }

            // Calculate the size of the content and padding based on the total size and padding percentage
            long paddingSize = (totalSizeInBytes * paddingPercentage) / 100;
            long contentSize = totalSizeInBytes - paddingSize;

            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create the file and write content and padding
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Write content (non-padding bytes)
                if (contentSize > 0)
                {
                    byte[] content = new byte[contentSize];
                    new Random().NextBytes(content); // Fill the content with random bytes
                    fs.Write(content, 0, content.Length);
                }

                // Write padding (zeros)
                if (paddingSize > 0)
                {
                    byte[] padding = new byte[paddingSize];
                    fs.Write(padding, 0, padding.Length); // Padding array is initialized to zeros
                }
            }

            Console.WriteLine($"Created file '{filePath}' with {contentSize} bytes of content and {paddingSize} bytes of padding.");
        }
    }
}