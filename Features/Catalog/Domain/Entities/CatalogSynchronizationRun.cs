namespace Ecommerce_Api.Features.Catalog.Domain.Entities;

public sealed class CatalogSynchronizationRun
{
    private CatalogSynchronizationRun() { }
    public Guid Id { get; private set; }
    public string Status { get; private set; } = "Running";
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int SourceProducts { get; private set; }
    public int SourceStores { get; private set; }
    public int SourceAssortments { get; private set; }
    public int ProductsCreated { get; private set; }
    public int ProductsUpdated { get; private set; }
    public int CategoriesCreated { get; private set; }
    public int StoresCreated { get; private set; }
    public int AssortmentsCreated { get; private set; }
    public string MissingPriceSkusJson { get; private set; } = "[]";
    public string? ErrorCode { get; private set; }

    public static CatalogSynchronizationRun Start(DateTimeOffset now) => new() { Id = Guid.NewGuid(), StartedAt = now };
    public void Complete(int products, int stores, int assortments, int productsCreated, int productsUpdated,
        int categoriesCreated, int storesCreated, int assortmentsCreated, string missingPriceSkusJson)
    {
        Status = "Succeeded";
        CompletedAt = DateTimeOffset.UtcNow;
        SourceProducts = products; SourceStores = stores; SourceAssortments = assortments;
        ProductsCreated = productsCreated; ProductsUpdated = productsUpdated; CategoriesCreated = categoriesCreated;
        StoresCreated = storesCreated; AssortmentsCreated = assortmentsCreated; MissingPriceSkusJson = missingPriceSkusJson;
    }
    public void Fail(string code) { Status = "Failed"; ErrorCode = code; CompletedAt = DateTimeOffset.UtcNow; }
}
