namespace AIINInterfaces;

public interface IGeneticOptimizer
{
    List<List<GraphNode>> Tournament(List<List<GraphNode>> population);

    List<List<GraphNode>> NegativeTournament(List<List<GraphNode>> population);

    float CalculateFitness(List<GraphNode> individual, Func<GraphNode, GraphNode, float> metric);

    List<List<GraphNode>> Step(List<List<GraphNode>> population);
}