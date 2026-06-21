using System.Diagnostics;
using System.Runtime.InteropServices;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Services;

public class ImpresionService
{
    private readonly ILogger<ImpresionService> _logger;

    public ImpresionService(ILogger<ImpresionService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static readonly string[] SumatraPossiblePaths =
    [
        @"C:\SistemaVentas\sumatra\SumatraPDF.exe",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SumatraPDF", "SumatraPDF.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "SumatraPDF", "SumatraPDF.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SumatraPDF", "SumatraPDF.exe"),
    ];

    public async Task<bool> ImprimirFacturaAsync(string rutaPdf, Venta venta, List<DetalleVenta> detalles)
    {
        if (!File.Exists(rutaPdf))
        {
            _logger.LogWarning("PDF no encontrado: {Ruta}", rutaPdf);
            return false;
        }
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _logger.LogWarning("No estamos en Windows, no se puede imprimir");
            return false;
        }

        _logger.LogInformation("Iniciando impresion para venta {Numero}", venta.NumeroVenta);

        var termica = await BuscarImpresoraTermicaAsync();
        if (termica == null)
        {
            _logger.LogWarning("No se encontro impresora termica");
            return false;
        }

        var ok = await ImprimirPDFAsync(rutaPdf, termica);

        if (ok)
            _logger.LogInformation("Factura impresa correctamente en {Impresora}", termica);

        return ok;
    }

    private async Task<bool> ImprimirPDFAsync(string rutaPdf, string printerName)
    {
        if (await ImprimirConSumatraPDFAsync(rutaPdf, printerName))
            return true;

        if (await ImprimirConPowerShellAsync(rutaPdf))
            return true;

        _logger.LogWarning("No se pudo imprimir PDF por ningun metodo");
        return false;
    }

    private async Task<bool> ImprimirConSumatraPDFAsync(string rutaPdf, string printerName)
    {
        try
        {
            var sumatra = SumatraPossiblePaths.FirstOrDefault(File.Exists);
            if (sumatra == null)
            {
                _logger.LogDebug("SumatraPDF no encontrado");
                return false;
            }

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sumatra,
                    Arguments = $"-print-to \"{printerName}\" -silent \"{rutaPdf}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            if (!process.WaitForExit(30000))
                process.Kill();

            var ok = process.ExitCode == 0;
            _logger.LogDebug("SumatraPDF exit code: {Code}", process.ExitCode);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error con SumatraPDF");
            return false;
        }
    }

    private async Task<bool> ImprimirConPowerShellAsync(string rutaPdf)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-WindowStyle Hidden -Command \"Start-Process -FilePath '{rutaPdf}' -Verb Print -WindowStyle Hidden; Start-Sleep -Seconds 5\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            process.WaitForExit(15000);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error con PowerShell Print");
            return false;
        }
    }

    private async Task<string?> BuscarImpresoraTermicaAsync()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = "printer get name",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit(5000);

            var lineas = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var linea in lineas)
            {
                var nombre = linea.Trim();
                if (nombre.Contains("EPSON", StringComparison.OrdinalIgnoreCase) ||
                    nombre.Contains("TM-", StringComparison.OrdinalIgnoreCase) ||
                    nombre.Contains("Termica", StringComparison.OrdinalIgnoreCase) ||
                    nombre.Contains("POS", StringComparison.OrdinalIgnoreCase) ||
                    nombre.Contains("Ticket", StringComparison.OrdinalIgnoreCase) ||
                    nombre.Contains("Recibo", StringComparison.OrdinalIgnoreCase))
                {
                    return nombre;
                }
            }

            _logger.LogDebug("No se encontro impresora termica. Impresoras disponibles: {Lista}", string.Join(" | ", lineas.Select(l => l.Trim())));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al buscar impresoras");
            return null;
        }
    }

    public async Task<bool> ImprimirReciboPDFAsync(Venta venta, List<DetalleVenta> detalles, string printerName)
    {
        try
        {
            var tempPdf = Path.Combine(Path.GetTempPath(), $"recibo_{venta.NumeroVenta}_{Guid.NewGuid():N}.pdf");
            GenerarPDFRecibo(tempPdf, venta, detalles);

            var ok = await ImprimirPDFAsync(tempPdf, printerName);

            try { File.Delete(tempPdf); } catch { }

            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al imprimir recibo PDF");
            return false;
        }
    }

    private static void GenerarPDFRecibo(string ruta, Venta venta, List<DetalleVenta> detalles)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(226.8f, 500f);
                page.Margin(12);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Consolas"));

                page.Content().Column(col =>
                {
                    col.Spacing(2);

                    col.Item().AlignCenter().Text("PAÑALERA Y VARIEDADES\nSAN MIGUEL").Bold().FontSize(11);
                    col.Item().AlignCenter().Text("NIT: 1032465085-1").FontSize(7);
                    col.Item().PaddingVertical(2).LineHorizontal(1);
                    col.Item().AlignCenter().Text("FACTURA DE VENTA (WEB)").Bold();
                    col.Item().PaddingVertical(2).LineHorizontal(1);

                    col.Item().Text($"No: {venta.NumeroVenta}");
                    col.Item().Text($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm}");
                    col.Item().Text($"Cliente: {venta.NombreCliente}");
                    if (!string.IsNullOrEmpty(venta.DocumentoCliente))
                        col.Item().Text($"Documento: {venta.DocumentoCliente}");
                    if (!string.IsNullOrEmpty(venta.TelefonoCliente))
                        col.Item().Text($"Telefono: {venta.TelefonoCliente}");
                    if (!string.IsNullOrEmpty(venta.DireccionCliente))
                        col.Item().Text($"Direccion: {venta.DireccionCliente}");
                    col.Item().Text($"Pago: {venta.MetodoPago}");
                    col.Item().PaddingVertical(2).LineHorizontal(1);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.ConstantColumn(28);
                            c.ConstantColumn(45);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("Producto").Bold();
                            h.Cell().AlignRight().Text("Cant").Bold();
                            h.Cell().AlignRight().Text("Total").Bold();
                        });
                        foreach (var d in detalles)
                        {
                            table.Cell().Text(d.NombreProducto ?? "");
                            table.Cell().AlignRight().Text(d.Cantidad.ToString());
                            table.Cell().AlignRight().Text($"{d.Total,8:C0}");
                        }
                    });

                    col.Item().PaddingVertical(2).LineHorizontal(1);
                    col.Item().AlignRight().Text($"Subtotal: {venta.SubTotal,10:C0}");
                    col.Item().AlignRight().Text($"Total:    {venta.Total,10:C0}").Bold().FontSize(11);
                    col.Item().PaddingVertical(2).LineHorizontal(1);

                    col.Item().PaddingTop(8).AlignCenter().Text("GRACIAS POR SU COMPRA!").Bold().FontSize(10);
                });
            });
        }).GeneratePdf(ruta);
    }
}
