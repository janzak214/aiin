using AIINInterfaces;
using Microsoft.Extensions.Logging;

namespace AIINLib;

public class GeneticOperations: IGeneticOperations
{
    private int _populationSize;
    private int _maxIndividualSize;
    private Random _random;
    private readonly ILogger _logger;
    private HashSet<GraphNode> _lockersToVisit;

    public GeneticOperations(int populationSize, int maxIndividualSize, HashSet<GraphNode> lockersToVisit, ILoggerFactory loggerFactory)
    {
        _populationSize = populationSize;
        _maxIndividualSize = maxIndividualSize;
        _random = new Random();
        _logger = loggerFactory.CreateLogger("GeneticOperations");
        _lockersToVisit = lockersToVisit;
    }
    
public List<List<GraphNode>> CreateRandomPopulation(
    List<GraphNode> parcelLockerGraph)
{
    _logger.LogInformation("\n------Creation of random population started------\n");
    
    int individualsVisitedAllLockers = 0;
    int individualsMissedLockers = 0;

    List<List<GraphNode>> population = new();
    

    for (int i = 0; i < _populationSize; i++)
    {
        int startNodeIndex = _random.Next(parcelLockerGraph.Count);
        List<GraphNode> individual = new() { parcelLockerGraph[startNodeIndex] };
        
        individual = growGraph(individual, ref individualsVisitedAllLockers, ref individualsMissedLockers);

        population.Add(individual);
    }

    _logger.LogInformation("\n------Random population creation completed------\n");
    return population;
    }

    private List<GraphNode> growGraph(List<GraphNode> individual, ref int individualsVisitedAllLockers, ref int individualsMissedLockers)
    {
        GraphNode? lastVisitedNode = null;
        
        if (individual.Count() > 1)
        {
            lastVisitedNode = individual[individual.Count - 2];
        }
        
        for (int j = individual.Count(); j < _maxIndividualSize; j++)
        {
            GraphNode currentNode = individual.Last();
            List<GraphNode> connectedNodes = 
                currentNode.ConnectedNodes.Select(edge => edge.Item1).ToList();

            // Remove the last visited node to avoid immediately revisiting
            List<GraphNode> validChoices = connectedNodes
                .Where(node => node != lastVisitedNode)
                .ToList();

            if (validChoices.Count == 0)
            {
                validChoices.Add(lastVisitedNode);
            }

            int nextNodeIndex = _random.Next(validChoices.Count);
            individual.Add(validChoices[nextNodeIndex]);
            
            lastVisitedNode = currentNode;

            // Check if all lockers are visited
            if (_lockersToVisit.IsSubsetOf(individual))
            {
                individualsVisitedAllLockers++;
                _logger.LogInformation(
                    $"Created new individual that visited all lockers. Total: {individualsVisitedAllLockers}");
                break;
            }
        }

        // Log if the individual did not visit all lockers
        if (individual.Count == _maxIndividualSize && !_lockersToVisit.IsSubsetOf(individual))
        {
            individualsMissedLockers++;
            _logger.LogInformation(
                $"Created new individual that did not visit all lockers. Total: {individualsMissedLockers}");
        }

        return individual;
    }

    public List<GraphNode> Mutate(List<GraphNode> individual)
    {
        _logger.LogInformation("\n------Mutating started------\n");
        _logger.LogInformation("Individual before mutation: {0}", string.Join(", ", individual));
        
        int nodeNumberFromWhichMutate = _random.Next(1,individual.Count-1);
        List<GraphNode> individualToMutate = individual.Take(nodeNumberFromWhichMutate).ToList();
        
        int individualsVisitedAllLockers = 0;
        int individualsMissedLockers = 0;
        
        var mutatedIndividual = growGraph(individualToMutate, ref individualsVisitedAllLockers, ref individualsMissedLockers);
        _logger.LogInformation("Individual after mutation: {0}", string.Join(", ", mutatedIndividual));
        
        _logger.LogInformation("\n------Individual mutated------\n");

        return mutatedIndividual;
    }

    public (List<GraphNode> childA, List<GraphNode> childB) Crossover(List<GraphNode> individualA, List<GraphNode> individualB)
    {
        _logger.LogInformation("\n------Crossover started------\n");
        _logger.LogInformation("Parent A: {0}", string.Join(", ", individualA.Select(node => node.Id)));
        _logger.LogInformation("Parent B: {0}", string.Join(", ", individualB.Select(node => node.Id)));

        HashSet<GraphNode> individualASet = new HashSet<GraphNode>(individualA);
        HashSet<GraphNode> individualBSet = new HashSet<GraphNode>(individualB);

        // Znalezienie części wspólnej
        var intersection = individualASet.Intersect(individualBSet).ToList();
        if (intersection.Count == 0)
        {
            throw new InvalidOperationException("No common elements found for crossover.");
        }

        GraphNode crossoverPoint = intersection[_random.Next(intersection.Count)];
        _logger.LogInformation("Crossover point: {0}", crossoverPoint.Id);

        int indexA = individualA.IndexOf(crossoverPoint);
        int indexB = individualB.IndexOf(crossoverPoint);

        List<GraphNode> childA = new List<GraphNode>();
        List<GraphNode> childB = new List<GraphNode>();

        childA.AddRange(individualA.Take(indexA + 1));
        childA.AddRange(individualB.Skip(indexB + 1));

        childB.AddRange(individualB.Take(indexB + 1));
        childB.AddRange(individualA.Skip(indexA + 1));

        _logger.LogInformation("Child A: {0}", string.Join(", ", childA.Select(node => node.Id)));
        _logger.LogInformation("Child B: {0}", string.Join(", ", childB.Select(node => node.Id)));
        _logger.LogInformation("\n------Crossover completed------\n");

        return (childA, childB);
    }

}