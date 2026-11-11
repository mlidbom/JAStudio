namespace JAStudio.Core.Reservoir.Sources;

/// <summary>
/// Where sentences in the reservoir come from: one work, such as a show's subs2srs deck, an audiobook's deck or a show's subtitles, within a<br/>
/// collection of such works, such as the MediaFire folder of user-uploaded decks or the kitsunekko backup.
/// </summary>
/// <param name="Collection">The collection the work came in, such as <c>mediafire-user-uploaded-anki-decks</c>.</param>
/// <param name="Work">The work, as the collection names it: a deck's file name, or a show's folder of subtitles.</param>
/// <param name="Kind">Whether it's a deck, whose sentences come with audio, or subtitles, text alone.</param>
/// <param name="Location">Where the work is on this computer, such as the deck's file, to take its media from later.</param>
public sealed record ReservoirSource(string Collection, string Work, ReservoirSourceKind Kind, string Location);
