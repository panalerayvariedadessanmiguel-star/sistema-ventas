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

try
{
    using var scope = app.Services.CreateScope();
    var seed = scope.ServiceProvider.GetRequiredService<ConfiguracionRepository>();
    await seed.SeedDesignDefaultsAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Error al inicializar configuracion de la BD (puede que no existan tablas aun)");
}

app.UseCors();
app.MapControllers();
app.Run();
