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
    /// <remarks>No generator: if Decide ever needs randomness it gets its own, seeded from (Seed, tick, Nation).</remarks>
    public static ImmutableArray<Intention> Decide(World snapshot, Nation nation) =>
        [new PlaceholderIntention(nation.NationId)];

    /// <summary>
    /// Resolve: the only Phase that produces new state, and the only one given the generator.
    /// Builds World N from World N−1, applying Intentions in NationId order so that results
    /// never depend on collection ordering. Draw order is part of the Chronicle's contract.
    /// </summary>
    public static World Resolve(World snapshot, ImmutableArray<Intention> intentions, SeededGenerator generator, int tick)
    {
        var money = snapshot.Nations.ToDictionary(n => n.NationId, n => n.Money);

        foreach (var intention in intentions.OrderBy(i => i.NationId))
        {
            switch (intention)
            {
                case PlaceholderIntention placeholder:
                    money[placeholder.NationId] += generator.NextBelow(10);
                    break;
                default:
                    throw new InvalidOperationException($"Resolve has no rule for {intention.GetType().Name}.");
            }
        }

        // Built by iterating the Snapshot's ordered Nations, never the dictionary.
        return snapshot with
        {
            Tick = tick,
            Nations = snapshot.Nations.Select(n => n with { Money = money[n.NationId] }).ToImmutableArray(),
        };
    }

    /// <summary>Derive: recomputes derived values from authoritative state (ENGINE_SPEC §3.6). None yet.</summary>
    public static World Derive(World snapshot, World resolved) => resolved;

    /// <summary>Log: the Chronicle entry for this Tick, one line per Nation in NationId order.</summary>
    public static ChronicleEntry Log(World world) =>
        new(world.Tick, world.Nations
            .OrderBy(n => n.NationId)
            .Select(n => string.Create(CultureInfo.InvariantCulture, $"{n.Name}: money {n.Money}"))
            .ToImmutableArray());
}
