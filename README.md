# Ecommerce-Api

API académica de Tiendas Mass en un único proyecto .NET 10. Catalog expone consulta de productos, browsing por tienda y sincronización POS; los otros cinco módulos conservan sus puntos de composición.

> Nota (2026-11-19): este README conserva el detalle histórico de la etapa Catalog-only. El contrato vigente se publica en `/openapi/v1.json` y la organización interna vigente está en [`docs/architecture/refactoring-report.md`](docs/architecture/refactoring-report.md).

## Arquitectura

El proyecto usa **Modular Monolith + Clean Architecture por módulo**. Cada módulo sigue el flujo:

`Controller → Application Service → Domain Repository Interface → Infrastructure Repository Implementation → DbContext`

La estructura final, la matriz de migración, los registros DI y las verificaciones ejecutadas están en el reporte de refactorización enlazado arriba.

## Flujo de la consulta

`CatalogController → IProductService → ProductService → IProductRepository → implementación configurada`

El Controller valida parámetros mediante ApiController, el servicio convierte Product a DTOs y el repositorio consulta datos. Se conserva JSON camelCase, precio anidado y ProblemDetails 400/500. Las entidades mantienen sus reglas y setters privados. Models, CatalogDbContext, configuraciones y migraciones permanecen sin cambios.

- `Catalog:DataSource=Demo`: producto ficticio, solo Development, sin PostgreSQL. Development lo configura explícitamente.
- `Catalog:DataSource=PostgreSQL`: consulta real con EF Core y `ConnectionStrings:PostgreSQL`. Es el valor predeterminado si no hay configuración de fuente.
- No hay fallback automático a Demo ante un error de PostgreSQL. Production rechaza Demo al arrancar.

No hay rutas de escritura, consulta por ID, categorías, tiendas, surtido ni zonas. Cambiar la fuente de datos no agrega endpoints.

## Ejecutar y consultar

Desde esta carpeta:

```powershell
dotnet restore
dotnet build
dotnet run --launch-profile http
```

| Método | URL | Uso |
|---|---|---|
| GET | http://localhost:5272/health | Estado del proceso |
| GET | http://localhost:5272/api/v1/catalog/products?page=1&pageSize=10 | Única operación de Catalog |
| GET | http://localhost:5272/openapi/v1.json | Contrato generado en Development |

`page` comienza en 1; `pageSize` admite 1..100. Valores por defecto: 1 y 10. Una página vacía devuelve HTTP 200 con items vacío. Los parámetros inválidos devuelven HTTP 400. No se confirma stock físico ni disponibilidad POS.

Consulta [la guía de Apidog](docs/catalog-apidog.md), [el contrato exportado](docs/openapi/catalog-v1.json) y [el informe actual de limpieza](docs/catalog-single-controller.md). El contrato se guarda explícitamente mediante `Scripts/Export-CatalogOpenApi.ps1`; consumir productos no crea ese archivo.

## PostgreSQL y conexión

La base prevista es `ecommerce_db`; Catalog es propietario exclusivo del esquema `catalog`. La configuración usa:

```text
ConnectionStrings:PostgreSQL
ConnectionStrings__PostgreSQL
```

La aplicación no trae una contraseña por defecto y puede iniciar sin PostgreSQL, pero las rutas de Catalog que consultan datos necesitan una conexión disponible. Desde Windows, el ejemplo local es `Host=localhost;Port=55432;Database=ecommerce_db;Username=postgres;Password=<local-secret>`.

Se preparó `docker-compose.yml` con la imagen oficial `postgres:17.12-alpine3.24`, volumen persistente, puerto configurable y healthcheck. Tiene el proyecto/contenedor/volumen `ecommerce-api-catalog`/`ecommerce-postgres-catalog`/`ecommerce_api_catalog_postgres_data`, separados del PostgreSQL existente `ecommerce-postgres`. La contraseña se lee de `.env`, excluido por `.gitignore`; `.env.example` solo contiene un valor de reemplazo. No subas credenciales reales.

En este entorno se reutiliza el contenedor existente `ecommerce-postgres` en `localhost:55432`; para iniciarlo si está detenido:

```powershell
docker start ecommerce-postgres
```

En una instalación nueva, desde `Ecommerce-Api/`:

```powershell
Copy-Item .env.example .env
# Edita .env y reemplaza POSTGRES_PASSWORD por una contraseña local

docker compose up -d postgres
docker compose ps
```

