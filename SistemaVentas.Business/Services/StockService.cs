using System;
using System.Collections.Generic;
using System.Linq;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class StockService
    {
        private readonly StockRepository _repository;

        public StockService()
        {
            _repository = new StockRepository();
        }

        public List<Stock> GetAll() => _repository.GetAll().ToList();

        public List<Stock> GetByProductoId(int productoId) => _repository.GetByProductoId(productoId).ToList();

        public Stock GetById(int id) => _repository.GetById(id);

        public List<Stock> GetByAñoMes(int año, int mes) => _repository.GetByAñoMes(año, mes).ToList();

        public void RegistrarMovimiento(int productoId, int año, int mes, int cantidadEntrante, int cantidadSaliente, int stockActual)
        {
            var stock = _repository.GetByProductoAñoMes(productoId, año, mes);

            if (stock == null)
            {
                var nuevoStock = new Stock
                {
                    ProductoId = productoId,
                    Año = año,
                    Mes = mes,
                    CantidadInicial = stockActual,
                    CantidadEntrante = cantidadEntrante,
                    CantidadSaliente = cantidadSaliente,
                    CantidadFinal = stockActual + cantidadEntrante - cantidadSaliente
                };
                _repository.Insert(nuevoStock);
            }
            else
            {
                stock.CantidadEntrante += cantidadEntrante;
                stock.CantidadSaliente += cantidadSaliente;
                stock.CantidadFinal = stock.CantidadInicial + stock.CantidadEntrante - stock.CantidadSaliente;
                _repository.Update(stock);
            }
        }

        public void InicializarStockDesdeProductos()
        {
            var productoService = new ProductoService();
            var productos = productoService.GetAll();
            int añoActual = DateTime.Now.Year;
            int mesActual = DateTime.Now.Month;

            foreach (var p in productos)
            {
                var stockExistente = _repository.GetByProductoAñoMes(p.Id, añoActual, mesActual);
                if (stockExistente == null)
                {
                    var nuevoStock = new Stock
                    {
                        ProductoId = p.Id,
                        Año = añoActual,
                        Mes = mesActual,
                        CantidadInicial = p.Stock,
                        CantidadEntrante = 0,
                        CantidadSaliente = 0,
                        CantidadFinal = p.Stock
                    };
                    _repository.Insert(nuevoStock);
                }
            }
        }

        public string TraspasoStockMes(int año, int mes)
        {
            int añoAnterior = mes == 1 ? año - 1 : año;
            int mesAnterior = mes == 1 ? 12 : mes - 1;

            var stockMesAnterior = _repository.GetAllStockConCantidadFinal(añoAnterior, mesAnterior).ToList();

            if (stockMesAnterior.Count == 0)
            {
                return $"No hay registros de stock para {GetNombreMes(mesAnterior)} {añoAnterior}. No se realizó traspaso.";
            }

            int productosCreados = 0;
            int productosOmitidos = 0;

            foreach (var stockAnterior in stockMesAnterior)
            {
                bool yaExiste = _repository.ExisteStockParaProductoAñoMes(stockAnterior.ProductoId, año, mes);
                if (yaExiste)
                {
                    productosOmitidos++;
                    continue;
                }

                _repository.InsertTraspaso(stockAnterior.ProductoId, año, mes, stockAnterior.CantidadFinal);
                productosCreados++;
            }

            return $"Traspaso completado: {productosCreados} productos trasladados, {productosOmitidos} omitidos (ya existían). Del mes {GetNombreMes(mesAnterior)} {añoAnterior} a {GetNombreMes(mes)} {año}.";
        }

        public string TraspasoAutomaticoMesActual()
        {
            int añoActual = DateTime.Now.Year;
            int mesActual = DateTime.Now.Month;
            return TraspasoStockMes(añoActual, mesActual);
        }

        private string GetNombreMes(int mes)
        {
            string[] meses = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                             "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            return mes >= 1 && mes <= 12 ? meses[mes] : "";
        }
    }
}
