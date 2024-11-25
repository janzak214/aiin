using AIINInterfaces;
using Microsoft.Extensions.Logging;

namespace AIINLib;

public class ProgramRunner: IProgramRunner
{
    private readonly List<GraphNode> _parcelLockerGraph;
    private List<List<GraphNode>> _population;
    private IGeneticOperations _geneticOperations { get; }
    private IGeneticOptimizer _geneticOptimizer { get; }
    private ILogger<ProgramRunner> _logger { get; }

    public ProgramRunner(List<GraphNode> parcelLockerGraph, ILoggerFactory loggerFactory)
    {
        _parcelLockerGraph = parcelLockerGraph ?? throw new ArgumentNullException(nameof(parcelLockerGraph));
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

        _geneticOperations = new GeneticOperations(AppConfig.GeneticAlgorithmSettings.PopulationSize, loggerFactory);
        _population = _geneticOperations.CreateRandomPopulation(_parcelLockerGraph);
        _geneticOptimizer = new GeneticOptimizer(_geneticOperations, loggerFactory, GeneticOptimizer.DefaultMetric);
        _logger = loggerFactory.CreateLogger<ProgramRunner>();
    }
    public void Run()
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
    }
}