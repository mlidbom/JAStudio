namespace JAStudio.Mcp.Tools.Collection;

/// <summary>What <see cref="CollectionTools.DescribeCollection"/> reports about the collection a JAStudio has loaded.</summary>
/// <param name="CorpusFolder">The folder the notes were loaded from: the corpus repository's checkout.</param>
/// <param name="Vocab">How many vocab there are, and are studied: null while there are no notes to use yet.</param>
/// <param name="Kanji">How many kanji there are, and are studied: null while there are no notes to use yet.</param>
/// <param name="Sentences">How many sentences there are, and are studied: null while there are no notes to use yet.</param>
/// <param name="WhyItHasNoNotesToUseYet">
/// Why the collection has no notes to use yet, such as a load of it that failed, or null once a load has succeeded<br/>
/// (<see cref="Core.Note.Collection.JPCollection.WhyItHasNoNotesToUseYet"/>).
/// </param>
public record CollectionDescription(string CorpusFolder, NoteCounts? Vocab, NoteCounts? Kanji, NoteCounts? Sentences, string? WhyItHasNoNotesToUseYet);
