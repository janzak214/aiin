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
        
        Random random = new();
        int nodeNumberFromWhichMutate = random.Next(1,individual.Count-1);
        _logger.LogInformation("Number: {0}", nodeNumberFromWhichMutate.ToString());
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
        throw new NotImplementedException();
    }
}