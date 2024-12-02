namespace AIINInterfaces;

public interface IGeneticOptimizer
{
    /// <summary>
    /// Selects the fittest individual from a random subset of the given population.
    /// </summary>
    /// <param name="population">The population to select the fittest individual from.</param>
    /// <param name="fitness">A list of fitness values corresponding to the population.</param>
    /// <returns>The fittest individual selected from subset of the population.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no fittest individual is found.</exception>
    List<GraphNode> Tournament(List<List<GraphNode>> population, List<double> fitness);

    /// <summary>
    /// Selects the worst individual from a random subset of the given population.
    /// </summary>
    /// <param name="population">The population to select the worst individual from.</param>
    /// <param name="fitness">A list of fitness values corresponding to the population.</param>
    /// <returns>The index of the worst individual selected from subset of the population.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no worst individual is found.</exception>
    int NegativeTournament(List<List<GraphNode>> population, List<double> fitness);

    /// <summary>
    /// Calculates the fitness of a given individual.
    /// The fitness is the sum of the distances between the nodes in the individual.
    /// </summary>
    /// <param name="individual">The individual to calculate the fitness for.</param>
    /// <returns>The fitness of the individual.</returns>
    double CalculateFitness(List<GraphNode> individual);

    /// <summary>
    /// Performs a single step of the genetic algorithm, including mutation and crossover operations.
    /// </summary>
    /// <param name="population">The current population of individuals.</param>
    /// <returns>The new population after performing the genetic algorithm step.</returns>
    List<List<GraphNode>> Step(List<List<GraphNode>> population);
}