El Compose nuevo publica `55433` por defecto; dentro de Docker PostgreSQL escucha en `5432`. Usa el puerto `55432` y el usuario existente para el contenedor actual; el Compose aislado usa el puerto `55433` y el usuario configurado en `.env`. No uses `docker compose down -v`: el volumen conserva la base local.

Antes de aplicar una migración, inspecciona la base/esquema existente en DataGrip o pgAdmin para evitar colisiones con tablas anteriores. Con la conexión en una variable de entorno:

```powershell
$env:ConnectionStrings__PostgreSQL = 'Host=localhost;Port=55432;Database=ecommerce_db;Username=ecommerce_user;Password=<local-secret>'
dotnet ef migrations list --project Ecommerce-Api.csproj --context CatalogDbContext
dotnet ef database update --project Ecommerce-Api.csproj --context CatalogDbContext
```

Catalog tiene su propio historial `catalog.__ef_migrations_history_catalog`. No se utiliza `EnsureCreated()` ni se crean contextos globales. Las migraciones crean únicamente las tablas de Catalog.

Para una base nueva, ejecuta `dotnet ef database update` directamente. Para una base legado con el historial público `InitialCreate` y las cuatro tablas originales de Catalog, primero inspecciona su esquema y ejecuta `Scripts/BaselineExistingCatalogSchema.sql`; el script valida la base, tablas, columnas, índices, constraints y datos antes de registrar el baseline. Después ejecuta `dotnet ef database update`. Si alguna validación falla, detente y revisa el esquema antes de seguir.

Las migraciones `Catalog_InitialCreate` y `Catalog_AssortmentAndCatalogConstraints`, junto con `Catalog_Migrations_Idempotent.sql`, están en `Features/Catalog/Persistence/Migrations/`. El SQL idempotente puede inspeccionarse antes de ejecutar. En el entorno actual se detectó y reutilizó el contenedor `ecommerce-postgres`; se inspeccionaron sus tablas vacías, claves, índices y el historial legado, se registró el baseline de Catalog y se aplicó `Catalog_AssortmentAndCatalogConstraints` sin recrear ni borrar tablas existentes.

## Tablas, relaciones y restricciones

| Tabla | Clave primaria | Relaciones | Índices y checks |
|---|---|---|---|
| `catalog.category` | `pk_category` | — | — |
| `catalog.product` | `pk_product` | `fk_product_category` → category | `uq_product_sku`, `ix_product_category_id`, checks de precio, moneda y formato SKU |
| `catalog.store` | `pk_store` | — | `uq_store_code`, checks de código, rangos y par de coordenadas |
| `catalog.delivery_zone` | `pk_delivery_zone` | `fk_delivery_zone_store` → store | `ix_delivery_zone_store_id` |
| `catalog.store_assortment` | `pk_store_assortment` | FKs a store y product | `uq_store_assortment_store_id_product_id`, `ix_store_assortment_product_id` |

Todas las FK quedan dentro del esquema `catalog`; no hay referencias a otros módulos. Los nombres de tablas y columnas usan `snake_case`; fechas son `timestamptz`, importes `numeric`, identificadores `uuid` y banderas `boolean`.

## Verificación

Solo existe Ecommerce-Api.csproj; no hay un proyecto de pruebas .NET. `dotnet test` termina sin descubrir pruebas. Las verificaciones ejecutables disponibles son:

```powershell
pwsh -NoProfile -File Scripts/Test-Architecture.ps1
pwsh -NoProfile -File Scripts/Test-CatalogDomain.ps1 -AssemblyPath bin/Debug/net10.0/Ecommerce-Api.dll
# Requiere API iniciada:
pwsh -NoProfile -File Scripts/Test-CatalogContract.ps1
pwsh -NoProfile -File Scripts/Test-CatalogApi.ps1
# Inicia instancias aisladas en 5290; no usa una base real:
pwsh -NoProfile -File Scripts/Test-CatalogModes.ps1
pwsh -NoProfile -File Scripts/Export-CatalogOpenApi.ps1
```

Test-CatalogApi ahora verifica solo la exposición de rutas; ya no crea ni elimina registros. Comprueba que OpenAPI publique una sola operación de Catalog, que las 20 rutas retiradas devuelvan 404 y que POST/PUT/PATCH/DELETE sobre products devuelvan 405. Test-CatalogModes repite esa comprobación con PostgreSQL seleccionado y una conexión deliberadamente inaccesible; también verifica el error 500 y el rechazo de Demo en Production. No equivale a probar PostgreSQL real.

No se añadieron funcionalidades de Inventory, Sales, Payments, Billing, Incidents, POS, reservas ni delivery.