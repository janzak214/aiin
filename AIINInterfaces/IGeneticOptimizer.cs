namespace AIINInterfaces;

public interface IGeneticOptimizer
{
    List<GraphNode> Tournament(List<List<GraphNode>> population);

    int NegativeTournament(List<List<GraphNode>> population);

    double CalculateFitness(List<GraphNode> individual);

    List<List<GraphNode>> Step(List<List<GraphNode>> population);
}