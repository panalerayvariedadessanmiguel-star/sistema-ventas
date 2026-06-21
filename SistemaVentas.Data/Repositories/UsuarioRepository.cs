using System;
using System.Collections.Generic;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class UsuarioRepository
    {
        public List<Usuario> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<Usuario>("SELECT * FROM Usuarios WHERE Activo = 1 ORDER BY Nombres").AsList();
        }

        public Usuario GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<Usuario>("SELECT * FROM Usuarios WHERE Id = @Id", new { Id = id });
        }

        public Usuario GetByDocumento(string documento)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<Usuario>("SELECT * FROM Usuarios WHERE Documento = @Documento", new { Documento = documento });
        }

        public int Insert(Usuario usuario)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO Usuarios (Nombres, Apellidos, Documento, TipoDocumento, Contraseña, Rol, Salario, Activo, FechaCreacion) 
                        VALUES (@Nombres, @Apellidos, @Documento, @TipoDocumento, @Contraseña, @Rol, @Salario, @Activo, @FechaCreacion);
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, usuario);
        }

        public bool Update(Usuario usuario)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"UPDATE Usuarios SET Nombres = @Nombres, Apellidos = @Apellidos, Documento = @Documento, 
                        TipoDocumento = @TipoDocumento, Contraseña = @Contraseña, Rol = @Rol, Salario = @Salario 
                        WHERE Id = @Id";
            return connection.Execute(sql, usuario) > 0;
        }

        public bool Delete(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Execute("UPDATE Usuarios SET Activo = 0 WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
