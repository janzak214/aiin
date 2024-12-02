using AIINInterfaces;
using Microsoft.Extensions.Logging;

namespace AIINLib;

public class ProgramRunner : IProgramRunner
{
    private readonly List<GraphNode> _parcelLockerGraph;
    private List<List<GraphNode>> _population;
    private IGeneticOperations _geneticOperations { get; }
    private IGeneticOptimizer _geneticOptimizer { get; }
    private ILogger<ProgramRunner> _logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgramRunner"/> class.
    /// </summary>
    /// <param name="parcelLockerGraph">A list of <see cref="GraphNode"/> objects representing the parcel locker graph to optimize.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create the logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parcelLockerGraph"/> or <paramref name="loggerFactory"/> is <c>null</c>.</exception>
    public ProgramRunner(List<GraphNode> parcelLockerGraph, ILoggerFactory loggerFactory)
    {
        _parcelLockerGraph = parcelLockerGraph ?? throw new ArgumentNullException(nameof(parcelLockerGraph));
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

        _geneticOperations = new GeneticOperations(AppConfig.GeneticAlgorithmSettings.PopulationSize, loggerFactory);
        _population = _geneticOperations.CreateRandomPopulation(_parcelLockerGraph);
        _geneticOptimizer = new GeneticOptimizer(_geneticOperations, loggerFactory, parcelLockerGraph);
        _logger = loggerFactory.CreateLogger<ProgramRunner>();
    }

    public List<GraphNode> Run()
    {
        int generationNumber = 0;
        _logger.LogInformation("----- Starting program -----");

        _logger.LogInformation(@"Program parameters:
                                Mutation Rate: {MutationRate}
                                Crossover Rate: {CrossoverRate}
                                Population Size: {PopulationSize}
                                Tournament Size: {TournamentSize}
                                Max Generations: {MaxGenerations}",
            AppConfig.GeneticAlgorithmSettings.MutationRate,
            AppConfig.GeneticAlgorithmSettings.CrossoverRate,
            AppConfig.GeneticAlgorithmSettings.PopulationSize,
            AppConfig.GeneticAlgorithmSettings.TournamentSize,
            AppConfig.GeneticAlgorithmSettings.MaxGenerations);

        while (generationNumber < AppConfig.GeneticAlgorithmSettings.MaxGenerations)
        {
            _logger.LogInformation("Creating generation number: {GenerationNumber}", generationNumber);

            _population = _geneticOptimizer.Step(population: _population);

            _logger.LogInformation("Generation number {GenerationNumber} has been created.", generationNumber);

            generationNumber++;
        }
        _logger.LogInformation("----- Program execution finished -----");

        var best = _population.MinBy(x => _geneticOptimizer.CalculateFitness(x)) ??
                   throw new InvalidOperationException("No fittest individual found.");
        return best;
    }
}