using Dapper;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Helpers;
using SistemaVentas.WebAPI.Services;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly LocalStorageService _storage;
    private readonly DbConnection _db;
    private readonly string _imagesDir;

    public StorageController(LocalStorageService storage, IConfiguration config, DbConnection db)
    {
        _storage = storage;
        _db = db;
        var configured = config["Storage:ImagesPath"];
        if (!string.IsNullOrEmpty(configured))
        {
            _imagesDir = Path.GetFullPath(configured);
        }
        else
        {
            _imagesDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Imagenes"));
        }
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromHeader] string? xAdminToken, [FromQuery] int? productoId = null)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        if (file == null || file.Length == 0)
            return BadRequest(new { mensaje = "Debe seleccionar un archivo" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowed.Contains(ext))
            return BadRequest(new { mensaje = "Formato no permitido. Use: jpg, png, webg, gif" });

        var safeName = _storage.SanitizeFileName(file.FileName);

        using var uploadStream = file.OpenReadStream();
        var url = await _storage.UploadAsync(uploadStream, file.FileName);

        if (productoId.HasValue && productoId.Value > 0)
        {
            try
            {
                using var conn = _db.GetConnection();
                using var dataStream = file.OpenReadStream();
                using var ms = new MemoryStream();
                await dataStream.CopyToAsync(ms);
                var data = ms.ToArray();

                var mime = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream",
                };

                await conn.ExecuteAsync(
                    @"INSERT INTO Imagenes (ProductoId, FileName, Data, MimeType)
                      VALUES (@ProductoId, @FileName, @Data, @MimeType)
                      ON CONFLICT (ProductoId, FileName) DO UPDATE SET Data = @Data, MimeType = @MimeType",
                    new { ProductoId = productoId.Value, FileName = safeName, Data = data, MimeType = mime });
            }
            catch { }
        }

        return Ok(new { url });
    }

    [HttpGet("files/{fileName}")]
    public async Task<IActionResult> GetFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains(".."))
            return BadRequest();

        var filePath = Path.Combine(_imagesDir, fileName);

        if (System.IO.File.Exists(filePath))
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var mime = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
            return PhysicalFile(filePath, mime);
        }

        try
        {
            using var conn = _db.GetConnection();
            var img = await conn.QueryFirstOrDefaultAsync<ImagenDb>(
                "SELECT Data, MimeType FROM Imagenes WHERE FileName = @FileName LIMIT 1",
                new { FileName = fileName });

            if (img != null && img.Data != null)
            {
                System.IO.File.WriteAllBytes(filePath, img.Data);
                return File(img.Data, img.MimeType ?? "image/jpeg");
            }
        }
        catch { }

        return NotFound();
    }

    private class ImagenDb
    {
        public byte[]? Data { get; set; }
        public string? MimeType { get; set; }
    }
}
