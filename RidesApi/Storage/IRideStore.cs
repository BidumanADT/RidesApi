using RidesApi.Models;

namespace RidesApi.Storage;

public interface IRidesStore
{
    int Count { get; }
    IEnumerable<RideListItem> List();
    RideListItem? Get(string id);
    RideListItem Create(RideListItem item);
    bool Upsert(RideListItem item);
}
