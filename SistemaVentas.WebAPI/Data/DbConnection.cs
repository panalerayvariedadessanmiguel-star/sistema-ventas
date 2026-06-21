using Npgsql;
using System.Data;

namespace SistemaVentas.WebAPI.Data;

public class DbConnection
{
    private readonly string _connectionString;

    public DbConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
