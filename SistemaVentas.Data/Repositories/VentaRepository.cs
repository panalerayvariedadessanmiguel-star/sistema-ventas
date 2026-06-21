using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class VentaRepository
    {
        public int InsertWithDetails(Venta venta, List<DetalleVenta> detalles)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sqlVenta = @"INSERT INTO Ventas (NumeroVenta, CajaId, ClienteId, FechaVenta, SubTotal, Impuesto, Total, 
                            MetodoPago, MontoPagado, Cambio, Usuario, Estado) 
                            VALUES (@NumeroVenta, @CajaId, @ClienteId, @FechaVenta, @SubTotal, @Impuesto, @Total, 
                            @MetodoPago, @MontoPagado, @Cambio, @Usuario, @Estado);
                            SELECT SCOPE_IDENTITY();";
                var ventaId = connection.ExecuteScalar<int>(sqlVenta, new
                {
                    venta.NumeroVenta,
                    venta.CajaId,
                    venta.ClienteId,
                    venta.FechaVenta,
                    venta.SubTotal,
                    venta.Impuesto,
                    venta.Total,
                    venta.MetodoPago,
                    venta.MontoPagado,
                    venta.Cambio,
                    venta.Usuario,
                    venta.Estado
                }, transaction);

                var sqlDetalle = @"INSERT INTO DetalleVentas (VentaId, ProductoId, ProductoVarianteId, Cantidad, PrecioUnitario, CostoUnitario, SubTotal, Impuesto, Total) 
                                   VALUES (@VentaId, @ProductoId, @ProductoVarianteId, @Cantidad, @PrecioUnitario, @CostoUnitario, @SubTotal, @Impuesto, @Total)";
                connection.Execute(sqlDetalle, detalles.Select(d => new
                {
                    VentaId = ventaId,
                    d.ProductoId,
                    ProductoVarianteId = d.ProductoVarianteId,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.CostoUnitario,
                    d.SubTotal,
                    d.Impuesto,
                    d.Total
                }), transaction);

                var sqlStock = "UPDATE Productos SET Stock = Stock - @Cantidad, FechaModificacion = GETDATE() WHERE Id = @Id";
                connection.Execute(sqlStock, detalles.Select(d => new { Id = d.ProductoId, d.Cantidad }), transaction);

                var sqlVarStock = "UPDATE ProductoVariantes SET Stock = Stock - @Cantidad WHERE Id = @Id AND Stock IS NOT NULL";
                var varUpdates = detalles.Where(d => d.ProductoVarianteId.HasValue)
                    .Select(d => new { Id = d.ProductoVarianteId.Value, d.Cantidad }).ToList();
                if (varUpdates.Count > 0)
                    connection.Execute(sqlVarStock, varUpdates, transaction);

                transaction.Commit();
                return ventaId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Venta> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT v.*, c.Nombre AS NombreCliente FROM Ventas v 
                        LEFT JOIN Clientes c ON v.ClienteId = c.Id 
                        ORDER BY v.FechaVenta DESC";
            return connection.Query<Venta>(sql).AsList();
        }

        public Venta GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT v.*, c.Nombre AS NombreCliente FROM Ventas v 
                        LEFT JOIN Clientes c ON v.ClienteId = c.Id 
                        WHERE v.Id = @Id";
            return connection.QueryFirstOrDefault<Venta>(sql, new { Id = id });
        }

        public List<DetalleVenta> GetDetallesByVentaId(int ventaId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT dv.*, p.Nombre AS NombreProducto FROM DetalleVentas dv 
                        INNER JOIN Productos p ON dv.ProductoId = p.Id 
                        WHERE dv.VentaId = @VentaId";
            return connection.Query<DetalleVenta>(sql, new { VentaId = ventaId }).AsList();
        }

        public bool AnularVenta(int ventaId, string motivo, string usuario)
        {
            using var connection = DbConnection.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                var venta = connection.QueryFirstOrDefault<Venta>("SELECT * FROM Ventas WHERE Id = @Id", new { Id = ventaId }, transaction);

                var sqlAnular = "UPDATE Ventas SET Anulada = 1, MotivoAnulacion = @Motivo WHERE Id = @Id";
                connection.Execute(sqlAnular, new { Id = ventaId, Motivo = motivo }, transaction);

                // Solo restaurar stock si estaba confirmada
                if (venta?.Estado == "Confirmada")
                {
                    var sqlStock = @"UPDATE Productos SET Stock = Stock + (SELECT Cantidad FROM DetalleVentas WHERE VentaId = @VentaId AND ProductoId = Productos.Id),
                                    FechaModificacion = GETDATE()
                                    WHERE Id IN (SELECT ProductoId FROM DetalleVentas WHERE VentaId = @VentaId)";
                    connection.Execute(sqlStock, new { VentaId = ventaId }, transaction);

                    var sqlVarStock = @"UPDATE pv SET pv.Stock = pv.Stock + dv.Cantidad
                                        FROM ProductoVariantes pv
                                        INNER JOIN DetalleVentas dv ON pv.Id = dv.ProductoVarianteId
                                        WHERE dv.VentaId = @VentaId AND pv.Stock IS NOT NULL";
                    connection.Execute(sqlVarStock, new { VentaId = ventaId }, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Venta> GetVentasByRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT v.*, c.Nombre AS NombreCliente FROM Ventas v 
                        LEFT JOIN Clientes c ON v.ClienteId = c.Id 
                        WHERE v.FechaVenta BETWEEN @FechaInicio AND @FechaFin AND v.Anulada = 0
                        ORDER BY v.FechaVenta DESC";
            return connection.Query<Venta>(sql, new { FechaInicio = fechaInicio, FechaFin = fechaFin }).AsList();
        }

        public decimal GetTotalVentasByRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT COALESCE(SUM(Total), 0) FROM Ventas 
                        WHERE FechaVenta BETWEEN @FechaInicio AND @FechaFin AND Anulada = 0";
            return connection.ExecuteScalar<decimal>(sql, new { FechaInicio = fechaInicio, FechaFin = fechaFin });
        }
        public List<dynamic> GetDetalleVenta(int ventaId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT
                            dv.Cantidad,
                            dv.PrecioUnitario,
                            dv.SubTotal,
                            p.Nombre AS Producto,
                            p.CodigoBarras,
                            pv.Nombre AS Color,
                            pv.Talla
                        FROM DetalleVentas dv
                        INNER JOIN Productos p ON dv.ProductoId = p.Id
                        LEFT JOIN ProductoVariantes pv ON dv.ProductoVarianteId = pv.Id
                        WHERE dv.VentaId = @VentaId";
            return connection.Query(sql, new { VentaId = ventaId }).AsList();
        }
    }
}
