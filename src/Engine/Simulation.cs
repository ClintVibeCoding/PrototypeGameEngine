namespace PrototypeGameEngine;

/// <summary>The engine's only public entry point.</summary>
public static class Simulation
{
    /// <summary>
    /// Runs a simulation from <paramref name="seed"/> for <paramref name="ticks"/> Ticks
    /// and returns the Chronicle as UTF-8 bytes (no BOM, <c>\n</c> line endings).
    /// </summary>
    /// <param name="seed">The run's 64-bit Seed.</param>
    /// <param name="ticks">
    /// How many Ticks to run after tick 0.
    /// OPEN(§11 Run length): a parameter so tests can choose; the real run length is undecided.
    /// </param>
    public static byte[] Run(ulong seed, int ticks)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ticks);

        var world = World.Starting();
        var entries = new List<ChronicleEntry>(ticks);

        // The single tick loop (ENGINE_SPEC §3.4). Tick 0 is the starting World.
        for (int tick = 1; tick <= ticks; tick++)
        {
            var snapshot = Phases.Observe(world);
            var intentions = Phases.DecideAll(snapshot);
            var resolved = Phases.Resolve(snapshot, intentions, tick);
            world = Phases.Derive(snapshot, resolved);
            entries.Add(Phases.Log(world));
        }

        return ChronicleWriter.Render(seed, ticks, entries);
    }
}
