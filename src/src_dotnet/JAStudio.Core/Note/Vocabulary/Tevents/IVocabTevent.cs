using System.Collections.Generic;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Storage.Dto;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleInterfaceMemberAmbiguity

namespace JAStudio.Core.Note.Vocabulary.Tevents;

/// <summary>
/// The tevent appliers apply only <c>PropertyUpdated</c> tevents, each the whole new value of one property. A domain tevent, such as<br/>
/// <see cref="Question.Renamed"/>, is also the <c>PropertyUpdated</c> tevent it causes.
/// </summary>
public interface IVocabTevent : INoteTevent
{
   public new static class PropertyUpdated
   {
      public interface CorpusAnswer : IVocabTevent { string CorpusAnswer { get; } }
      public interface Explanation : IVocabTevent { string Explanation { get; } }
      public interface ExplanationLong : IVocabTevent { string ExplanationLong { get; } }
      public interface Mnemonic : IVocabTevent { string Mnemonic { get; } }
      public interface TechnicalNotes : IVocabTevent { string TechnicalNotes { get; } }
      public interface References : IVocabTevent { string References { get; } }
      public interface Readings : IVocabTevent { IReadOnlyList<string> Readings { get; } }
      public interface Audio : IVocabTevent { IReadOnlyList<AudioReference> Audio { get; } }
      public interface Images : IVocabTevent { IReadOnlyList<ImageReference> Images { get; } }
      public interface PartsOfSpeech : IVocabTevent { IReadOnlyList<PartOfSpeech> PartsOfSpeech { get; } }
   }

   public interface Created : IVocabTevent, ITaggregateCreatedTevent, Question.PropertyUpdated.Text;

   /// <summary>
   /// A vocab created as a clone of another, <see cref="Original"/>: holding <see cref="State"/>, the original's data under a question of its own,<br/>
   /// which no other vocab has. The vocab's creation, as <see cref="IVocabTevent.Created"/> is a new vocab's.
   /// </summary>
   public interface Cloned : IVocabTevent, ITaggregateCreatedTevent
   {
      VocabId Original { get; }
      VocabDto State { get; }
   }

   /// <remarks>
   /// Carries the deleted vocab's name, which nothing else has inside the unit of work: the vocab is gone from it as it publishes this. A deleted<br/>
   /// vocab leaves its web of perfect synonyms (<see cref="JAStudio.Core.Note.Vocabulary.RelatedVocab.PerfectSynonyms"/>).
   /// </remarks>
   public new interface Deleted : IVocabTevent, INoteTevent.Deleted
   {
      WordReference Word { get; }
   }

