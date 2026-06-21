using System;
using System.Collections.Generic;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class ProductoService
    {
        private readonly ProductoRepository _repository;
        private readonly InventarioMovimientoRepository _inventarioRepository;
        private readonly StockService _stockService;

        public ProductoService()
        {
            _repository = new ProductoRepository();
            _inventarioRepository = new InventarioMovimientoRepository();
            _stockService = new StockService();
        }

        public List<Producto> GetAll() => _repository.GetAll();

        public Producto GetById(int id) => _repository.GetById(id);

        public Producto GetByCodigoBarras(string codigoBarras) => _repository.GetByCodigoBarras(codigoBarras);

        public List<Producto> Search(string termino) => _repository.Search(termino);

        public int Create(Producto producto)
        {
            return _repository.Insert(producto);
        }

        public bool Update(Producto producto)
        {
            return _repository.Update(producto);
        }

        public bool Delete(int id)
        {
            return _repository.Delete(id);
        }

        public bool RegistrarEntrada(int productoId, int cantidad, string motivo, string usuario)
        {
            var producto = _repository.GetById(productoId);
            if (producto == null) return false;

            int stockAnterior = producto.Stock;
            bool resultado = _repository.ActualizarStock(productoId, cantidad, esEntrada: true);

            if (resultado)
            {
                try
                {
                    _inventarioRepository.Insert(new InventarioMovimiento
                    {
                        ProductoId = productoId,
                        Tipo = "Entrada",
                        Cantidad = cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo = stockAnterior + cantidad,
                        Motivo = motivo,
                        Fecha = DateTime.Now,
                        Usuario = usuario
                    });

                    _stockService.RegistrarMovimiento(productoId, DateTime.Now.Year, DateTime.Now.Month, cantidad, 0, stockAnterior + cantidad);
                }
                catch
                {
                    _repository.ActualizarStock(productoId, cantidad, esEntrada: false);
                    throw;
                }
            }

            return resultado;
        }

        public bool RegistrarSalida(int productoId, int cantidad, string motivo, string usuario)
        {
            var producto = _repository.GetById(productoId);
            if (producto == null || producto.Stock < cantidad) return false;

            int stockAnterior = producto.Stock;
            bool resultado = _repository.ActualizarStock(productoId, cantidad, esEntrada: false);

            if (resultado)
            {
                try
                {
                    _inventarioRepository.Insert(new InventarioMovimiento
                    {
                        ProductoId = productoId,
                        Tipo = "Salida",
                        Cantidad = cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo = stockAnterior - cantidad,
                        Motivo = motivo,
                        Fecha = DateTime.Now,
                        Usuario = usuario
                    });

                    _stockService.RegistrarMovimiento(productoId, DateTime.Now.Year, DateTime.Now.Month, 0, cantidad, stockAnterior - cantidad);
                }
                catch
                {
                    _repository.ActualizarStock(productoId, cantidad, esEntrada: true);
                    throw;
                }
            }

            return resultado;
        }

        public List<Producto> GetProductosBajoStock()
        {
            var todos = _repository.GetAll();
            return todos.FindAll(p => p.Stock <= p.StockMinimo);
        }
    }
}
