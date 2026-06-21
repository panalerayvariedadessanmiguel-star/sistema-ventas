using System;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;

namespace SistemaVentas.Data
{
    public static class DbConnection
    {
        private static string connectionString;

        static DbConnection()
        {
            LoadConnectionString();
        }

        private static void LoadConnectionString()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db.config");
            if (File.Exists(configPath))
            {
                connectionString = File.ReadAllText(configPath).Trim();
            }
            else
            {
                connectionString = "Server=(LocalDB)\\MSSQLLocalDB;Database=SistemaVentasDB;Integrated Security=true;TrustServerCertificate=true;";
            }
        }

        public static IDbConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static void UpdateConnectionString(string newConnectionString)
        {
            connectionString = newConnectionString;
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db.config");
            File.WriteAllText(configPath, newConnectionString);
        }

        public static string GetCurrentConnectionString()
        {
            return connectionString;
        }
    }
}
