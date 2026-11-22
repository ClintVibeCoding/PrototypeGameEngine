using System.Runtime.CompilerServices;
using PrototypeGameEngine;

namespace Engine.Tests;

/// <summary>ENGINE_SPEC §8.1. These must never go red.</summary>
public class DeterminismTests
{
    private const ulong ExpectedSeed = 1;
    private const int ExpectedTicks = 50;
    private const string ExpectedFileName = "seed-1-50-ticks.chronicle";

    /// <summary>
    /// Set to "1" to rewrite the committed expected Chronicle from the current engine instead of
    /// checking it. Only do this when the history is meant to change, and say why in the PR.
    /// </summary>
    private const string RegenerateVariable = "REGENERATE_EXPECTED_CHRONICLE";

    [Fact]
    public void Same_seed_run_twice_gives_byte_identical_chronicles()
    {
        byte[] first = Simulation.Run(seed: 12345, ticks: 50);
        byte[] second = Simulation.Run(seed: 12345, ticks: 50);

        Assert.Equal(first, second);
    }

    /// <summary>
    /// Proves the Seed is actually used. The header prints the Seed, so comparing whole
    /// Chronicles would pass even with the generator ignored: compare only the Tick entries.
    /// </summary>
    [Fact]
    public void Different_seeds_give_different_histories()
    {
        string seed1 = TickEntries(Simulation.Run(seed: 1, ticks: 50));
        string seed2 = TickEntries(Simulation.Run(seed: 2, ticks: 50));

        Assert.NotEqual(seed1, seed2);
    }

    // Everything after the header, which ends at the first blank line.
    private static string TickEntries(byte[] chronicle)
    {
        string text = System.Text.Encoding.UTF8.GetString(chronicle);
        return text[(text.IndexOf("\n\n", StringComparison.Ordinal) + 2)..];
    }

    /// <summary>
    /// The committed file was produced in another process, usually on another machine and OS,
    /// so this catches hash-randomization, locale and line-ending differences that a
    /// same-process comparison can't see.
    /// </summary>
    [Fact]
    public void Seed_1_for_50_ticks_matches_the_committed_expected_chronicle()
    {
        byte[] actual = Simulation.Run(ExpectedSeed, ExpectedTicks);

        if (Environment.GetEnvironmentVariable(RegenerateVariable) == "1")
        {
            File.WriteAllBytes(SourceExpectedPath(), actual);
            return;
        }

        string committed = Path.Combine(AppContext.BaseDirectory, "Expected", ExpectedFileName);
        Assert.True(File.Exists(committed),
            $"No expected Chronicle at {committed}. Run the tests with {RegenerateVariable}=1 to create it.");
        Assert.Equal(File.ReadAllBytes(committed), actual);
    }

    private static string SourceExpectedPath([CallerFilePath] string thisFile = "") =>
        Path.Combine(Path.GetDirectoryName(thisFile)!, "Expected", ExpectedFileName);
}
