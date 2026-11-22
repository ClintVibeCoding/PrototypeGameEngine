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
