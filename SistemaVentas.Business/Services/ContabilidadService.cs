using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class ContabilidadService
    {
        private readonly TransaccionRepository _repo;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private static readonly string ApiUrl = "https://sistema-ventas-api-6x1w.onrender.com/api";

        public ContabilidadService()
        {
            _repo = new TransaccionRepository();
        }

        public void Registrar(string tipo, string categoria, string concepto, decimal monto, string usuario)
        {
            _repo.Insert(new Transaccion
            {
                Fecha = DateTime.Now,
                Tipo = tipo,
                Categoria = categoria,
                Concepto = concepto,
                Monto = monto,
                Usuario = usuario
            });
        }

        public void RegistrarConFecha(DateTime fecha, string tipo, string categoria, string concepto, decimal monto, string usuario)
        {
            _repo.Insert(new Transaccion
            {
                Fecha = fecha,
                Tipo = tipo,
                Categoria = categoria,
                Concepto = concepto,
                Monto = monto,
                Usuario = usuario
            });
        }

        public void Eliminar(int id)
        {
            _repo.Delete(id);
        }

        public List<Transaccion> GetTransacciones(DateTime desde, DateTime hasta)
        {
            return _repo.GetByDateRange(desde, hasta);
        }

        public Transaccion GetById(int id)
        {
            return _repo.GetById(id);
        }

        public decimal GetTotalIngresos(DateTime desde, DateTime hasta)
        {
            return _repo.GetTotalIngresos(desde, hasta);
        }

        public decimal GetTotalGastos(DateTime desde, DateTime hasta)
        {
            return _repo.GetTotalGastos(desde, hasta);
        }

        public List<dynamic> GetResumenPorCategoria(DateTime desde, DateTime hasta)
        {
            return _repo.GetResumenPorCategoria(desde, hasta).ToList();
        }

        public int ImportarVentasPasadas()
        {
            var ventaRepo = new VentaRepository();
            var ventas = ventaRepo.GetAll();
            int count = 0;

            foreach (var v in ventas.Where(v => v.Anulada))
            {
                _repo.DeleteByConcepto("Venta " + v.NumeroVenta);
                _repo.DeleteByConcepto("Costo " + v.NumeroVenta);
            }

            foreach (var v in ventas.Where(v => !v.Anulada))
                count += ImportarVenta(v, ventaRepo);

            try
            {
                var webVentas = _httpClient.GetFromJsonAsync<List<VentaWebItem>>($"{ApiUrl}/ventas").Result;
                if (webVentas != null)
                {
                    foreach (var wv in webVentas.Where(wv => wv.Anulada))
                    {
                        _repo.DeleteByConcepto("Venta " + wv.NumeroVenta);
                        _repo.DeleteByConcepto("Costo " + wv.NumeroVenta);
                    }

                    foreach (var wv in webVentas.Where(wv => !wv.Anulada))
                    {
                        var conceptoIngreso = "Venta " + wv.NumeroVenta;
                        var conceptoCosto = "Costo " + wv.NumeroVenta;

                        if (!_repo.ExistePorConcepto(conceptoIngreso))
                        {
                            _repo.Insert(new Transaccion
                            {
                                Fecha = wv.FechaVenta,
                                Tipo = "Ingreso",
                                Categoria = "Ventas del Dia",
                                Concepto = conceptoIngreso,
                                Monto = wv.Total,
                                Usuario = "WEB"
                            });
                            count++;
                        }

                        if (!_repo.ExistePorConcepto(conceptoCosto))
                        {
                            try
                            {
                                var response = _httpClient.GetFromJsonAsync<VentaDetalleResponse>(
                                    $"{ApiUrl}/ventas/{wv.Id}").Result;
                                if (response?.detalles != null)
                                {
                                    decimal costoTotal = 0;
                                    foreach (var d in response.detalles)
                                        costoTotal += d.CostoUnitario * d.Cantidad;

                                    _repo.Insert(new Transaccion
                                    {
                                        Fecha = wv.FechaVenta,
                                        Tipo = "Gasto",
                                        Categoria = "Costo de Ventas",
                                        Concepto = conceptoCosto,
                                        Monto = costoTotal,
                                        Usuario = "WEB"
                                    });
                                    count++;
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }

            return count;
        }

        private int ImportarVenta(dynamic v, VentaRepository ventaRepo)
        {
            string numVenta;
            try { numVenta = v.NumeroVenta; } catch { return 0; }

            var conceptoIngreso = "Venta " + numVenta;
            var conceptoCosto = "Costo " + numVenta;
            int count = 0;

            if (!_repo.ExistePorConcepto(conceptoIngreso))
            {
                _repo.Insert(new Transaccion
                {
                    Fecha = v.FechaVenta,
                    Tipo = "Ingreso",
                    Categoria = "Ventas del Dia",
                    Concepto = conceptoIngreso,
                    Monto = v.Total,
                    Usuario = v.Usuario
                });
                count++;
            }

            if (!_repo.ExistePorConcepto(conceptoCosto))
            {
                try
                {
                    var detalles = ventaRepo.GetDetallesByVentaId(v.Id);
                    decimal costoTotal = 0;
                    foreach (var d in detalles)
                        costoTotal += d.CostoUnitario * d.Cantidad;

                    _repo.Insert(new Transaccion
                    {
                        Fecha = v.FechaVenta,
                        Tipo = "Gasto",
                        Categoria = "Costo de Ventas",
                        Concepto = conceptoCosto,
                        Monto = costoTotal,
                        Usuario = v.Usuario
                    });
                    count++;
                }
                catch { }
            }

            return count;
        }

        public int ImportarMovimientosCaja()
        {
            var movRepo = new MovimientoCajaRepository();
            var movs = movRepo.GetAll();
            int count = 0;
            foreach (var m in movs)
            {
                if (!_repo.ExistePorConcepto(m.Concepto))
                {
                    _repo.Insert(new Transaccion
                    {
                        Fecha = m.Fecha,
                        Tipo = m.Tipo == "Entrada" ? "Ingreso" : "Gasto",
                        Categoria = "Movimientos de Caja",
                        Concepto = m.Concepto,
                        Monto = m.Monto,
                        Usuario = m.Usuario
                    });
                    count++;
                }
            }
            return count;
        }

        public static string[] CategoriasIngreso = { "Ventas del Dia", "Otros Ingresos" };
        public static string[] CategoriasGasto = { "Costo de Ventas", "Salarios", "Arriendo", "Servicios", "Bolsas y Cajas", "Transporte", "Mercancia", "Otros Gastos" };
    }

    public class VentaWebItem
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = "";
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public string Usuario { get; set; } = "";
        public bool Anulada { get; set; }
    }

    public class DetalleWebItem
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
    }

    public class VentaDetalleResponse
    {
        public VentaWebItem venta { get; set; } = new();
        public List<DetalleWebItem> detalles { get; set; } = new();
    }
}
