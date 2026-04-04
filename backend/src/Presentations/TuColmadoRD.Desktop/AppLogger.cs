using System.Diagnostics;

namespace TuColmadoRD.Desktop;

internal static class AppLogger
{
    private static readonly object SyncRoot = new();
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TuColmadoRD",
        "logs",
        "desktop-startup.log");

    public static string LogFilePath => LogPath;

    public static void Info(string message) => Write("INFO", message, null);

    public static void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    private static void Write(string level, string message, Exception? exception)
    {
        try
        {
            var directory = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            if (exception != null)
            {
                line += $"{Environment.NewLine}{exception}";
            }

            lock (SyncRoot)
            {
                File.AppendAllText(LogPath, line + Environment.NewLine + Environment.NewLine);
            }

            Debug.WriteLine(line);
        }
        catch
        {
        }
    }
}