Add-Type -AssemblyName System.Data
Add-Type -AssemblyName System.Net.Http

$api = "https://sistema-ventas-api-6x1w.onrender.com/api"
$token = "MTpBZG1pbmlzdHJhZG9y"
$imgsDir = "C:\SistemaVentas\Imagenes"

$client = New-Object System.Net.Http.HttpClient
$client.DefaultRequestHeaders.Add("xAdminToken", $token)

$conn = New-Object System.Data.SqlClient.SqlConnection("Server=(LocalDB)\MSSQLLocalDB;Database=SistemaVentasDB;Integrated Security=true;TrustServerCertificate=true")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT p.Id, p.Nombre, p.ImagenUrl FROM Productos p WHERE p.ImagenUrl IS NOT NULL AND p.ImagenUrl != '' AND p.Activo = 1"
$reader = $cmd.ExecuteReader()
$products = @()
while ($reader.Read()) { $products += @{ id = $reader["Id"]; nombre = $reader["Nombre"]; imagenUrl = $reader["ImagenUrl"]; found = $false } }
$reader.Close()
$conn.Close()
Write-Host "Productos con imagen local: $($products.Count)" -ForegroundColor Cyan

$resp = $client.GetAsync("$api/productos").GetAwaiter().GetResult()
$cloudList = ($resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()) | ConvertFrom-Json
$cloudByName = @{}
$cloudList | ForEach-Object { $cloudByName[$_.nombre] = $_ }
Write-Host "Productos en nube: $($cloudList.Count)" -ForegroundColor Cyan

# Build file index
$files = @{}
Get-ChildItem $imgsDir -File | ForEach-Object { $files[$_.BaseName.ToLowerInvariant()] = $_.FullName }

$ok = 0; $fail = 0; $skip = 0
foreach ($p in $products) {
    $nombre = $p.nombre
    $cloud = $cloudByName[$nombre]
    if (-not $cloud) { Write-Host "  SKIP: '$nombre' no en nube" -ForegroundColor DarkYellow; $skip++; continue }

    $file = $null
    if ($p.imagenUrl -match "/([^/]+\.\w+)$") {
        $fname = [System.Uri]::UnescapeDataString($matches[1])
        $path = Join-Path $imgsDir $fname
        if (Test-Path $path) { $file = $path }
    }
    if (-not $file) {
        $base = $nombre.ToLowerInvariant()
        $matchKey = $files.Keys | Where-Object { $_ -eq $base -or $_.StartsWith($base) } | Select-Object -First 1
        if ($matchKey) { $file = $files[$matchKey] }
    }
    if (-not $file) { Write-Host "  SKIP: '$nombre' - sin archivo" -ForegroundColor DarkYellow; $skip++; continue }

    Write-Host "  $nombre ..." -NoNewline
    try {
        $imgBytes = [System.IO.File]::ReadAllBytes($file)
        $imgName = [System.IO.Path]::GetFileName($file)

        # Build multipart form data using StreamContent
        $boundary = "---BOUNDARY$([Guid]::NewGuid().ToString('N'))"
        $crlf = "`r`n"
        $stream = New-Object System.IO.MemoryStream
        $writer = New-Object System.IO.StreamWriter($stream)
        $writer.Write("--$boundary$crlf")
        $writer.Write("Content-Disposition: form-data; name=`"file`"; filename=`"$imgName`"$crlf")
        $writer.Write("Content-Type: application/octet-stream$crlf$crlf")
        $writer.Flush()
        $stream.Write($imgBytes, 0, $imgBytes.Length)
        $writer.Write("$crlf--$boundary--$crlf")
        $writer.Flush()
        $stream.Position = 0

        $content = New-Object System.Net.Http.StreamContent($stream)
        $content.Headers.Remove("Content-Type")
        $content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=$boundary")

        $resp2 = $client.PostAsync("$api/storage/upload", $content).GetAwaiter().GetResult()
        $body = $resp2.Content.ReadAsStringAsync().GetAwaiter().GetResult()

        if ($resp2.IsSuccessStatusCode) {
            $result = $body | ConvertFrom-Json
            $url = $result.url
            Write-Host " OK" -ForegroundColor Green

            $updateBody = @{
                codigoBarras = $cloud.codigoBarras
                nombre = $cloud.nombre
                descripcion = $cloud.descripcion
                categoriaId = $cloud.categoriaId
                precioCompra = $cloud.precioCompra
                precioVenta = $cloud.precioVenta
                stock = $cloud.stock
                stockMinimo = $cloud.stockMinimo
                imagenUrl = "$api$url"
                orden = $cloud.orden
                activo = $cloud.activo
            } | ConvertTo-Json

            $updContent = New-Object System.Net.Http.StringContent($updateBody, [System.Text.Encoding]::UTF8, "application/json")
            $resp3 = $client.PutAsync("$api/productos/$($cloud.id)", $updContent).GetAwaiter().GetResult()
            if ($resp3.IsSuccessStatusCode) { $ok++ } else { Write-Host "  (DB update: $($resp3.StatusCode))" -ForegroundColor Yellow; $ok++ }
            $stream.Dispose()
        } else {
            Write-Host " FAIL: $body" -ForegroundColor Red; $fail++
            $stream.Dispose()
        }
    } catch {
        Write-Host " ERROR: $_" -ForegroundColor Red; $fail++
    }
}

$client.Dispose()
Write-Host "`nOK=$ok SKIP=$skip FAIL=$fail" -ForegroundColor $(if($fail -eq 0){"Green"}else{"Yellow"})