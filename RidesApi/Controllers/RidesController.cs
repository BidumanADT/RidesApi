using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RidesApi.Models;
using RidesApi.Storage;

namespace RidesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // all endpoints require JWT
public class RidesController(IRidesStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<RideListItem>> GetAll() => Ok(store.List());

    [HttpGet("{id}")]
    public ActionResult<RideListItem> GetById(string id)
        => store.Get(id) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public ActionResult<RideListItem> Create([FromBody] RideCreate req)
    {
        var id = $"R-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var pickup = req.PickupTime == default ? DateTime.Now : req.PickupTime;

        var item = new RideListItem(
            Id: id,
            PickupTime: pickup,
            PickupAddress: req.PickupAddress ?? "Unknown pickup",
            DropoffAddress: req.DropoffAddress ?? "Unknown dropoff",
            Status: "Created",
            Price: 0m
        );

        store.Create(item);
        return CreatedAtAction(nameof(GetById), new { id }, item);
    }

    // Optional: confirm/decline/cancel
    [HttpPatch("{id}/status")]
    public ActionResult<RideListItem> UpdateStatus(string id, [FromBody] RideStatusUpdate body)
    {
        var existing = store.Get(id);
        if (existing is null) return NotFound();

        var updated = existing with { Status = body.Status };
        store.Upsert(updated);
        return Ok(updated);
    }
}

public record RideStatusUpdate(string Status);
