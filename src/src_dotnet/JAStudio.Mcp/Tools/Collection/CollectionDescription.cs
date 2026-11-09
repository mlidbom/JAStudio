namespace JAStudio.Mcp.Tools.Collection;

/// <summary>What <see cref="CollectionTools.DescribeCollection"/> reports about the collection a JAStudio has loaded.</summary>
/// <param name="CorpusFolder">The folder the notes were loaded from: the corpus repository's checkout.</param>
public record CollectionDescription(string CorpusFolder, NoteCounts Vocab, NoteCounts Kanji, NoteCounts Sentences);
