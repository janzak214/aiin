namespace AIINInterfaces;

public record GraphNode(List<(Node, float)> ConnectedNodes)
{
}

public record ParcelLockerGraphNode(List<(Node, float)> ConnectedNodes) : GraphNode(ConnectedNodes)
{
}