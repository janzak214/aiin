namespace AIINInterfaces;

public interface IParcelLockerGraphBuilder
{
    List<ParcelLockerGraphNode> CreateParcelLockerGraph(List<GraphNode> roadGraph);

    /// <summary>
    /// Converts a path in the parcel locker graph to a path in the original road graph.
    /// </summary>
    /// <param name="path">A path in the parcel locker graph.</param>
    /// <param name="roadGraph">The original road graph.</param>
    /// <returns>A path in the original road graph.</returns>
    public List<GraphNode> ExpandPath(List<GraphNode> path, List<GraphNode> roadGraph);
}