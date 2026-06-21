using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class ProductoVarianteRepository
    {
        public List<ProductoVariante> GetByProducto(int productoId)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<ProductoVariante>(
                "SELECT * FROM ProductoVariantes WHERE ProductoId = @ProductoId AND Activo = 1 ORDER BY Orden",
                new { ProductoId = productoId }).AsList();
        }

        public ProductoVariante GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<ProductoVariante>(
                "SELECT * FROM ProductoVariantes WHERE Id = @Id", new { Id = id });
        }

        public void UpdateStock(int varianteId, int cantidad)
        {
            using var connection = DbConnection.GetConnection();
            connection.Execute(
                "UPDATE ProductoVariantes SET Stock = Stock - @Cantidad WHERE Id = @Id AND Stock IS NOT NULL",
                new { Id = varianteId, Cantidad = cantidad });
        }

        public void DeleteByProducto(int productoId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Execute(
                "DELETE FROM ProductoVariantes WHERE ProductoId = @ProductoId",
                new { ProductoId = productoId });
        }

        public void Insert(ProductoVariante variante, bool useProvidedId = false)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            if (useProvidedId && variante.Id > 0)
            {
                connection.Execute("SET IDENTITY_INSERT ProductoVariantes ON");
                var sql = @"INSERT INTO ProductoVariantes (Id, ProductoId, Nombre, ColorHex, Talla, Stock, ImagenUrl, Activo, Orden)
                            VALUES (@Id, @ProductoId, @Nombre, @ColorHex, @Talla, @Stock, @ImagenUrl, @Activo, @Orden)";
                connection.Execute(sql, variante);
                connection.Execute("SET IDENTITY_INSERT ProductoVariantes OFF");
            }
            else
            {
                var sql = @"INSERT INTO ProductoVariantes (ProductoId, Nombre, ColorHex, Talla, Stock, ImagenUrl, Activo, Orden)
                            VALUES (@ProductoId, @Nombre, @ColorHex, @Talla, @Stock, @ImagenUrl, @Activo, @Orden)";
                connection.Execute(sql, variante);
            }
        }

        public void UpdateFromSync(ProductoVariante variante)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"UPDATE ProductoVariantes SET
                        Nombre = @Nombre, ColorHex = @ColorHex, Talla = @Talla,
                        Stock = @Stock, ImagenUrl = @ImagenUrl, Activo = @Activo, Orden = @Orden
                        WHERE Id = @Id AND ProductoId = @ProductoId";
            connection.Execute(sql, variante);
        }
    }
}
