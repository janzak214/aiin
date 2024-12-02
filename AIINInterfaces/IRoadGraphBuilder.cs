namespace AIINInterfaces;

public interface IRoadGraphBuilder
{
    /// <summary>
    /// Creates a graph of roads based on the OpenStreetMap data.
    /// Each parcel locker is paired with the closest road node.
    /// </summary>
    /// <param name="roads">A list of roads extracted from the OSM data.</param>
    /// <param name="nodes">A list of road nodes extracted from the OSM data.</param>
    /// <param name="parcelLockers">A list of parcel locker nodes extracted from the OSM data.</param>
    /// <returns>A list of nodes of the created road graph.</returns>
    List<GraphNode> CreateRoadGraph(List<Road> roads, List<Node> nodes, List<Node> parcelLockers);


    /// <summary>
    /// Optimizes a road graph for pathfinding.
    /// Removes superfluous nodes between intersections, prunes dead ends. 
    /// </summary>
    /// <param name="roadGraph">The road graph to optimize.</param>
    /// <returns>The optimized road graph.</returns>
    List<GraphNode> OptimizeRoadGraph(List<GraphNode> roadGraph);
}