using AIINInterfaces;
using Microsoft.Extensions.Logging;

namespace AIINLib;

public class GeneticOperations: IGeneticOperations
{
    private int _populationSize;
    private Random _random;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneticOperations"/> class.
    /// </summary>
    /// <param name="populationSize">The size of the population to be created and operated on by the genetic algorithm.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create a logger for the <see cref="GeneticOperations"/> class.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/> is <c>null</c>.</exception>
    public GeneticOperations(int populationSize, ILoggerFactory loggerFactory)
    {
        _populationSize = populationSize;
        _random = new Random();
        _logger = loggerFactory.CreateLogger<GeneticOperations>();
    }

    public List<List<GraphNode>> CreateRandomPopulation(
        List<GraphNode> parcelLockerGraph)
    {
        _logger.LogDebug("\n------Creation of random population started------\n");

        List<List<GraphNode>> population = new();
        

        for (int i = 0; i < _populationSize; i++)
        {
            List<GraphNode> individual = new();
            
            GrowGraph(individual, parcelLockerGraph);

            population.Add(individual);
            _logger.LogDebug("New individual added to population: {0}", string.Join(", ", individual.Select(node => node.Id)));
        }

        _logger.LogDebug("\n------Random population creation completed------\n");
        
        return population;
    }

    private void GrowGraph(List<GraphNode> individual, List<GraphNode> parcelLockerGraph)
    {
        if (individual == null)
        {
            throw new ArgumentNullException(nameof(individual));
        }
        
        var nodesToVisit =
            parcelLockerGraph.Where(node => !individual.Select(individualNode => individualNode.Id).Contains(node.Id)).ToList();
        
        while (nodesToVisit.Count() != 0)
        {
            var randomNodeIndex = _random.Next(nodesToVisit.Count);
            individual.Add(nodesToVisit[randomNodeIndex]);
            nodesToVisit.RemoveAt(randomNodeIndex);
        }
    }

    public List<GraphNode> Mutate(List<GraphNode> individual)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("\n------Mutating started------\n");
            _logger.LogDebug("Individual before mutation: {0}", string.Join(", ", individual.Select(node => node.Id)));
        }

        var mutationCandidat = individual.Select(node => node).ToList();
        
        int firstRandomNodeIndex = _random.Next(mutationCandidat.Count());
        int secondRandomNodeIndex;
        
        do
        {
            secondRandomNodeIndex = _random.Next(mutationCandidat.Count());
        } 
        while(secondRandomNodeIndex == firstRandomNodeIndex);
        
        mutationCandidat.ExtensionReverse(firstRandomNodeIndex, secondRandomNodeIndex);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Individual after mutation: {0}",
                string.Join(", ", mutationCandidat.Select(node => node.Id)));
            _logger.LogDebug("\n------Individual mutated------\n");
        }

        return mutationCandidat;
    }

    public List<GraphNode> Crossover(List<GraphNode> individualA, List<GraphNode> individualB)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("\n------ Crossover started ------\n");
            _logger.LogDebug("Parent A: {0}", string.Join(", ", individualA.Select(node => node.Id)));
            _logger.LogDebug("Parent B: {0}", string.Join(", ", individualB.Select(node => node.Id)));
        }

        int middleIndex = individualA.Count / 2;

        List<GraphNode> successor = individualA.GetRange(0, middleIndex);
        HashSet<long> successorIds = successor.Select(x => x.Id).ToHashSet();
        
        foreach (var node in individualB)
        {
            if (!successorIds.Contains(node.Id))
            {
                successorIds.Add(node.Id);
                successor.Add(node);
            }
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Successor: {0}", string.Join(", ", successor.Select(node => node.Id)));
            _logger.LogDebug("\n------ Crossover finished ------\n");
        }

        return successor;
    }
}