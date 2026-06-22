$r = Invoke-RestMethod 'https://sistema-ventas-api-6x1w.onrender.com/api/productos?page=1&limit=5' -UseBasicParsing
Write-Host ($r | ConvertTo-Json -Depth 2)
