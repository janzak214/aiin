using AIINInterfaces;
using Geo;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace AIINLib.Test;

[TestFixture]
public class GraphSerializerTest
{
    [Test]
    public void RoundTrip()
    {
        var nodeA = new GraphNode(1, [], new Coordinate(0, 0));
        var nodeB = new ParcelLockerGraphNode(2, [], new Coordinate(0, 0),
            42, new Coordinate(42, 29)
        );
        nodeA.ConnectedNodes.Add((nodeB, 1));
        nodeB.ConnectedNodes.Add((nodeA, 1));

        var graphSerializer = new GraphSerializer();
        var stream = new MemoryStream();
        graphSerializer.Serialize(stream, [nodeA, nodeB]);
        stream = new MemoryStream(stream.GetBuffer());
        stream.Seek(0, SeekOrigin.Begin);
        var deserialized = graphSerializer.Deserialize(stream);

        Assert.That(deserialized, Has.Count.EqualTo(2));
        Assert.That(deserialized[0], Has.Property(nameof(nodeA.Id)).EqualTo(nodeA.Id));
        Assert.That(deserialized[1], Is.InstanceOf<ParcelLockerGraphNode>());
    }
}