using System;
using System.Collections.Generic;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class ClienteRepository
    {
        public List<Cliente> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<Cliente>("SELECT * FROM Clientes WHERE Activo = 1 ORDER BY Nombre").AsList();
        }

        public int Insert(Cliente cliente)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO Clientes (Documento, Nombre, Telefono, Email, Direccion) 
                        VALUES (@Documento, @Nombre, @Telefono, @Email, @Direccion);
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, cliente);
        }

        public Cliente GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<Cliente>("SELECT * FROM Clientes WHERE Id = @Id", new { Id = id });
        }
    }
}
