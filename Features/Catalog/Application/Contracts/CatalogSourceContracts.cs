namespace Ecommerce_Api.Features.Catalog.Application.Contracts;

public sealed record CatalogSourceProduct(string Sku, string Nombre, string? Descripcion, string Categoria, bool Activo);
public sealed record CatalogSourceStore(string Codigo, string Nombre, string Direccion, decimal? Latitud, decimal? Longitud, bool Activa);
public sealed record CatalogSourceAssortment(string TiendaCodigo, string Sku);
public sealed record CatalogSourceSnapshot(IReadOnlyList<CatalogSourceProduct> Products,
    IReadOnlyList<CatalogSourceStore> Stores,
    IReadOnlyDictionary<string, IReadOnlyList<CatalogSourceAssortment>> Assortments);
