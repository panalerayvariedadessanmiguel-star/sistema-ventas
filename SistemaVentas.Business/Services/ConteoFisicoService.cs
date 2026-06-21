using System;
using System.Collections.Generic;
using System.Linq;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class ConteoFisicoService
    {
        private readonly ConteoFisicoRepository _repository;
        private readonly ProductoRepository _productoRepository;

        public ConteoFisicoService()
        {
            _repository = new ConteoFisicoRepository();
            _productoRepository = new ProductoRepository();
        }

        public int CrearConteo(string usuario, string observaciones, int tipoConteo = 1, int? conteoOriginalId = null)
        {
            var conteo = new ConteoFisico
            {
                Usuario = usuario,
                Observaciones = observaciones,
                TipoConteo = tipoConteo,
                ConteoOriginalId = conteoOriginalId
            };
            return _repository.InsertConteo(conteo);
        }

        public void RegistrarConteoProducto(int conteoId, int productoId, int stockFisico)
        {
            var producto = _productoRepository.GetById(productoId);
            if (producto == null) return;

            int diferencia = stockFisico - producto.Stock;
            decimal valorFaltante = 0;
            decimal valorSobrante = 0;

            if (diferencia < 0)
                valorFaltante = Math.Abs(diferencia) * producto.PrecioCompra;
            else if (diferencia > 0)
                valorSobrante = diferencia * producto.PrecioCompra;

            var detalle = new DetalleConteoFisico
            {
                ConteoId = conteoId,
                ProductoId = productoId,
                StockSistema = producto.Stock,
                StockFisico = stockFisico,
                ValorFaltante = valorFaltante,
                ValorSobrante = valorSobrante
            };
            _repository.InsertDetalle(detalle);
        }

        public List<DetalleConteoFisico> GetDetalles(int conteoId)
        {
            return _repository.GetDetallesByConteoId(conteoId).ToList();
        }

        public List<ConteoFisico> GetAll()
        {
            return _repository.GetAll().ToList();
        }

        public ConteoFisico GetById(int id)
        {
            return _repository.GetConteoById(id);
        }

        public void FinalizarConteo(int conteoId)
        {
            _repository.FinalizarConteo(conteoId);
        }

        public List<Producto> GetProductosParaConteo()
        {
            return _productoRepository.GetAll().Where(p => p.Activo).ToList();
        }

        public List<ConteoFisico> GetConteosFinalizadosTipo1()
        {
            return _repository.GetConteosFinalizadosTipo1().ToList();
        }

        public List<DetalleConteoFisico> GetDetallesConDiferencia(int conteoId)
        {
            return _repository.GetDetallesConDiferencia(conteoId).ToList();
        }
    }
}
