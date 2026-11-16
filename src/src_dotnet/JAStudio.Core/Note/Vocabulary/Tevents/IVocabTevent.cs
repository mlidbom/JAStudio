using System.Collections.Generic;
using Compze.Tessaging.TessageTypes;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.NoteFields.Tevents;
using JAStudio.Core.Note.Tevents;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Note.Vocabulary.Tevents;

/// <summary>
/// A part's tevent appliers apply only its <c>PropertyUpdated</c> tevents. A domain tevent, such as <see cref="Forms.Added"/>, is also the<br/>
/// <c>PropertyUpdated</c> tevent it causes, and exists only where something reacts to more than the new value.
/// </summary>
public interface IVocabTevent : INoteTevent
{
   public interface Created : IVocabTevent, ITaggregateCreatedTevent, Question.PropertyUpdated.Text;

   public interface Question : IVocabTevent
   {
      public static class PropertyUpdated
      {
         public interface Text : Question
         {
            string Text { get; }
         }

         public interface Disambiguation : Question
         {
            string Disambiguation { get; }
         }
      }

      /// <remarks>Carries the old name, which nothing else has inside the unit of work: the vocab read there is its own copy, already renamed.</remarks>
      public interface Renamed : PropertyUpdated.Text, PropertyUpdated.Disambiguation
      {
         WordReference From { get; }
      }
   }

   public interface Forms : IVocabTevent
   {
      public static class PropertyUpdated
      {
         public interface EnteredForms : Forms
         {
            IReadOnlyList<EnteredForm> EnteredForms { get; }
         }
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

   public interface CorpusAnswer<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;
   public interface Explanation<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;
   public interface ExplanationLong<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;
   public interface Mnemonic<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;
   public interface TechnicalNotes<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;
   public interface References<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;
}

/// <summary>Takes any <see cref="INoteTevent"/>: a vocab also publishes the tevents every note has.</summary>
public interface IVocabTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
