namespace PrototypeGameEngine;

/// <summary>
/// The engine's only source of randomness: SplitMix64 with a 64-bit Seed (ADR 0001).
/// Never <c>System.Random</c>, whose algorithm .NET may change between versions.
/// Changing this algorithm, or how draws are mapped, changes every Chronicle ever produced.
/// Passed explicitly to Resolve, the only Phase that receives it (ENGINE_SPEC §3.1).
/// </summary>
internal sealed class SeededGenerator(ulong seed)
{
    private ulong _state = seed;

    /// <summary>The next raw 64-bit SplitMix64 output.</summary>
    public ulong NextUInt64()
    {
        ulong z = unchecked(_state += 0x9E3779B97F4A7C15UL);
        z = unchecked((z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL);
        z = unchecked((z ^ (z >> 27)) * 0x94D049BB133111EBUL);
        return z ^ (z >> 31);
    }

    /// <summary>
    /// A draw in [0, <paramref name="exclusiveMax"/>): the high 64 bits of draw × max.
    /// Integer-only, so identical on every machine. The bias is at most max / 2^64.
    /// </summary>
    public int NextBelow(int exclusiveMax)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveMax);
        return (int)(((UInt128)NextUInt64() * (uint)exclusiveMax) >> 64);
    }
}
