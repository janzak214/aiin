namespace AIINInterfaces;

public interface IGraphSerializer
{
    void Serialize(Stream stream, List<GraphNode> graph);
    List<GraphNode> Deserialize(Stream stream);

    void SerializePath(Stream stream, List<GraphNode> path);
    
    List<GraphNode> DeserializePath(Stream stream, List<GraphNode> graph);
}
