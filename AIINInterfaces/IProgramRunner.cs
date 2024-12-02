namespace AIINInterfaces;

public interface IProgramRunner
{
    /// <summary>
    /// Runs the genetic algorithm, evolving the population until the maximum number of generations is reached.
    /// </summary>
    /// <returns>A list of <see cref="GraphNode"/> objects representing the best solution found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no fittest individual is found in the population.</exception>
    List<GraphNode> Run();
}