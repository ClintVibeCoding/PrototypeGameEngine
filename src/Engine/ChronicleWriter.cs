using System.Globalization;
using System.Text;

namespace PrototypeGameEngine;

/// <summary>
/// Renders the Chronicle to bytes that don't depend on machine, OS or locale:
/// UTF-8 without BOM, <c>\n</c> line endings, culture-invariant numbers.
/// </summary>
internal static class ChronicleWriter
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static byte[] Render(ulong seed, int ticks, IReadOnlyList<ChronicleEntry> entries)
    {
        var text = new StringBuilder();
        Line(text, "Chronicle");
        Line(text, string.Create(CultureInfo.InvariantCulture, $"Seed: {seed}"));
        Line(text, string.Create(CultureInfo.InvariantCulture, $"Ticks: {ticks}"));

        foreach (var entry in entries)
        {
            Line(text, "");
            Line(text, string.Create(CultureInfo.InvariantCulture, $"Tick {entry.Tick}"));
            foreach (var line in entry.Lines)
                Line(text, line);
        }

        return Utf8NoBom.GetBytes(text.ToString());
    }

    // Never AppendLine: its newline is Environment.NewLine, which is "\r\n" on Windows.
    private static void Line(StringBuilder text, string line) => text.Append(line).Append('\n');
}
