namespace AIINInterfaces;

public interface IGeneticOperations
{
    /// <summary>
    /// Generates the initial random population for the genetic algorithm.
    /// </summary>
    /// <param name="parcelLockerGraph">
    /// A list representing all parcel lockers in the graph.
    /// </param>
    /// <returns>
    /// A random population, where each element is a list representing the order of visiting the parcel lockers.
    /// </returns>

    List<List<GraphNode>> CreateRandomPopulation(List<GraphNode> parcelLockerGraph);

    List<GraphNode> Mutate(List<GraphNode> individual);

    List<GraphNode> Crossover(List<GraphNode> individualA, List<GraphNode> individualB);
}