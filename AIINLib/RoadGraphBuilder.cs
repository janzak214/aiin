using AIINInterfaces;
using Geo;
using Geo.Geodesy;
using RBush;
using Envelope = RBush.Envelope;

namespace AIINLib;

public class RoadGraphBuilder : IRoadGraphBuilder
{
    private readonly SpheroidCalculator _calculator = new();

    public List<GraphNode> CreateRoadGraph(List<Road> roads, List<Node> nodes, List<Node> parcelLockers)
    {
        var parcelLockerNodes = FindClosestNodes(nodes, parcelLockers)
            .GroupBy(x => x.roadNode.Id)
            .Select(group =>
            {
                if (group.Count() > 1)
                {
                    Console.WriteLine("warning: road node {0} shares multiple parcel lockers: {1}", group.Key,
                        string.Join(", ", group.Select(x => x.parcelLocker.Id).ToList()));
                }

                var item = group.MinBy(x => x.distance);
                return (item.roadNode.Id, item.parcelLocker);
            })
            .ToDictionary();

        Dictionary<long, (GraphNode, Coordinate)> graph = nodes
            .Select(node => (node.Id,
                (parcelLockerNodes.TryGetValue(node.Id, out var parcelLocker)
                        ? new ParcelLockerGraphNode(
                            Id: node.Id,
                            ConnectedNodes: [],
                            Position: node.Position,
                            ParcelLockerId: parcelLocker.Id,
                            ParcelLockerPosition: parcelLocker.Position
                        )
                        : new GraphNode(
                            Id: node.Id,
                            ConnectedNodes: [],
                            Position: node.Position
                        ),
                    node.Position)))
            .ToDictionary();

        foreach (var road in roads)
        {
            foreach (var (first, second) in road.Nodes.Zip(road.Nodes.Skip(1)))
            {
                var (firstNode, firstPosition) = graph[first];
                var (secondNode, secondPosition) = graph[second];
                var distance = _calculator.CalculateLength(
                    new CoordinateSequence(firstPosition, secondPosition)
                );

                firstNode.ConnectedNodes.Add((secondNode, double.Ceiling(distance.SiValue)));

                if (!road.OneWay)
                {
                    secondNode.ConnectedNodes.Add((firstNode, double.Ceiling(distance.SiValue)));
                }
            }
        }

        return graph.Values.Select(x => x.Item1).ToList();
    }

    private class IndexNode(Node node) : ISpatialData
    {
        public Node Node => node;

        private readonly Envelope _envelope = new(node.Position.Longitude,
            node.Position.Latitude,
            node.Position.Longitude, node.Position.Latitude);

        public ref readonly Envelope Envelope => ref _envelope;
    }

    private List<(Node parcelLocker, Node roadNode, double distance)> FindClosestNodes(List<Node> nodes,
        List<Node> parcelLockers)
    {
        var tree = new RBush<IndexNode>(maxEntries: 256);
        tree.BulkLoad(nodes.Select(x => new IndexNode(x)));
        var result = parcelLockers.AsParallel().Select(parcelLocker =>
        {
            var roadNode = tree.Knn(k: 1, x: parcelLocker.Position.Longitude, y: parcelLocker.Position.Latitude)[0]
                .Node;
            var distance = _calculator.CalculateLength(
                new CoordinateSequence(parcelLocker.Position, roadNode.Position)
            ).SiValue;
            return (parcelLocker, roadNode, distance);
        }).ToList();
        return result;
    }

    public List<GraphNode> OptimizeRoadGraph(List<GraphNode> roadGraph)
    {
        var graph = roadGraph;
        while (true)
        {
            var newGraph = OptimizeRoadGraphStep(graph);
            if (newGraph.Count == graph.Count)
            {
                return RemoveSpuriousConnections(newGraph);
            }

            graph = newGraph;
        }
    }

