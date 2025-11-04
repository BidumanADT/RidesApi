using System.Collections.Concurrent;
using RidesApi.Models;

namespace RidesApi.Storage;

public sealed class InMemoryRidesStore : IRidesStore
{
    private readonly ConcurrentDictionary<string, RideListItem> _items = new();

    public int Count => _items.Count;

    public IEnumerable<RideListItem> List() => _items.Values.OrderByDescending(x => x.PickupTime);

    public RideListItem? Get(string id) => _items.TryGetValue(id, out var v) ? v : null;

    public RideListItem Create(RideListItem item)
    {
        _items[item.Id] = item;
        return item;
    }

    public bool Upsert(RideListItem item)
    {
        _items[item.Id] = item;
        return true;
    }
}
