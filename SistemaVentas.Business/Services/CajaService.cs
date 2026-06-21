using System;
using System.Collections.Generic;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class CajaService
    {
        private readonly CajaRepository _cajaRepository;
        private readonly MovimientoCajaRepository _movimientoRepository;

        public CajaService()
        {
            _cajaRepository = new CajaRepository();
            _movimientoRepository = new MovimientoCajaRepository();
        }

        public Caja GetCajaAbierta() => _cajaRepository.GetCajaAbierta();

        public int AbrirCaja(string usuario, decimal montoInicial, string observaciones, int numeroCaja = 1)
        {
            if (GetCajaAbierta() != null)
                throw new Exception("Ya hay una caja abierta. Cierrela antes de abrir una nueva");

            var caja = new Caja
            {
                NumeroCaja = numeroCaja,
                Usuario = usuario,
                MontoInicial = montoInicial,
                FechaApertura = DateTime.Now,
                ObservacionesApertura = observaciones
            };

            return _cajaRepository.AbrirCaja(caja);
        }

        public decimal CerrarCaja(int cajaId, decimal montoReal, string observaciones)
        {
            var caja = _cajaRepository.GetById(cajaId);
            if (caja == null || caja.Estado != "Abierta")
                throw new Exception("La caja no esta abierta o no existe");

            var movimientos = _movimientoRepository.GetByCajaId(cajaId);
            var entradas = 0m;
            var salidas = 0m;

            foreach (var m in movimientos)
            {
                if (m.Tipo == "Entrada") entradas += m.Monto;
                else if (m.Tipo == "Salida") salidas += m.Monto;
            }

            decimal montoEsperado = caja.MontoInicial + entradas - salidas;
            decimal diferencia = montoReal - montoEsperado;

            _cajaRepository.CerrarCaja(cajaId, montoReal, montoEsperado, diferencia, observaciones);

            return montoEsperado;
        }

        public int RegistrarMovimiento(int cajaId, string tipo, string concepto, decimal monto, string usuario)
        {
            var cajaAbierta = GetCajaAbierta();
            if (cajaAbierta == null || cajaAbierta.Id != cajaId)
                throw new Exception("No hay una caja abierta con ese ID");

            var movimiento = new MovimientoCaja
            {
                CajaId = cajaId,
                Tipo = tipo,
                Concepto = concepto,
                Monto = monto,
                Fecha = DateTime.Now,
                Usuario = usuario
            };

            return _movimientoRepository.Insert(movimiento);
        }

        public List<MovimientoCaja> GetMovimientosByCajaId(int cajaId)
        {
            return _movimientoRepository.GetByCajaId(cajaId);
        }

        public List<string> GetVendedores()
        {
            return _cajaRepository.GetVendedores();
        }

        public List<Caja> GetHistorial() => _cajaRepository.GetHistorial();

        public decimal GetMontoEnCaja(int cajaId)
        {
            var caja = _cajaRepository.GetById(cajaId);
            if (caja == null) return 0;

            var movimientos = _movimientoRepository.GetByCajaId(cajaId);
            decimal monto = caja.MontoInicial;

            foreach (var m in movimientos)
            {
                if (m.Tipo == "Entrada") monto += m.Monto;
                else if (m.Tipo == "Salida") monto -= m.Monto;
            }

            return monto;
        }

        public decimal GetMontoInicial(int cajaId)
        {
            var caja = _cajaRepository.GetById(cajaId);
            return caja?.MontoInicial ?? 0;
        }

        public decimal GetTotalVentas(int cajaId)
        {
            var movimientos = _movimientoRepository.GetByCajaId(cajaId);
            decimal totalVentas = 0;

            foreach (var m in movimientos)
            {
                if (m.Tipo == "Entrada" && m.Concepto.StartsWith("Venta"))
                    totalVentas += m.Monto;
            }

            return totalVentas;
        }
    }
}
