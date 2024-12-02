namespace AIINInterfaces;

public interface IParcelLockerGraphBuilder
{
    /// <summary>
    /// Creates a new graph consisting only of the parcel lockers and the distances between them.
    /// Calculates the shortest distances between all nodes in the graph using Dijkstra's algorithm.
    /// </summary>
    /// <param name="roadGraph">The original road graph.</param>
    /// <returns>A new graph consisting of the parcel lockers and the distances between them.</returns>
    List<ParcelLockerGraphNode> CreateParcelLockerGraph(List<GraphNode> roadGraph);

    /// <summary>
    /// Converts a path in the parcel locker graph to a path in the original road graph.
    /// </summary>
    /// <param name="path">A path in the parcel locker graph.</param>
    /// <param name="roadGraph">The original road graph.</param>
    /// <returns>A path in the original road graph.</returns>
    public List<GraphNode> ExpandPath(List<GraphNode> path, List<GraphNode> roadGraph);
}