    private static List<GraphNode> OptimizeRoadGraphStep(List<GraphNode> roadGraph)
    {
        HashSet<long> toVisit = roadGraph.Select(x => x.Id).ToHashSet();
        HashSet<long> finalGraph = [..toVisit];

        Dictionary<long, GraphNode> graph = roadGraph.Select(x => (x.Id, x)).ToDictionary();
        Dictionary<long, HashSet<long>> incoming = new();
        foreach (var node in roadGraph)
        {
            foreach (var (connected, _) in node.ConnectedNodes)
            {
                if (!incoming.ContainsKey(connected.Id))
                {
                    incoming.Add(connected.Id, []);
                }

                incoming[connected.Id].Add(node.Id);
            }
        }

        Stack<GraphNode> stack = new();

        while (toVisit.Count > 0)
        {
            var parent = toVisit.First();
            stack.Push(graph[parent]);

            while (stack.Count > 0)
            {
                var firstNode = stack.Pop();
                if (!toVisit.Contains(firstNode.Id)) continue;
                toVisit.Remove(firstNode.Id);
                List<int> connectionsToDelete = [];

                for (var i = 0; i < firstNode.ConnectedNodes.Count; i++)
                {
                    var prevNode = firstNode;
                    var (currentNode, totalDistance) = firstNode.ConnectedNodes[i];
                    if (!toVisit.Contains(currentNode.Id))
                    {
                        continue;
                    }

                    bool isDeadEnd;
                    while (true)
                    {
                        var outgoingIds = currentNode.ConnectedNodes.Select(x => x.Item1.Id).ToHashSet();
                        var incomingIds = incoming[currentNode.Id];
                        var noIntersection = (outgoingIds.Count == 2 && outgoingIds.SetEquals(incomingIds)) ||
                                             (outgoingIds.Count == 1 && incomingIds.Count == 1 &&
                                              !outgoingIds.Overlaps(incomingIds));
                        isDeadEnd = ((outgoingIds.Count == 1 && incomingIds.Count == 1 &&
                                      outgoingIds.SetEquals(incomingIds)) || outgoingIds.Count == 0) &&
                                    currentNode is not ParcelLockerGraphNode;

                        if (isDeadEnd || !noIntersection || currentNode is ParcelLockerGraphNode ||
                            !toVisit.Contains(currentNode.Id))
                            break;

                        toVisit.Remove(currentNode.Id);
                        finalGraph.Remove(currentNode.Id);


                        var (nextNode, nextDistance) = currentNode.ConnectedNodes
                            .Find(x => !ReferenceEqualityComparer.Instance.Equals(x.Item1, prevNode));

                        prevNode = currentNode;
                        totalDistance += nextDistance;
                        currentNode = nextNode ?? throw new InvalidOperationException(currentNode.Id.ToString());
                    }

                    if (isDeadEnd)
                    {
                        connectionsToDelete.Add(i);
                        finalGraph.Remove(currentNode.Id);
                        toVisit.Remove(currentNode.Id);
                    }
                    else if (prevNode.Id != firstNode.Id)
                    {
                        var currentToPrevIndex = currentNode.ConnectedNodes.FindIndex(x => x.node.Id == prevNode.Id);
                        var currentToFirstIndex = currentNode.ConnectedNodes.FindIndex(x => x.node.Id == firstNode.Id);

                        if (currentToPrevIndex != -1)
                        {
                            if (currentToFirstIndex != -1)
                            {
                                var (_, weight) = currentNode.ConnectedNodes[currentToFirstIndex];
                                currentNode.ConnectedNodes[currentToFirstIndex] =
                                    (firstNode, double.Min(totalDistance, weight));
                                currentNode.ConnectedNodes.RemoveAt(currentToPrevIndex);
                            }
                            else
                            {
                                currentNode.ConnectedNodes[currentToPrevIndex] = (firstNode, totalDistance);
                            }
                        }

                        var firstToCurrentIndex = firstNode.ConnectedNodes.FindIndex(x => x.node.Id == currentNode.Id);

                        if (firstToCurrentIndex != -1)
                        {
                            var (_, weight) = firstNode.ConnectedNodes[firstToCurrentIndex];
                            firstNode.ConnectedNodes[firstToCurrentIndex] =
                                (currentNode, double.Min(totalDistance, weight));
                            connectionsToDelete.Add(i);
                        }
                        else
                        {
                            firstNode.ConnectedNodes[i] = (currentNode, totalDistance);
                        }

                        if (toVisit.Contains(currentNode.Id))
                            stack.Push(currentNode);
                    }
                }

                foreach (var indexToDelete in connectionsToDelete.OrderByDescending(x => x))
                {
                    firstNode.ConnectedNodes.RemoveAt(indexToDelete);
                }
            }
        }

        List<GraphNode> result = [];

        foreach (var id in finalGraph)
        {
            var currentNode = graph[id];
            var outgoingIds = currentNode.ConnectedNodes.Select(x => x.Item1.Id).ToHashSet();
            if (!incoming.ContainsKey(id))
            {
                result.Add(currentNode);
                continue;
            }

            var incomingIds = incoming[currentNode.Id];
            var isDeadEnd = ((outgoingIds.Count == 1 && incomingIds.Count == 1 &&
                              outgoingIds.SetEquals(incomingIds)) || outgoingIds.Count == 0) &&
                            currentNode is not ParcelLockerGraphNode;

            if (isDeadEnd)
            {
                graph[incomingIds.First()].ConnectedNodes.RemoveAll(x => x.node.Id == currentNode.Id);
            }
            else
            {
                result.Add(currentNode);
            }
        }

        return result;
    }

    private static List<GraphNode> RemoveSpuriousConnections(List<GraphNode> roadGraph)
    {
        var graph = roadGraph.Select(x => x.Id).ToHashSet();

        foreach (var node in roadGraph)
        {
            node.ConnectedNodes.RemoveAll(x => !graph.Contains(x.node.Id));
        }

        return roadGraph;
    }
}