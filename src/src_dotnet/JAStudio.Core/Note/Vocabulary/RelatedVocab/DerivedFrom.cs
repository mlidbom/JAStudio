using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>The word a vocab is derived from, if one is given, such as <c>終わる</c> for <c>終わり</c>.</summary>
public class DerivedFrom
{
   readonly VocabNote _vocab;

   public DerivedFrom(VocabNote vocab, WordReference? word)
   {
      _vocab = vocab;
      Word = word;
   }

   /// <summary>The word the vocab is derived from, or null when none is given.</summary>
   public WordReference? Word { get; private set; }

   /// <summary>The vocab notes the <see cref="Word"/> refers to: none when there is no word.</summary>
   public List<VocabNote> Notes() => Word is {} word ? _vocab.Services.Collection.Vocab.ReferredToBy(word) : [];

   /// <summary>Sets the word the vocab is derived from: null for none.</summary>
   public void Set(WordReference? word) => _vocab.PublishIfChanged(Word, word, static it => new VocabTevent.RelatedVocab.DerivedFromUpdated(it));

   /// <summary>Makes <paramref name="word"/> the word the vocab is derived from: what its tevent's applier does.</summary>
   internal void Apply(WordReference? word) => Word = word;

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, if it's the word.</summary>
   internal void Rename(WordReference oldName, WordReference newName)
   {
      if(Word == oldName)
         Set(newName);
   }

   public override string ToString() => Word?.DisambiguationName ?? "";
}
