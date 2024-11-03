namespace AIINInterfaces;

public interface IGeneticOperations
{
    /**
     * Tworzy pierwszą losową populację
     */
    List<List<GraphNode>> CreateRandomPopulation(List<GraphNode> parcelLockerGraph);

    List<GraphNode> Mutate(List<GraphNode> individual);

    (List<GraphNode> childA, List<GraphNode> childB)
        Crossover(List<GraphNode> individualA, List<GraphNode> individualB);
}