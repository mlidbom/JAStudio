using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.NoteFields.AutoSaveWrappers;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

public class RelatedVocab
{
   readonly VocabNote _vocab;
   readonly RelatedVocabData _data;
   readonly NoteGuard _guard;
   readonly Lazy<HashSet<NoteId>> _inCompoundIds;

   public RelatedVocab(VocabNote vocab, VocabRelatedSubData? subData, NoteGuard guard)
   {
      _vocab = vocab;
      _guard = guard;

      _data = subData != null
                 ? new RelatedVocabData(
                    subData.ErgativeTwin,
                    new ValueWrapper<string>(subData.DerivedFrom),
                    subData.PerfectSynonyms.ToHashSet(),
                    subData.Synonyms.ToHashSet(),
                    subData.Antonyms.ToHashSet(),
                    subData.ConfusedWith.ToHashSet(),
                    subData.SeeAlso.ToHashSet())
                 : new RelatedVocabData(
                    string.Empty,
                    new ValueWrapper<string>(string.Empty),
                    [],
                    [],
                    [],
                    [],
                    []);

      ErgativeTwin = new ErgativeTwin(vocab, _data, guard);
      Synonyms = new Synonyms(vocab, _data, guard);
      PerfectSynonyms = new PerfectSynonyms(vocab, _data, guard);
      Antonyms = new Antonyms(vocab, _data, guard);
      SeeAlso = new SeeAlso(vocab, _data, guard);
      DerivedFrom = new DerivedFromField(_data, guard);
      ConfusedWith = new ConfusedWithField(_data, guard);

      _inCompoundIds = new Lazy<HashSet<NoteId>>(() => InCompounds().Select(voc => voc.GetId()).ToHashSet());
   }

   public ErgativeTwin ErgativeTwin { get; }
   public Synonyms Synonyms { get; }
   public PerfectSynonyms PerfectSynonyms { get; }
   public Antonyms Antonyms { get; }
   public SeeAlso SeeAlso { get; }
   public DerivedFromField DerivedFrom { get; }
   public ConfusedWithField ConfusedWith { get; }

   public string RawJson
   {
      get => RelatedVocabData.Serializer().Serialize(_data);
      set
      {
         var deserialized = RelatedVocabData.Serializer().Deserialize(value);
         _guard.Update(() =>
         {
            _data.ErgativeTwin = deserialized.ErgativeTwin;
            _data.DerivedFrom = deserialized.DerivedFrom;
            _data.PerfectSynonyms = deserialized.PerfectSynonyms;
            _data.Synonyms = deserialized.Synonyms;
            _data.Antonyms = deserialized.Antonyms;
            _data.ConfusedWith = deserialized.ConfusedWith;
            _data.SeeAlso = deserialized.SeeAlso;
         });
      }
   }

   public HashSet<NoteId> InCompoundIds => _inCompoundIds.Value;

   /// <summary>
   /// The vocab notes this vocab is part of, directly or through compounds of compounds, most relevant first:<br/>
   /// first those with a compound part that is exactly this vocab's <see cref="VocabNoteQuestion.DisambiguationName"/>,<br/>
   /// then those whose question starts with this vocab or one of its stems, then the rest. Alphabetical within each group.
   /// </summary>
   /// <remarks>See <see cref="VocabCollection.CompoundsContaining"/> for which compounds count as containing this vocab.</remarks>
   public List<VocabNote> InCompounds()
   {
      var thisVocabAndItsStems = Conjugator.GetVocabStems(_vocab).Append(_vocab.Question.Text).ToList();

      return _vocab.Services.Collection.Vocab.CompoundsContaining(_vocab)
                   .OrderByDescending(HasAPartThatIsThisVocabsDisambiguationName)
                   .ThenByDescending(StartsWithThisVocab)
                   .ThenBy(it => it.GetQuestion())
                   .ToList();

      bool HasAPartThatIsThisVocabsDisambiguationName(VocabNote compound) => compound.CompoundParts.AnyPartIsTheDisambiguationNameOf(_vocab);

      bool StartsWithThisVocab(VocabNote compound) => thisVocabAndItsStems.Any(it => compound.Question.Text.StartsWith(it, StringComparison.Ordinal));
   }

   public HashSet<VocabNote> HomophonesNotes()
   {
      return _vocab.GetReadings()
                   .SelectMany(reading => _vocab.Services.Collection.Vocab.WithReading(reading))
                   .Where(homophone => !Equals(homophone, _vocab))
                   .ToHashSet();
   }

   public HashSet<VocabNote> StemsNotes()
   {
      return _vocab.Conjugator.GetStemsForPrimaryForm()
                   .SelectMany(stem => _vocab.Services.Collection.Vocab.WithQuestion(stem))
                   .ToHashSet();
   }
}
