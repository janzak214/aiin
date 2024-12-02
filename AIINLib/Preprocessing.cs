using AIINInterfaces;
using Geo;
using OsmSharp;
using Node = AIINInterfaces.Node;

namespace AIINLib;

public static class Preprocessing
{
    
    /// <summary>
    /// Splits a collection of <see cref="OsmGeo"/> items into separate lists of parcel lockers, road nodes, and roads.
    /// </summary>
    /// <param name="items">The collection of <see cref="OsmGeo"/> items to split.</param>
    /// <returns>
    /// A tuple containing three lists:
    /// <list type="bullet">
    ///     <item><description><c>parcelLockers</c>: A list of nodes representing parcel lockers.</description></item>
    ///     <item><description><c>roadNodes</c>: A list of nodes representing road nodes.</description></item>
    ///     <item><description><c>roads</c>: A list of roads.</description></item>
    /// </list>
    /// </returns>
    public static
        (List<Node> parcelLockers, List<Node> roadNodes, List<Road> roads)
        SplitByType(IEnumerable<OsmGeo> items)
    {
        List<Node> parcelLockers = [];
        List<Node> roadNodes = [];
        List<Road> roads = [];

        foreach (var item in items)
        {
            switch (item)
            {
                case OsmSharp.Node
                {
                    Id: { } id,
                    Longitude: { } longitude,
                    Latitude: { } latitude,
                    Tags: var tags
                }:
                    var node = new Node(id, new Coordinate(latitude, longitude));
                    if (tags != null && tags.Contains("amenity", "parcel_locker"))
                        parcelLockers.Add(node);
                    else
                        roadNodes.Add(node);
                    break;

                case Way { Id: { } id, Nodes: { } nodes, Tags: var tags }:
                    var isOneWay = tags?.Contains("oneway", "yes") ?? false;
                    var road = new Road(id, nodes, isOneWay);
                    roads.Add(road);
                    break;
            }
        }

        return (parcelLockers, roadNodes, roads);
    }
}