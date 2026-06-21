using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using SistemaVentas.Data;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Business.Services
{
    public class SincronizacionService
    {
        private readonly HttpClient _httpClient;
        private static readonly string ApiUrl = "https://sistema-ventas-api-6x1w.onrender.com/api";

        public SincronizacionService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> SincronizarVentaAsync(Venta venta, List<DetalleVenta> detalles)
        {
            try
            {
                var dto = new
                {
                    clienteId = venta.ClienteId,
                    metodoPago = venta.MetodoPago,
                    montoPagado = venta.MontoPagado,
                    cambio = venta.Cambio,
                    usuario = venta.Usuario,
                    origen = "Fisico",
                    domicilio = 0m,
                    detalles = detalles.Select(d => new
                    {
                        productoId = d.ProductoId,
                        productoVarianteId = d.ProductoVarianteId,
                        cantidad = d.Cantidad,
                        precioUnitario = d.PrecioUnitario,
                        costoUnitario = d.CostoUnitario
                    }).ToList()
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/ventas", dto);
                if (response.IsSuccessStatusCode)
                {
                    await MarkAsSyncedLocal(venta.Id);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SincronizarConColaAsync(Venta venta, List<DetalleVenta> detalles)
        {
            var exito = await SincronizarVentaAsync(venta, detalles);
            if (!exito)
            {
                await EnqueuePendienteAsync(venta, detalles);
            }
            return exito;
        }

        private async Task EnqueuePendienteAsync(Venta venta, List<DetalleVenta> detalles)
        {
            try
            {
                using var conn = DbConnection.GetConnection();
                var jsonVenta = JsonSerializer.Serialize(venta);
                var jsonDetalles = JsonSerializer.Serialize(detalles);
                var sql = @"INSERT INTO SincronizacionPendiente (VentaId, JsonVenta, JsonDetalles)
                            VALUES (@VentaId, @JsonVenta, @JsonDetalles)";
                await conn.ExecuteAsync(sql, new { venta.Id, JsonVenta = jsonVenta, JsonDetalles = jsonDetalles });
            }
            catch { }
        }

        private async Task MarkAsSyncedLocal(int ventaId)
        {
            try
            {
                using var conn = DbConnection.GetConnection();
                await conn.ExecuteAsync("UPDATE Ventas SET SincronizadoAPI = 1 WHERE Id = @Id", new { Id = ventaId });
            }
            catch { }
        }

        public async Task<int> ProcessPendingAsync()
        {
            int procesados = 0;
            try
            {
                using var conn = DbConnection.GetConnection();
                var pendientes = (await conn.QueryAsync<dynamic>(
                    "SELECT * FROM SincronizacionPendiente WHERE Intentos < 10 ORDER BY FechaCreacion ASC")).ToList();

                foreach (var item in pendientes)
                {
                    try
                    {
                        var venta = JsonSerializer.Deserialize<Venta>(item.JsonVenta);
                        var detalles = JsonSerializer.Deserialize<List<DetalleVenta>>(item.JsonDetalles);
                        if (venta == null || detalles == null) continue;

                        var exito = await SincronizarVentaAsync(venta, detalles);
                        if (exito)
                        {
                            await conn.ExecuteAsync("DELETE FROM SincronizacionPendiente WHERE Id = @Id", new { Id = item.Id });
                            procesados++;
                        }
                        else
                        {
                            await conn.ExecuteAsync(
                                "UPDATE SincronizacionPendiente SET Intentos = Intentos + 1, FechaUltimoIntento = GETDATE() WHERE Id = @Id",
                                new { Id = item.Id });
                        }
                    }
                    catch (Exception ex)
                    {
                        await conn.ExecuteAsync(
                            "UPDATE SincronizacionPendiente SET Intentos = Intentos + 1, UltimoError = @Error, FechaUltimoIntento = GETDATE() WHERE Id = @Id",
                            new { Id = item.Id, Error = ex.Message });
                    }
                }
            }
            catch { }
            return procesados;
        }
    }
}
