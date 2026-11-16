using System.Collections.Generic;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Tevents;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Note.Vocabulary.Tevents;

/// <summary>
/// The tevent appliers apply only the tevents named <c>…Updated</c>, each the whole new value of one property. A domain tevent, such as<br/>
/// <see cref="Forms.Added"/>, is also the <c>…Updated</c> tevent it causes, and exists only where something reacts to more than the new value.
/// </summary>
public interface IVocabTevent : INoteTevent
{
   public interface CorpusAnswerUpdated : IVocabTevent { string CorpusAnswer { get; } }
   public interface ExplanationUpdated : IVocabTevent { string Explanation { get; } }
   public interface ExplanationLongUpdated : IVocabTevent { string ExplanationLong { get; } }
   public interface MnemonicUpdated : IVocabTevent { string Mnemonic { get; } }
   public interface TechnicalNotesUpdated : IVocabTevent { string TechnicalNotes { get; } }
   public interface ReferencesUpdated : IVocabTevent { string References { get; } }

   public interface Created : IVocabTevent, ITaggregateCreatedTevent, Question.TextUpdated;

   public interface Question : IVocabTevent
   {
      public interface TextUpdated : Question { string Text { get; } }
      public interface DisambiguationUpdated : Question { string Disambiguation { get; } }

      /// <remarks>Carries the old name, which nothing else has inside the unit of work: the vocab read there is its own copy, already renamed.</remarks>
      public interface Renamed : TextUpdated, DisambiguationUpdated
      {
         WordReference From { get; }
      }
   }

   public interface Forms : IVocabTevent
   {
      public interface EnteredFormsUpdated : Forms { IReadOnlyList<EnteredForm> EnteredForms { get; } }

      public interface Added : EnteredFormsUpdated
      {
         string Form { get; }
      }

      public interface Removed : EnteredFormsUpdated
      {
         string Form { get; }
      }
   }
}

/// <summary>Takes any <see cref="INoteTevent"/>: a vocab also publishes the tevents every note has.</summary>
public interface IVocabTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
