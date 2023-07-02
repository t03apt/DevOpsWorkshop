namespace AzureStorageSample.Tests
{
    public static class Utilities
    {
        public static string CreateTempPath(string extension = ".txt")
        {
            return Path.ChangeExtension(Path.GetTempFileName(), extension);
        }

        public static FileInfo CreateTempFile(string content)
        {
            var path = CreateTempPath();
            File.WriteAllText(path, content);
            return new FileInfo(path);
        }
    }
}
