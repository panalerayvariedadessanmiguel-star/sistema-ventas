using Microsoft.Data.SqlClient;
using System.Text;

var sqlServerConn = @"Server=(LocalDB)\MSSQLLocalDB;Database=SistemaVentasDB;Integrated Security=true;TrustServerCertificate=true;";

var output = new StringBuilder();
output.AppendLine("-- Migracion CORRECTA de datos desde SQL Server");
output.AppendLine();

using var conn = new SqlConnection(sqlServerConn);
conn.Open();

// Categorias
var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT Id, Nombre, Descripcion, FechaCreacion, Activo FROM Categorias";
var reader = cmd.ExecuteReader();
var catRows = new List<string>();
while (reader.Read())
{
    var activo = reader.GetBoolean(4) ? "TRUE" : "FALSE";
    catRows.Add($"({reader.GetInt32(0)}, '{reader.GetString(1).Replace("'", "''")}', " +
        $"'{reader.GetString(2).Replace("'", "''")}', '{reader.GetDateTime(3):yyyy-MM-dd HH:mm:ss}', {activo})");
}
reader.Close();
output.AppendLine("INSERT INTO Categorias (Id, Nombre, Descripcion, FechaCreacion, Activo) VALUES");
output.AppendLine(string.Join(",\n", catRows) + ";");
output.AppendLine();
Console.WriteLine($"Categorias: {catRows.Count}");

// Productos (CON EL ID CORRECTO)
cmd.CommandText = "SELECT Id, CodigoBarras, Nombre, Descripcion, CategoriaId, PrecioCompra, PrecioVenta, Stock, StockMinimo, FechaCreacion, Activo FROM Productos";
reader = cmd.ExecuteReader();
var prodRows = new List<string>();
var prodIds = new List<int>();
while (reader.Read())
{
    prodIds.Add(reader.GetInt32(0));
    var activo = reader.GetBoolean(10) ? "TRUE" : "FALSE";
    var codigo = reader.IsDBNull(1) ? "NULL" : $"'{reader.GetString(1).Replace("'", "''")}'";
    var desc = reader.IsDBNull(3) ? "NULL" : $"'{reader.GetString(3).Replace("'", "''")}'";
    prodRows.Add($"({reader.GetInt32(0)}, {codigo}, '{reader.GetString(2).Replace("'", "''")}', {desc}, " +
        $"{reader.GetInt32(4)}, {reader.GetDecimal(5):F2}, {reader.GetDecimal(6):F2}, {reader.GetInt32(7)}, {reader.GetInt32(8)}, " +
        $"'{reader.GetDateTime(9):yyyy-MM-dd HH:mm:ss}', {activo})");
}
reader.Close();

// Split into batches of 50
for (int i = 0; i < prodRows.Count; i += 50)
{
    var batch = prodRows.Skip(i).Take(50).ToList();
    output.AppendLine("INSERT INTO Productos (Id, CodigoBarras, Nombre, Descripcion, CategoriaId, PrecioCompra, PrecioVenta, Stock, StockMinimo, FechaCreacion, Activo) VALUES");
    output.AppendLine(string.Join(",\n", batch) + ";");
    output.AppendLine();
}
Console.WriteLine($"Productos: {prodRows.Count}");

// Client
cmd.CommandText = "SELECT Id, Documento, Nombre, Telefono, Email, Direccion, FechaRegistro, Activo FROM Clientes";
reader = cmd.ExecuteReader();
var clientRows = new List<string>();
while (reader.Read())
{
    var doc = reader.IsDBNull(1) ? "NULL" : $"'{reader.GetString(1).Replace("'", "''")}'";
    var tel = reader.IsDBNull(3) ? "NULL" : $"'{reader.GetString(3).Replace("'", "''")}'";
    var email = reader.IsDBNull(4) ? "NULL" : $"'{reader.GetString(4).Replace("'", "''")}'";
    var dir = reader.IsDBNull(5) ? "NULL" : $"'{reader.GetString(5).Replace("'", "''")}'";
    var activo = reader.GetBoolean(7) ? "TRUE" : "FALSE";
    clientRows.Add($"({reader.GetInt32(0)}, {doc}, '{reader.GetString(2).Replace("'", "''")}', {tel}, {email}, {dir}, '{reader.GetDateTime(6):yyyy-MM-dd HH:mm:ss}', {activo})");
}
reader.Close();
if (clientRows.Count > 0)
{
    output.AppendLine("INSERT INTO Clientes (Id, Documento, Nombre, Telefono, Email, Direccion, FechaRegistro, Activo) VALUES");
    output.AppendLine(string.Join(",\n", clientRows) + ";");
    output.AppendLine();
}
Console.WriteLine($"Clientes: {clientRows.Count}");

