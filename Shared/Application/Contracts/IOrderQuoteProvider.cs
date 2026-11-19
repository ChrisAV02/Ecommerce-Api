namespace Ecommerce_Api.Shared.Application.Contracts;

public sealed record QuoteLine(string Sku, int Quantity);
public sealed record QuotedLine(string Sku, string Name, int Quantity, decimal UnitPrice);
public sealed record OrderQuote(Guid StoreId, string StoreCode, IReadOnlyList<QuotedLine> Items, decimal TotalAmount, string Currency);

public interface IOrderQuoteProvider
{
    Task<OrderQuote> QuoteAsync(string storeCode, IReadOnlyList<QuoteLine> items, CancellationToken ct);
}
