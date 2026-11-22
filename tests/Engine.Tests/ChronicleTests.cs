using System.Text;
using PrototypeGameEngine;

namespace Engine.Tests;

public class ChronicleTests
{
    [Fact]
    public void One_tick_run_records_every_nation_in_order_with_no_entry_for_tick_0()
    {
        var expected =
            "Chronicle\n" +
            "Seed: 7\n" +
            "Ticks: 1\n" +
            "\n" +
            "Tick 1\n" +
            "Nation 1: money 0\n" +
            "Nation 2: money 0\n" +
            "Nation 3: money 0\n" +
            "Nation 4: money 0\n" +
            "Nation 5: money 0\n" +
            "Nation 6: money 0\n" +
            "Nation 7: money 0\n" +
            "Nation 8: money 0\n";

        byte[] chronicle = Simulation.Run(seed: 7, ticks: 1);

        Assert.Equal(Encoding.UTF8.GetBytes(expected), chronicle);
    }
}
