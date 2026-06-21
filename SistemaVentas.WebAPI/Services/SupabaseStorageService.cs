using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SistemaVentas.WebAPI.Services;

public class SupabaseStorageService
{
    private readonly HttpClient _http;
    private readonly HttpClient _adminHttp;
    private readonly ILogger<SupabaseStorageService> _logger;
    private readonly string _bucketName;
    private readonly string _baseStorageUrl;
    private bool _bucketReady;

    public SupabaseStorageService(HttpClient http, IConfiguration config, ILogger<SupabaseStorageService> logger)
    {
        _http = http;
        _logger = logger;

        string url = config["Supabase:Url"] ?? "";
        url = url.TrimEnd('/');
        string anonKey = config["Supabase:AnonKey"] ?? "";
        string serviceRoleKey = config["Supabase:ServiceRoleKey"] ?? "";
        _bucketName = config["Supabase:BucketName"] ?? "productos";

        _baseStorageUrl = string.Concat(url, "/storage/v1");
        _http.BaseAddress = new Uri(string.Concat(_baseStorageUrl, "/"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", anonKey);
        _http.DefaultRequestHeaders.Add("apikey", anonKey);

        _adminHttp = new HttpClient();
        _adminHttp.BaseAddress = new Uri(string.Concat(_baseStorageUrl, "/"));
        string adminKey = !string.IsNullOrEmpty(serviceRoleKey) ? serviceRoleKey : anonKey;
        _adminHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminKey);
        _adminHttp.DefaultRequestHeaders.Add("apikey", adminKey);
    }

    public async Task EnsureBucketExistsAsync()
    {
        try
        {
            var listResp = await _adminHttp.GetAsync("bucket");
            if (listResp.IsSuccessStatusCode)
            {
                string json = await listResp.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var buckets = JsonSerializer.Deserialize<List<Bucket>>(json, opts);
                if (buckets != null && buckets.Any(b => b.Id == _bucketName))
                {
                    _bucketReady = true;
                    return;
                }
            }

            string body = string.Format("{{\"id\":\"{0}\",\"name\":\"{0}\",\"public\":true}}", _bucketName);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var createResp = await _adminHttp.PostAsync("bucket", content);

            if (createResp.IsSuccessStatusCode)
            {
                _bucketReady = true;
            }
            else
            {
                string errorBody = await createResp.Content.ReadAsStringAsync();
                _logger.LogWarning("No se pudo crear el bucket '{Bucket}': {Status} - {Body}", _bucketName, createResp.StatusCode, errorBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error al verificar/crear bucket '{Bucket}': {Msg}", _bucketName, ex.Message);
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName)
    {
        string ext = Path.GetExtension(fileName);
        string uniqueName = string.Concat(Guid.NewGuid().ToString(), ext);
        var fileContent = new StreamContent(fileStream);
        string mime = GetMimeType(ext);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mime);

        if (!_bucketReady)
        {
            await EnsureBucketExistsAsync();
        }

        string encodedPath = string.Concat("object/", _bucketName, "/", uniqueName);
        var resp = await _http.PutAsync(encodedPath, fileContent);

        if (!resp.IsSuccessStatusCode && _adminHttp != _http)
        {
            if (fileStream.CanSeek) fileStream.Seek(0, SeekOrigin.Begin);
            fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mime);
            resp = await _adminHttp.PutAsync(encodedPath, fileContent);
        }

        if (!resp.IsSuccessStatusCode)
        {
            string errBody = await resp.Content.ReadAsStringAsync();
            _logger.LogError("Error al subir imagen a Supabase: {Status} - {Body}", resp.StatusCode, errBody);
            throw new InvalidOperationException(
                $"Error al subir la imagen ({(int)resp.StatusCode}). " +
                $"Asegurate de que el bucket '{_bucketName}' exista en Supabase Storage y sea publico, " +
                "y que tenga las politicas RLS configuradas para permitir inserciones del rol anon.");
        }

        return string.Concat(_baseStorageUrl, "/object/public/", _bucketName, "/", uniqueName);
    }

    public async Task DeleteAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return;
        }

        string prefix = string.Concat(_baseStorageUrl, "/object/public/", _bucketName, "/");
        if (!imageUrl.StartsWith(prefix))
        {
            return;
        }

        string fileName = imageUrl.Substring(prefix.Length);
        string body = string.Format("{{\"prefixes\":[\"{0}\"]}}", fileName);
        var request = new HttpRequestMessage(HttpMethod.Delete, string.Concat("object/", _bucketName));
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        var resp = await _http.SendAsync(request);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("No se pudo eliminar imagen '{File}': {Status}", fileName, resp.StatusCode);
        }
    }

    private static string GetMimeType(string ext)
    {
        switch (ext.ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".webp":
                return "image/webp";
            case ".gif":
                return "image/gif";
            default:
                return "application/octet-stream";
        }
    }

    private record Bucket(string Id, string Name);
}
