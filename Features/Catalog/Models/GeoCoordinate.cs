using Ecommerce_Api.Features.Catalog.Models;

namespace Ecommerce_Api.Features.Catalog.Models;

public sealed record GeoCoordinate
{
    private GeoCoordinate() { }

    private GeoCoordinate(decimal latitude, decimal longitude)
    {
        Latitude = decimal.Round(latitude, 6, MidpointRounding.AwayFromZero);
        Longitude = decimal.Round(longitude, 6, MidpointRounding.AwayFromZero);
    }

    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    public static GeoCoordinate Create(decimal latitude, decimal longitude)
    {
        if (latitude is < -90m or > 90m)
        {
            throw new CatalogDomainException("catalog.location.latitude.invalid", "Latitude must be between -90 and 90 degrees.");
        }

        if (longitude is < -180m or > 180m)
        {
            throw new CatalogDomainException("catalog.location.longitude.invalid", "Longitude must be between -180 and 180 degrees.");
        }

        return new GeoCoordinate(latitude, longitude);
    }
}
