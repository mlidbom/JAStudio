using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A vocab's relationships to other words, each word named by a <see cref="WordReference"/>, and the vocab related to it in other ways that are<br/>
/// looked up rather than stored, such as the compounds it's part of.
/// </summary>
/// <remarks>A component of its vocab: each relationship publishes a tevent of its own, <see cref="IVocabTevent.RelatedVocab"/>, which its appliers apply.</remarks>
public class RelatedVocab : TeventiveComponent<VocabNote, INoteTevent, NoteTevent, RelatedVocab, IVocabTevent.RelatedVocab, VocabTevent.RelatedVocab>, ITeventRouted
{
   static readonly TeventRouting<RelatedVocab> Appliers = TeventRouting<RelatedVocab>.DeclareAppliers(it => it
     .For<IVocabTevent.RelatedVocab.PropertyUpdated.ErgativeTwins>(static (related, tevent) => related.ErgativeTwins.Apply(tevent.ErgativeTwins))
     .For<IVocabTevent.RelatedVocab.PropertyUpdated.Synonyms>(static (related, tevent) => related.Synonyms.Apply(tevent.Synonyms))
     .For<IVocabTevent.RelatedVocab.PropertyUpdated.PerfectSynonyms>(static (related, tevent) => related.PerfectSynonyms.Apply(tevent.PerfectSynonyms))
     .For<IVocabTevent.RelatedVocab.PropertyUpdated.Antonyms>(static (related, tevent) => related.Antonyms.Apply(tevent.Antonyms))
     .For<IVocabTevent.RelatedVocab.PropertyUpdated.SeeAlso>(static (related, tevent) => related.SeeAlso.Apply(tevent.SeeAlso))
     .For<IVocabTevent.RelatedVocab.PropertyUpdated.DerivedFrom>(static (related, tevent) => related.DerivedFrom.Apply(tevent.DerivedFrom))
     .For<IVocabTevent.RelatedVocab.PropertyUpdated.ConfusedWith>(static (related, tevent) => related.ConfusedWith.Apply(tevent.ConfusedWith)));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly VocabNote _vocab;

   public RelatedVocab(VocabNote vocab, VocabRelatedDto? data) : base(vocab)
   {
      _vocab = vocab;

      ErgativeTwins = new MutualWordRelationship(vocab, WordsIn(data?.ErgativeTwins), static it => new VocabTevent.RelatedVocab.ErgativeTwinsUpdated(it), it => it.ErgativeTwins);
      Synonyms = new Synonyms(vocab, WordsIn(data?.Synonyms));
      PerfectSynonyms = new PerfectSynonyms(vocab, WordsIn(data?.PerfectSynonyms));
      Antonyms = new MutualWordRelationship(vocab, WordsIn(data?.Antonyms), static it => new VocabTevent.RelatedVocab.AntonymsUpdated(it), it => it.Antonyms);
      SeeAlso = new MutualWordRelationship(vocab, WordsIn(data?.SeeAlso), static it => new VocabTevent.RelatedVocab.SeeAlsoUpdated(it), it => it.SeeAlso);
      DerivedFrom = new DerivedFrom(vocab, data?.DerivedFrom is {} derivedFrom ? WordReference.FromDto(derivedFrom) : null);
      ConfusedWith = new WordRelationship(vocab, WordsIn(data?.ConfusedWith), static it => new VocabTevent.RelatedVocab.ConfusedWithUpdated(it));
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

   /// <summary>The ids of the <see cref="InCompounds"/>.</summary>
   /// <remarks>
   /// Computed at each call, from the other vocab as they are then. Kept, it went stale whenever a compound was added, and a vocab can't keep what<br/>
   /// it computes from other notes once the notes are kept in Compze's state store, where a committed vocab is never changed.
   /// </remarks>
   internal HashSet<NoteId> InCompoundIds() => [.. InCompounds().Select(it => it.GetId())];

   /// <summary>
   /// Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, in every relationship:<br/>
   /// those <see cref="HoldsTheWord"/> looks in.
   /// </summary>
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

   /// <summary>Whether any relationship has <paramref name="word"/>: those <see cref="Rename"/> renames in.</summary>
   internal bool HoldsTheWord(WordReference word) =>
      ErgativeTwins.Contains(word)
   || Synonyms.Contains(word)
   || PerfectSynonyms.Contains(word)
   || Antonyms.Contains(word)
   || SeeAlso.Contains(word)
   || DerivedFrom.Word == word
   || ConfusedWith.Contains(word);

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
