using System.Collections.Generic;
using System.Data;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class ProductoCodigoBarrasRepository
    {
        public List<ProductoCodigoBarras> GetByProductoId(int productoId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT * FROM ProductoCodigosBarras WHERE ProductoId = @ProductoId";
            return connection.Query<ProductoCodigoBarras>(sql, new { ProductoId = productoId }).AsList();
        }

        public void Insert(ProductoCodigoBarras barcode)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO ProductoCodigosBarras (ProductoId, CodigoBarras) VALUES (@ProductoId, @CodigoBarras)";
            connection.Execute(sql, barcode);
        }

        public void Delete(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Execute("DELETE FROM ProductoCodigosBarras WHERE Id = @Id", new { Id = id });
        }

        public void DeleteByProductoId(int productoId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Execute("DELETE FROM ProductoCodigosBarras WHERE ProductoId = @ProductoId", new { ProductoId = productoId });
        }

        public int? GetProductoIdByCodigoBarras(string codigoBarras)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT TOP 1 ProductoId FROM ProductoCodigosBarras WHERE CodigoBarras = @CodigoBarras";
            return connection.QueryFirstOrDefault<int?>(sql, new { CodigoBarras = codigoBarras });
        }

        public ProductoCodigoBarras GetByCodigoBarras(string codigoBarras)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT * FROM ProductoCodigosBarras WHERE CodigoBarras = @CodigoBarras";
            return connection.QueryFirstOrDefault<ProductoCodigoBarras>(sql, new { CodigoBarras = codigoBarras });
        }
    }
}
