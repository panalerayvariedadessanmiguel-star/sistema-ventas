using System;
using System.Collections.Generic;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class CajaRepository
    {
        public Caja GetCajaAbierta()
        {
            using var connection = DbConnection.GetConnection();
            var sql = "SELECT * FROM Cajas WHERE Estado = 'Abierta' ORDER BY FechaApertura DESC";
            return connection.QueryFirstOrDefault<Caja>(sql);
        }

        public int AbrirCaja(Caja caja)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO Cajas (NumeroCaja, Usuario, MontoInicial, FechaApertura, ObservacionesApertura, Estado) 
                        VALUES (@NumeroCaja, @Usuario, @MontoInicial, @FechaApertura, @ObservacionesApertura, 'Abierta');
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, caja);
        }

        public bool CerrarCaja(int cajaId, decimal montoReal, decimal montoEsperado, decimal diferencia, string observaciones)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"UPDATE Cajas SET FechaCierre = GETDATE(), MontoCierreReal = @MontoReal,
                        MontoCierreEsperado = @MontoEsperado, Diferencia = @Diferencia,
                        ObservacionesCierre = @Observaciones, Estado = 'Cerrada' WHERE Id = @Id";
            return connection.Execute(sql, new { Id = cajaId, MontoReal = montoReal, MontoEsperado = montoEsperado, Diferencia = diferencia, Observaciones = observaciones }) > 0;
        }

        public decimal GetMontoEsperado(int cajaId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT 
                        c.MontoInicial + 
                        COALESCE((SELECT SUM(CASE WHEN mc.Tipo = 'Entrada' THEN mc.Monto ELSE 0 END) FROM MovimientosCaja mc WHERE mc.CajaId = c.Id), 0) + 
                        COALESCE((SELECT SUM(v.Total) FROM Ventas v WHERE v.CajaId = c.Id AND v.Anulada = 0), 0) - 
                        COALESCE((SELECT SUM(CASE WHEN mc.Tipo = 'Salida' THEN mc.Monto ELSE 0 END) FROM MovimientosCaja mc WHERE mc.CajaId = c.Id), 0) 
                        FROM Cajas c WHERE c.Id = @CajaId";
            return connection.ExecuteScalar<decimal>(sql, new { CajaId = cajaId });
        }

        public List<string> GetVendedores()
        {
            using var connection = DbConnection.GetConnection();
            var sql = "SELECT Nombres FROM Usuarios WHERE Activo = 1 ORDER BY Nombres";
            return connection.Query<string>(sql).AsList();
        }

        public List<Caja> GetHistorial()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<Caja>("SELECT * FROM Cajas ORDER BY FechaApertura DESC").AsList();
        }

        public Caja GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<Caja>("SELECT * FROM Cajas WHERE Id = @Id", new { Id = id });
        }
    }
}
