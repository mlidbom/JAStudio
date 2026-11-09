using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>The word a vocab is derived from, if one is given, such as <c>終わる</c> for <c>終わり</c>.</summary>
public class DerivedFrom
{
   readonly VocabNote _vocab;
   readonly NoteGuard _guard;

   public DerivedFrom(VocabNote vocab, WordReference? word, NoteGuard guard)
   {
      _vocab = vocab;
      Word = word;
      _guard = guard;
   }

   /// <summary>The word the vocab is derived from, or null when none is given.</summary>
   public WordReference? Word { get; private set; }

   /// <summary>The vocab notes the <see cref="Word"/> refers to: none when there is no word.</summary>
   public List<VocabNote> Notes() => Word is {} word ? _vocab.Services.Collection.Vocab.ReferredToBy(word) : [];

   /// <summary>Sets the word the vocab is derived from: null for none.</summary>
   public void Set(WordReference? word) => _guard.Update(() => Word = word);

   public override string ToString() => Word?.DisambiguationName ?? "";
}
