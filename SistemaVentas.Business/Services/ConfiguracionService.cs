using System.Data;
using Dapper;
using SistemaVentas.Data;

namespace SistemaVentas.Business.Services
{
    public class ConfiguracionService
    {
        public string LeerConfig(string clave, string valorPorDefecto)
        {
            using var connection = DbConnection.GetConnection();
            var valor = connection.QueryFirstOrDefault<string>(
                "SELECT Valor FROM Configuracion WHERE Clave = @Clave",
                new { Clave = clave });
            return string.IsNullOrEmpty(valor) ? valorPorDefecto : valor;
        }

        public decimal LeerDecimal(string clave, decimal valorPorDefecto)
        {
            var valor = LeerConfig(clave, valorPorDefecto.ToString());
            return decimal.TryParse(valor, out var result) ? result : valorPorDefecto;
        }

        public void GuardarValor(string clave, string valor)
        {
            using var connection = DbConnection.GetConnection();
            var existe = connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(*) FROM Configuracion WHERE Clave = @Clave",
                new { Clave = clave });
            if (existe > 0)
                connection.Execute("UPDATE Configuracion SET Valor = @Valor WHERE Clave = @Clave", new { Clave = clave, Valor = valor });
            else
                connection.Execute("INSERT INTO Configuracion (Clave, Valor) VALUES (@Clave, @Valor)", new { Clave = clave, Valor = valor });
        }
    }
}
