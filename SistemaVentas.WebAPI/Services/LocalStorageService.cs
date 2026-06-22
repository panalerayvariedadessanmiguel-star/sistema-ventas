namespace SistemaVentas.WebAPI.Services;

public class LocalStorageService
{
    private readonly string _storagePath;
    private readonly ILogger<LocalStorageService> _logger;
    private readonly string _baseUrl;

    public LocalStorageService(IConfiguration config, ILogger<LocalStorageService> logger)
    {
        _logger = logger;
        var configured = config["Storage:ImagesPath"];
        if (!string.IsNullOrEmpty(configured))
        {
            _storagePath = Path.GetFullPath(configured);
        }
        else
        {
            _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Imagenes");
            _storagePath = Path.GetFullPath(_storagePath);
        }
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
        _baseUrl = "/api/storage/files";
        _logger.LogInformation("Almacenamiento local en: {Path}", _storagePath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName)
    {
        string safeName = SanitizeFileName(fileName);
        string filePath = Path.Combine(_storagePath, safeName);

        using (var fs = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        _logger.LogInformation("Imagen guardada: {Path}", filePath);
        return $"{_baseUrl}/{Uri.EscapeDataString(safeName)}";
    }

    public static string SanitizeFileName(string fileName)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidos, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(sanitized) ? $"imagen_{Guid.NewGuid():N}.jpg" : sanitized;
    }

    public void Delete(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;
        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(_storagePath, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Imagen eliminada: {Path}", filePath);
        }
    }
}
