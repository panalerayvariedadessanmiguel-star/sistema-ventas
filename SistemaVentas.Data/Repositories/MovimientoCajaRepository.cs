using System;
using System.Collections.Generic;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class MovimientoCajaRepository
    {
        public int Insert(MovimientoCaja movimiento)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO MovimientosCaja (CajaId, Tipo, Concepto, Monto, Fecha, Usuario) 
                        VALUES (@CajaId, @Tipo, @Concepto, @Monto, @Fecha, @Usuario);
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, movimiento);
        }

        public List<MovimientoCaja> GetByCajaId(int cajaId)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<MovimientoCaja>(
                "SELECT * FROM MovimientosCaja WHERE CajaId = @CajaId ORDER BY Fecha DESC",
                new { CajaId = cajaId }).AsList();
        }

        public List<MovimientoCaja> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<MovimientoCaja>(
                "SELECT * FROM MovimientosCaja ORDER BY Fecha DESC").AsList();
        }
    }
}
