using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;

namespace SistemaVentas.Presentation
{
    public static class PrintHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;

        public static void PrintPdf(string pdfPath)
        {
            if (!File.Exists(pdfPath)) return;

            try
            {
                ProcessStartInfo info = new ProcessStartInfo
                {
                    Verb = "print",
                    FileName = pdfPath,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(info);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al imprimir: {ex.Message}");
            }
        }

        public static void OpenCashDrawer(string printerName = null)
        {
            try
            {
                string printer = string.IsNullOrEmpty(printerName)
                    ? new PrinterSettings().PrinterName
                    : printerName;

                // Comando ESC/POS para abrir caja: ESC p 0 60 120
                byte[] command = { 27, 112, 0, 60, 120 };

                // Intentar escribir directo a la impresora
                string port = GetPrinterPort(printer);
                if (!string.IsNullOrEmpty(port))
                {
                    IntPtr handle = CreateFile(port, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                    if (handle != IntPtr.Zero && handle != new IntPtr(-1))
                    {
                        uint written;
                        WriteFile(handle, command, (uint)command.Length, out written, IntPtr.Zero);
                        CloseHandle(handle);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir caja: {ex.Message}");
            }
        }

        private static string GetPrinterPort(string printerName)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\Print\Printers\" + printerName))
                {
                    if (key != null)
                    {
                        var port = key.GetValue("Port") as string;
                        if (!string.IsNullOrEmpty(port)) return port;
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
