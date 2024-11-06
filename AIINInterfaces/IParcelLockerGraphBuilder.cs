namespace AIINInterfaces;

public interface IParcelLockerGraphBuilder
{
    /**
     * Tworzy graf połączeń pomiędzy paczkomatami
     */
    List<ParcelLockerGraphNode> CreateParcelLockerGraph(List<GraphNode> roadGraph);
}