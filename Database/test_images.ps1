Add-Type -AssemblyName System.Net.Http
$c = New-Object System.Net.Http.HttpClient

# Test image URL
$r = $c.GetAsync('https://sistema-ventas-api-6x1w.onrender.com/api/storage/files/Cauchos%20B-N.jpg').GetAwaiter().GetResult()
Write-Host ("Image: {0}, Length: {1}" -f $r.StatusCode, $r.Content.Headers.ContentLength)

# Test a product to verify URL is correct
$r2 = $c.GetAsync('https://sistema-ventas-api-6x1w.onrender.com/api/productos?page=1&limit=3').GetAwaiter().GetResult()
$j = $r2.Content.ReadAsStringAsync().GetAwaiter().GetResult()
Write-Host "Products JSON (first 200 chars):"
Write-Host $j.Substring(0, [Math]::Min(200, $j.Length))

$c.Dispose()
