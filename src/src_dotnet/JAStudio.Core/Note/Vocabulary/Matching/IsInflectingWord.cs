using System.Collections.Generic;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// The flag that the vocab is an inflecting word, such as ない or たい: a word that inflects the word before it, which then counts as inflected.<br/>
/// Some parts of speech make a vocab one without the flag (<see cref="IsActive"/>).
/// </summary>
public class IsInflectingWord : FlagField
{
   readonly VocabNotePartsOfSpeech _partsOfSpeech;

   internal IsInflectingWord(VocabNotePartsOfSpeech partsOfSpeech, IReadOnlyCollection<string> namesOfTheSetFlags, NoteGuard guard)
      : base("isInflectingWord", namesOfTheSetFlags, guard) =>
      _partsOfSpeech = partsOfSpeech;

   /// <summary>Whether the vocab is an inflecting word: the flag is set, or the vocab's parts of speech make it one.</summary>
   public bool IsActive => IsSet() || _partsOfSpeech.IsInflectingWordType();

   public override string ToString() => $"{Name}: {IsActive}";
}
