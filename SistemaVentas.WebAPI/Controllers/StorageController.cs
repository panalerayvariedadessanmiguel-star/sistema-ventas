using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.Helpers;
using SistemaVentas.WebAPI.Services;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly LocalStorageService _storage;
    private readonly string _imagesDir;

    public StorageController(LocalStorageService storage, IConfiguration config)
    {
        _storage = storage;
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
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        if (file == null || file.Length == 0)
            return BadRequest(new { mensaje = "Debe seleccionar un archivo" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowed.Contains(ext))
            return BadRequest(new { mensaje = "Formato no permitido. Use: jpg, png, webg, gif" });

        using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName);

        return Ok(new { url });
    }

    [HttpGet("files/{fileName}")]
    public IActionResult GetFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains(".."))
            return BadRequest();

        var filePath = Path.Combine(_imagesDir, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

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
}
