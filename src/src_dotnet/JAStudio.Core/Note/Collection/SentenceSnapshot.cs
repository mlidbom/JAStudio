using System.Linq;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Note.Collection;

class SentenceSnapshot : CachedNote
{
   public string[] Words { get; }
   public NoteId[] DetectedVocab { get; }
   public string[] UserHighlightedVocab { get; }
   public string[] MarkedIncorrectVocab { get; }

   public SentenceSnapshot(SentenceNote note) : base(note)
   {
      Words = note.GetWords().ToArray();
      DetectedVocab = note.GetParsingResult().MatchedVocabIds.ToArray();
      UserHighlightedVocab = note.Configuration.HighlightedWords.ToArray();
      MarkedIncorrectVocab = note.Configuration.IncorrectMatches.Words().ToArray();
   }
}
