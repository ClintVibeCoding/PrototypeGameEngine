using System.Collections.Immutable;
using System.Globalization;

namespace PrototypeGameEngine;

/// <summary>
/// The five Phases of a Tick (ENGINE_SPEC §4), each its own function, called in order
/// by the single loop in <see cref="Simulation.Run"/>.
/// </summary>
internal static class Phases
{
    /// <summary>Observe: hands out the Snapshot, which is the World from the end of the previous Tick.</summary>
    public static World Observe(World previous) => previous;

    /// <summary>Decide for every Nation, in NationId order, against the same Snapshot.</summary>
    public static ImmutableArray<Intention> DecideAll(World snapshot) =>
        snapshot.Nations.OrderBy(n => n.NationId).SelectMany(n => Decide(snapshot, n)).ToImmutableArray();

    /// <summary>Decide: pure (ENGINE_SPEC §3.3). Reads the Snapshot, changes nothing, returns Intentions.</summary>
    public static ImmutableArray<Intention> Decide(World snapshot, Nation nation) => [];

    /// <summary>Resolve: the only Phase that produces new state. Builds World N from World N−1.</summary>
    public static World Resolve(World snapshot, ImmutableArray<Intention> intentions, int tick) =>
        snapshot with { Tick = tick };

    /// <summary>Derive: recomputes derived values from authoritative state (ENGINE_SPEC §3.6). None yet.</summary>
    public static World Derive(World snapshot, World resolved) => resolved;

    /// <summary>Log: the Chronicle entry for this Tick, one line per Nation in NationId order.</summary>
    public static ChronicleEntry Log(World world) =>
        new(world.Tick, world.Nations
            .OrderBy(n => n.NationId)
            .Select(n => string.Create(CultureInfo.InvariantCulture, $"{n.Name}: money {n.Money}"))
            .ToImmutableArray());
}
