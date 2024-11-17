using AIINInterfaces;
using Geo;
using Microsoft.Extensions.Logging;

namespace AIINLib.Test;

[TestFixture]
public class GeneticOperationsTests
{
    private void ConnectNodes(GraphNode node1, GraphNode node2, double distance)
    {
        node1.ConnectedNodes.Add((node2, distance));
        node2.ConnectedNodes.Add((node1, distance));
    }

    private readonly RoadGraphBuilder _builder = new RoadGraphBuilder();

    private static GraphNode MakeNode(long id, double latitude, double longitude, bool isParcelLocker = false) =>
        isParcelLocker
            ? new ParcelLockerGraphNode(id, [], new Coordinate(latitude, longitude), 100 + id,
                new Coordinate(latitude, longitude))
            : new GraphNode(id, [], new Coordinate(latitude, longitude));
    
    

    private ILoggerFactory loggerFactory;
    private GeneticOperations geneticOperations;
    private List<GraphNode> parcelLockerGraph;
    private HashSet<GraphNode> lockersToVisite;

    private GraphNode node1;
    private GraphNode node2;
    private GraphNode node3;
    private GraphNode node4;
    private GraphNode node5;

    [SetUp]
    public void SetUp()
    {
        loggerFactory = LoggerFactory.Create(build => build.AddConsole());
        
        
        node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        node2 = MakeNode(2, 1, 0);
        node3 = MakeNode(3, 2, 0);
        node4 = MakeNode(4, 3, 0, isParcelLocker: true);
        node5 = MakeNode(5, 4, 0);
        ConnectNodes(node1, node2, 1);
        ConnectNodes(node2, node3, 1);
        ConnectNodes(node3, node4, 1);
        ConnectNodes(node4, node5, 1);
        
        parcelLockerGraph = new List<GraphNode>([node1, node2, node3, node4]);
        lockersToVisite = new HashSet<GraphNode>([node1, node2, node3]);
    }

    [Test]
    public void CreateRandomPopulation_PopulationSize3_True()
    {
        geneticOperations = new GeneticOperations(3, 3, lockersToVisite, loggerFactory);
        var pupulation = geneticOperations.CreateRandomPopulation(parcelLockerGraph);
        
        Assert.That(pupulation.Count, Is.EqualTo(3));
        foreach (List<GraphNode> individual in pupulation)
        {
            Assert.That(individual, Is.Not.Null);
            Assert.That(individual.Count, Is.LessThanOrEqualTo(3));
        }
    }
    
    [Test]
    public void CreateRandomPopulation_populationSize3IndividualsSize3_True()
    {
        geneticOperations = new GeneticOperations(3, 3,lockersToVisite, loggerFactory);
        var population = geneticOperations.CreateRandomPopulation(parcelLockerGraph);
        
        foreach (List<GraphNode> individual in population)
        {
            Assert.That(individual, Is.Not.Null);
            Assert.That(individual.Count, Is.LessThanOrEqualTo(3));
        }
    }
    
    [Test]
    public void CreateRandomPopulation_PopulationSize4IndividualSize4_NoIndividualsThatComeBackToLatestNodeIfNotInAtTheEndOfTheRode()
    {
        geneticOperations = new GeneticOperations(4, 4,lockersToVisite, loggerFactory);
        var pupulation = geneticOperations.CreateRandomPopulation(parcelLockerGraph);
        
        foreach (List<GraphNode> individual in pupulation)
        {
            Assert.That(individual, Has.Member(node3));
        }
    }
    
    
    [Test]
    public void Mutate_PopulationSize4IndividualSize4_NoExceptions()
    {
        ConnectNodes(node2, node3, 3);
        geneticOperations = new GeneticOperations(1, 5, lockersToVisite, loggerFactory);
        var population = geneticOperations.CreateRandomPopulation(parcelLockerGraph);
        var individual = population[0];
        
        var mutatedIndividual = geneticOperations.Mutate(individual);
        Assert.Pass();
    }
}