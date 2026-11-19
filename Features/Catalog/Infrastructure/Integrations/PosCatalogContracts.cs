using System.Text.Json.Serialization;

namespace Ecommerce_Api.Features.Catalog.Infrastructure.Integrations;

// POS wire DTOs are deliberately separate from public Catalog DTOs and EF entities.
// Quantities are not deserialized: inventory rows only establish SKU/store associations.
internal sealed record PosCatalogPage<T>([property: JsonRequired] IReadOnlyList<T> Items, [property: JsonRequired] int Total,
    [property: JsonRequired] int Pagina, [property: JsonRequired] int TamanoPagina);
internal sealed record PosCatalogProduct([property: JsonRequired] string Sku, [property: JsonRequired] string Nombre,
    string? Descripcion, [property: JsonRequired] string Categoria, [property: JsonRequired] bool Activo);
internal sealed record PosCatalogStore([property: JsonRequired] string Codigo, [property: JsonRequired] string Nombre,
    [property: JsonRequired] string Direccion, decimal? Latitud, decimal? Longitud, [property: JsonRequired] bool Activa);
internal sealed record PosCatalogAssortment([property: JsonRequired] string TiendaCodigo, [property: JsonRequired] string Sku);
