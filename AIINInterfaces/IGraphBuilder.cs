namespace AIINInterfaces;

public interface IGraphBuilder
{
    /**
     * Tworzy graf połączeń pomiędzy węzłami dróg
     */
    List<GraphNode> CreateRoadGraph(List<Road> roads, List<Node> nodes);

    /**
     * Tworzy graf połączeń pomiędzy paczkomatami
     */
    List<GraphNode> CreateParcelLockerGraph(List<GraphNode> roadGraph, List<Node> parcelLockers);
}