using System.Text;
using PrototypeGameEngine;

namespace Engine.Tests;

public class ChronicleTests
{
    /// <summary>
    /// The Money values are not computed by the engine: they come from a separate reference
    /// SplitMix64 written in PowerShell with BigInteger arithmetic. That reference reproduces
    /// the published vector (seed 1234567 → 6457827717110365317, 3203168211198807973).
    /// Each Tick, each Nation in NationId order gets one draw, mapped to 0–9 as (draw × 10) >> 64.
    /// </summary>
    [Fact]
    public void Two_tick_run_records_every_nation_in_order_with_seeded_money_and_no_entry_for_tick_0()
    {
        var expected =
            "Chronicle\n" +
            "Seed: 7\n" +
            "Ticks: 2\n" +
            "\n" +
            "Tick 1\n" +
            "Nation 1: money 3\n" +
            "Nation 2: money 0\n" +
            "Nation 3: money 9\n" +
            "Nation 4: money 5\n" +
            "Nation 5: money 4\n" +
            "Nation 6: money 2\n" +
            "Nation 7: money 4\n" +
            "Nation 8: money 3\n" +
            "\n" +
            "Tick 2\n" +
            "Nation 1: money 4\n" +
            "Nation 2: money 4\n" +
            "Nation 3: money 10\n" +
            "Nation 4: money 14\n" +
            "Nation 5: money 13\n" +
            "Nation 6: money 10\n" +
            "Nation 7: money 12\n" +
            "Nation 8: money 8\n";

        byte[] chronicle = Simulation.Run(seed: 7, ticks: 2);

        Assert.Equal(Encoding.UTF8.GetBytes(expected), chronicle);
    }
}
