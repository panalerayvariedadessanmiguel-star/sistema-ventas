using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class ReporteService
    {
        private readonly ReporteRepository _repository;
        private readonly VentaRepository _ventaRepository;
        private readonly CajaRepository _cajaRepository;

        public ReporteService()
        {
            _repository = new ReporteRepository();
            _ventaRepository = new VentaRepository();
            _cajaRepository = new CajaRepository();
        }

        public decimal GetTotalUtilidadMensual(int anio, int mes)
        {
            return _repository.GetTotalUtilidadMensual(anio, mes);
        }

        public decimal GetTotalVentasMensual(int anio, int mes)
        {
            return _repository.GetTotalVentasMensual(anio, mes);
        }

        public int GetTotalTransaccionesMensual(int anio, int mes)
        {
            return _repository.GetTotalTransaccionesMensual(anio, mes);
        }

        public System.Collections.Generic.List<dynamic> GetDetalleUtilidades(int anio, int mes)
        {
            return _repository.GetUtilidadesMensuales(anio, mes);
        }

        public System.Collections.Generic.List<dynamic> GetVentasDiarias(DateTime fecha)
        {
            return _repository.GetVentasDiarias(fecha);
        }

        public decimal GetTotalVentasDiarias(DateTime fecha)
        {
            return _repository.GetTotalVentasDiarias(fecha);
        }

        public int GetTotalTransaccionesDiarias(DateTime fecha)
        {
            return _repository.GetTotalTransaccionesDiarias(fecha);
        }

        public System.Collections.Generic.List<dynamic> GetUtilidadPorCategoriaDiaria(DateTime fecha)
        {
            return _repository.GetUtilidadPorCategoriaDiaria(fecha);
        }

        public System.Collections.Generic.List<dynamic> GetProductosVendidosDiarios(DateTime fecha)
        {
            return _repository.GetProductosVendidosDiarios(fecha);
        }

        public System.Collections.Generic.List<dynamic> GetDetalleVenta(int ventaId)
        {
            return _ventaRepository.GetDetalleVenta(ventaId);
        }
    }
}
