using AIINInterfaces;

namespace AIINLib;

public class ParcelLockerGraphBuilder : IParcelLockerGraphBuilder
{
    public List<ParcelLockerGraphNode> CreateParcelLockerGraph(List<GraphNode> roadGraph)
    {
        return roadGraph
            .FindAll(x => x is ParcelLockerGraphNode)
            .Cast<ParcelLockerGraphNode>()
            .Select(x =>
            {
                return new ParcelLockerGraphNode(
                    Id: x.Id,
                    ConnectedNodes: CalculateDistances(roadGraph, x).FindAll(x => x.node is ParcelLockerGraphNode),
                    Position: x.Position,
                    ParcelLockerId: x.ParcelLockerId,
                    ParcelLockerPosition: x.ParcelLockerPosition
                );
            })
            .ToList();
    }

    private static List<(GraphNode node, double weight)> CalculateDistances(List<GraphNode> roadGraph,
        GraphNode startNode)
    {
        PriorityQueue<long, double> toVisit = new();
        HashSet<long> visited = [];
        Dictionary<long, double> distances = roadGraph
            .Select(x => x.Id)
            .ToDictionary(x => x, x => x == startNode.Id ? 0 : double.MaxValue);

        Dictionary<long, GraphNode> graph = roadGraph.Select(x => (x.Id, x)).ToDictionary();

        toVisit.Enqueue(startNode.Id, 0);
        while (toVisit.Count > 0)
        {
            var node = toVisit.Dequeue();
            if (visited.Contains(node)) continue;
            visited.Add(node);

            var nodeDistance = distances[node];
            foreach (var (connected, weight) in graph[node].ConnectedNodes)
            {
                if (nodeDistance + weight < distances[connected.Id])
                {
                    distances[connected.Id] = distances[node] + weight;
                    toVisit.Enqueue(connected.Id, distances[connected.Id]);
                }
            }
        }

        return distances.Select(x => (graph[x.Key], x.Value)).ToList();
    }

    public List<GraphNode> ExpandPath(List<GraphNode> path, List<GraphNode> roadGraph)
    {
        return path.AsParallel().SelectMany(((node, i) =>
        {
            var next = i == path.Count - 1 ? 0 : i + 1;
            return Enumerable.Concat([node], ShortestPath(roadGraph, node, path[next]));
        })).ToList();
    }

    private static List<GraphNode> ShortestPath(List<GraphNode> roadGraph, GraphNode startNode, GraphNode endNode)
    {
        PriorityQueue<long, double> toVisit = new();
        HashSet<long> visited = [];
        Dictionary<long, long> predecessors = [];
        Dictionary<long, double> distances = roadGraph
            .Select(x => x.Id)
            .ToDictionary(x => x, x => x == startNode.Id ? 0 : double.MaxValue);

        Dictionary<long, GraphNode> graph = roadGraph.Select(x => (x.Id, x)).ToDictionary();

        toVisit.Enqueue(startNode.Id, 0);
        while (toVisit.Count > 0)
        {
            var node = toVisit.Dequeue();
            if (node == endNode.Id) break;
            if (visited.Contains(node)) continue;
            visited.Add(node);

            var nodeDistance = distances[node];
            foreach (var (connected, weight) in graph[node].ConnectedNodes)
            {
                if (nodeDistance + weight < distances[connected.Id])
                {
                    distances[connected.Id] = distances[node] + weight;
                    predecessors[connected.Id] = node;
                    toVisit.Enqueue(connected.Id, distances[connected.Id]);
                }
            }
        }

        List<GraphNode> path = [];
        var currentNode = endNode.Id;

        while (currentNode != startNode.Id)
        {
            currentNode = predecessors[currentNode];
            if (graph[currentNode] is not ParcelLockerGraphNode)
            {
                path.Add(graph[currentNode]);
            }
        }

        path.Reverse();
        return path;
    }
}