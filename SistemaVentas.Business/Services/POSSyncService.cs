using System.Net.Http.Json;
using SistemaVentas.Data.Repositories;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Business.Services
{
    public class POSSyncService
    {
        private readonly HttpClient _httpClient;
        private static readonly string ApiUrl = "http://localhost:5062/api";
        private readonly ProductoRepository _productoRepo;
        private readonly ProductoVarianteRepository _varianteRepo;
        private readonly StockService _stockService;
        private readonly ContabilidadService _contabilidadService;
        private readonly SincronizacionService _sincronizacionService;
        private CancellationTokenSource _cts;

        public POSSyncService()
        {
            _httpClient = new HttpClient();
            _productoRepo = new ProductoRepository();
            _varianteRepo = new ProductoVarianteRepository();
            _stockService = new StockService();
            _contabilidadService = new ContabilidadService();
            _sincronizacionService = new SincronizacionService();
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => SyncLoop(_cts.Token));
        }

        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }

        private async Task SyncLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await SincronizarStockAsync();
                }
                catch { }

                try
                {
                    await _sincronizacionService.ProcessPendingAsync();
                }
                catch { }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private void AddPosToken(HttpRequestMessage req)
        {
            req.Headers.Add("xPosToken", "S1st3m4V3nt4s-P05-2024");
        }

        public async Task SincronizarStockAsync()
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/ventas/pendientes-pos");
                AddPosToken(req);
                var response = await _httpClient.SendAsync(req);
                if (!response.IsSuccessStatusCode) return;

                var pendientes = await response.Content.ReadFromJsonAsync<List<POSSyncItem>>();
                if (pendientes == null || pendientes.Count == 0) return;

                var ventasProcesadas = new HashSet<int>();

                foreach (var item in pendientes)
                {
                    try
                    {
                        var markReq = new HttpRequestMessage(HttpMethod.Post, $"{ApiUrl}/ventas/marcar-sincronizado/{item.Id}");
                        AddPosToken(markReq);
                        var markResponse = await _httpClient.SendAsync(markReq);
                        if (!markResponse.IsSuccessStatusCode) continue;

                        var producto = _productoRepo.GetById(item.ProductoId);
                        if (producto == null) continue;

                        if (producto.Stock >= item.Cantidad)
                        {
                            _productoRepo.ActualizarStock(item.ProductoId, item.Cantidad, esEntrada: false);

                            if (item.ProductoVarianteId.HasValue)
                            {
                                _varianteRepo.UpdateStock(item.ProductoVarianteId.Value, item.Cantidad);
                            }

                            int stockAnterior = producto.Stock;
                            int stockNuevo = stockAnterior - item.Cantidad;
                            _stockService.RegistrarMovimiento(
                                item.ProductoId,
                                DateTime.Now.Year,
                                DateTime.Now.Month,
                                0,
                                item.Cantidad,
                                stockNuevo
                            );
                        }

                        ventasProcesadas.Add(item.VentaId);
                    }
                    catch { }
                }

                foreach (var ventaId in ventasProcesadas)
                {
                    try
                    {
                        var itemsVenta = pendientes.Where(p => p.VentaId == ventaId).ToList();
                        if (itemsVenta.Count == 0) continue;

                        var first = itemsVenta.First();
                        var conceptoIngreso = "Venta " + first.NumeroVenta;
                        var conceptoCosto = "Costo " + first.NumeroVenta;

                        var txRepo = new TransaccionRepository();
                        if (!txRepo.ExistePorConcepto(conceptoIngreso))
                        {
                            txRepo.Insert(new Transaccion
                            {
                                Fecha = first.Fecha,
                                Tipo = "Ingreso",
                                Categoria = "Ventas del Dia",
                                Concepto = conceptoIngreso,
                                Monto = first.Total,
                                Usuario = "WEB"
                            });
                        }

                        if (!txRepo.ExistePorConcepto(conceptoCosto))
                        {
                            decimal costoTotal = itemsVenta.Sum(i => i.CostoUnitario * i.Cantidad);
                            txRepo.Insert(new Transaccion
                            {
                                Fecha = first.Fecha,
                                Tipo = "Gasto",
                                Categoria = "Costo de Ventas",
                                Concepto = conceptoCosto,
                                Monto = costoTotal,
                                Usuario = "WEB"
                            });
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }

    public class POSSyncItem
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public int? ProductoVarianteId { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public bool Sincronizado { get; set; }
        public string ProductoNombre { get; set; } = "";
        public string NumeroVenta { get; set; } = "";
        public decimal Total { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