// Usuarios
cmd.CommandText = "SELECT Id, Nombres, Apellidos, Documento, TipoDocumento, Contraseña, Rol, Activo, FechaCreacion FROM Usuarios";
reader = cmd.ExecuteReader();
var userRows = new List<string>();
while (reader.Read())
{
    var td = reader.IsDBNull(4) ? "NULL" : $"'{reader.GetString(4).Replace("'", "''")}'";
    var activo = reader.GetBoolean(7) ? "TRUE" : "FALSE";
    userRows.Add($"({reader.GetInt32(0)}, '{reader.GetString(1).Replace("'", "''")}', '{reader.GetString(2).Replace("'", "''")}', " +
        $"'{reader.GetString(3).Replace("'", "''")}', {td}, '{reader.GetString(5).Replace("'", "''")}', " +
        $"'{reader.GetString(6).Replace("'", "''")}', {activo}, '{reader.GetDateTime(8):yyyy-MM-dd HH:mm:ss}')");
}
reader.Close();
if (userRows.Count > 0)
{
    output.AppendLine("INSERT INTO Usuarios (Id, Nombres, Apellidos, Documento, TipoDocumento, Contrasena, Rol, Activo, FechaCreacion) VALUES");
    output.AppendLine(string.Join(",\n", userRows) + ";");
    output.AppendLine();
}
Console.WriteLine($"Usuarios: {userRows.Count}");

// Configuracion
cmd.CommandText = "SELECT Id, Clave, Valor, Descripcion FROM Configuracion";
reader = cmd.ExecuteReader();
var cfgRows = new List<string>();
while (reader.Read())
{
    var val = reader.IsDBNull(2) ? "NULL" : $"'{reader.GetString(2).Replace("'", "''")}'";
    var desc = reader.IsDBNull(3) ? "NULL" : $"'{reader.GetString(3).Replace("'", "''")}'";
    cfgRows.Add($"({reader.GetInt32(0)}, '{reader.GetString(1).Replace("'", "''")}', {val}, {desc})");
}
reader.Close();
if (cfgRows.Count > 0)
{
    output.AppendLine("INSERT INTO Configuracion (Id, Clave, Valor, Descripcion) VALUES");
    output.AppendLine(string.Join(",\n", cfgRows) + ";");
    output.AppendLine();
}
Console.WriteLine($"Configuracion: {cfgRows.Count}");

// ProductoCodigosBarras
cmd.CommandText = "SELECT Id, ProductoId, CodigoBarras FROM ProductoCodigosBarras";
reader = cmd.ExecuteReader();
var barcodeRows = new List<string>();
while (reader.Read())
{
    barcodeRows.Add($"({reader.GetInt32(0)}, {reader.GetInt32(1)}, '{reader.GetString(2).Replace("'", "''")}')");
}
reader.Close();
if (barcodeRows.Count > 0)
{
    for (int i = 0; i < barcodeRows.Count; i += 50)
    {
        var batch = barcodeRows.Skip(i).Take(50).ToList();
        output.AppendLine("INSERT INTO ProductoCodigosBarras (Id, ProductoId, CodigoBarras) VALUES");
        output.AppendLine(string.Join(",\n", batch) + ";");
        output.AppendLine();
    }
}
Console.WriteLine($"CodigosBarras: {barcodeRows.Count}");

