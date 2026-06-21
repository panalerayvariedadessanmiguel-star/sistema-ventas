using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class CompraRepository
    {
        public int InsertWithDetails(Compra compra, List<DetalleCompra> detalles)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sqlCompra = @"INSERT INTO Compras (NumeroCompra, FechaCompra, Proveedor, SubTotal, Impuesto, Total, Usuario) 
                                  VALUES (@NumeroCompra, @FechaCompra, @Proveedor, @SubTotal, @Impuesto, @Total, @Usuario);
                                  SELECT SCOPE_IDENTITY();";
                var compraId = connection.ExecuteScalar<int>(sqlCompra, new
                {
                    compra.NumeroCompra,
                    compra.FechaCompra,
                    compra.Proveedor,
                    compra.SubTotal,
                    compra.Impuesto,
                    compra.Total,
                    compra.Usuario
                }, transaction);

                var sqlDetalle = @"INSERT INTO DetalleCompras (CompraId, ProductoId, Cantidad, PrecioUnitario, SubTotal) 
                                   VALUES (@CompraId, @ProductoId, @Cantidad, @PrecioUnitario, @SubTotal)";
                connection.Execute(sqlDetalle, detalles.Select(d => new
                {
                    CompraId = compraId,
                    d.ProductoId,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.SubTotal
                }), transaction);

                // Actualizar stock y precio de compra promedio
                foreach (var detalle in detalles)
                {
                    var producto = connection.QueryFirstOrDefault<Producto>(
                        "SELECT * FROM Productos WHERE Id = @Id", new { Id = detalle.ProductoId }, transaction);
                    if (producto != null)
                    {
                        int nuevoStock = producto.Stock + detalle.Cantidad;
                        // Precio promedio ponderado
                        decimal nuevoPrecioCompra = ((producto.PrecioCompra * producto.Stock) + (detalle.PrecioUnitario * detalle.Cantidad)) / nuevoStock;
                        connection.Execute(
                            "UPDATE Productos SET Stock = @Stock, PrecioCompra = @PrecioCompra WHERE Id = @Id",
                            new { Id = producto.Id, Stock = nuevoStock, PrecioCompra = Math.Round(nuevoPrecioCompra, 2) },
                            transaction);
                    }
                }

                transaction.Commit();
                return compraId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Compra> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<Compra>("SELECT * FROM Compras ORDER BY FechaCompra DESC").AsList();
        }

        public Compra GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<Compra>("SELECT * FROM Compras WHERE Id = @Id", new { Id = id });
        }

        public List<DetalleCompra> GetDetallesByCompraId(int compraId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT dc.*, p.Nombre AS NombreProducto FROM DetalleCompras dc 
                        INNER JOIN Productos p ON dc.ProductoId = p.Id 
                        WHERE dc.CompraId = @CompraId";
            return connection.Query<DetalleCompra>(sql, new { CompraId = compraId }).AsList();
        }

        public int GetNextConsecutivo(int año, int mes)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<int>(
                "SELECT ISNULL(COUNT(*), 0) + 1 FROM Compras WHERE YEAR(FechaCompra) = @Anio AND MONTH(FechaCompra) = @Mes",
                new { Anio = año, Mes = mes });
        }
    }
}
