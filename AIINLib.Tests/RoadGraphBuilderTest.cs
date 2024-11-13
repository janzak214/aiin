using AIINInterfaces;
using Geo;
using NUnit.Framework.Constraints;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace AIINLib.Test;

class ConnectedConstraint(GraphNode expected, bool twoWay) : Constraint
{
    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (actual is not GraphNode node)
        {
            return new ConstraintResult(this, actual, ConstraintStatus.Error);
        }

        var connected = node.ConnectedNodes.Any(x => x.Item1.Id == expected.Id);
        if (!twoWay)
        {
            return new ConstraintResult(this, actual, connected ? ConstraintStatus.Success : ConstraintStatus.Failure);
        }

        var connectedBack = expected.ConnectedNodes.Any(x => x.Item1.Id == node.Id);
        return new ConstraintResult(this, actual,
            connected && connectedBack ? ConstraintStatus.Success : ConstraintStatus.Failure);
    }
}

class Is : NUnit.Framework.Is
{
    public static ConnectedConstraint ConnectedTo(GraphNode expected, bool twoWay = true)
    {
        return new ConnectedConstraint(expected, twoWay);
    }
}

static class IsExtensions
{
    public static ConnectedConstraint ConnectedTo(this ConstraintExpression expression, GraphNode expected,
        bool twoWay = true)
    {
        return new ConnectedConstraint(expected, twoWay);
    }
}

