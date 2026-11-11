namespace JAStudio.Core.Reservoir.Sources;

/// <summary>What kind of work a <see cref="ReservoirSource"/> is, which says what its sentences come with.</summary>
public enum ReservoirSourceKind
{
   /// <summary>An Anki deck, such as one made with subs2srs: each sentence a note, usually with its audio, a screenshot and a translation.</summary>
   Deck,

   /// <summary>Subtitle files: each sentence a line of text, with the time it's shown at, and nothing more.</summary>
   Subtitles
}
