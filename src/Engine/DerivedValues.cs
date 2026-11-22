using System.Collections.Immutable;

namespace PrototypeGameEngine;

/// <summary>A Nation's derived values for one World. Never stored on the Nation (ENGINE_SPEC §5).</summary>
internal sealed record NationDerived(int Holdings, long MoneyDelta);

/// <summary>
/// Values worked out fresh from authoritative state at the end of every Tick, never kept
/// as running counts (ENGINE_SPEC §3.6). Keyed by NationId in a sorted map, so iteration
/// order is a contract.
/// </summary>
internal sealed record DerivedValues(ImmutableSortedDictionary<int, NationDerived> ByNation)
{
    public static readonly DerivedValues None = new(ImmutableSortedDictionary<int, NationDerived>.Empty);

    /// <summary>
    /// Holdings: Cells each Nation owns in <paramref name="current"/>.
    /// Money delta: Money in <paramref name="current"/> minus Money in <paramref name="previous"/>.
    /// </summary>
    public static DerivedValues Compute(World previous, World current)
    {
        var previousMoney = previous.Nations.ToImmutableSortedDictionary(n => n.NationId, n => n.Money);

        return new(current.Nations.ToImmutableSortedDictionary(
            n => n.NationId,
            n => new NationDerived(
                Holdings: current.Cells.Count(c => c.Owner == n.NationId),
                MoneyDelta: n.Money - previousMoney[n.NationId])));
    }

    public NationDerived For(int nationId) =>
        ByNation.TryGetValue(nationId, out var derived)
            ? derived
            : throw new InvalidOperationException($"No derived values for Nation {nationId}: Derive has not run on this World.");
}
