namespace PrototypeGameEngine;

/// <summary>
/// Maps a tick number to an in-world date, for display only (ENGINE_SPEC §3.5).
/// No logic may ever read it, and the Chronicle shows tick numbers, never dates.
/// </summary>
internal static class Calendar
{
    /// <summary>
    /// OPEN(§11 Tick length): placeholder of one month per Tick from an arbitrary year 1.
    /// The real tick length is undecided; this is just the single change point.
    /// </summary>
    public static (int Year, int Month) DateOf(int tick) => (1 + tick / 12, 1 + tick % 12);
}
