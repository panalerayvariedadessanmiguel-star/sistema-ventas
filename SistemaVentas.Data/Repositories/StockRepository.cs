using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class StockRepository
    {
        public IEnumerable<Stock> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT s.*, p.Nombre AS NombreProducto, p.CodigoBarras 
                        FROM Stock s 
                        INNER JOIN Productos p ON s.ProductoId = p.Id AND p.Activo = 1
                        ORDER BY s.Año DESC, s.Mes DESC, s.Id";
            return connection.Query<Stock>(sql);
        }

        public IEnumerable<Stock> GetByProductoId(int productoId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT s.*, p.Nombre AS NombreProducto, p.CodigoBarras 
                        FROM Stock s 
                        INNER JOIN Productos p ON s.ProductoId = p.Id 
                        WHERE s.ProductoId = @ProductoId 
                        ORDER BY s.Año DESC, s.Mes DESC";
            return connection.Query<Stock>(sql, new { ProductoId = productoId });
        }

        public Stock GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT s.*, p.Nombre AS NombreProducto, p.CodigoBarras 
                        FROM Stock s 
                        INNER JOIN Productos p ON s.ProductoId = p.Id 
                        WHERE s.Id = @Id";
            return connection.QueryFirstOrDefault<Stock>(sql, new { Id = id });
        }

        public int Insert(Stock stock)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO Stock (ProductoId, Año, Mes, CantidadInicial, CantidadEntrante, CantidadSaliente, CantidadFinal) 
                        VALUES (@ProductoId, @Año, @Mes, @CantidadInicial, @CantidadEntrante, @CantidadSaliente, @CantidadFinal);
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, stock);
        }

        public bool Update(Stock stock)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"UPDATE Stock SET CantidadInicial = @CantidadInicial, CantidadEntrante = @CantidadEntrante, 
                        CantidadSaliente = @CantidadSaliente, CantidadFinal = @CantidadFinal 
                        WHERE Id = @Id";
            return connection.Execute(sql, stock) > 0;
        }

        public Stock GetByProductoAñoMes(int productoId, int año, int mes)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT s.*, p.Nombre AS NombreProducto, p.CodigoBarras 
                        FROM Stock s 
                        INNER JOIN Productos p ON s.ProductoId = p.Id 
                        WHERE s.ProductoId = @ProductoId AND s.Año = @Año AND s.Mes = @Mes";
            return connection.QueryFirstOrDefault<Stock>(sql, new { ProductoId = productoId, Año = año, Mes = mes });
        }

        public IEnumerable<Stock> GetByAñoMes(int año, int mes)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT s.*, p.Nombre AS NombreProducto, p.CodigoBarras 
                        FROM Stock s 
                        INNER JOIN Productos p ON s.ProductoId = p.Id AND p.Activo = 1
                        WHERE s.Año = @Año AND s.Mes = @Mes 
                        ORDER BY s.Id";
            return connection.Query<Stock>(sql, new { Año = año, Mes = mes });
        }

        public IEnumerable<Stock> GetAllStockConCantidadFinal(int año, int mes)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT s.*, p.Nombre AS NombreProducto, p.CodigoBarras 
                        FROM Stock s 
                        INNER JOIN Productos p ON s.ProductoId = p.Id AND p.Activo = 1
                        WHERE s.Año = @Año AND s.Mes = @Mes AND s.CantidadFinal > 0
                        ORDER BY s.Id";
            return connection.Query<Stock>(sql, new { Año = año, Mes = mes });
        }

        public void InsertTraspaso(int productoId, int año, int mes, int cantidadInicial)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO Stock (ProductoId, Año, Mes, CantidadInicial, CantidadEntrante, CantidadSaliente, CantidadFinal) 
                        VALUES (@ProductoId, @Año, @Mes, @CantidadInicial, 0, 0, @CantidadInicial)";
            connection.Execute(sql, new { ProductoId = productoId, Año = año, Mes = mes, CantidadInicial = cantidadInicial });
        }

        public bool ExisteStockParaProductoAñoMes(int productoId, int año, int mes)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT COUNT(1) FROM Stock WHERE ProductoId = @ProductoId AND Año = @Año AND Mes = @Mes";
            return connection.ExecuteScalar<int>(sql, new { ProductoId = productoId, Año = año, Mes = mes }) > 0;
        }

        public int EjecutarTraspasoMasivo(int añoAnterior, int mesAnterior, int añoNuevo, int mesNuevo)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"
                INSERT INTO Stock (ProductoId, Año, Mes, CantidadInicial, CantidadEntrante, CantidadSaliente, CantidadFinal, FechaRegistro)
                SELECT s.ProductoId, @AñoNuevo, @MesNuevo, s.CantidadFinal, 0, 0, s.CantidadFinal, GETDATE()
                FROM Stock s
                WHERE s.Año = @AñoAnterior AND s.Mes = @MesAnterior
                  AND NOT EXISTS (
                      SELECT 1 FROM Stock s2 
                      WHERE s2.ProductoId = s.ProductoId AND s2.Año = @AñoNuevo AND s2.Mes = @MesNuevo
                  )";
            return connection.Execute(sql, new { AñoAnterior = añoAnterior, MesAnterior = mesAnterior, AñoNuevo = añoNuevo, MesNuevo = mesNuevo });
        }
    }
}
