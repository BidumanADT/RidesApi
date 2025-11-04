namespace RidesApi.Models;

public record RideCreate(
    string? PickupAddress,
    string? DropoffAddress,
    DateTime PickupTime,
    string? VehicleClass = null,
    string? Notes = null
);