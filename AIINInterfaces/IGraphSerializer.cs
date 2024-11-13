namespace AIINInterfaces;

public interface IGraphSerializer
{
    void Serialize(Stream stream, List<GraphNode> graph);
    List<GraphNode> Deserialize(Stream stream);
}
