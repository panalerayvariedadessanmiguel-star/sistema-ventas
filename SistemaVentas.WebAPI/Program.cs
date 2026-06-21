using Npgsql;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Repositories;
using SistemaVentas.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddSingleton(new DbConnection(connectionString));
builder.Services.AddScoped<ProductoRepository>();
builder.Services.AddScoped<ProductoVarianteRepository>();
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<VentaRepository>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<ConfiguracionRepository>();
builder.Services.AddScoped<StockRepository>();
builder.Services.AddScoped<FacturacionService>();
builder.Services.AddSingleton<ImpresionService>();
builder.Services.AddHttpClient<NotificacionService>(client => { client.Timeout = TimeSpan.FromSeconds(5); });
builder.Services.AddSingleton<LocalStorageService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var seed = scope.ServiceProvider.GetRequiredService<ConfiguracionRepository>();
    await seed.SeedDesignDefaultsAsync();
    var db = scope.ServiceProvider.GetRequiredService<DbConnection>();
    using var conn = db.GetConnection();
    conn.Open();
    using var cmd = conn.CreateCommand();

    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Categorias (
            Id SERIAL PRIMARY KEY,
            Nombre VARCHAR(100) NOT NULL,
            Descripcion VARCHAR(500),
            FechaCreacion TIMESTAMP DEFAULT NOW(),
            Activo BOOLEAN DEFAULT TRUE
        );
        CREATE TABLE IF NOT EXISTS Productos (
            Id SERIAL PRIMARY KEY,
            CodigoBarras VARCHAR(50),
            Nombre VARCHAR(200) NOT NULL,
            Descripcion VARCHAR(500),
            CategoriaId INT REFERENCES Categorias(Id),
            PrecioCompra DECIMAL(18,2) NOT NULL DEFAULT 0,
            PrecioVenta DECIMAL(18,2) NOT NULL DEFAULT 0,
            Stock INT NOT NULL DEFAULT 0,
            StockMinimo INT NOT NULL DEFAULT 5,
            FechaCreacion TIMESTAMP DEFAULT NOW(),
            FechaModificacion TIMESTAMP DEFAULT NOW(),
            Activo BOOLEAN DEFAULT TRUE,
            ImagenUrl TEXT NOT NULL DEFAULT '',
            Orden INT NOT NULL DEFAULT 0
        );
        CREATE TABLE IF NOT EXISTS ProductoVariantes (
            Id SERIAL PRIMARY KEY,
            ProductoId INT NOT NULL REFERENCES Productos(Id) ON DELETE CASCADE,
            Nombre VARCHAR(100) NOT NULL,
            ColorHex VARCHAR(7) NOT NULL DEFAULT '#000000',
            Talla VARCHAR(20),
            Stock INT,
            ImagenUrl VARCHAR(500),
            Activo BOOLEAN NOT NULL DEFAULT TRUE,
            Orden INT NOT NULL DEFAULT 0
        );
        CREATE TABLE IF NOT EXISTS Clientes (
            Id SERIAL PRIMARY KEY,
            Documento VARCHAR(50) UNIQUE,
            Nombre VARCHAR(200) NOT NULL,
            Telefono VARCHAR(50),
            Email VARCHAR(200),
            Direccion VARCHAR(500),
            FechaRegistro TIMESTAMP DEFAULT NOW(),
            Activo BOOLEAN DEFAULT TRUE,
            Contrasena VARCHAR(200) NOT NULL DEFAULT ''
        );
        CREATE TABLE IF NOT EXISTS Ventas (
            Id SERIAL PRIMARY KEY,
            NumeroVenta VARCHAR(50) NOT NULL UNIQUE,
            CajaId INT DEFAULT 1,
            ClienteId INT REFERENCES Clientes(Id),
            FechaVenta TIMESTAMP NOT NULL DEFAULT NOW(),
            SubTotal DECIMAL(18,2) NOT NULL,
            Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
            Total DECIMAL(18,2) NOT NULL,
            MetodoPago VARCHAR(50) NOT NULL,
            MontoPagado DECIMAL(18,2) NOT NULL,
            Cambio DECIMAL(18,2) NOT NULL DEFAULT 0,
            Usuario VARCHAR(100) NOT NULL,
            Anulada BOOLEAN DEFAULT FALSE,
            MotivoAnulacion VARCHAR(500),
            Origen VARCHAR(20) DEFAULT 'Web',
            Estado VARCHAR(20) DEFAULT 'Confirmada',
            Domicilio DECIMAL(18,2) NOT NULL DEFAULT 0,
            SincronizadoAPI BOOLEAN NOT NULL DEFAULT FALSE
        );
        CREATE TABLE IF NOT EXISTS DetalleVentas (
            Id SERIAL PRIMARY KEY,
            VentaId INT REFERENCES Ventas(Id),
            ProductoId INT REFERENCES Productos(Id),
            ProductoVarianteId INT,
            Cantidad INT NOT NULL,
            PrecioUnitario DECIMAL(18,2) NOT NULL,
            CostoUnitario DECIMAL(18,2) NOT NULL,
            SubTotal DECIMAL(18,2) NOT NULL,
            Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
            Total DECIMAL(18,2) NOT NULL
        );
        CREATE TABLE IF NOT EXISTS POSSync (
            Id SERIAL PRIMARY KEY,
            VentaId INT NOT NULL REFERENCES Ventas(Id),
            ProductoId INT NOT NULL REFERENCES Productos(Id),
            ProductoVarianteId INT,
            Cantidad INT NOT NULL,
            Fecha TIMESTAMP NOT NULL DEFAULT NOW(),
            Sincronizado BOOLEAN NOT NULL DEFAULT FALSE
        );
        CREATE TABLE IF NOT EXISTS Configuracion (
            Id SERIAL PRIMARY KEY,
            Clave VARCHAR(100) NOT NULL UNIQUE,
            Valor VARCHAR(500),
            Descripcion VARCHAR(500)
        );
        CREATE TABLE IF NOT EXISTS Stock (
            Id SERIAL PRIMARY KEY,
            ProductoId INT REFERENCES Productos(Id),
            Anio INT NOT NULL,
            Mes INT NOT NULL,
            CantidadInicial INT NOT NULL DEFAULT 0,
            CantidadEntrante INT NOT NULL DEFAULT 0,
            CantidadSaliente INT NOT NULL DEFAULT 0,
            CantidadFinal INT NOT NULL DEFAULT 0,
            FechaRegistro TIMESTAMP DEFAULT NOW(),
            UNIQUE (ProductoId, Anio, Mes)
        );
        CREATE TABLE IF NOT EXISTS Usuarios (
            Id SERIAL PRIMARY KEY,
            Nombres VARCHAR(100) NOT NULL,
            Apellidos VARCHAR(100) NOT NULL,
            Documento VARCHAR(50),
            TipoDocumento VARCHAR(20),
            Contrasena VARCHAR(200) NOT NULL,
            Rol VARCHAR(20) DEFAULT 'Cajero',
            Salario DECIMAL(18,2),
            Activo BOOLEAN DEFAULT TRUE,
            FechaCreacion TIMESTAMP DEFAULT NOW()
        );
        CREATE TABLE IF NOT EXISTS InventarioMovimientos (
            Id SERIAL PRIMARY KEY,
            ProductoId INT REFERENCES Productos(Id),
            Tipo VARCHAR(50) NOT NULL,
            Cantidad INT NOT NULL,
            StockAnterior INT NOT NULL,
            StockNuevo INT NOT NULL,
            Motivo VARCHAR(200),
            Fecha TIMESTAMP DEFAULT NOW(),
            Usuario VARCHAR(100) NOT NULL
        );
        CREATE TABLE IF NOT EXISTS ProductoCodigosBarras (
            Id SERIAL PRIMARY KEY,
            ProductoId INT REFERENCES Productos(Id),
            CodigoBarras VARCHAR(50)
        );
        CREATE TABLE IF NOT EXISTS SincronizacionPendiente (
            Id SERIAL PRIMARY KEY,
            VentaId INT NOT NULL,
            JsonVenta TEXT NOT NULL,
            JsonDetalles TEXT NOT NULL,
            Intentos INT DEFAULT 0,
            UltimoError TEXT,
            FechaCreacion TIMESTAMP DEFAULT NOW(),
            FechaUltimoIntento TIMESTAMP
        );
    ";
    await ((NpgsqlCommand)cmd).ExecuteNonQueryAsync();
}

app.UseCors();
app.MapControllers();
app.Run();
