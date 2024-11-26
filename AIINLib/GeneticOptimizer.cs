using AIINInterfaces;
using Microsoft.Extensions.Logging;

namespace AIINLib;

public class GeneticOptimizer : IGeneticOptimizer
{
    private readonly Random _random = new();
    private readonly ILogger<GeneticOptimizer> _logger;
    private readonly Dictionary<long, Dictionary<long, double>> _connections;
    private readonly IGeneticOperations _geneticOperations;

    public GeneticOptimizer(
        IGeneticOperations geneticOperations,
        ILoggerFactory loggerFactory,
        List<GraphNode> graph
    )
    {
        _geneticOperations = geneticOperations;
        _logger = loggerFactory.CreateLogger<GeneticOptimizer>();
        _connections = graph
            .Select(node => (node.Id, node.ConnectedNodes.Select(x => (x.node.Id, x.weight)).ToDictionary()))
            .ToDictionary();
    }

    public double CalculateFitness(List<GraphNode> individual)
    {
        GraphNode previousNode = individual[0];
        double fitness = 0;
        for (int i = 1; i < individual.Count; i++)
        {
            fitness += Metric(previousNode, individual[i]);
            previousNode = individual[i];
        }

        return fitness;
    }

    public List<GraphNode> Tournament(List<List<GraphNode>> population, List<double> fitness)
    {
        List<GraphNode>? fittestIndividual = null;
        double fittestFitness = double.MaxValue;
        for (int i = 0; i < AppConfig.GeneticAlgorithmSettings.TournamentSize; i++)
        {
            var individualIndex = _random.Next(0, population.Count);
            var currentFitness = fitness[individualIndex];

            if (currentFitness < fittestFitness)
            {
                fittestIndividual = population[individualIndex];
                fittestFitness = currentFitness;
            }
        }

        return fittestIndividual ?? throw new InvalidOperationException("No fittest individual found.");
    }

    public int NegativeTournament(List<List<GraphNode>> population, List<double> fitness)
    {
        int? worstIndividualIndex = null;
        double worstFitness = double.MinValue;
        for (int i = 0; i < AppConfig.GeneticAlgorithmSettings.TournamentSize; i++)
        {
            var individualIndex = _random.Next(0, population.Count);
            var currentFitness = fitness[individualIndex];

            if (currentFitness > worstFitness)
            {
                worstIndividualIndex = individualIndex;
                worstFitness = currentFitness;
            }
        }

        return worstIndividualIndex ?? throw new InvalidOperationException("No worst individual found.");
    }

    public List<List<GraphNode>> Step(List<List<GraphNode>> population)
    {
        var fitness = population.AsParallel().Select(CalculateFitness).ToList();
        _logger.LogInformation("Average fitness: {AverageFitness}, Best fitness: {MinFitness}",
            fitness.Average(),
            fitness.Min()
        );

        var populationSize = population.Count;
        var mutationCount = (int)Math.Floor(AppConfig.GeneticAlgorithmSettings.MutationRate * populationSize);
        var crossoverCount = (int)Math.Floor(AppConfig.GeneticAlgorithmSettings.CrossoverRate * populationSize);
        var order = population.Select(_ => 2).ToList();

        for (var i = 0; i < mutationCount + crossoverCount; i++)
        {
            var indexToReplace = NegativeTournament(population, fitness);
            order[indexToReplace] = i < mutationCount ? 0 : 1;
        }

        var sorted =
            population.Select((individual, index) => (individual, index))
                .OrderBy(x => order[x.index])
                .Select(x => x.individual)
                .ToList();

        return sorted.AsParallel().Select((individual, index) =>
        {
            if (index < mutationCount)
            {
                var parent = Tournament(population, fitness);
                return _geneticOperations.Mutate(parent);
            }
            else if (index < mutationCount + crossoverCount)
            {
                var parentA = Tournament(population, fitness);
                var parentB = Tournament(population, fitness);
                return _geneticOperations.Crossover(parentA, parentB);
            }
            else
            {
                return individual;
            }
        }).ToList();
    }

    private double Metric(GraphNode nodeA, GraphNode nodeB) => _connections[nodeA.Id][nodeB.Id];
}