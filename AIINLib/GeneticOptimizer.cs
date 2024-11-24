using AIINInterfaces;

namespace AIINLib;

public class GeneticOptimizer : IGeneticOptimizer
{
    private readonly Func<GraphNode, GraphNode, double> metric;
    private readonly Random random = new();

    public double CalculateFitness(List<GraphNode> individual)
    {
        GraphNode previousNode = individual[0];
        double fitness = 0;
        for (int i = 1; i < individual.Count; i++)
        {
            fitness += metric(previousNode, individual[i]);
            previousNode = individual[i];
        }

        return fitness;
    }

    public List<GraphNode> Tournament(List<List<GraphNode>> population)
    {
        List<GraphNode>? fittestIndividual = null;
        double fittestFitness = double.MaxValue;
        for (int i = 0; i < AppSettings.TournamentSize; i++)
        {
            var individual = population[random.Next(0, population.Count)];
            var fitness = CalculateFitness(individual);

            if (fitness < fittestFitness)
            {
                fittestIndividual = individual;
                fittestFitness = fitness;
            }
        }

        return fittestIndividual ?? throw new InvalidOperationException("No fittest individual found.");
    }

    public int NegativeTournament(List<List<GraphNode>> population)
    {
        int? worstIndividualIndex = null;
        double worstFitness = double.MinValue;
        for (int i = 0; i < AppSettings.TournamentSize; i++)
        {
            var individualIndex = random.Next(0, population.Count);
            var fitness = CalculateFitness(population[individualIndex]);

            if (fitness > worstFitness)
            {
                worstIndividualIndex = individualIndex;
                worstFitness = fitness;
            }
        }

        return worstIndividualIndex ?? throw new InvalidOperationException("No worst individual found.");
    }

    public List<List<GraphNode>> Step(List<List<GraphNode>> population)
    {
        throw new NotImplementedException();
    }
}