using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Vocabulary;

public class IsInflectingWord : TagFlagField
{
   readonly VocabNote _vocab;

   public IsInflectingWord(VocabNote vocab)
      : base(vocab, Tags.Vocab.Matching.IsInflectingWord) =>
      _vocab = vocab;

   public bool IsActive => IsSet() || _vocab.PartsOfSpeech.IsInflectingWordType();

   public override string ToString() => $"{Tag}: {IsActive}";
}
