using System;
using System.Collections.Generic;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class CategoriaRepository
    {
        public List<Categoria> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<Categoria>("SELECT * FROM Categorias WHERE Activo = 1 ORDER BY Nombre").AsList();
        }

        public int Insert(Categoria categoria)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO Categorias (Nombre, Descripcion) VALUES (@Nombre, @Descripcion);
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, categoria);
        }
    }
}
