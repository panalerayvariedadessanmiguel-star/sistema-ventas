# Guia de Configuracion - Sistema de Ventas Online

## Arquitectura
```
Web Store (Next.js) ───→ .NET Web API ───→ Supabase (PostgreSQL) ←─── SistemaVentas WinForms
```

## 1. Configurar Supabase (Base de Datos Compartida)

1. Ve a https://supabase.com y crea una cuenta gratis
2. Crea un nuevo proyecto (elige una region cercana, password seguro)
3. En SQL Editor, pega el contenido de `Database/CreateDatabasePostgreSQL.sql` y ejecutalo
4. Ve a Project Settings → Database → Connection string (Pooler)
5. Copia la cadena de conexion (formato: `Host=db.xxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require`)

## 2. Configurar la Web API

1. Abre `SistemaVentas.WebAPI/appsettings.json`
2. Reemplaza `"Supabase"` connection string con la que copiaste de Supabase
3. Prueba la API:
   ```
   cd SistemaVentas.WebAPI
   dotnet run
   ```
   La API correra en: http://localhost:5062

## 3. Configurar la Tienda Web (Next.js)

1. Asegurate de tener Node.js 18+ instalado
2. En `tienda-web/.env.local` ajusta la URL de la API si es necesario
3. Inicia el frontend:
   ```
   cd tienda-web
   npm install
   npm run dev
   ```
   La tienda correra en: http://localhost:3000

## 4. Sincronizar SistemaVentas WinForms

La sincronizacion es automatica: cada venta realizada en el sistema fisico se envia a la Web API.

Para configurar, edita la URL en `SistemaVentas.Business/Services/SincronizacionService.cs`:
```csharp
private static readonly string ApiUrl = "http://localhost:5062/api";
```

Si la API no esta disponible, la venta local NO se afecta (la sincronizacion falla silenciosamente).

## 5. Probar el Flujo Completo

1. Inicia la API: `cd SistemaVentas.WebAPI && dotnet run`
2. Inicia la tienda web: `cd tienda-web && npm run dev`
3. Abre http://localhost:3000 en tu navegador
4. Agrega productos al carrito y realiza una compra
5. Revisa las ventas en `GET http://localhost:5062/api/ventas`

## 6. Despliegue Gratuito

### Frontend (Next.js) → Vercel (gratis)
1. Crea cuenta en https://vercel.com
2. Conecta tu repositorio de GitHub
3. Selecciona la carpeta `tienda-web`
4. Agrega variable de entorno: `NEXT_PUBLIC_API_URL=https://tu-api.onrender.com/api`
5. Despliega

### Backend (.NET Web API) → Render (gratis)
1. Crea cuenta en https://render.com
2. Crea un "Web Service"
3. Build command: `dotnet publish -c Release`
4. Start command: `dotnet SistemaVentas.WebAPI.dll`
5. Root directory: `SistemaVentas.WebAPI`
6. Agrega variable de entorno: `ConnectionStrings__Supabase=tu-cadena-supabase`

### Base de Datos → Supabase (gratis)
Ya lo configuraste en el paso 1. 500MB gratis para siempre.

## Estructura del Proyecto
```
C:\SistemaVentas\
├── SistemaVentas.slnx
├── SistemaVentas.Data\          # Modelos y repositorios (Dapper)
├── SistemaVentas.Business\      # Logica de negocio (con sincronizacion)
├── SistemaVentas.Presentation\  # WinForms (punto de venta fisico)
├── SistemaVentas.WebAPI\        # API REST .NET 10
│   ├── Controllers\             # Productos, Ventas, Clientes, Auth
│   ├── Repositories\            # Acceso a PostgreSQL via Dapper
│   └── Models\                  # DTOs compartidos
├── tienda-web\                  # Frontend Next.js 16
│   ├── src/
│   │   ├── app/                 # Paginas (Home, Carrito, Checkout)
│   │   ├── components/          # ProductCard, Header, ProductList
│   │   └── lib/                 # API client, CarritoContext
│   └── .env.local               # Configuracion de API URL
└── Database\                    # Scripts SQL (SQL Server + PostgreSQL)
    └── CreateDatabasePostgreSQL.sql  # Esquema para Supabase
```
