namespace AIINInterfaces;

/// <summary>
/// Represents an OpenStreetMap way.
/// </summary>
/// <param name="Id">ID of the way.</param>
/// <param name="Nodes">A list of nodes the way consists of.</param>
/// <param name="OneWay">Whether the road is one-way.</param>
public record Road(long Id, long[] Nodes, bool OneWay);