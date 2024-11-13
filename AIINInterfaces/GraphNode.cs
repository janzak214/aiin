using Geo;

namespace AIINInterfaces;

public record GraphNode(long Id, List<(GraphNode node, double weight)> ConnectedNodes, Coordinate Position)
{
    public override string ToString()
    {
        var connections = string.Join(", ", ConnectedNodes.Select(x => $"({x.node.Id}, {x.weight}m)"));
        return $"node({Id}, [{connections}])";
    }
}

public record ParcelLockerGraphNode(
    long Id,
    List<(GraphNode node, double weight)> ConnectedNodes,
    Coordinate Position,
    long ParcelLockerId,
    Coordinate ParcelLockerPosition
)
    : GraphNode(Id, ConnectedNodes, Position)
{
    public override string ToString()
    {
        var connections = string.Join(", ", ConnectedNodes.Select(x => $"({x.node.Id}, {x.weight}m)"));
        return $"node(pl:{ParcelLockerId}, {Id}, [{connections}])";
    }
}