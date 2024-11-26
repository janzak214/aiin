namespace AIINInterfaces;

public interface IGeneticOptimizer
{
    List<GraphNode> Tournament(List<List<GraphNode>> population, List<double> fitness);

    int NegativeTournament(List<List<GraphNode>> population, List<double> fitness);

    double CalculateFitness(List<GraphNode> individual);

    List<List<GraphNode>> Step(List<List<GraphNode>> population);
}