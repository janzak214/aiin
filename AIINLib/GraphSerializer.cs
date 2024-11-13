using System.IO.Compression;
using System.Text.Json;
using AIINInterfaces;
using Geo;

namespace AIINLib;

public class GraphSerializer : IGraphSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private record SerializedNode(
        Dictionary<string, double> Connections,
        string? ParcelLockerId,
        double Latitude,
        double Longitude,
        double? ParcelLockerLatitude,
        double? ParcelLockerLongitude
    );

    public void Serialize(Stream stream, List<GraphNode> graph)
    {
        using GZipStream compressedStream = new(stream, CompressionLevel.SmallestSize);
        using Utf8JsonWriter writer = new(compressedStream);
        writer.WriteStartObject();
        foreach (var node in graph)
        {
            writer.WritePropertyName(node.Id.ToString());
            Dictionary<string, double> connections = [];
            foreach (var (connectedNode, weight) in node.ConnectedNodes)
            {
                if (!connections.TryAdd(connectedNode.Id.ToString(), weight))
                {
                    Console.WriteLine("warning: duplicate connection - parent: {0}, child: {1}", node.Id, connectedNode.Id);
                }
            }

            JsonSerializer.Serialize(writer, new SerializedNode(
                Connections: connections,
                ParcelLockerId: node is ParcelLockerGraphNode p1 ? p1.ParcelLockerId.ToString() : null,
                Latitude: node.Position.Latitude,
                Longitude: node.Position.Longitude,
                ParcelLockerLatitude: node is ParcelLockerGraphNode p2 ? p2.ParcelLockerPosition.Latitude : null,
                ParcelLockerLongitude: node is ParcelLockerGraphNode p3 ? p3.ParcelLockerPosition.Longitude : null
            ), _options);
        }

        writer.WriteEndObject();
        writer.Flush();
    }

    public List<GraphNode> Deserialize(Stream stream)
    {
        using GZipStream decompressedStream = new(stream, CompressionMode.Decompress);
        var data = JsonSerializer.Deserialize<Dictionary<string, SerializedNode>>(decompressedStream, _options)
                   ?? throw new InvalidOperationException("Failed to parse file");

        Dictionary<string, GraphNode> nodes = [];

        foreach (var (id, serialized) in data)
        {
            if (serialized.ParcelLockerId is { } parcelLocker)
            {
                nodes[id] = new ParcelLockerGraphNode(
                    Id: long.Parse(id),
                    ConnectedNodes: [],
                    Position: new Coordinate(serialized.Latitude, serialized.Longitude),
                    ParcelLockerId: long.Parse(parcelLocker),
                    ParcelLockerPosition: new Coordinate(
                        (double)serialized.ParcelLockerLatitude!,
                        (double)serialized.ParcelLockerLongitude!
                    )
                );
            }
            else
            {
                nodes[id] = new GraphNode(
                    Id: long.Parse(id),
                    ConnectedNodes: [],
                    Position: new Coordinate(serialized.Latitude, serialized.Longitude)
                );
            }
        }

        foreach (var (id, serialized) in data)
        {
            var node = nodes[id];
            foreach (var (connectedId, weight) in serialized.Connections)
            {
                node.ConnectedNodes.Add((nodes[connectedId], weight));
            }
        }

        return nodes.Values.ToList();
    }
}