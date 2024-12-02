namespace AIINInterfaces;

public interface IGraphSerializer
{
    /// <summary>
    /// Serializes the given graph to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to which the graph will be serialized.</param>
    /// <param name="graph">A list of graph nodes to serialize.</param>
    void Serialize(Stream stream, List<GraphNode> graph);

    /// <summary>
    /// Deserializes a graph from the specified stream.
    /// </summary>
    /// <param name="stream">The stream from which the graph will be deserialized.</param>
    /// <returns>A list of nodes of the deserialized graph.</returns>
    List<GraphNode> Deserialize(Stream stream);

    /// <summary>
    /// Serializes the given path to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to which the path will be serialized.</param>
    /// <param name="path">The path to serialize.</param>
    void SerializePath(Stream stream, List<GraphNode> path);

    /// <summary>
    /// Deserializes a path from the specified stream, using the provided graph.
    /// </summary>
    /// <param name="stream">The stream from which the path will be deserialized.</param>
    /// <param name="graph">The graph to use for deserialization.</param>
    /// <returns>The deserialized path.</returns>
    List<GraphNode> DeserializePath(Stream stream, List<GraphNode> graph);
}