   public interface Question : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface Text : Question { string Text { get; } }
         public interface Disambiguation : Question { string Disambiguation { get; } }
      }

      /// <remarks>Carries the old name, which nothing else has inside the unit of work: the vocab read there is its own copy, already renamed.</remarks>
      public interface Renamed : PropertyUpdated.Text, PropertyUpdated.Disambiguation
      {
         WordReference From { get; }
      }
   }

   public interface SourceData : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface Answer : SourceData { string Answer { get; } }
         public interface Mnemonic : SourceData { string Mnemonic { get; } }
         public interface ReadingMnemonic : SourceData { string ReadingMnemonic { get; } }
         public interface Audio : SourceData { IReadOnlyList<AudioReference> Audio { get; } }
         public interface Images : SourceData { IReadOnlyList<ImageReference> Images { get; } }
      }
   }

   public interface MetaData : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface SentenceCount : MetaData { int SentenceCount { get; } }
      }
   }

   public interface CompoundParts : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface Primary : CompoundParts { IReadOnlyList<WordReference> Primary { get; } }
         public interface EnteredAlternatives : CompoundParts { IReadOnlySet<WordReference> EnteredAlternatives { get; } }
      }
   }

   /// <remarks>
   /// A relationship that goes both ways (<see cref="JAStudio.Core.Note.Vocabulary.RelatedVocab.MutualWordRelationship"/>) publishes the word it changed, with its whole new set:<br/>
   /// <c>Added</c> and <c>Removed</c> when an edit of the vocab changed it, which a participation handler mirrors in each vocab the word refers to,<br/>
   /// and <c>AddedAsTheMirrorOf</c> and <c>RemovedAsTheMirrorOf</c> when that mirror changed it, which nothing mirrors back. The word of a mirror's<br/>
   /// tevent names the vocab whose edit it mirrors.
   /// </remarks>
   /// <remarks>
   /// The perfect synonyms are one web (<see cref="JAStudio.Core.Note.Vocabulary.RelatedVocab.PerfectSynonyms"/>): <c>Added</c> and <c>Removed</c> come<br/>
   /// from an edit of the vocab, and a participation handler of each changes the rest of the web, whose changes publish only the whole new set.
   /// </remarks>
   public interface RelatedVocab : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface ErgativeTwins : RelatedVocab
         {
            IReadOnlySet<WordReference> ErgativeTwins { get; }

            public interface Added : ErgativeTwins { WordReference Word { get; } }
            public interface Removed : ErgativeTwins { WordReference Word { get; } }
            public interface AddedAsTheMirrorOf : ErgativeTwins { WordReference Word { get; } }
            public interface RemovedAsTheMirrorOf : ErgativeTwins { WordReference Word { get; } }
         }

         public interface Synonyms : RelatedVocab
         {
            IReadOnlySet<WordReference> Synonyms { get; }

            public interface Added : Synonyms { WordReference Word { get; } }
            public interface Removed : Synonyms { WordReference Word { get; } }
            public interface AddedAsTheMirrorOf : Synonyms { WordReference Word { get; } }
            public interface RemovedAsTheMirrorOf : Synonyms { WordReference Word { get; } }
         }

         public interface PerfectSynonyms : RelatedVocab
         {
            IReadOnlySet<WordReference> PerfectSynonyms { get; }

            public interface Added : PerfectSynonyms { WordReference Word { get; } }
            public interface Removed : PerfectSynonyms { WordReference Word { get; } }
         }

         public interface Antonyms : RelatedVocab
         {
            IReadOnlySet<WordReference> Antonyms { get; }

            public interface Added : Antonyms { WordReference Word { get; } }
            public interface Removed : Antonyms { WordReference Word { get; } }
            public interface AddedAsTheMirrorOf : Antonyms { WordReference Word { get; } }
            public interface RemovedAsTheMirrorOf : Antonyms { WordReference Word { get; } }
         }

         public interface SeeAlso : RelatedVocab
         {
            IReadOnlySet<WordReference> SeeAlso { get; }

            public interface Added : SeeAlso { WordReference Word { get; } }
            public interface Removed : SeeAlso { WordReference Word { get; } }
            public interface AddedAsTheMirrorOf : SeeAlso { WordReference Word { get; } }
            public interface RemovedAsTheMirrorOf : SeeAlso { WordReference Word { get; } }
         }

         public interface DerivedFrom : RelatedVocab { WordReference? DerivedFrom { get; } }
         public interface ConfusedWith : RelatedVocab { IReadOnlySet<WordReference> ConfusedWith { get; } }
      }
   }

   public interface Register : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface NamesOfTheSetFlags : Register { IReadOnlyList<string> NamesOfTheSetFlags { get; } }
      }
   }

   public interface UsuallyWrittenInKana : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface Decision : UsuallyWrittenInKana { UsuallyWrittenInKanaDecision Decision { get; } }
      }
   }

   public interface MatchingConfiguration : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface NamesOfTheSetFlags : MatchingConfiguration { IReadOnlyList<string> NamesOfTheSetFlags { get; } }
         public interface DictionaryFormStart : MatchingConfiguration { RequireForbidChoice DictionaryFormStart { get; } }
         public interface YieldLastToken : MatchingConfiguration { RequireForbidChoice YieldLastToken { get; } }
         public interface YieldToSurface : MatchingConfiguration { IReadOnlyList<string> YieldToSurface { get; } }
         public interface MatchCondition : MatchingConfiguration { AllOfGroup<CandidateWordVariant> MatchCondition { get; } }
      }

      /// <summary>
      /// A shared-condition its match-condition uses was renamed (<see cref="Matching.Conditions.Sharing.SharedConditions.Rename"/>), so the vocab's file<br/>
      /// changes, though nothing of the vocab's own does: the match-condition holds the shared-condition object itself, which<br/>
      /// <see cref="Matching.Conditions.Sharing.SharedConditions"/> owns and renamed in place, while the file stores it by its name. The tevent sets<br/>
      /// nothing: it has the state store write the vocab again, which it does only for a vocab that published a tevent.
      /// </summary>
      /// <remarks>review-naming:matching: SharedConditionRenamed, a tevent that sets no property (Claude, 2026-11-17).</remarks>
      public interface SharedConditionRenamed : MatchingConfiguration;
   }

   public interface Forms : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface EnteredForms : Forms
         {
            IReadOnlyList<EnteredForm> EnteredForms { get; }

            public interface Added : EnteredForms { string Form { get; } }
            public interface Removed : EnteredForms { string Form { get; } }
         }
      }
   }
}

/// <summary>Takes any <see cref="INoteTevent"/>: a vocab also publishes the tevents every note has.</summary>
public interface IVocabTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
