using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public abstract class VocabTevent : NoteTevent, IVocabTevent
{
   protected VocabTevent() {}

   VocabTevent(VocabId vocabId) : base(vocabId) {}

   public class CorpusAnswerUpdated(string corpusAnswer) : VocabTevent, IVocabTevent.PropertyUpdated.CorpusAnswer { public string CorpusAnswer { get; } = corpusAnswer; }
   public class ExplanationUpdated(string explanation) : VocabTevent, IVocabTevent.PropertyUpdated.Explanation { public string Explanation { get; } = explanation; }
   public class ExplanationLongUpdated(string explanationLong) : VocabTevent, IVocabTevent.PropertyUpdated.ExplanationLong { public string ExplanationLong { get; } = explanationLong; }
   public class MnemonicUpdated(string mnemonic) : VocabTevent, IVocabTevent.PropertyUpdated.Mnemonic { public string Mnemonic { get; } = mnemonic; }
   public class TechnicalNotesUpdated(string technicalNotes) : VocabTevent, IVocabTevent.PropertyUpdated.TechnicalNotes { public string TechnicalNotes { get; } = technicalNotes; }
   public class ReferencesUpdated(string references) : VocabTevent, IVocabTevent.PropertyUpdated.References { public string References { get; } = references; }
   public class ReadingsUpdated(IReadOnlyList<string> readings) : VocabTevent, IVocabTevent.PropertyUpdated.Readings { public IReadOnlyList<string> Readings { get; } = readings; }
   public class AudioUpdated(IReadOnlyList<AudioReference> audio) : VocabTevent, IVocabTevent.PropertyUpdated.Audio { public IReadOnlyList<AudioReference> Audio { get; } = audio; }
   public class ImagesUpdated(IReadOnlyList<ImageReference> images) : VocabTevent, IVocabTevent.PropertyUpdated.Images { public IReadOnlyList<ImageReference> Images { get; } = images; }

   public class PartsOfSpeechUpdated(IReadOnlyList<PartOfSpeech> partsOfSpeech) : VocabTevent, IVocabTevent.PropertyUpdated.PartsOfSpeech
   {
      public IReadOnlyList<PartOfSpeech> PartsOfSpeech { get; } = partsOfSpeech;
   }

   public class Created(VocabId vocabId, string question) : VocabTevent(vocabId), IVocabTevent.Created
   {
      public string Text { get; } = question;
   }

   public class Deleted(WordReference word) : VocabTevent, IVocabTevent.Deleted
   {
      public WordReference Word { get; } = word;
   }

   public abstract class Question : VocabTevent, IVocabTevent.Question
   {
      public class Renamed(WordReference from, string text, string disambiguation) : Question, IVocabTevent.Question.Renamed
      {
         public WordReference From { get; } = from;
         public string Text { get; } = text;
         public string Disambiguation { get; } = disambiguation;
      }
   }

   public abstract class SourceData : VocabTevent, IVocabTevent.SourceData
   {
      public class SourceAnswerUpdated(string answer) : SourceData, IVocabTevent.SourceData.PropertyUpdated.Answer { public string Answer { get; } = answer; }
      public class SourceMnemonicUpdated(string mnemonic) : SourceData, IVocabTevent.SourceData.PropertyUpdated.Mnemonic { public string Mnemonic { get; } = mnemonic; }

      public class SourceReadingMnemonicUpdated(string readingMnemonic) : SourceData, IVocabTevent.SourceData.PropertyUpdated.ReadingMnemonic
      {
         public string ReadingMnemonic { get; } = readingMnemonic;
      }

      public class SourceAudioUpdated(IReadOnlyList<AudioReference> audio) : SourceData, IVocabTevent.SourceData.PropertyUpdated.Audio
      {
         public IReadOnlyList<AudioReference> Audio { get; } = audio;
      }

      public class SourceImagesUpdated(IReadOnlyList<ImageReference> images) : SourceData, IVocabTevent.SourceData.PropertyUpdated.Images
      {
         public IReadOnlyList<ImageReference> Images { get; } = images;
      }
   }

   public abstract class MetaData : VocabTevent, IVocabTevent.MetaData
   {
      public class SentenceCountUpdated(int sentenceCount) : MetaData, IVocabTevent.MetaData.PropertyUpdated.SentenceCount { public int SentenceCount { get; } = sentenceCount; }
   }

   public abstract class CompoundParts : VocabTevent, IVocabTevent.CompoundParts
   {
      public class PrimaryUpdated(IReadOnlyList<WordReference> primary) : CompoundParts, IVocabTevent.CompoundParts.PropertyUpdated.Primary
      {
         public IReadOnlyList<WordReference> Primary { get; } = primary;
      }

      public class EnteredAlternativesUpdated(IReadOnlySet<WordReference> enteredAlternatives) : CompoundParts, IVocabTevent.CompoundParts.PropertyUpdated.EnteredAlternatives
      {
         public IReadOnlySet<WordReference> EnteredAlternatives { get; } = enteredAlternatives;
      }
   }

   public abstract class RelatedVocab : VocabTevent, IVocabTevent.RelatedVocab
   {
      public class ErgativeTwinsUpdated(IReadOnlySet<WordReference> ergativeTwins) : RelatedVocab, IVocabTevent.RelatedVocab.PropertyUpdated.ErgativeTwins
      {
         public IReadOnlySet<WordReference> ErgativeTwins { get; } = ergativeTwins;
      }

      public class ErgativeTwinAdded(WordReference word, IReadOnlySet<WordReference> ergativeTwins) : ErgativeTwinsUpdated(ergativeTwins), IVocabTevent.RelatedVocab.PropertyUpdated.ErgativeTwins.Added
      {
         public WordReference Word { get; } = word;
      }

      public class ErgativeTwinRemoved(WordReference word, IReadOnlySet<WordReference> ergativeTwins) : ErgativeTwinsUpdated(ergativeTwins), IVocabTevent.RelatedVocab.PropertyUpdated.ErgativeTwins.Removed
      {
         public WordReference Word { get; } = word;
      }

      public class ErgativeTwinAddedAsTheMirrorOf(WordReference word, IReadOnlySet<WordReference> ergativeTwins) : ErgativeTwinsUpdated(ergativeTwins), IVocabTevent.RelatedVocab.PropertyUpdated.ErgativeTwins.AddedAsTheMirrorOf
      {
         public WordReference Word { get; } = word;
      }

      public class ErgativeTwinRemovedAsTheMirrorOf(WordReference word, IReadOnlySet<WordReference> ergativeTwins) : ErgativeTwinsUpdated(ergativeTwins), IVocabTevent.RelatedVocab.PropertyUpdated.ErgativeTwins.RemovedAsTheMirrorOf
      {
         public WordReference Word { get; } = word;
      }

      public class SynonymsUpdated(IReadOnlySet<WordReference> synonyms) : RelatedVocab, IVocabTevent.RelatedVocab.PropertyUpdated.Synonyms
      {
         public IReadOnlySet<WordReference> Synonyms { get; } = synonyms;
      }

      public class SynonymAdded(WordReference word, IReadOnlySet<WordReference> synonyms) : SynonymsUpdated(synonyms), IVocabTevent.RelatedVocab.PropertyUpdated.Synonyms.Added
      {
         public WordReference Word { get; } = word;
      }

      public class SynonymRemoved(WordReference word, IReadOnlySet<WordReference> synonyms) : SynonymsUpdated(synonyms), IVocabTevent.RelatedVocab.PropertyUpdated.Synonyms.Removed
      {
         public WordReference Word { get; } = word;
      }

      public class SynonymAddedAsTheMirrorOf(WordReference word, IReadOnlySet<WordReference> synonyms) : SynonymsUpdated(synonyms), IVocabTevent.RelatedVocab.PropertyUpdated.Synonyms.AddedAsTheMirrorOf
      {
         public WordReference Word { get; } = word;
      }

      public class SynonymRemovedAsTheMirrorOf(WordReference word, IReadOnlySet<WordReference> synonyms) : SynonymsUpdated(synonyms), IVocabTevent.RelatedVocab.PropertyUpdated.Synonyms.RemovedAsTheMirrorOf
      {
         public WordReference Word { get; } = word;
      }

      public class PerfectSynonymsUpdated(IReadOnlySet<WordReference> perfectSynonyms) : RelatedVocab, IVocabTevent.RelatedVocab.PropertyUpdated.PerfectSynonyms
      {
         public IReadOnlySet<WordReference> PerfectSynonyms { get; } = perfectSynonyms;
      }

      public class PerfectSynonymAdded(WordReference word, IReadOnlySet<WordReference> perfectSynonyms) : PerfectSynonymsUpdated(perfectSynonyms), IVocabTevent.RelatedVocab.PropertyUpdated.PerfectSynonyms.Added
      {
         public WordReference Word { get; } = word;
      }

      public class PerfectSynonymRemoved(WordReference word, IReadOnlySet<WordReference> perfectSynonyms) : PerfectSynonymsUpdated(perfectSynonyms), IVocabTevent.RelatedVocab.PropertyUpdated.PerfectSynonyms.Removed
      {
         public WordReference Word { get; } = word;
      }

      public class AntonymsUpdated(IReadOnlySet<WordReference> antonyms) : RelatedVocab, IVocabTevent.RelatedVocab.PropertyUpdated.Antonyms
      {
         public IReadOnlySet<WordReference> Antonyms { get; } = antonyms;
      }

      public class AntonymAdded(WordReference word, IReadOnlySet<WordReference> antonyms) : AntonymsUpdated(antonyms), IVocabTevent.RelatedVocab.PropertyUpdated.Antonyms.Added
      {
         public WordReference Word { get; } = word;
      }

      public class AntonymRemoved(WordReference word, IReadOnlySet<WordReference> antonyms) : AntonymsUpdated(antonyms), IVocabTevent.RelatedVocab.PropertyUpdated.Antonyms.Removed
      {
         public WordReference Word { get; } = word;
      }

      public class AntonymAddedAsTheMirrorOf(WordReference word, IReadOnlySet<WordReference> antonyms) : AntonymsUpdated(antonyms), IVocabTevent.RelatedVocab.PropertyUpdated.Antonyms.AddedAsTheMirrorOf
      {
         public WordReference Word { get; } = word;
      }

      public class AntonymRemovedAsTheMirrorOf(WordReference word, IReadOnlySet<WordReference> antonyms) : AntonymsUpdated(antonyms), IVocabTevent.RelatedVocab.PropertyUpdated.Antonyms.RemovedAsTheMirrorOf
      {
         public WordReference Word { get; } = word;
      }

      public class SeeAlsoUpdated(IReadOnlySet<WordReference> seeAlso) : RelatedVocab, IVocabTevent.RelatedVocab.PropertyUpdated.SeeAlso
      {
         public IReadOnlySet<WordReference> SeeAlso { get; } = seeAlso;
      }

      public class SeeAlsoAdded(WordReference word, IReadOnlySet<WordReference> seeAlso) : SeeAlsoUpdated(seeAlso), IVocabTevent.RelatedVocab.PropertyUpdated.SeeAlso.Added
      {
         public WordReference Word { get; } = word;
      }

      public class SeeAlsoRemoved(WordReference word, IReadOnlySet<WordReference> seeAlso) : SeeAlsoUpdated(seeAlso), IVocabTevent.RelatedVocab.PropertyUpdated.SeeAlso.Removed
      {
         public WordReference Word { get; } = word;
      }

      public class SeeAlsoAddedAsTheMirrorOf(WordReference word, IReadOnlySet<WordReference> seeAlso) : SeeAlsoUpdated(seeAlso), IVocabTevent.RelatedVocab.PropertyUpdated.SeeAlso.AddedAsTheMirrorOf
      {
         public WordReference Word { get; } = word;
      }

      public class SeeAlsoRemovedAsTheMirrorOf(WordReference word, IReadOnlySet<WordReference> seeAlso) : SeeAlsoUpdated(seeAlso), IVocabTevent.RelatedVocab.PropertyUpdated.SeeAlso.RemovedAsTheMirrorOf
      {
         public WordReference Word { get; } = word;
      }

      public class DerivedFromUpdated(WordReference? derivedFrom) : RelatedVocab, IVocabTevent.RelatedVocab.PropertyUpdated.DerivedFrom
      {
         public WordReference? DerivedFrom { get; } = derivedFrom;
      }

      public class ConfusedWithUpdated(IReadOnlySet<WordReference> confusedWith) : RelatedVocab, IVocabTevent.RelatedVocab.PropertyUpdated.ConfusedWith
      {
         public IReadOnlySet<WordReference> ConfusedWith { get; } = confusedWith;
      }
   }

   public abstract class Register : VocabTevent, IVocabTevent.Register
   {
      public class NamesOfTheSetFlagsUpdated(IReadOnlyList<string> namesOfTheSetFlags) : Register, IVocabTevent.Register.PropertyUpdated.NamesOfTheSetFlags
      {
         public IReadOnlyList<string> NamesOfTheSetFlags { get; } = namesOfTheSetFlags;
      }
   }

   public abstract class MatchingConfiguration : VocabTevent, IVocabTevent.MatchingConfiguration
   {
      public class NamesOfTheSetFlagsUpdated(IReadOnlyList<string> namesOfTheSetFlags) : MatchingConfiguration, IVocabTevent.MatchingConfiguration.PropertyUpdated.NamesOfTheSetFlags
      {
         public IReadOnlyList<string> NamesOfTheSetFlags { get; } = namesOfTheSetFlags;
      }

      public class DictionaryFormStartUpdated(RequireForbidChoice dictionaryFormStart) : MatchingConfiguration, IVocabTevent.MatchingConfiguration.PropertyUpdated.DictionaryFormStart
      {
         public RequireForbidChoice DictionaryFormStart { get; } = dictionaryFormStart;
      }

      public class YieldLastTokenUpdated(RequireForbidChoice yieldLastToken) : MatchingConfiguration, IVocabTevent.MatchingConfiguration.PropertyUpdated.YieldLastToken
      {
         public RequireForbidChoice YieldLastToken { get; } = yieldLastToken;
      }

      public class YieldToSurfaceUpdated(IReadOnlyList<string> yieldToSurface) : MatchingConfiguration, IVocabTevent.MatchingConfiguration.PropertyUpdated.YieldToSurface
      {
         public IReadOnlyList<string> YieldToSurface { get; } = yieldToSurface;
      }

      public class MatchConditionUpdated(AllOfGroup<CandidateWordVariant> matchCondition) : MatchingConfiguration, IVocabTevent.MatchingConfiguration.PropertyUpdated.MatchCondition
      {
         public AllOfGroup<CandidateWordVariant> MatchCondition { get; } = matchCondition;
      }

      public class SharedConditionRenamed : MatchingConfiguration, IVocabTevent.MatchingConfiguration.SharedConditionRenamed;
   }

   public abstract class UsuallyWrittenInKana : VocabTevent, IVocabTevent.UsuallyWrittenInKana
   {
      public class DecisionUpdated(UsuallyWrittenInKanaDecision decision) : UsuallyWrittenInKana, IVocabTevent.UsuallyWrittenInKana.PropertyUpdated.Decision
      {
         public UsuallyWrittenInKanaDecision Decision { get; } = decision;
      }
   }

   public abstract class Forms : VocabTevent, IVocabTevent.Forms
   {
      public class EnteredFormsUpdated(IReadOnlyList<EnteredForm> enteredForms) : Forms, IVocabTevent.Forms.PropertyUpdated.EnteredForms
      {
         public IReadOnlyList<EnteredForm> EnteredForms { get; } = enteredForms;
      }

      public class EnteredFormAdded(string form, IReadOnlyList<EnteredForm> enteredForms) : Forms, IVocabTevent.Forms.PropertyUpdated.EnteredForms.Added
      {
         public string Form { get; } = form;
         public IReadOnlyList<EnteredForm> EnteredForms { get; } = enteredForms;
      }

      public class EnteredFormRemoved(string form, IReadOnlyList<EnteredForm> enteredForms) : Forms, IVocabTevent.Forms.PropertyUpdated.EnteredForms.Removed
      {
         public string Form { get; } = form;
         public IReadOnlyList<EnteredForm> EnteredForms { get; } = enteredForms;
      }
   }
}

public class VocabTevent<TTevent>(TTevent tevent) : NoteTevent<TTevent>(tevent), IVocabTevent<TTevent> where TTevent : INoteTevent;
