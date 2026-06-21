using System;
using System.Collections.Generic;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaVentas.Data.Models;
using System.Data;
using Dapper;

namespace SistemaVentas.Business.Services
{
    public class FacturacionService
    {
        private readonly string _rutaFacturas = @"C:\Users\Familia_Jica\Desktop\Pañalera y Variedades San Miguel\Facturas de Venta";

        public FacturacionService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public string GenerarFactura(Venta venta, List<DetalleVenta> detalles)
        {
            if (!Directory.Exists(_rutaFacturas))
                Directory.CreateDirectory(_rutaFacturas);

            string nombreArchivo = $"Factura-{venta.NumeroVenta}-{DateTime.Now:yyyyMMdd-HHmmss}.pdf";
            string rutaCompleta = Path.Combine(_rutaFacturas, nombreArchivo);

            // Leer configuración directamente
            string nit = LeerConfig("NIT", "123456789-0");
            string direccion = LeerConfig("DIRECCION", "Calle 123 #45-67, Bogotá D.C.");
            string telefono = LeerConfig("TELEFONO", "601 123 4567");
            string nombreEmpresa = LeerConfig("NOMBRE_EMPRESA", "Sistema de Ventas SAS");
            
            string nombreCliente = string.IsNullOrEmpty(venta.NombreCliente) ? "Consumidor Final" : venta.NombreCliente;
            string documentoCliente = "";
            if (!string.IsNullOrEmpty(venta.TipoDocumentoCliente) || !string.IsNullOrEmpty(venta.DocumentoCliente))
            {
                documentoCliente = $"{venta.TipoDocumentoCliente} {venta.DocumentoCliente}".Trim();
            }

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(226.8f, 800f);
                    page.Margin(15);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Encabezado
                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text(nombreEmpresa).Bold().FontSize(14);
                        col.Item().AlignCenter().Text($"NIT: {nit}").FontSize(10);
                        col.Item().AlignCenter().Text($"{direccion}").FontSize(10);
                        col.Item().AlignCenter().Text($"Tel: {telefono}").FontSize(10);
                        col.Item().Height(8);
                        col.Item().AlignCenter().Text("FACTURA DE VENTA").Bold().FontSize(12);
                        col.Item().AlignCenter().Text("----------------------------------------");
                    });

                    // Contenido
                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        // Datos de la factura
                        col.Item().Column(c =>
                        {
                            c.Item().Text($"No: {venta.NumeroVenta}").FontSize(10).Bold();
                            c.Item().Text($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm:ss}").FontSize(10);
                            c.Item().Text($"Cliente: {nombreCliente}").FontSize(10);
                            if (!string.IsNullOrEmpty(documentoCliente))
                            {
                                c.Item().Text($"Doc: {documentoCliente}").FontSize(10);
                            }
                            c.Item().Text($"Usuario: {venta.Usuario}").FontSize(10);
                            c.Item().Text($"Pago: {venta.MetodoPago}").FontSize(10);
                        });

                        col.Item().Height(8);
                        col.Item().LineHorizontal(1f);
                        col.Item().Height(8);

                        // Tabla de productos
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
                                string nombreProd = (detalle.NombreProducto ?? "").Length > 20
                                    ? detalle.NombreProducto.Substring(0, 17) + "..."
                                    : detalle.NombreProducto;

                                table.Cell().Padding(3).Text(nombreProd).FontSize(10);
                                table.Cell().Padding(3).AlignRight().Text(detalle.Cantidad.ToString()).FontSize(10);
                                table.Cell().Padding(3).AlignRight().Text(detalle.PrecioUnitario.ToString("C0")).FontSize(10);
                                table.Cell().Padding(3).AlignRight().Text(detalle.Total.ToString("C0")).FontSize(10);
                            }
                        });

                        col.Item().Height(8);
                        col.Item().LineHorizontal(1f);
                        col.Item().Height(8);

                        // Totales
                        col.Item().Column(c =>
                        {
                            c.Item().AlignRight().Text($"Subtotal: {venta.SubTotal:C0}").FontSize(11);
                            c.Item().AlignRight().Text($"Impuesto: {venta.Impuesto:C0}").FontSize(11);
                            c.Item().AlignRight().Text($"TOTAL: {venta.Total:C0}").Bold().FontSize(13);
                            c.Item().Height(5);
                            c.Item().AlignRight().Text($"Pagado: {venta.MontoPagado:C0}").FontSize(10);
                            c.Item().AlignRight().Text($"Cambio: {venta.Cambio:C0}").FontSize(10);
                        });
                    });

                    // Pie de página
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1f);
                        col.Item().Height(5);
                        col.Item().AlignCenter().Text("GRACIAS POR SU COMPRA!").Bold().FontSize(11);
                        col.Item().AlignCenter().Text("----------------------------------------");
                    });
                });
            }).GeneratePdf(rutaCompleta);

            return rutaCompleta;
        }

        private string LeerConfig(string clave, string valorPorDefecto)
        {
            using var connection = SistemaVentas.Data.DbConnection.GetConnection();
            var valor = connection.QueryFirstOrDefault<string>(
                "SELECT Valor FROM Configuracion WHERE Clave = @Clave",
                new { Clave = clave });
            return string.IsNullOrEmpty(valor) ? valorPorDefecto : valor;
        }
    }
}
