using System.Collections.Immutable;

namespace PrototypeGameEngine;

/// <summary>
/// The complete state at the end of a Tick. Never changed once built (ENGINE_SPEC §3.2):
/// each Tick produces a new World from the previous one.
/// </summary>
internal sealed record World(int Tick, ImmutableArray<Nation> Nations)
{
    public const int NationCount = 8;

    /// <summary>Tick 0: the placeholder starting World.</summary>
    public static World Starting() =>
        new(0, Enumerable.Range(1, NationCount).Select(Nation.Placeholder).ToImmutableArray());
}