[TestFixture]
public class RoadGraphBuilderTest
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

    [Test]
    public void OptimizationTwoWayRoadBetweenParcelLockers()
    {
        // input: ① ⇆ 2 ⇆ 3 ⇆ ④
        // expected: ① ⇆ ④

        var node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        var node2 = MakeNode(2, 1, 0);
        var node3 = MakeNode(3, 2, 0);
        var node4 = MakeNode(4, 3, 0, isParcelLocker: true);

        ConnectNodes(node1, node2, 1);
        ConnectNodes(node2, node3, 1);
        ConnectNodes(node3, node4, 1);

        var roadGraph = new List<GraphNode> { node1, node2, node3, node4 };

        var optimizedRoadGraph = _builder.OptimizeRoadGraph(roadGraph);

        var expected = new List<GraphNode> { node1, node4 };

        Assert.That(optimizedRoadGraph, Is.EqualTo(expected));
        Assert.Multiple(() =>
        {
            Assert.That(node1, Is.ConnectedTo(node4));
            Assert.That(node4, Is.ConnectedTo(node1));
        });
    }

    [Test]
    public void OptimizationOneWayThreeParcelLockers()
    {
        // input: ① → 2 → ③ → 4 → ⑤
        // expected: ① → ③ → ⑤

        var node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        var node2 = MakeNode(2, 1, 0);
        var node3 = MakeNode(3, 2, 0, isParcelLocker: true);
        var node4 = MakeNode(4, 3, 0);
        var node5 = MakeNode(5, 4, 0, isParcelLocker: true);

        node1.ConnectedNodes.Add((node2, 1));
        node2.ConnectedNodes.Add((node3, 1));
        node3.ConnectedNodes.Add((node4, 1));
        node4.ConnectedNodes.Add((node5, 1));

        var roadGraph = new List<GraphNode> { node1, node2, node3, node4, node5 };

        var optimizedRoadGraph = _builder.OptimizeRoadGraph(roadGraph);

        var expected = new List<GraphNode> { node1, node3, node5 };

        Assert.That(optimizedRoadGraph, Is.EqualTo(expected));
        Assert.Multiple(() =>
        {
            Assert.That(node1, Is.ConnectedTo(node3, twoWay: false));
            Assert.That(node3, Is.ConnectedTo(node5, twoWay: false));
            Assert.That(node3.ConnectedNodes, Has.Count.EqualTo(1));
            Assert.That(node5.ConnectedNodes, Is.Empty);
        });
    }

    public void OptimizationTwoWayThreeParcelLockers()
    {
        // input: ① ⇆ 2 ⇆ ③ ⇆ 4 ⇆ ⑤
        // expected: ① ⇆ ③ ⇆ ⑤

        var node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        var node2 = MakeNode(2, 1, 0);
        var node3 = MakeNode(3, 2, 0, isParcelLocker: true);
        var node4 = MakeNode(4, 3, 0);
        var node5 = MakeNode(5, 4, 0, isParcelLocker: true);

        ConnectNodes(node1, node2, 1);
        ConnectNodes(node2, node3, 1);
        ConnectNodes(node3, node4, 1);
        ConnectNodes(node4, node5, 1);

        var roadGraph = new List<GraphNode> { node1, node2, node3, node4, node5 };

        var optimizedRoadGraph = _builder.OptimizeRoadGraph(roadGraph);

        var expected = new List<GraphNode> { node1, node3, node5 };

        Assert.That(optimizedRoadGraph, Is.EqualTo(expected));
        Assert.Multiple(() =>
        {
            Assert.That(node1, Is.ConnectedTo(node3));
            Assert.That(node3, Is.ConnectedTo(node5));
        });
    }

    [Test]
    public void OptimizationOneWayRoadBetweenTwoParcelLockers()
    {
        // input: ① → 2 → ③
        // expected: ① → ③

        var node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        var node2 = MakeNode(2, 1, 0);
        var node3 = MakeNode(3, 2, 0, isParcelLocker: true);

        node1.ConnectedNodes.Add((node2, 1));
        node2.ConnectedNodes.Add((node3, 1));

        var roadGraph = new List<GraphNode> { node1, node2, node3 };

        var optimizedRoadGraph = _builder.OptimizeRoadGraph(roadGraph);

        var expected = new List<GraphNode> { node1, node3 };

        Assert.That(optimizedRoadGraph, Is.EqualTo(expected));
        Assert.Multiple(() =>
        {
            Assert.That(node1, Is.ConnectedTo(node3, twoWay: false));
            Assert.That(node3.ConnectedNodes, Is.Empty);
        });
    }

    [Test]
    public void OptimizationTwoWayRoadBetweenIntersections()
    {
        // input: ①        ⑤
        // input: ⇵        ⇵
        // input: 2 ⇆ 3 ⇆ 4
        // input: ⇵        ⇵
        // input: ⑥        ⑦

        // expected: ①   ⑤
        // expected: ⇵   ⇵
        // expected: 2 ⇆ 4  
        // expected: ⇵   ⇵
        // expected: ⑥   ⑦

        var node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        var node2 = MakeNode(2, 1, 0);
        var node3 = MakeNode(3, 1, 1);
        var node4 = MakeNode(4, 1, 2);
        var node5 = MakeNode(5, 2, 0, isParcelLocker: true);
        var node6 = MakeNode(6, 2, 1, isParcelLocker: true);
        var node7 = MakeNode(7, 2, 2, isParcelLocker: true);

        ConnectNodes(node1, node2, 1);
        ConnectNodes(node2, node3, 1);
        ConnectNodes(node3, node4, 1);
        ConnectNodes(node4, node5, 1);
        ConnectNodes(node2, node6, 1);
        ConnectNodes(node4, node7, 1);

        var roadGraph = new List<GraphNode> { node1, node2, node3, node4, node5, node6, node7 };

        var optimizedRoadGraph = _builder.OptimizeRoadGraph(roadGraph);

        var expected = new List<GraphNode> { node1, node2, node4, node5, node6, node7 };

        Assert.That(optimizedRoadGraph, Is.EqualTo(expected));
        Assert.Multiple(() =>
        {
            Assert.That(node1, Is.ConnectedTo(node2));
            Assert.That(node2, Is.ConnectedTo(node4));
            Assert.That(node4, Is.ConnectedTo(node5));
            Assert.That(node2, Is.ConnectedTo(node6));
            Assert.That(node4, Is.ConnectedTo(node7));
        });
    }

    [Test]
    public void OptimizationTwoWayDeadEndParcelLocker()
    {
        // input: ① ⇆ 2 ⇆ 3
        // expected: ①

        var node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        var node2 = MakeNode(2, 1, 0);
        var node3 = MakeNode(3, 2, 0);

        node1.ConnectedNodes.Add((node2, 1));
        node2.ConnectedNodes.Add((node1, 1));
        node2.ConnectedNodes.Add((node3, 1));
        node3.ConnectedNodes.Add((node2, 1));

        var roadGraph = new List<GraphNode> { node1, node2, node3 };

        var optimizedRoadGraph = _builder.OptimizeRoadGraph(roadGraph);

        var expected = new List<GraphNode> { node1 };

        Assert.That(optimizedRoadGraph, Is.EqualTo(expected));
        Assert.That(optimizedRoadGraph[0].ConnectedNodes, Is.Empty);
    }

    [Test]
    public void OptimizationOneWayDeadEndParcelLocker()
    {
        // input: ① → 2 → 3
        // expected: ①

        var node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        var node2 = MakeNode(2, 1, 0);
        var node3 = MakeNode(3, 2, 0);

        node1.ConnectedNodes.Add((node2, 1));
        node2.ConnectedNodes.Add((node3, 1));

        var roadGraph = new List<GraphNode> { node1, node2, node3 };

        var optimizedRoadGraph = _builder.OptimizeRoadGraph(roadGraph);

        var expected = new List<GraphNode> { node1 };

        Assert.That(optimizedRoadGraph, Is.EqualTo(expected));
        Assert.That(optimizedRoadGraph[0].ConnectedNodes, Is.Empty);
    }


    [Test]
    public void OptimizationParallelRoads()
    {
        // input: ① ⇆ ④
        // input: ⇵   ⇵
        // input: 2 ⇆ 3

        // output: ① ⇆ ④

        var node1 = MakeNode(1, 0, 0, isParcelLocker: true);
        var node2 = MakeNode(2, 1, 0);
        var node3 = MakeNode(3, 1, 1);
        var node4 = MakeNode(4, 2, 0, isParcelLocker: true);

        ConnectNodes(node1, node2, 1);
        ConnectNodes(node2, node3, 1);
        ConnectNodes(node3, node4, 1);
        ConnectNodes(node1, node4, 1);

        var roadGraph = new List<GraphNode> { node1, node2, node3, node4 };

        var optimizedRoadGraph = _builder.OptimizeRoadGraph(roadGraph);

        var expected = new List<GraphNode> { node1, node4 };

        Assert.That(optimizedRoadGraph, Is.EqualTo(expected));
        Assert.Multiple(() =>
        {
            Assert.That(node1, Is.ConnectedTo(node4));
            Assert.That(node4, Is.ConnectedTo(node1));
            Assert.That(node1, Has.Property("ConnectedNodes").Count.EqualTo(1));
            Assert.That(node4, Has.Property("ConnectedNodes").Count.EqualTo(1));
            Assert.That(node1.ConnectedNodes[0].Item2, Is.EqualTo(1));
        });
    }
}