namespace AIINInterfaces;

public interface IParcelLockerGraphBuilder
{
    /**
     * Tworzy graf połączeń pomiędzy paczkomatami
     */
    List<ParcelLockerGraphNode> CreateParcelLockerGraph(List<GraphNode> roadGraph);

    public List<GraphNode> ExpandPath(List<GraphNode> path, List<GraphNode> roadGraph);
}