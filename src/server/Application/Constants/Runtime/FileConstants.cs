namespace Application.Constants.Runtime;

public static class FileConstants
{
    public const int BufferSize = 131_072;  // Based on testing appears to be the most optimal buffer size, See: https://stackoverflow.com/a/56091135
    public static bool IsLargeFile(int sizeBytes) => sizeBytes > 999_999;  // Over ~1GB benefits in asynchronous data streams, See: https://stackoverflow.com/a/56091135
    public static bool IsLargeFile(long sizeBytes) => sizeBytes > 999_999;
}