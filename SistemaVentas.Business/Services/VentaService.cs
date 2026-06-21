using System;
using System.Collections.Generic;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class VentaResult
    {
        public int VentaId { get; set; }
        public string RutaFactura { get; set; }
    }

    public class VentaService
    {
        private readonly VentaRepository _repository;
        private readonly ProductoRepository _productoRepository;
        private readonly CajaService _cajaService;
        private readonly FacturacionService _facturacionService;
        private readonly StockService _stockService;
        private readonly TransaccionRepository _transaccionRepository;
        private readonly SincronizacionService _sincronizacionService;

        public VentaService()
        {
            _repository = new VentaRepository();
            _productoRepository = new ProductoRepository();
            _cajaService = new CajaService();
            _facturacionService = new FacturacionService();
            _stockService = new StockService();
            _transaccionRepository = new TransaccionRepository();
            _sincronizacionService = new SincronizacionService();
        }

        public VentaResult RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            if (detalles == null || detalles.Count == 0)
                throw new Exception("La venta debe tener al menos un producto");

            foreach (var detalle in detalles)
            {
                var producto = _productoRepository.GetById(detalle.ProductoId);
                if (producto == null)
                    throw new Exception($"El producto con ID {detalle.ProductoId} no existe");
                if (producto.Stock < detalle.Cantidad)
                    throw new Exception($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}");
            }

            venta.FechaVenta = DateTime.Now;
            int ventaId = _repository.InsertWithDetails(venta, detalles);

            // Registrar salida de stock por venta
            foreach (var detalle in detalles)
            {
                var producto = _productoRepository.GetById(detalle.ProductoId);
                if (producto != null)
                {
                    _stockService.RegistrarMovimiento(
                        detalle.ProductoId,
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        0,
                        detalle.Cantidad,
                        producto.Stock
                    );
                }
            }

            // Registrar el movimiento de entrada en caja
            _cajaService.RegistrarMovimiento(
                venta.CajaId,
                "Entrada",
                "Venta " + venta.NumeroVenta,
                venta.Total,
                venta.Usuario
            );

            // Sincronizar con contabilidad: registrar ingreso y costo de venta
            try
            {
                var conceptoIngreso = "Venta " + venta.NumeroVenta;
                var conceptoCosto = "Costo " + venta.NumeroVenta;

                if (!_transaccionRepository.ExistePorConcepto(conceptoIngreso))
                {
                    _transaccionRepository.Insert(new Transaccion
                    {
                        Fecha = venta.FechaVenta,
                        Tipo = "Ingreso",
                        Categoria = "Ventas del Dia",
                        Concepto = conceptoIngreso,
                        Monto = venta.Total,
                        Usuario = venta.Usuario
                    });
                }

                if (!_transaccionRepository.ExistePorConcepto(conceptoCosto))
                {
                    decimal costoTotal = 0;
                    foreach (var d in detalles)
                        costoTotal += d.CostoUnitario * d.Cantidad;

                    _transaccionRepository.Insert(new Transaccion
                    {
                        Fecha = venta.FechaVenta,
                        Tipo = "Gasto",
                        Categoria = "Costo de Ventas",
                        Concepto = conceptoCosto,
                        Monto = costoTotal,
                        Usuario = venta.Usuario
                    });
                }
            }
            catch { }

            // Generar factura electronica
            string rutaFactura = null;
            try
            {
                var detallesCompletos = _repository.GetDetallesByVentaId(ventaId);
                rutaFactura = _facturacionService.GenerarFactura(
                    venta,
                    detallesCompletos
                );
            }
            catch (Exception ex)
            {
                // Log del error pero no interrumpir el proceso de la venta
                Console.WriteLine($"Error al generar factura: {ex.Message}");
            }

            // Sincronizar con la tienda web (con cola de reintentos)
            try
            {
                var ventaCompleta = _repository.GetById(ventaId);
                if (ventaCompleta != null)
                {
                    _ = _sincronizacionService.SincronizarConColaAsync(ventaCompleta, detalles);
                }
            }
            catch { }

            return new VentaResult { VentaId = ventaId, RutaFactura = rutaFactura };
        }

        public List<Venta> GetAll() => _repository.GetAll();

        public Venta GetById(int id) => _repository.GetById(id);

        public List<DetalleVenta> GetDetallesByVentaId(int ventaId) => _repository.GetDetallesByVentaId(ventaId);

        public bool AnularVenta(int ventaId, string motivo, string usuario)
        {
            var venta = _repository.GetById(ventaId);
            if (venta == null)
                throw new Exception("La venta no existe");
            if (venta.Anulada)
                throw new Exception("La venta ya fue anulada");

            return _repository.AnularVenta(ventaId, motivo, usuario);
        }

        public List<Venta> GetVentasByRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            return _repository.GetVentasByRangoFechas(fechaInicio, fechaFin);
        }

        public decimal GetTotalVentasByRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            return _repository.GetTotalVentasByRangoFechas(fechaInicio, fechaFin);
        }

        public string GenerarNumeroVenta()
        {
            var anio = DateTime.Now.Year.ToString("D4");
            var mes = DateTime.Now.Month.ToString("D2");
            var ventas = _repository.GetAll();
            var consecutivo = (ventas.Count + 1).ToString("D6");
            return $"VTA-{anio}{mes}-{consecutivo}";
        }
    }
}
