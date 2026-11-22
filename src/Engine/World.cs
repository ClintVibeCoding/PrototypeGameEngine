using System.Collections.Immutable;

namespace PrototypeGameEngine;

/// <summary>
/// The complete state at the end of a Tick, including the values derived from it.
/// Never changed once built (ENGINE_SPEC §3.2): each Tick produces a new World from the previous one.
/// </summary>
internal sealed record World(int Tick, ImmutableArray<Nation> Nations, ImmutableArray<Cell> Cells, DerivedValues Derived)
{
    public const int NationCount = 8;

    /// <summary>Tick 0: the placeholder starting World. With no earlier World, every Money delta is 0.</summary>
    public static World Starting()
    {
        var authoritative = new World(
            0,
            Enumerable.Range(1, NationCount).Select(Nation.Placeholder).ToImmutableArray(),
            Enumerable.Range(1, NationCount * Cell.CellsPerNation).Select(Cell.Placeholder).ToImmutableArray(),
            DerivedValues.None);

        return authoritative with { Derived = DerivedValues.Compute(authoritative, authoritative) };
    }
}
