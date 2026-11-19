using System.Text.Json;
using Ecommerce_Api.Features.Catalog.Application.Contracts;
using Ecommerce_Api.Features.Catalog.Application.Dtos.Response;
using Ecommerce_Api.Features.Catalog.Application.Services;
using Ecommerce_Api.Features.Catalog.Domain.Entities;
using Ecommerce_Api.Features.Catalog.Domain.Exceptions;
using Ecommerce_Api.Features.Catalog.Domain.ValueObjects;

namespace Ecommerce_Api.Features.Catalog.Application.Services.Impl;

public sealed class CatalogSynchronizationService(
    IPosCatalogSource source, IConfiguration configuration, ICatalogSynchronizationStore repository,
    ILogger<CatalogSynchronizationService> logger) : ICatalogSynchronizationService
{
    public async Task<CatalogSynchronizationResponseDto> SynchronizeAsync(string correlationId, CancellationToken ct)
    {
        var run = await repository.StartAsync(ct);
        try
        {
            var prices = ReadAcademicPrices();
            var products = await source.ReadProductsAsync(correlationId, ct);
            var stores = await source.ReadStoresAsync(correlationId, ct);
            ValidateSource(products, stores);
            var assortments = new Dictionary<string, IReadOnlyList<CatalogSourceAssortment>>(StringComparer.Ordinal);
            foreach (var store in stores.Where(store => store.Activa))
            {
                var items = await source.ReadAssortmentsAsync(store.Codigo, correlationId, ct);
                if (items.Any(item => item.TiendaCodigo != store.Codigo || string.IsNullOrWhiteSpace(item.Sku))
                    || items.Select(item => item.Sku).Distinct(StringComparer.Ordinal).Count() != items.Count)
                    throw new InvalidDataException("Invalid assortment source.");
                var knownSkus = products.Select(product => product.Sku).ToHashSet(StringComparer.Ordinal);
                if (items.Any(item => !knownSkus.Contains(item.Sku))) throw new InvalidDataException("Unknown source SKU.");
                assortments.Add(store.Codigo, items);
            }
            await repository.ApplyAsync(run.Id, new(products, stores, assortments), prices, ct);
        }
        catch (Exception exception)
        {
            var code = exception switch
            {
                OperationCanceledException when ct.IsCancellationRequested => "synchronization_cancelled",
                OperationCanceledException => "pos_timeout",
                HttpRequestException { StatusCode: { } status } => $"pos_http_{(int)status}",
                HttpRequestException => "pos_unavailable",
                JsonException or InvalidDataException => "pos_invalid_payload",
                CatalogDomainException => "catalog_source_validation_failed",
                InvalidOperationException or FormatException => "synchronization_configuration_invalid",
                _ => "catalog_synchronization_failed"
            };
            // Do not persist exception messages, URLs, credentials, SQL or response bodies.
            logger.LogWarning("Catalog synchronization {SynchronizationId} failed with {ErrorCode}", run.Id, code);
            using var failureTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await repository.FailAsync(run.Id, code, failureTimeout.Token);
        }
        var completed = await repository.FindAsync(run.Id, CancellationToken.None)
            ?? throw new InvalidOperationException("Synchronization result not found.");
        return Map(completed);
    }

    public async Task<CatalogSynchronizationResponseDto?> FindAsync(Guid id, CancellationToken ct)
    {
        var run = await repository.FindAsync(id, ct);
        return run is null ? null : Map(run);
    }

    private Dictionary<string, decimal> ReadAcademicPrices()
    {
        var result = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var entry in configuration.GetSection("Catalog:AcademicPrices").GetChildren())
        {
            var sku = Sku.Create(entry.Key).Value;
            if (!decimal.TryParse(entry.Value, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign,
                System.Globalization.CultureInfo.InvariantCulture, out var amount))
                throw new FormatException("Academic prices require explicit decimal amounts.");
            result.Add(sku, Money.Create(amount, "PEN").Amount);
        }
        return result;
    }

    private static void ValidateSource(IReadOnlyList<CatalogSourceProduct> products, IReadOnlyList<CatalogSourceStore> stores)
    {
        if (products.Any(product => string.IsNullOrWhiteSpace(product.Sku) || string.IsNullOrWhiteSpace(product.Nombre)
                || string.IsNullOrWhiteSpace(product.Categoria))
            || stores.Any(store => string.IsNullOrWhiteSpace(store.Codigo) || string.IsNullOrWhiteSpace(store.Nombre)
                || string.IsNullOrWhiteSpace(store.Direccion) || store.Latitud.HasValue != store.Longitud.HasValue
                || store.Latitud is < -90 or > 90 || store.Longitud is < -180 or > 180))
            throw new InvalidDataException("Required source properties missing.");
        if (products.Select(product => Sku.Create(product.Sku).Value).Distinct(StringComparer.Ordinal).Count() != products.Count
            || stores.Select(store => store.Codigo.Trim().ToUpperInvariant()).Distinct(StringComparer.Ordinal).Count() != stores.Count)
            throw new InvalidDataException("Duplicate source business keys.");
    }

    private static CatalogSynchronizationResponseDto Map(CatalogSynchronizationRun run) => new(
        run.Id, run.Status, run.StartedAt, run.CompletedAt, run.SourceProducts, run.SourceStores, run.SourceAssortments,
        run.ProductsCreated, run.ProductsUpdated, run.CategoriesCreated, run.StoresCreated, run.AssortmentsCreated,
        JsonSerializer.Deserialize<string[]>(run.MissingPriceSkusJson) ?? [],
        "Academic prices explicitly configured in Catalog:AcademicPrices; existing commercial prices preserved.", run.ErrorCode);
}
