namespace AIINInterfaces;

public interface IGeneticOptimizer
{
    List<GraphNode> Tournament(List<List<GraphNode>> population, List<double> fitness);

    int NegativeTournament(List<List<GraphNode>> population, List<double> fitness);

    double CalculateFitness(List<GraphNode> individual);

    /// <summary>
    /// Performs a single step of the genetic algorithm, including mutation and crossover operations.
    /// </summary>
    /// <param name="population">The current population of individuals.</param>
    /// <returns>The new population after performing the genetic algorithm step.</returns>
    List<List<GraphNode>> Step(List<List<GraphNode>> population);
}