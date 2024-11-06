namespace AIINInterfaces;

public interface IRoadGraphBuilder
{
    /**
     * Tworzy graf połączeń pomiędzy węzłami dróg
     */
    List<GraphNode> CreateRoadGraph(List<Road> roads, List<Node> nodes, List<Node> parcelLockers);


    /**
     * Optymalizuje graf połączeń pomiędzy węzłami dróg
     */
    List<GraphNode> OptimizeRoadGraph(List<GraphNode> roadGraph);
}