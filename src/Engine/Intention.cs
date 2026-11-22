namespace PrototypeGameEngine;

/// <summary>What a Nation means to do this Tick. Takes effect only in Resolve.</summary>
internal abstract record Intention(int NationId);

/// <summary>
/// OPEN(§11 Action set): a placeholder that exercises Decide → Resolve and nothing more.
/// It decides nothing about which actions exist. Resolve adds a random 0–9 to the Nation's Money.
/// </summary>
internal sealed record PlaceholderIntention(int NationId) : Intention(NationId);
