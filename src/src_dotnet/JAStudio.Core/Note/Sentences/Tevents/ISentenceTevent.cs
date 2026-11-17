using System.Collections.Generic;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Tevents;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleInterfaceMemberAmbiguity

namespace JAStudio.Core.Note.Sentences.Tevents;

/// <summary>
/// The tevent appliers apply only <c>PropertyUpdated</c> tevents, each the whole new value of one property, as a vocab's do
/// (<see cref="Vocabulary.Tevents.IVocabTevent"/>).
/// </summary>
public interface ISentenceTevent : INoteTevent
{
   public new static class PropertyUpdated
   {
      public interface CorpusQuestion : ISentenceTevent { string CorpusQuestion { get; } }
      public interface CorpusAnswer : ISentenceTevent { string CorpusAnswer { get; } }
      public interface Comments : ISentenceTevent { string Comments { get; } }
      public interface SourceName : ISentenceTevent { string SourceName { get; } }
      public interface Reading : ISentenceTevent { string Reading { get; } }
      public interface ParsingResult : ISentenceTevent { Sentences.ParsingResult ParsingResult { get; } }
      public interface ParsingCase : ISentenceTevent { ParsingCases.ParsingCase? ParsingCase { get; } }
      public interface Audio : ISentenceTevent { IReadOnlyList<AudioReference> Audio { get; } }
      public interface Screenshots : ISentenceTevent { IReadOnlyList<ImageReference> Screenshots { get; } }
   }

   /// <remarks>Carries the question the sentence's source gave it, which a new sentence is created with.</remarks>
   public interface Created : ISentenceTevent, ITaggregateCreatedTevent, SourceData.PropertyUpdated.Question;

   public interface SourceData : ISentenceTevent
   {
      public new static class PropertyUpdated
      {
         public interface Question : SourceData { string Question { get; } }
         public interface Answer : SourceData { string Answer { get; } }
         public interface Comments : SourceData { string Comments { get; } }
         public interface ExternalId : SourceData { string ExternalId { get; } }
         public interface Audio : SourceData { IReadOnlyList<AudioReference> Audio { get; } }
         public interface Screenshots : SourceData { IReadOnlyList<ImageReference> Screenshots { get; } }
      }
   }

   public interface Configuration : ISentenceTevent
   {
      public new static class PropertyUpdated
      {
         public interface HighlightedWords : Configuration { IReadOnlyList<string> HighlightedWords { get; } }
         public interface IncorrectMatches : Configuration { IReadOnlySet<WordExclusion> IncorrectMatches { get; } }
         public interface HiddenMatches : Configuration { IReadOnlySet<WordExclusion> HiddenMatches { get; } }
      }
   }
}

/// <summary>Takes any <see cref="INoteTevent"/>: a sentence also publishes the tevents every note has.</summary>
public interface ISentenceTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
