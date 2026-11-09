namespace JAStudio.Mcp.Tools.Editing;

/// <summary>What each of <see cref="KanjiEditingTools"/>' tools reports: the kanji's readings before and after the change.</summary>
/// <param name="Kanji">The kanji's question, to recognize it by.</param>
public record KanjiReadingsChange(string Kanji, KanjiReadingsReported Before, KanjiReadingsReported After);
