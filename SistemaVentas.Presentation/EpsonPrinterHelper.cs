using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public static class EpsonPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOCINFOA pDocInfo);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool WritePrinter(IntPtr hPrinter, byte[] pBuf, int cbBuf, out uint pcWritten);

        // Comandos ESC/POS
        private static readonly byte[] INIT = { 0x1B, 0x40 };
        private static readonly byte[] CUT = { 0x1D, 0x56, 0x41, 0x00 };
        private static readonly byte[] OPEN_DRAWER = { 0x1B, 0x70, 0x00, 0x3C, 0x78 };
        private static readonly byte[] CENTER = { 0x1B, 0x61, 0x01 };
        private static readonly byte[] LEFT = { 0x1B, 0x61, 0x00 };
        private static readonly byte[] BOLD_ON = { 0x1B, 0x45, 0x01 };
        private static readonly byte[] BOLD_OFF = { 0x1B, 0x45, 0x00 };
        private static readonly byte[] LINE_FEED = { 0x0A };

        public static void ImprimirFacturaEpson(string nombreImpresora, string nombreEmpresa, string nit, string direccion, string telefono, Venta venta, List<DetalleVenta> detalles, string nombreCliente, string documentoCliente)
        {
            try
            {
                var commands = new List<byte>();

                // Inicializar
                commands.AddRange(INIT);

                // Centrar
                commands.AddRange(CENTER);

                // Encabezado
                commands.AddRange(BOLD_ON);
                commands.AddRange(Encoding.GetEncoding(437).GetBytes(nombreEmpresa + "\n"));
                commands.AddRange(BOLD_OFF);

                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"NIT: {nit}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"{direccion}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Tel: {telefono}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("--------------------------------\n"));

                commands.AddRange(BOLD_ON);
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("FACTURA DE VENTA\n"));
                commands.AddRange(BOLD_OFF);
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("--------------------------------\n"));

                // Alinear a la izquierda
                commands.AddRange(LEFT);

                // Datos de la factura
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"No: {venta.NumeroVenta}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm:ss}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Cliente: {nombreCliente}\n"));
                if (!string.IsNullOrEmpty(documentoCliente))
                {
                    commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Doc: {documentoCliente}\n"));
                }
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Usuario: {venta.Usuario}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Pago: {venta.MetodoPago}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("--------------------------------\n"));

                // Tabla de productos
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("Producto         Cant  P.Unit Total\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("--------------------------------\n"));

                foreach (var detalle in detalles)
                {
                    string nombre = (detalle.NombreProducto ?? "").Length > 17
                        ? detalle.NombreProducto.Substring(0, 17)
                        : detalle.NombreProducto.PadRight(17);

                    string linea = $"{nombre} {detalle.Cantidad.ToString().PadLeft(4)} ${detalle.PrecioUnitario.ToString("N0").PadLeft(5)} ${detalle.Total.ToString("N0").PadLeft(4)}\n";
                    commands.AddRange(Encoding.GetEncoding(437).GetBytes(linea));
                }

                commands.AddRange(Encoding.GetEncoding(437).GetBytes("--------------------------------\n"));

                // Totales
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Subtotal: ${venta.SubTotal.ToString("N0").PadLeft(19)}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Impuesto: ${venta.Impuesto.ToString("N0").PadLeft(19)}\n"));

                commands.AddRange(BOLD_ON);
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"TOTAL: ${venta.Total.ToString("N0").PadLeft(21)}\n"));
                commands.AddRange(BOLD_OFF);

                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Pagado: ${venta.MontoPagado.ToString("N0").PadLeft(21)}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes($"Cambio: ${venta.Cambio.ToString("N0").PadLeft(21)}\n"));
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("--------------------------------\n"));

                // Pie de página
                commands.AddRange(CENTER);
                commands.AddRange(BOLD_ON);
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("GRACIAS POR SU COMPRA!\n"));
                commands.AddRange(BOLD_OFF);
                commands.AddRange(Encoding.GetEncoding(437).GetBytes("--------------------------------\n"));

                // Avance de papel
                for (int i = 0; i < 3; i++)
                    commands.AddRange(LINE_FEED);

                // Corte
                commands.AddRange(CUT);

                // Enviar a impresora
                byte[] data = commands.ToArray();
                IntPtr hPrinter;

                if (!OpenPrinter(nombreImpresora, out hPrinter, IntPtr.Zero))
                {
                    throw new Exception($"No se pudo abrir la impresora: {nombreImpresora}");
                }

                try
                {
                    DOCINFOA docInfo = new DOCINFOA
                    {
                        pDocName = "Factura Venta",
                        pDataType = "RAW"
                    };

                    if (!StartDocPrinter(hPrinter, 1, ref docInfo))
                    {
                        throw new Exception("Error al iniciar documento");
                    }

                    try
                    {
                        if (!StartPagePrinter(hPrinter))
                        {
                            throw new Exception("Error al iniciar página");
                        }

                        try
                        {
                            uint written;
                            if (!WritePrinter(hPrinter, data, data.Length, out written))
                            {
                                throw new Exception("Error al escribir en impresora");
                            }
                        }
                        finally
                        {
                            EndPagePrinter(hPrinter);
                        }
                    }
                    finally
                    {
                        EndDocPrinter(hPrinter);
                    }
                }
                finally
                {
                    ClosePrinter(hPrinter);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al imprimir: {ex.Message}");
            }
        }

        public static void AbrirCaja(string nombreImpresora)
        {
            try
            {
                IntPtr hPrinter;
                if (!OpenPrinter(nombreImpresora, out hPrinter, IntPtr.Zero))
                    return;

                try
                {
                    DOCINFOA docInfo = new DOCINFOA
                    {
                        pDocName = "Abrir Caja",
                        pDataType = "RAW"
                    };

                    if (StartDocPrinter(hPrinter, 1, ref docInfo))
                    {
                        try
                        {
                            if (StartPagePrinter(hPrinter))
                            {
                                try
                                {
                                    uint written;
                                    WritePrinter(hPrinter, OPEN_DRAWER, OPEN_DRAWER.Length, out written);
                                }
                                finally
                                {
                                    EndPagePrinter(hPrinter);
                                }
                            }
                        }
                        finally
                        {
                            EndDocPrinter(hPrinter);
                        }
                    }
                }
                finally
                {
                    ClosePrinter(hPrinter);
                }
            }
            catch { }
        }

        public static string DetectarImpresoraEpson()
        {
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                if (printer.ToLower().Contains("epson") || printer.ToLower().Contains("tm"))
                {
                    return printer;
                }
            }
            // Devolver la impresora predeterminada
            return new System.Drawing.Printing.PrinterSettings().PrinterName;
        }
    }
}
