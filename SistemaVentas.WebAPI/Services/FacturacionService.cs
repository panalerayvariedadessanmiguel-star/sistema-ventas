using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaVentas.WebAPI.Repositories;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Services;

public class FacturacionService
{
    private readonly ConfiguracionRepository _configRepo;
    private readonly ClienteRepository _clienteRepo;

    public FacturacionService(ConfiguracionRepository configRepo, ClienteRepository clienteRepo)
    {
        _configRepo = configRepo;
        _clienteRepo = clienteRepo;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerarFacturaAsync(Venta venta, List<DetalleVenta> detalles)
    {
        var configs = await _configRepo.GetAllAsync();
        var dict = configs.ToDictionary(c => c.Clave, c => c.Valor);

        var nombreEmpresa = dict.GetValueOrDefault("SITE_TITLE", "Tienda Online");
        var nit = dict.GetValueOrDefault("INFO_NIT", "N/A");
        var direccion = dict.GetValueOrDefault("INFO_DIRECCION", "");
        var telefono = dict.GetValueOrDefault("INFO_TELEFONO", "");

        string nombreCliente = venta.NombreCliente;
        string documentoCliente = "";

        if (string.IsNullOrEmpty(nombreCliente) && venta.ClienteId.HasValue)
        {
            var cliente = await _clienteRepo.GetByIdAsync(venta.ClienteId.Value);
            if (cliente != null)
            {
                nombreCliente = cliente.Nombre;
                documentoCliente = cliente.Documento;
            }
        }

        if (string.IsNullOrEmpty(nombreCliente))
            nombreCliente = "Consumidor Final";

        var rutaBase = @"C:\Users\Familia_Jica\Desktop\Pañalera y Variedades San Miguel\Facturas de Venta Web";
        if (!Directory.Exists(rutaBase))
            Directory.CreateDirectory(rutaBase);

        string nombreArchivo = $"Factura-{venta.NumeroVenta}-{DateTime.Now:yyyyMMdd-HHmmss}.pdf";
        string rutaCompleta = Path.Combine(rutaBase, nombreArchivo);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(226.8f, 800f);
                page.Margin(15);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text(nombreEmpresa).Bold().FontSize(14);
                    col.Item().AlignCenter().Text($"NIT: {nit}").FontSize(10);
                    if (!string.IsNullOrEmpty(direccion))
                        col.Item().AlignCenter().Text(direccion).FontSize(10);
                    if (!string.IsNullOrEmpty(telefono))
                        col.Item().AlignCenter().Text($"Tel: {telefono}").FontSize(10);
                    col.Item().Height(8);
                    col.Item().AlignCenter().Text("FACTURA DE VENTA").Bold().FontSize(12);
                    col.Item().AlignCenter().Text("----------------------------------------");
                });

                page.Content().PaddingVertical(5).Column(col =>
                {
                    col.Item().Column(c =>
                    {
                        c.Item().Text($"No: {venta.NumeroVenta}").FontSize(10).Bold();
                        c.Item().Text($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm:ss}").FontSize(10);
                        c.Item().Text($"Cliente: {nombreCliente}").FontSize(10);
                        if (!string.IsNullOrEmpty(documentoCliente))
                            c.Item().Text($"Documento: {documentoCliente}").FontSize(10);
                        if (!string.IsNullOrEmpty(venta.TelefonoCliente))
                            c.Item().Text($"Telefono: {venta.TelefonoCliente}").FontSize(10);
                        if (!string.IsNullOrEmpty(venta.DireccionCliente))
                            c.Item().Text($"Direccion: {venta.DireccionCliente}").FontSize(10);
                        c.Item().Text($"Pago: {venta.MetodoPago}").FontSize(10);
                        c.Item().Text($"Origen: Web").FontSize(10);
                    });

                    col.Item().Height(8);
                    col.Item().LineHorizontal(1f);
                    col.Item().Height(8);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Padding(3).Text("Producto").Bold().FontSize(10);
                            header.Cell().Padding(3).AlignRight().Text("Cant").Bold().FontSize(10);
                            header.Cell().Padding(3).AlignRight().Text("P.Unit").Bold().FontSize(10);
                            header.Cell().Padding(3).AlignRight().Text("Total").Bold().FontSize(10);
                        });

                        foreach (var detalle in detalles)
                        {
                            var nombreProd = detalle.NombreProducto ?? "";
                            if (nombreProd.Length > 20)
                                nombreProd = nombreProd.Substring(0, 17) + "...";

                            table.Cell().Padding(3).Text(nombreProd).FontSize(10);
                            table.Cell().Padding(3).AlignRight().Text(detalle.Cantidad.ToString()).FontSize(10);
                            table.Cell().Padding(3).AlignRight().Text(detalle.PrecioUnitario.ToString("C0")).FontSize(10);
                            table.Cell().Padding(3).AlignRight().Text(detalle.Total.ToString("C0")).FontSize(10);
                        }
                    });

                    col.Item().Height(8);
                    col.Item().LineHorizontal(1f);
                    col.Item().Height(8);

                    col.Item().Column(c =>
                    {
                        c.Item().AlignRight().Text($"Subtotal: {venta.SubTotal:C0}").FontSize(11);
                        var domicilio = venta.Total - venta.SubTotal - venta.Impuesto;
                        if (domicilio > 0)
                            c.Item().AlignRight().Text($"Domicilio: {domicilio:C0}").FontSize(11);
                        if (venta.Impuesto > 0)
                            c.Item().AlignRight().Text($"Impuesto: {venta.Impuesto:C0}").FontSize(11);
                        c.Item().AlignRight().Text($"TOTAL: {venta.Total:C0}").Bold().FontSize(13);
                    });
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1f);
                    col.Item().Height(5);
                    col.Item().AlignCenter().Text("GRACIAS POR SU COMPRA!").Bold().FontSize(11);
                    col.Item().AlignCenter().Text("----------------------------------------");
                    col.Item().AlignCenter().Text("Pedido sincronizado con la tienda web").FontSize(8);
                });
            });
        }).GeneratePdf(rutaCompleta);

        return rutaCompleta;
    }
}
