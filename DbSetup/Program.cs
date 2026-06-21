using Npgsql;

var connStr = "Host=aws-1-sa-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.uwtrxrmfrhzbjtguvofy;Password=0103016Jica*;SSL Mode=Require;Trust Server Certificate=true";
await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();

Console.WriteLine("Dropping all tables...");
var tables = new[] {
    "DetalleConteoFisico", "ConteosFisicos", "DetalleVentas", "Ventas",
    "MovimientosCaja", "Cajas", "InventarioMovimientos", "Stock",
    "ProductoCodigosBarras", "Productos", "Categorias",
    "Transaccion", "Configuracion", "Usuarios", "Clientes"
};
foreach (var t in tables)
{
    try
    {
        await using var cmd = new NpgsqlCommand($"DROP TABLE IF EXISTS \"{t}\" CASCADE", conn);
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine($"  Dropped {t}");
    }
    catch { }
}

Console.WriteLine("Creating tables...");
string sql = File.ReadAllText(@"C:\SistemaVentas\Database\CreateDatabasePostgreSQL_clean.sql");
var lines = sql.Split('\n');
string clean = "";
foreach (var line in lines)
{
    var tl = line.Trim();
    if (tl.StartsWith("--") || tl.StartsWith("\ufeff--")) continue;
    clean += line + "\n";
}
foreach (var stmt in clean.Split(';', StringSplitOptions.RemoveEmptyEntries))
{
    var ts = stmt.Trim();
    if (ts.Length < 5) continue;
    try { await using var cmd = new NpgsqlCommand(ts, conn); await cmd.ExecuteNonQueryAsync(); }
    catch { }
}

Console.WriteLine("Recreating seed data with correct categories (Higiene=4 with Base 5 En 1 Nailen)...");
var seedSql = @"
INSERT INTO Categorias (Nombre, Descripcion) VALUES ('General', 'Categoria general'), ('Bebidas', 'Bebidas'), ('Alimentos', 'Alimentos'), ('Limpieza', 'Limpieza'), ('Higiene', 'Higiene'), ('Lacteos', 'Lacteos'), ('Mecato', 'Mecato');

INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, PrecioCompra, PrecioVenta, Stock, StockMinimo) VALUES
    ('77010001', 'Base 5 En 1 Nailen', 'Base 5 en 1 Nailen 120ml', 5, 5000, 12000, 2, 1),
    ('77010002', 'Shampoo Sedal 400ml', 'Shampoo Sedal Ceramidas 400ml', 5, 8000, 18000, 5, 2),
    ('77010003', 'Crema Dental Colgate 75ml', 'Crema dental Colgate triple accion', 5, 3000, 7000, 10, 3),
    ('77010004', 'Jabon Rexona 90g', 'Jabon Rexona antibacterial', 5, 1500, 3500, 15, 5),
    ('77010005', 'Desodorante Axe 150ml', 'Desodorante Axe aerosol', 5, 7000, 15000, 8, 3),
    ('77020001', 'Coca-Cola 1.5L', 'Gaseosa Coca-Cola 1.5 litros', 2, 2500, 5000, 12, 5),
    ('77020002', 'Agua Brisa 600ml', 'Agua Brisa sin gas 600ml', 2, 1000, 2000, 20, 5),
    ('77020003', 'Jugo Hit 1L', 'Jugo Hit en caja 1 litro', 2, 2000, 4000, 8, 3),
    ('77030001', 'Arroz Diana 1kg', 'Arroz Diana 1kg', 3, 2000, 3800, 10, 3),
    ('77030002', 'Aceite Gourmet 900ml', 'Aceite vegetal Gourmet 900ml', 3, 4000, 8500, 6, 2),
    ('77030003', 'Pan Bimbo 500g', 'Pan de molde Bimbo 500g', 3, 3500, 7000, 5, 2),
    ('77060001', 'Leche Colanta 1L', 'Leche entera Colanta 1 litro', 6, 2500, 4500, 10, 3),
    ('77060002', 'Yogurt Alpina 500ml', 'Yogurt griego Alpina 500ml', 6, 3000, 6000, 7, 2),
    ('77070001', 'Papas Margarita 40g', 'Papas fritas Margarita limon', 7, 1000, 2000, 30, 10),
    ('77070002', 'Chocolatina Jet 30g', 'Chocolatina Jet blanco', 7, 800, 1500, 40, 10);

INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES
    ('NIT', '123456789-0', 'NIT'), ('DIRECCION', 'Calle 123 #45-67, Bogota', 'Direccion'),
    ('TELEFONO', '601 123 4567', 'Telefono'), ('NOMBRE_EMPRESA', 'Tienda de Prueba SAS', 'Nombre'),
    ('SITE_NAME', 'Tienda de Prueba', 'Nombre del sitio'),
    ('COLOR_PRIMARY', '#3b82f6', 'Color principal'),
    ('TITLE_HOME', 'Bienvenido a Tienda de Prueba', 'Titulo'),
    ('SUBTITLE_HOME', 'Productos de prueba para validar el sistema', 'Subtitulo'),
    ('QR_IMAGE', '', 'URL QR Nequi');

INSERT INTO Usuarios (Nombres, Apellidos, Documento, TipoDocumento, Contrasena, Rol) VALUES
    ('Admin', 'Prueba', '1053803950', 'CC', '', 'Administrador');
";
foreach (var stmt in seedSql.Split(';', StringSplitOptions.RemoveEmptyEntries))
{
    var ts = stmt.Trim();
    if (ts.Length < 5) continue;
    try { await using var cmd = new NpgsqlCommand(ts, conn); await cmd.ExecuteNonQueryAsync(); }
    catch (Exception ex) { Console.WriteLine($"  ERR: {ex.Message.Split('.')[0]}: {ts[..Math.Min(ts.Length, 80)]}"); }
}

Console.WriteLine("Test DB ready!");
