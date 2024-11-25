using AIINInterfaces;
using Microsoft.Extensions.Logging;

namespace AIINLib;

public class GeneticOptimizer(
    IGeneticOperations geneticOperations,
    ILoggerFactory loggerFactory,
    Func<GraphNode, GraphNode, double> metric
)
    : IGeneticOptimizer
{
    private readonly Random _random = new();
    private readonly ILogger<GeneticOptimizer> _logger = loggerFactory.CreateLogger<GeneticOptimizer>();

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
        for (int i = 0; i < AppConfig.GeneticAlgorithmSettings.TournamentSize; i++)
        {
            var individual = population[_random.Next(0, population.Count)];
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
        for (int i = 0; i < AppConfig.GeneticAlgorithmSettings.TournamentSize; i++)
        {
            var individualIndex = _random.Next(0, population.Count);
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
        var fitness = population.Select(CalculateFitness).ToList();
        _logger.LogInformation("Average fitness: {AverageFitness}, Best fitness: {MinFitness}",
            fitness.Average(),
            fitness.Min()
        );

        var populationSize = population.Count;
        List<List<GraphNode>> result = [..population];
        var mutationCount = (int)Math.Floor(AppConfig.GeneticAlgorithmSettings.MutationRate * populationSize);
        var crossoverCount = (int)Math.Floor(AppConfig.GeneticAlgorithmSettings.CrossoverRate * populationSize);

        for (var i = 0; i < mutationCount; i++)
        {
            var parent = Tournament(population);
            var mutated = geneticOperations.Mutate(parent);
            var indexToReplace = NegativeTournament(population);
            result[indexToReplace] = mutated;
        }

        for (var i = 0; i < crossoverCount; i++)
        {
            var parentA = Tournament(population);
            var parentB = Tournament(population);
            var offspring = geneticOperations.Crossover(parentA, parentB);
            var indexToReplace = NegativeTournament(population);
            result[indexToReplace] = offspring;
        }

        return result;
    }

    public static double DefaultMetric(GraphNode nodeA, GraphNode nodeB) =>
        nodeA.ConnectedNodes.Find(x => x.node.Id == nodeB.Id).weight;
}