// Cajas
cmd.CommandText = "SELECT Id, NumeroCaja, Usuario, MontoInicial, FechaApertura, FechaCierre, MontoCierreEsperado, MontoCierreReal, Diferencia, ObservacionesApertura, ObservacionesCierre, Estado FROM Cajas";
reader = cmd.ExecuteReader();
var cajaRows = new List<string>();
while (reader.Read())
{
    var fc = reader.IsDBNull(5) ? "NULL" : $"'{reader.GetDateTime(5):yyyy-MM-dd HH:mm:ss}'";
    var mce = reader.IsDBNull(6) ? "NULL" : reader.GetDecimal(6).ToString("F2");
    var mcr = reader.IsDBNull(7) ? "NULL" : reader.GetDecimal(7).ToString("F2");
    var dif = reader.IsDBNull(8) ? "NULL" : reader.GetDecimal(8).ToString("F2");
    var oba = reader.IsDBNull(9) ? "NULL" : $"'{reader.GetString(9).Replace("'", "''")}'";
    var obc = reader.IsDBNull(10) ? "NULL" : $"'{reader.GetString(10).Replace("'", "''")}'";
    cajaRows.Add($"({reader.GetInt32(0)}, {reader.GetInt32(1)}, '{reader.GetString(2).Replace("'", "''")}', {reader.GetDecimal(3):F2}, " +
        $"'{reader.GetDateTime(4):yyyy-MM-dd HH:mm:ss}', {fc}, {mce}, {mcr}, {dif}, {oba}, {obc}, '{reader.GetString(11).Replace("'", "''")}')");
}
reader.Close();
if (cajaRows.Count > 0)
{
    output.AppendLine("INSERT INTO Cajas (Id, NumeroCaja, Usuario, MontoInicial, FechaApertura, FechaCierre, MontoCierreEsperado, MontoCierreReal, Diferencia, ObservacionesApertura, ObservacionesCierre, Estado) VALUES");
    output.AppendLine(string.Join(",\n", cajaRows) + ";");
    output.AppendLine();
}
Console.WriteLine($"Cajas: {cajaRows.Count}");

// Ventas
cmd.CommandText = "SELECT Id, NumeroVenta, CajaId, ClienteId, FechaVenta, SubTotal, Impuesto, Total, MetodoPago, MontoPagado, Cambio, Usuario, Anulada, MotivoAnulacion FROM Ventas";
reader = cmd.ExecuteReader();
var ventaRows = new List<string>();
while (reader.Read())
{
    var cliId = reader.IsDBNull(3) ? "NULL" : reader.GetInt32(3).ToString();
    var motivo = reader.IsDBNull(13) ? "NULL" : $"'{reader.GetString(13).Replace("'", "''")}'";
    var anulada = reader.GetBoolean(12) ? "TRUE" : "FALSE";
    ventaRows.Add($"({reader.GetInt32(0)}, '{reader.GetString(1).Replace("'", "''")}', {reader.GetInt32(2)}, {cliId}, " +
        $"'{reader.GetDateTime(4):yyyy-MM-dd HH:mm:ss}', {reader.GetDecimal(5):F2}, {reader.GetDecimal(6):F2}, {reader.GetDecimal(7):F2}, " +
        $"'{reader.GetString(8).Replace("'", "''")}', {reader.GetDecimal(9):F2}, {reader.GetDecimal(10):F2}, " +
        $"'{reader.GetString(11).Replace("'", "''")}', {anulada}, {motivo})");
}
reader.Close();
if (ventaRows.Count > 0)
{
    output.AppendLine("INSERT INTO Ventas (Id, NumeroVenta, CajaId, ClienteId, FechaVenta, SubTotal, Impuesto, Total, MetodoPago, MontoPagado, Cambio, Usuario, Anulada, MotivoAnulacion) VALUES");
    output.AppendLine(string.Join(",\n", ventaRows) + ";");
    output.AppendLine();
}
Console.WriteLine($"Ventas: {ventaRows.Count}");

// DetalleVentas
cmd.CommandText = "SELECT Id, VentaId, ProductoId, Cantidad, PrecioUnitario, CostoUnitario, SubTotal, Impuesto, Total FROM DetalleVentas";
reader = cmd.ExecuteReader();
var detRows = new List<string>();
while (reader.Read())
{
    detRows.Add($"({reader.GetInt32(0)}, {reader.GetInt32(1)}, {reader.GetInt32(2)}, {reader.GetInt32(3)}, {reader.GetDecimal(4):F2}, " +
        $"{reader.GetDecimal(5):F2}, {reader.GetDecimal(6):F2}, {reader.GetDecimal(7):F2}, {reader.GetDecimal(8):F2})");
}
reader.Close();
if (detRows.Count > 0)
{
    output.AppendLine("INSERT INTO DetalleVentas (Id, VentaId, ProductoId, Cantidad, PrecioUnitario, CostoUnitario, SubTotal, Impuesto, Total) VALUES");
    output.AppendLine(string.Join(",\n", detRows) + ";");
    output.AppendLine();
}
Console.WriteLine($"DetalleVentas: {detRows.Count}");

var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..", "Database", "MigracionCorrecta.sql");
File.WriteAllText(outputPath, output.ToString());
Console.WriteLine($"\nArchivo generado: {outputPath}");
