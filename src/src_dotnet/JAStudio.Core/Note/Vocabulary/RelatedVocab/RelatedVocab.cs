using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A vocab's relationships to other words, each word named by a <see cref="WordReference"/>, and the vocab related to it in other ways that are<br/>
/// looked up rather than stored, such as the compounds it's part of.
/// </summary>
public class RelatedVocab
{
   readonly VocabNote _vocab;
   readonly Lazy<HashSet<NoteId>> _inCompoundIds;

   public RelatedVocab(VocabNote vocab, VocabRelatedDto? data, NoteGuard guard)
   {
      _vocab = vocab;

      ErgativeTwins = new MutualWordRelationship(vocab, WordsIn(data?.ErgativeTwins), guard, it => it.ErgativeTwins);
      Synonyms = new Synonyms(vocab, WordsIn(data?.Synonyms), guard);
      PerfectSynonyms = new PerfectSynonyms(vocab, WordsIn(data?.PerfectSynonyms), guard);
      Antonyms = new MutualWordRelationship(vocab, WordsIn(data?.Antonyms), guard, it => it.Antonyms);
      SeeAlso = new MutualWordRelationship(vocab, WordsIn(data?.SeeAlso), guard, it => it.SeeAlso);
      DerivedFrom = new DerivedFrom(vocab, data?.DerivedFrom is {} derivedFrom ? WordReference.FromDto(derivedFrom) : null, guard);
      ConfusedWith = new WordRelationship(vocab, WordsIn(data?.ConfusedWith), guard);

      _inCompoundIds = new Lazy<HashSet<NoteId>>(() => [.. InCompounds().Select(it => it.GetId())]);
      return;

      static IEnumerable<WordReference> WordsIn(List<WordReferenceDto>? stored) => stored?.Select(WordReference.FromDto) ?? [];
   }

   /// <summary>
   /// The vocab's transitive or intransitive counterparts, such as 開ける for 開く. A vocab can have more than one: 起こす is the transitive counterpart<br/>
   /// of both 起きる and 起こる, and the corpus can hold a verb's kana and kanji spellings as vocab of their own, such as かける and 掛ける, which are both<br/>
   /// 掛かる's twins.
   /// </summary>
   public MutualWordRelationship ErgativeTwins { get; }

   public Synonyms Synonyms { get; }
   public PerfectSynonyms PerfectSynonyms { get; }

   /// <summary>Words with the opposite meaning, such as 開ける for 閉める. Linked both ways (<see cref="MutualWordRelationship"/>).</summary>
   public MutualWordRelationship Antonyms { get; }

   /// <summary>Words worth looking at alongside the vocab, related in a way none of the other relationships says. Linked both ways (<see cref="MutualWordRelationship"/>).</summary>
   public MutualWordRelationship SeeAlso { get; }

   public DerivedFrom DerivedFrom { get; }

   /// <summary>Words the vocab is easily confused with, such as 礼 for 札. Only this vocab knows of it.</summary>
   public WordRelationship ConfusedWith { get; }

   public HashSet<NoteId> InCompoundIds => _inCompoundIds.Value;

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, in every relationship.</summary>
   internal void Rename(WordReference oldName, WordReference newName)
   {
      ErgativeTwins.Rename(oldName, newName);
      Synonyms.Rename(oldName, newName);
      PerfectSynonyms.Rename(oldName, newName);
      Antonyms.Rename(oldName, newName);
      SeeAlso.Rename(oldName, newName);
      DerivedFrom.Rename(oldName, newName);
      ConfusedWith.Rename(oldName, newName);
   }

   /// <summary>
   /// The vocab notes this vocab is part of, directly or through compounds of compounds, most relevant first:<br/>
   /// first those with a compound part that names exactly this vocab (<see cref="VocabNoteQuestion.Reference"/>),<br/>
   /// then those whose question starts with this vocab or one of its stems, then the rest. Alphabetical within each group.
   /// </summary>
   /// <remarks>See <see cref="VocabCollection.CompoundsContaining"/> for which compounds count as containing this vocab.</remarks>
   public List<VocabNote> InCompounds()
   {
      var thisVocabAndItsStems = Conjugator.GetVocabStems(_vocab).Append(_vocab.Question.Text).ToList();

      return
      [
         .. _vocab.Services.Collection.Vocab.CompoundsContaining(_vocab)
                  .OrderByDescending(HasAPartThatNamesThisVocab)
                  .ThenByDescending(StartsWithThisVocab)
                  .ThenBy(it => it.GetQuestion())
      ];

      bool HasAPartThatNamesThisVocab(VocabNote compound) => compound.CompoundParts.AnyPartNamesExactly(_vocab);

      bool StartsWithThisVocab(VocabNote compound) => thisVocabAndItsStems.Any(it => compound.Question.Text.StartsWith(it, StringComparison.Ordinal));
   }

   public HashSet<VocabNote> HomophonesNotes() =>
   [
      .. _vocab.GetReadings()
               .SelectMany(reading => _vocab.Services.Collection.Vocab.WithReading(reading))
               .Where(homophone => !Equals(homophone, _vocab))
   ];

   public HashSet<VocabNote> StemsNotes() =>
   [
      .. _vocab.Conjugator.GetStemsForPrimaryForm()
               .SelectMany(stem => _vocab.Services.Collection.Vocab.WithQuestion(stem))
   ];
}
