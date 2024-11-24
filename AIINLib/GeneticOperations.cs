﻿using AIINInterfaces;
using Microsoft.Extensions.Logging;

namespace AIINLib;

public class GeneticOperations: IGeneticOperations
{
    private int _populationSize;
    private Random _random;
    private readonly ILogger _logger;

    public GeneticOperations(int populationSize, ILoggerFactory loggerFactory)
    {
        _populationSize = populationSize;
        _random = new Random();
        _logger = loggerFactory.CreateLogger<GeneticOperations>();
    }
    
    /// <summary>
    /// Creates random permutations of the parcel-locker graph.
    /// </summary>
    /// <param name="parcelLockerGraph">All parcel-locker nodes in the graph.</param>
    /// <returns>
    /// Random permutations of parcelLockerGraph argument.
    /// </returns>
    public List<List<GraphNode>> CreateRandomPopulation(
        List<GraphNode> parcelLockerGraph)
    {
        _logger.LogInformation("\n------Creation of random population started------\n");

        List<List<GraphNode>> population = new();
        

        for (int i = 0; i < _populationSize; i++)
        {
            List<GraphNode> individual = new();
            
            GrowGraph(individual, parcelLockerGraph);

            population.Add(individual);
            _logger.LogInformation("New individual added to population: {0}", string.Join(", ", individual.Select(node => node.Id)));
        }

        _logger.LogInformation("\n------Random population creation completed------\n");
        
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


    /// <summary>
    /// Utilizes two-opt algorithm as mutation method. Mutates the individual.
    /// </summary>
    /// <param name="individual">Order of parcel lockers to visit</param>
    /// <returns>
    /// Mutated order of parcel lockers to visit.
    /// </returns>
    public List<GraphNode> Mutate(List<GraphNode> individual)
    {
        if (individual == null)
        {
            throw new ArgumentNullException(nameof(individual));
        }

        if (individual.Count <=1)
        {
            throw new ArgumentException("Argument count must be higher than 1.");
        }
        
        _logger.LogInformation("\n------Mutating started------\n");
        _logger.LogInformation("Individual before mutation: {0}", string.Join(", ", individual.Select(node=>node.Id)));
        
        
        var mutationCandidat = individual.Select(node => node).ToList();
        
        int firstRandomNodeIndex = _random.Next(mutationCandidat.Count());
        int secondRandomNodeIndex;
        
        do
        {
            secondRandomNodeIndex = _random.Next(mutationCandidat.Count());
        } 
        while(secondRandomNodeIndex == firstRandomNodeIndex);
        
        mutationCandidat.ExtensionReverse(firstRandomNodeIndex, secondRandomNodeIndex);
        
        
        _logger.LogInformation("Individual after mutation: {0}", string.Join(", ", mutationCandidat.Select(node=>node.Id)));
        _logger.LogInformation("\n------Individual mutated------\n");
        
        return mutationCandidat;
    }

    /// <summary>
    /// Performs a crossover between two individuals by taking the first half of the nodes
    /// from the first parent and appending the remaining nodes from the second parent
    /// in the order they appear, skipping duplicates.
    /// </summary>
    /// <param name="individualA">The first parent.</param>
    /// <param name="individualB">The second parent.</param>
    /// <returns>A new individual (list of GraphNodes) created through crossover.</returns>
    public List<GraphNode> Crossover(List<GraphNode> individualA, List<GraphNode> individualB)
    {
        if (individualA == null || individualB == null)
        {
            throw new ArgumentNullException();
        }
        
        _logger.LogInformation("\n------ Crossover started ------\n");
        _logger.LogInformation("Parent A: {0}", string.Join(", ", individualA.Select(node => node.Id)));
        _logger.LogInformation("Parent B: {0}", string.Join(", ", individualB.Select(node => node.Id)));

        int middleIndex = individualA.Count / 2;

        List<GraphNode> successor = individualA.GetRange(0, middleIndex);

        foreach (var node in individualB)
        {
            if (!successor.Select(x=> x.Id).Contains(node.Id))
            {
                successor.Add(node);
            }
        }

        _logger.LogInformation("Successor: {0}", string.Join(", ", successor.Select(node => node.Id)));
        _logger.LogInformation("\n------ Crossover finished ------\n");

        return successor;
    }

}