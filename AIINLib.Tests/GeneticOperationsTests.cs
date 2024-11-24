using AIINInterfaces;
using Geo;
using Microsoft.Extensions.Logging;

namespace AIINLib.Test;

[TestFixture]
public class GeneticOperationsTests
{
    private static GraphNode MakeNode(long id, double latitude, double longitude, bool isParcelLocker = false) =>
        isParcelLocker
            ? new ParcelLockerGraphNode(id, [], new Coordinate(latitude, longitude), 100 + id,
                new Coordinate(latitude, longitude))
            : new GraphNode(id, [], new Coordinate(latitude, longitude));
    
    private GeneticOperations _geneticOperations;
    private ILoggerFactory _LoggerFactory;
    private List<GraphNode> nodes;
    [SetUp]
    public void SetUp()
    {
        _LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        _geneticOperations = new GeneticOperations(5, _LoggerFactory);
    }

    [Test]
    public void CreateRandomPopulation_ShouldGeneratePopulationOfCorrectSize()
    {
        // Arrange
        var nodes = new List<GraphNode>
        {
            MakeNode(1, 0, 0, isParcelLocker: true),
            MakeNode(2, 1, 0),
            MakeNode(3, 2, 0),
            MakeNode(4, 3, 0, isParcelLocker: true)
        };

        // Act
        var population = _geneticOperations.CreateRandomPopulation(nodes);

        // Assert
        Assert.AreEqual(5, population.Count, "Population size should match the specified size.");
        Assert.IsTrue(population.All(individual => individual.Count == nodes.Count),
            "Each individual should contain all nodes in the graph.");
        Assert.IsTrue(population.All(individual => individual.Distinct().Count() == nodes.Count),
            "Each individual should not have duplicate nodes.");
    }

    [Test]
    public void Mutate_ShouldAlterIndividualButRetainNodes()
    {
        // Arrange
        var individual = new List<GraphNode>
        {
            MakeNode(1, 0, 0, isParcelLocker: true),
            MakeNode(2, 1, 0),
            MakeNode(3, 2, 0),
            MakeNode(4, 3, 0, isParcelLocker: true)
        };

        // Act
        var mutatedIndividual = _geneticOperations.Mutate(individual);

        // Assert
        Assert.AreEqual(individual.Count, mutatedIndividual.Count, "Mutated individual should have the same number of nodes.");
        CollectionAssert.AreEquivalent(individual, mutatedIndividual,
            "Mutated individual should contain the same nodes as the original.");
        Assert.AreNotEqual(individual, mutatedIndividual, 
            "Mutated individual should differ from the original.");
    }

    [Test]
    public void Crossover_ShouldCombineParentsCorrectly()
    {
        // Arrange
        var parentA = new List<GraphNode>
        {
            MakeNode(1, 0, 0, isParcelLocker: true),
            MakeNode(2, 1, 0),
            MakeNode(3, 2, 0),
            MakeNode(4, 3, 0, isParcelLocker: true)
        };
        var parentB = new List<GraphNode>
        {
            MakeNode(4, 0, 0, isParcelLocker: true),
            MakeNode(3, 1, 0),
            MakeNode(2, 2, 0),
            MakeNode(1, 3, 0, isParcelLocker: true)
        };

        // Act
        var successor = _geneticOperations.Crossover(parentA, parentB);

        // Assert
        Assert.AreEqual(4, successor.Count, "Successor should include all unique nodes from both parents.");
        CollectionAssert.AreEqual(new List<int> { 1, 2, 4, 3}, successor.Select(node => node.Id),
            "Successor should contain Parent A's first half and Parent B's remaining nodes.");
    }

    [Test]
    public void GrowGraph_ShouldBuildIndividualWithAllNodes()
    {
        // Arrange
        var parcelLockerGraph = new List<GraphNode>
        {
            MakeNode(1, 0, 0, isParcelLocker: true),
            MakeNode(2, 1, 0),
            MakeNode(3, 2, 0),
        };
        var individual = new List<GraphNode>();

        // Act
        var method = typeof(GeneticOperations).GetMethod("GrowGraph", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(_geneticOperations, new object[] { individual, parcelLockerGraph });

        // Assert
        Assert.AreEqual(parcelLockerGraph.Count, individual.Count, "Individual should include all nodes.");
        CollectionAssert.AreEquivalent(parcelLockerGraph, individual,
            "Individual should contain the same nodes as the parcel-locker graph.");
    }
}