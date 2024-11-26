using System;
using System.Collections.Generic;
using System.Linq;
using AIINInterfaces;
using BenchmarkDotNet.Attributes;
using Geo;
using Microsoft.Extensions.Logging;

namespace AIINLib.Benchmarks
{
    [PlainExporter]
    [HtmlExporter]
    public class Benchmarks
    {
        private GeneticOptimizer _geneticOptimizer;
        private GeneticOperations _geneticOperations;
        private List<GraphNode> _graph;
        private List<List<GraphNode>> _population;

        [GlobalSetup]
        public void Setup()
        {
            _geneticOperations = new GeneticOperations(AppConfig.GeneticAlgorithmSettings.PopulationSize,
                LoggerFactory.Create(_ => { }));
            _graph = CreateRandomGraph(300);
            _geneticOptimizer = new GeneticOptimizer(_geneticOperations, LoggerFactory.Create(
                _ => { }), _graph);
        }

        static List<GraphNode> CreateRandomGraph(int size)
        {
            var nodes = Enumerable.Range(0, size)
                .Select(GraphNode (i) =>
                    new ParcelLockerGraphNode(
                        Id: i,
                        ConnectedNodes: [],
                        Position: new Coordinate(),
                        ParcelLockerId: i,
                        ParcelLockerPosition: new Coordinate()
                    )
                ).ToList();

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    var distance = Random.Shared.NextDouble() * 1000;
                    nodes[i].ConnectedNodes.Add((nodes[j], distance));
                    nodes[j].ConnectedNodes.Add((nodes[i], distance));
                }
            }

            return nodes;
        }

        [IterationSetup]
        public void MutationSetup() => _population = _geneticOperations.CreateRandomPopulation(_graph);

        [Benchmark]
        public List<GraphNode> Mutation() => _geneticOperations.Mutate(_population[0]);

        [Benchmark]
        public List<GraphNode> Crossover() => _geneticOperations.Crossover(_population[0], _population[1]);

        [Benchmark]
        public List<List<GraphNode>> Step() => _geneticOptimizer.Step(_population);

        [Benchmark]
        public double Fitness() =>
            _population
                .Select(x => _geneticOptimizer.CalculateFitness(x))
                .Average();
    }
}