using System;
using System.Collections.Generic;
using System.Linq;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class CompraService
    {
        private readonly CompraRepository _repository;
        private readonly InventarioMovimientoRepository _inventarioRepository;
        private readonly StockService _stockService;
        private readonly TransaccionRepository _transaccionRepository;

        public CompraService()
        {
            _repository = new CompraRepository();
            _inventarioRepository = new InventarioMovimientoRepository();
            _stockService = new StockService();
            _transaccionRepository = new TransaccionRepository();
        }

        public int RegistrarCompra(Compra compra, List<DetalleCompra> detalles)
        {
            if (detalles == null || detalles.Count == 0)
                throw new Exception("La compra debe tener al menos un producto");

            compra.SubTotal = 0;
            foreach (var d in detalles)
            {
                d.SubTotal = d.Cantidad * d.PrecioUnitario;
                compra.SubTotal += d.SubTotal;
            }
            compra.Impuesto = 0;
            compra.Total = compra.SubTotal;
            compra.FechaCompra = DateTime.Now;

            // Capturar stock actual antes de la compra
            var productoService = new ProductoService();
            var stocksAnteriores = new Dictionary<int, int>();
            foreach (var detalle in detalles)
            {
                var prod = productoService.GetById(detalle.ProductoId);
                stocksAnteriores[detalle.ProductoId] = prod?.Stock ?? 0;
            }

            int compraId = _repository.InsertWithDetails(compra, detalles);

            // Registrar en InventarioMovimientos y stock historico
            foreach (var detalle in detalles)
            {
                var producto = productoService.GetById(detalle.ProductoId);
                if (producto != null)
                {
                    int stockAnterior = stocksAnteriores.GetValueOrDefault(detalle.ProductoId);

                    _inventarioRepository.Insert(new InventarioMovimiento
                    {
                        ProductoId = detalle.ProductoId,
                        Tipo = "Entrada",
                        Cantidad = detalle.Cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo = producto.Stock,
                        Motivo = "Compra " + compra.NumeroCompra,
                        Fecha = DateTime.Now,
                        Usuario = compra.Usuario
                    });

                    _stockService.RegistrarMovimiento(
                        detalle.ProductoId,
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        detalle.Cantidad,
                        0,
                        producto.Stock
                    );
                }
            }

            // Sincronizar con contabilidad: registrar gasto
            try
            {
                var existe = _transaccionRepository.ExistePorConcepto("Compra " + compra.NumeroCompra);
                if (!existe)
                {
                    _transaccionRepository.Insert(new Transaccion
                    {
                        Fecha = compra.FechaCompra,
                        Tipo = "Gasto",
                        Categoria = "Mercancia",
                        Concepto = "Compra " + compra.NumeroCompra,
                        Monto = compra.Total,
                        Usuario = compra.Usuario
                    });
                }
            }
            catch { }

            return compraId;
        }

        public List<Compra> GetAll() => _repository.GetAll();

        public Compra GetById(int id) => _repository.GetById(id);

        public List<DetalleCompra> GetDetallesByCompraId(int compraId) => _repository.GetDetallesByCompraId(compraId);

        public string GenerarNumeroCompra()
        {
            var anio = DateTime.Now.Year;
            var mes = DateTime.Now.Month;
            var consecutivo = _repository.GetNextConsecutivo(anio, mes);
            return $"CMP-{anio}{mes:D2}-{consecutivo:D6}";
        }
    }
}
