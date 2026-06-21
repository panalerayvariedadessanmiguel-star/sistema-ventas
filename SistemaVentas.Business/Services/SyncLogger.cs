using System.IO;

namespace SistemaVentas.Business.Services
{
    internal static class SyncLogger
    {
        private static readonly string LogPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "sync-debug.log");

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogPath,
                    $"{DateTime.Now:HH:mm:ss.fff} [{Environment.CurrentManagedThreadId}] {message}\n");
            }
            catch { }
        }
    }
}
