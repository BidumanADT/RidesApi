namespace RidesApi.Models;

public record RideListItem(
    string Id,
    DateTime PickupTime,
    string PickupAddress,
    string DropoffAddress,
    string Status,
    decimal Price
);
