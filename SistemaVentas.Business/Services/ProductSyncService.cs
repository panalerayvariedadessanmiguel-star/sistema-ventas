using System.IO;
using System.Net.Http.Json;
using Dapper;
using SistemaVentas.Data;
using SistemaVentas.Data.Repositories;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Business.Services
{
    public class SyncVariantItem
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = "";
        public string ColorHex { get; set; } = "#000000";
        public string Talla { get; set; } = "";
        public int? Stock { get; set; }
        public string ImagenUrl { get; set; } = "";
        public bool Activo { get; set; } = true;
        public int Orden { get; set; }
    }

    public class ProductSyncItem
    {
        public int LocalId { get; set; }
        public string CodigoBarras { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int CategoriaId { get; set; }
        public string NombreCategoria { get; set; } = "";
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string ImagenUrl { get; set; } = "";
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public List<SyncVariantItem> Variantes { get; set; } = new List<SyncVariantItem>();
    }

    public class ProductSyncResult
    {
        public int LocalId { get; set; }
        public int RemoteId { get; set; }
        public string Nombre { get; set; } = "";
        public bool Creado { get; set; }
    }

    public class ProductSyncService
    {
        private readonly HttpClient _httpClient;
        private static readonly string ApiUrl = "http://localhost:5062/api";
        private readonly ProductoRepository _productoRepo;
        private readonly CategoriaRepository _categoriaRepo;
        private readonly ProductoVarianteRepository _varianteRepo;
        private CancellationTokenSource _cts;
        private DateTime _ultimoPull = DateTime.MinValue;
        private DateTime _ultimoPush = DateTime.MinValue;

        public ProductSyncService()
        {
            _httpClient = new HttpClient();
            _productoRepo = new ProductoRepository();
            _categoriaRepo = new CategoriaRepository();
            _varianteRepo = new ProductoVarianteRepository();
            _ultimoPull = LoadLastSync("UltimoPullProductos");
            _ultimoPush = LoadLastSync("UltimoPushProductos");
        }

        private static DateTime LoadLastSync(string clave)
        {
            try
            {
                using var conn = DbConnection.GetConnection();
                var valor = conn.QueryFirstOrDefault<string>(
                    "SELECT Valor FROM Configuracion WHERE Clave = @Clave", new { Clave = clave });
                if (!string.IsNullOrEmpty(valor) && DateTime.TryParse(valor, out var dt))
                    return dt;
            }
            catch { }
            return DateTime.MinValue;
        }

        private static void SaveLastSync(string clave, DateTime fecha)
        {
            try
            {
                using var conn = DbConnection.GetConnection();
                var existente = conn.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) FROM Configuracion WHERE Clave = @Clave", new { Clave = clave });
                if (existente > 0)
                    conn.Execute("UPDATE Configuracion SET Valor = @Valor WHERE Clave = @Clave", new { Clave = clave, Valor = fecha.ToString("O") });
                else
                    conn.Execute("INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES (@Clave, @Valor, @Desc)",
                        new { Clave = clave, Valor = fecha.ToString("O"), Desc = "Ultima sincronizacion de productos" });
            }
            catch { }
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => SyncLoop(_cts.Token));
        }

        public async Task ForceFullPull()
        {
            _ultimoPull = DateTime.MinValue;
            try { await PullFromWeb(); }
            catch { }
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private async Task SyncLoop(CancellationToken ct)
        {
            // First sync immediately on startup
            try { await PushToWeb(); } catch { }
            try { await PullFromWeb(); } catch { }

            while (!ct.IsCancellationRequested)
            {
                try { await Task.Delay(TimeSpan.FromSeconds(30), ct); }
                catch (TaskCanceledException) { break; }
                try { await PushToWeb(); } catch { }
                try { await PullFromWeb(); } catch { }
            }
        }

        public async Task PushToWeb()
        {
            try
            {
                var productos = _productoRepo.GetModificadosDesde(_ultimoPush);

                if (productos.Count == 0) return;

                var items = productos.Select(p => new ProductSyncItem
                {
                    LocalId = p.Id,
                    CodigoBarras = p.CodigoBarras ?? "",
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion ?? "",
                    CategoriaId = p.CategoriaId,
                    NombreCategoria = p.NombreCategoria ?? "",
                    PrecioCompra = p.PrecioCompra,
                    PrecioVenta = p.PrecioVenta,
                    Stock = p.Stock,
                    StockMinimo = p.StockMinimo,
                    ImagenUrl = "",
                    Orden = 0,
                    Activo = p.Activo,
                    FechaModificacion = p.FechaModificacion > p.FechaCreacion ? p.FechaModificacion : p.FechaCreacion
                }).ToList();

                var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/productos/sync-pos", items);
                if (response.IsSuccessStatusCode)
                {
                    _ultimoPush = DateTime.Now;
                    SaveLastSync("UltimoPushProductos", _ultimoPush);
                }
            }
            catch { }
        }

        public async Task PullFromWeb()
        {
            try
            {
                var url = _ultimoPull == DateTime.MinValue
                    ? $"{ApiUrl}/productos/sync-pos"
                    : $"{ApiUrl}/productos/sync-pos?desde={_ultimoPull:O}";
                SyncLogger.Log($"PullFromWeb: llamando {url}");
                var response = await _httpClient.GetAsync(url);
                SyncLogger.Log($"PullFromWeb: status={response.StatusCode}");
                if (!response.IsSuccessStatusCode) return;

                var remotos = await response.Content.ReadFromJsonAsync<List<ProductSyncItem>>();
                SyncLogger.Log($"PullFromWeb: recibidos {remotos?.Count ?? 0} productos");
                if (remotos == null || remotos.Count == 0) return;

                var categorias = _categoriaRepo.GetAll();
                int nuevos = 0, actualizados = 0;

                foreach (var remoto in remotos)
                {
                    try
                    {
                        var local = string.IsNullOrEmpty(remoto.CodigoBarras)
                            ? _productoRepo.GetByNombre(remoto.Nombre)
                            : _productoRepo.GetByCodigoBarras(remoto.CodigoBarras);

                        int localId;
                        if (local != null)
                        {
                            local.Nombre = remoto.Nombre;
                            local.Descripcion = remoto.Descripcion;
                            local.CategoriaId = ResolverCategoriaLocal(remoto, categorias);
                            local.PrecioCompra = remoto.PrecioCompra;
                            local.PrecioVenta = remoto.PrecioVenta;
                            local.Stock = remoto.Stock;
                            local.StockMinimo = remoto.StockMinimo;
                            local.Activo = remoto.Activo;
                            _productoRepo.UpdateFromSync(local);
                            localId = local.Id;
                            actualizados++;
                        }
                        else
                        {
                            var nuevo = new Producto
                            {
                                CodigoBarras = remoto.CodigoBarras,
                                Nombre = remoto.Nombre,
                                Descripcion = remoto.Descripcion,
                                CategoriaId = ResolverCategoriaLocal(remoto, categorias),
                                PrecioCompra = remoto.PrecioCompra,
                                PrecioVenta = remoto.PrecioVenta,
                                Stock = remoto.Stock,
                                StockMinimo = remoto.StockMinimo,
                                Activo = true
                            };
                            localId = _productoRepo.Insert(nuevo);
                            nuevos++;
                        }

                        // Sync variants
                        if (remoto.Variantes != null && remoto.Variantes.Count > 0)
                        {
                            SyncLogger.Log($"  Producto '{remoto.Nombre}' (localId={localId}): {remoto.Variantes.Count} variantes");
                            var variantesLocales = _varianteRepo.GetByProducto(localId);
                            SyncLogger.Log($"    Variantes locales encontradas: {variantesLocales.Count}");
                            foreach (var vr in remoto.Variantes)
                            {
                                // Match by Id first, then by business key (Nombre+ColorHex+Talla)
                                var vl = variantesLocales.FirstOrDefault(v => v.Id == vr.Id);
                                if (vl == null)
                                {
                                    vl = variantesLocales.FirstOrDefault(v =>
                                        v.Nombre == vr.Nombre && v.ColorHex == vr.ColorHex && v.Talla == vr.Talla);
                                }

                                if (vl != null)
                                {
                                    SyncLogger.Log($"    Actualizando variante local Id={vl.Id} '{vr.Nombre}'");
                                    vl.Nombre = vr.Nombre;
                                    vl.ColorHex = vr.ColorHex;
                                    vl.Talla = vr.Talla;
                                    vl.Stock = vr.Stock;
                                    vl.ImagenUrl = vr.ImagenUrl;
                                    vl.Activo = vr.Activo;
                                    vl.Orden = vr.Orden;
                                    _varianteRepo.UpdateFromSync(vl);
                                }
                                else
                                {
                                    SyncLogger.Log($"    Insertando nueva variante Id={vr.Id} '{vr.Nombre}' (useProvidedId=true)");
                                    _varianteRepo.Insert(new ProductoVariante
                                    {
                                        ProductoId = localId,
                                        Id = vr.Id,
                                        Nombre = vr.Nombre,
                                        ColorHex = vr.ColorHex,
                                        Talla = vr.Talla ?? string.Empty,
                                        Stock = vr.Stock,
                                        ImagenUrl = vr.ImagenUrl,
                                        Activo = vr.Activo,
                                        Orden = vr.Orden
                                    }, useProvidedId: true);
                                }
                            }
                        }
                        else if (remoto.Variantes != null && remoto.Variantes.Count == 0)
                        {
                            // Product has no variants, delete old ones if any
                            var variantesLocales = _varianteRepo.GetByProducto(localId);
                            if (variantesLocales.Count > 0)
                            {
                                SyncLogger.Log($"  Producto '{remoto.Nombre}': eliminando {variantesLocales.Count} variantes locales (ya no existen)");
                                _varianteRepo.DeleteByProducto(localId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SyncLogger.Log($"  ERROR en producto '{remoto?.Nombre}': {ex.Message}");
                    }
                }

                _ultimoPull = DateTime.Now;
                SaveLastSync("UltimoPullProductos", _ultimoPull);
                SyncLogger.Log($"PullFromWeb: completado. Nuevos={nuevos}, Actualizados={actualizados}");
            }
            catch (Exception ex)
            {
                SyncLogger.Log($"PullFromWeb: ERROR GENERAL: {ex.Message}");
            }
        }

        private int ResolverCategoriaLocal(ProductSyncItem producto, List<Categoria> categoriasLocal)
        {
            var match = categoriasLocal.FirstOrDefault(c =>
                c.Nombre.Equals(producto.NombreCategoria, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                return match.Id;

            var nombre = string.IsNullOrEmpty(producto.NombreCategoria) ? "General" : producto.NombreCategoria;
            var nuevoId = _categoriaRepo.Insert(new Categoria
            {
                Nombre = nombre,
                Descripcion = "Sincronizada desde web",
                Activo = true,
                FechaCreacion = DateTime.Now
            });
            categoriasLocal.Add(new Categoria { Id = nuevoId, Nombre = nombre });
            return nuevoId;
        }
    }
}
