using System;
using System.Collections.Generic;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class InventarioMovimientoRepository
    {
        public int Insert(InventarioMovimiento movimiento)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO InventarioMovimientos (ProductoId, Tipo, Cantidad, StockAnterior, StockNuevo, Motivo, Fecha, Usuario) 
                        VALUES (@ProductoId, @Tipo, @Cantidad, @StockAnterior, @StockNuevo, @Motivo, @Fecha, @Usuario);
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, movimiento);
        }

        public List<InventarioMovimiento> GetByProductoId(int productoId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT im.*, p.Nombre AS NombreProducto FROM InventarioMovimientos im 
                        INNER JOIN Productos p ON im.ProductoId = p.Id 
                        WHERE im.ProductoId = @ProductoId ORDER BY im.Fecha DESC";
            return connection.Query<InventarioMovimiento>(sql, new { ProductoId = productoId }).AsList();
        }

        public List<InventarioMovimiento> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT im.*, p.Nombre AS NombreProducto FROM InventarioMovimientos im 
                        INNER JOIN Productos p ON im.ProductoId = p.Id 
                        ORDER BY im.Fecha DESC";
            return connection.Query<InventarioMovimiento>(sql).AsList();
        }
    }
}
