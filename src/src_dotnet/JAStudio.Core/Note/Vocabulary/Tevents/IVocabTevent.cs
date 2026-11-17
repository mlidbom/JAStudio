using System.Collections.Generic;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Tevents;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleInterfaceMemberAmbiguity

namespace JAStudio.Core.Note.Vocabulary.Tevents;

/// <summary>
/// The tevent appliers apply only <c>PropertyUpdated</c> tevents, each the whole new value of one property. A domain tevent, such as<br/>
/// <see cref="Question.Renamed"/>, is also the <c>PropertyUpdated</c> tevent it causes.
/// </summary>
public interface IVocabTevent : INoteTevent
{
   public static class PropertyUpdated
   {
      public interface CorpusAnswer : IVocabTevent { string CorpusAnswer { get; } }
      public interface Explanation : IVocabTevent { string Explanation { get; } }
      public interface ExplanationLong : IVocabTevent { string ExplanationLong { get; } }
      public interface Mnemonic : IVocabTevent { string Mnemonic { get; } }
      public interface TechnicalNotes : IVocabTevent { string TechnicalNotes { get; } }
      public interface References : IVocabTevent { string References { get; } }
      public interface Readings : IVocabTevent { IReadOnlyList<string> Readings { get; } }
   }

   public interface Created : IVocabTevent, ITaggregateCreatedTevent, Question.PropertyUpdated.Text;

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
      }
   }

   public interface MetaData : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface SentenceCount : MetaData { int SentenceCount { get; } }
      }
   }

   public interface UsuallyWrittenInKana : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface Decision : UsuallyWrittenInKana { UsuallyWrittenInKanaDecision Decision { get; } }
      }
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
