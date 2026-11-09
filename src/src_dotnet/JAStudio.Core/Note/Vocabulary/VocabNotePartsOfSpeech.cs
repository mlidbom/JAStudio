using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A vocab's <see cref="PartOfSpeech"/>s, such as noun and suru verb: a set, so each at most once, of parts of speech JAStudio knows.<br/>
/// They decide how the vocab is matched, such as whether it inflects as a godan or an ichidan verb, and the markers its page shows.
/// </summary>
public class VocabNotePartsOfSpeech
{
   readonly NoteGuard _guard;
   IReadOnlyList<PartOfSpeech> _partsOfSpeech;

   public VocabNotePartsOfSpeech(VocabNote vocab, VocabDto? data, NoteGuard guard)
   {
      Vocab = vocab;
      _guard = guard;
      _partsOfSpeech = DistinctInAlphabeticalOrder((data?.PartsOfSpeech ?? []).Select(PartOfSpeech.Named));
   }

   VocabNote Vocab { get; }

   /// <summary>The parts of speech, in alphabetical order of their names.</summary>
   public IReadOnlyList<PartOfSpeech> All() => _partsOfSpeech;

   public bool Contains(PartOfSpeech partOfSpeech) => _partsOfSpeech.Contains(partOfSpeech);

   /// <summary>Adds <paramref name="partOfSpeech"/>. Adding one the vocab has already changes nothing.</summary>
   public void Add(PartOfSpeech partOfSpeech) => Set([.. _partsOfSpeech, partOfSpeech]);

   /// <summary>Removes <paramref name="partOfSpeech"/>. Removing one the vocab doesn't have changes nothing.</summary>
   public void Remove(PartOfSpeech partOfSpeech) => Set(_partsOfSpeech.Where(it => it != partOfSpeech));

   /// <summary>Replaces the parts of speech with <paramref name="partsOfSpeech"/>.</summary>
   public void Set(IEnumerable<PartOfSpeech> partsOfSpeech) => _guard.Update(() => _partsOfSpeech = DistinctInAlphabeticalOrder(partsOfSpeech));

   public bool IsIchidan() => Contains(PartOfSpeech.IchidanVerb);
   public bool IsGodan() => Contains(PartOfSpeech.GodanVerb);
   public bool IsTransitive() => Contains(PartOfSpeech.Transitive);
   public bool IsIntransitive() => Contains(PartOfSpeech.Intransitive);
   public bool IsInflectingWordType() => IsGodan() || IsIchidan();

   public bool IsSuruVerbIncluded()
   {
      var question = Vocab.Question.WithoutNoiseCharacters;
      return question.Length > 2 && question.EndsWith("する");
   }

   static readonly HashSet<string> GaSuruNiSuruEndings = ["がする", "にする", "くする"];

   public bool IsNiSuruGaSuruKuSuruCompound()
   {
      var question = Vocab.Question.WithoutNoiseCharacters;
      if(question.Length <= 3)
         return false;

      var ending = question[^3..];
      return GaSuruNiSuruEndings.Contains(ending);
   }

   /// <summary>
   /// Replaces the parts of speech with the dictionary's for the vocab. For a suru verb the dictionary doesn't have, such as 勉強する, it's a suru verb,<br/>
   /// transitive or intransitive as the dictionary has the word without する. Otherwise, when the dictionary doesn't have the vocab, nothing changes.
   /// </summary>
   public void SetAutomaticallyFromDictionary()
   {
      var lookup = Vocab.Services.DictLookup.LookupVocabWordOrName(Vocab);
      if(lookup.FoundWords())
      {
         Set(lookup.PartsOfSpeech());
      } else if(IsSuruVerbIncluded())
      {
         var question = Vocab.Question.WithoutNoiseCharacters;
         question = question[..^2];
         var readings = Vocab.GetReadings().Select(it => it[..^2]).ToList();
         lookup = Vocab.Services.DictLookup.LookupWordOrNameWithMatchingReading(question, readings);
         Set([PartOfSpeech.SuruVerb, .. lookup.PartsOfSpeech().Intersect([PartOfSpeech.Transitive, PartOfSpeech.Intransitive])]);
      }
   }

   public bool IsPassiveVerbCompound() => Vocab.CompoundParts.LastPrimaryPartIsAPlainWordAmong(AnalysisConstants.PassiveVerbEndings);

   public bool IsCausativeVerbCompound() => Vocab.CompoundParts.LastPrimaryPartIsAPlainWordAmong(AnalysisConstants.CausativeVerbEndings);

   public bool IsCompleteNaAdjective() => Vocab.Question.Text.EndsWith("な") && Contains(PartOfSpeech.NaAdjective);

   internal List<string> ToDto() => [.. _partsOfSpeech.Select(it => it.Name)];

   public override string ToString() => string.Join(", ", _partsOfSpeech);

   static IReadOnlyList<PartOfSpeech> DistinctInAlphabeticalOrder(IEnumerable<PartOfSpeech> partsOfSpeech) => [.. partsOfSpeech.Distinct().Order()];
}
