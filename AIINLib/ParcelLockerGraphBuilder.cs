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

    private static List<(GraphNode node, double weight)> CalculateDistances(List<GraphNode> roadGraph, GraphNode startNode)
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
}
