namespace PrototypeGameEngine;

/// <summary>
/// One Nation. Deliberately minimal: only Money so far. People, Power, Cohesion and
/// CurrentActivity (ENGINE_SPEC §5) arrive with the mechanics that need them.
/// </summary>
internal sealed record Nation(int NationId, string Name, long Money)
{
    /// <summary>Placeholder content, not game design: "Nation N" with no Money.</summary>
    public static Nation Placeholder(int nationId) => new(nationId, $"Nation {nationId}", 0);
}
