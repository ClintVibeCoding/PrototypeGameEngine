using System.Collections.Immutable;

namespace PrototypeGameEngine;

/// <summary>One item in the Chronicle, belonging to a single Tick.</summary>
internal sealed record ChronicleEntry(int Tick, ImmutableArray<string> Lines);
