using Geo;

namespace AIINInterfaces;

/// <summary>
/// Represents a node in the road graph.
/// </summary>
/// <param name="Id">OpenStreetMap ID of the original road node.</param>
/// <param name="ConnectedNodes">A list of connected nodes and connections weights.</param>
/// <param name="Position">Geographic coordinates of the node.</param>
public record GraphNode(long Id, List<(GraphNode node, double weight)> ConnectedNodes, Coordinate Position)
{
    public override string ToString()
    {
        var connections = string.Join(", ", ConnectedNodes.Select(x => $"({x.node.Id}, {x.weight}m)"));
        return $"node({Id}, [{connections}])";
    }
}

/// <summary>
/// Represents a parcel locker node in the road graph.
/// </summary>
/// <param name="Id">OpenStreetMap ID of the original road node.</param>
/// <param name="ConnectedNodes">A list of connected nodes and connections weights.</param>
/// <param name="Position">Geographic coordinates of the node.</param>
/// <param name="ParcelLockerId">OpenStreetMap ID of the original parcel locker node.</param>
/// <param name="ParcelLockerPosition">Geographic coordinates of the original parcel locker node.</param>
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