using Geo;

namespace AIINInterfaces;

/// <summary>
/// Represents an OpenStreetMap node.
/// </summary>
/// <param name="Id">ID of the node.</param>
/// <param name="Position">Geographic coordinates of the node.</param>
public record Node(long Id, Coordinate Position);