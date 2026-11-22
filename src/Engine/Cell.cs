namespace PrototypeGameEngine;

/// <summary>
/// One unit of territory, owned by exactly one Nation. Deliberately minimal: adjacency,
/// infrastructure and coordinates (ENGINE_SPEC §5) arrive with the Ireland map.
/// </summary>
internal sealed record Cell(int CellId, int Owner)
{
    public const int CellsPerNation = 4;

    /// <summary>Placeholder content, not game design: Cells 1–4 to Nation 1, 5–8 to Nation 2, and so on.</summary>
    public static Cell Placeholder(int cellId) => new(cellId, (cellId - 1) / CellsPerNation + 1);
}
