using System.Collections.Generic;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Tevents;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleInterfaceMemberAmbiguity

namespace JAStudio.Core.Note.Vocabulary.Tevents;

/// <summary>
/// The tevent appliers apply only <c>PropertyUpdated</c> tevents, each the whole new value of one property. A domain tevent, such as<br/>
/// <see cref="Forms.Added"/>, is also the <c>PropertyUpdated</c> tevent it causes, and exists only where something reacts to more than the new value.
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

   public interface Forms : IVocabTevent
   {
      public new static class PropertyUpdated
      {
         public interface EnteredForms : Forms { IReadOnlyList<EnteredForm> EnteredForms { get; } }
      }

      public interface Added : PropertyUpdated.EnteredForms
      {
         string Form { get; }
      }

      public interface Removed : PropertyUpdated.EnteredForms
      {
         string Form { get; }
      }
   }
}

/// <summary>Takes any <see cref="INoteTevent"/>: a vocab also publishes the tevents every note has.</summary>
public interface IVocabTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
