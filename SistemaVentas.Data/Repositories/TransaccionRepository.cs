using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class TransaccionRepository
    {
        public int Insert(Transaccion t)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO Transacciones (Fecha, Tipo, Categoria, Concepto, Monto, Usuario) 
                        VALUES (@Fecha, @Tipo, @Categoria, @Concepto, @Monto, @Usuario);
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, t);
        }

        public bool Delete(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Execute("DELETE FROM Transacciones WHERE Id=@Id", new { Id = id }) > 0;
        }

        public List<Transaccion> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<Transaccion>("SELECT * FROM Transacciones ORDER BY Fecha DESC, Id DESC").ToList();
        }

        public List<Transaccion> GetByDateRange(DateTime desde, DateTime hasta)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<Transaccion>(
                "SELECT * FROM Transacciones WHERE Fecha >= @Desde AND Fecha <= @Hasta ORDER BY Fecha DESC, Id DESC",
                new { Desde = desde, Hasta = hasta }).ToList();
        }

        public Transaccion GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<Transaccion>(
                "SELECT * FROM Transacciones WHERE Id=@Id", new { Id = id });
        }

        public bool DeleteByConcepto(string concepto)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Execute("DELETE FROM Transacciones WHERE Concepto=@Concepto", new { Concepto = concepto }) > 0;
        }

        public bool ExistePorConcepto(string concepto)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(1) FROM Transacciones WHERE Concepto=@Concepto", new { Concepto = concepto }) > 0;
        }

        public decimal GetTotalIngresos(DateTime desde, DateTime hasta)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<decimal>(
                "SELECT ISNULL(SUM(Monto),0) FROM Transacciones WHERE Tipo='Ingreso' AND Fecha >= @Desde AND Fecha <= @Hasta",
                new { Desde = desde, Hasta = hasta });
        }

        public decimal GetTotalGastos(DateTime desde, DateTime hasta)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<decimal>(
                "SELECT ISNULL(SUM(Monto),0) FROM Transacciones WHERE Tipo='Gasto' AND Fecha >= @Desde AND Fecha <= @Hasta",
                new { Desde = desde, Hasta = hasta });
        }

        public IEnumerable<dynamic> GetResumenPorCategoria(DateTime desde, DateTime hasta)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query(
                @"SELECT Tipo, Categoria, ISNULL(SUM(Monto),0) AS Total, COUNT(*) AS Cantidad
                  FROM Transacciones WHERE Fecha >= @Desde AND Fecha <= @Hasta
                  GROUP BY Tipo, Categoria ORDER BY Tipo, Total DESC",
                new { Desde = desde, Hasta = hasta });
        }
    }
}
