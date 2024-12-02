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

    /// <summary>
    /// Utilizes two-opt algorithm as mutation method. Mutates the individual.
    /// </summary>
    /// <param name="individual">Order of parcel lockers to visit</param>
    /// <returns>
    /// Mutated order of parcel lockers to visit.
    /// </returns>
    List<GraphNode> Mutate(List<GraphNode> individual);


    /// <summary>
    /// Performs a crossover between two individuals by taking the first half of the nodes
    /// from the first parent and appending the remaining nodes from the second parent
    /// in the order they appear, skipping duplicates.
    /// </summary>
    /// <param name="individualA">The first parent.</param>
    /// <param name="individualB">The second parent.</param>
    /// <returns>A new individual (list of GraphNodes) created through crossover.</returns>
    List<GraphNode> Crossover(List<GraphNode> individualA, List<GraphNode> individualB);
}