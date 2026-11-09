namespace Taipan.Tests;

/// <summary>
/// Removes a test's temporary directory. Windows keeps a program's files open for a moment after it
/// exits, and a loaded assembly's until the test process ends, so this retries, then leaves the rest.
/// </summary>
internal static class TempDirectory
{
    public static void Remove(string path)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
                return;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                Thread.Sleep(200);
            }
        }
    }
}
