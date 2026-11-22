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
    /// Deltas are the differences of those values (signed, so "+0" is checked); every Nation holds 4 Cells.
    /// </summary>
    [Fact]
    public void Two_tick_run_records_money_signed_delta_and_holdings_for_every_nation_with_no_entry_for_tick_0()
    {
        var expected =
            "Chronicle\n" +
            "Seed: 7\n" +
            "Ticks: 2\n" +
            "\n" +
            "Tick 1\n" +
            "Nation 1: money 3 (+3), 4 cells\n" +
            "Nation 2: money 0 (+0), 4 cells\n" +
            "Nation 3: money 9 (+9), 4 cells\n" +
            "Nation 4: money 5 (+5), 4 cells\n" +
            "Nation 5: money 4 (+4), 4 cells\n" +
            "Nation 6: money 2 (+2), 4 cells\n" +
            "Nation 7: money 4 (+4), 4 cells\n" +
            "Nation 8: money 3 (+3), 4 cells\n" +
            "\n" +
            "Tick 2\n" +
            "Nation 1: money 4 (+1), 4 cells\n" +
            "Nation 2: money 4 (+4), 4 cells\n" +
            "Nation 3: money 10 (+1), 4 cells\n" +
            "Nation 4: money 14 (+9), 4 cells\n" +
            "Nation 5: money 13 (+9), 4 cells\n" +
            "Nation 6: money 10 (+8), 4 cells\n" +
            "Nation 7: money 12 (+8), 4 cells\n" +
            "Nation 8: money 8 (+5), 4 cells\n";

        byte[] chronicle = Simulation.Run(seed: 7, ticks: 2);

        Assert.Equal(Encoding.UTF8.GetBytes(expected), chronicle);
    }
}
