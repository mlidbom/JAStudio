using System.Collections.Generic;
using Compze.Tessaging.TessageTypes;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.NoteFields.Tevents;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

/// <summary>
/// Something that happened to a vocab (<see cref="VocabNote"/>): the root of a vocab's tevents. Each part of a vocab that changes has a group<br/>
/// below it, such as <see cref="Forms"/>, and each group has its <c>PropertyUpdated</c> tevents: each says which property of the part got<br/>
/// which value, and the part's tevent appliers apply only those. A domain tevent says what happened beyond the value, for those who react to that,<br/>
/// such as <see cref="Forms.Added"/>, and is also the <c>PropertyUpdated</c> tevent it causes.
/// </summary>
/// <remarks>
/// A domain tevent is added only where something reacts to more than the new value: this domain is mostly edits of values. The tevents aren't<br/>
/// stored, so one can be added the day something needs it.<br/>
/// rationale:violation-approved:Magnus-2026-11-16: the whole nested model of a vocab's tevents in this one file, not one file for each nested type,<br/>
/// since reading what can happen to a taggregate in one place is what matters here.
/// </remarks>
public interface IVocabTevent : INoteTevent
{
   /// <summary>
   /// A new vocab was created, with <see cref="IVocabTevent.Question.PropertyUpdated.Text.Text"/> as its question. Never published for a vocab<br/>
   /// loaded from the corpus: a vocab is loaded from its stored data, and publishes nothing.
   /// </summary>
   public interface Created : IVocabTevent, ITaggregateCreatedTevent, Question.PropertyUpdated.Text;

   /// <summary>Something happened to the vocab's question (<see cref="VocabNoteQuestion"/>): the text that names the vocab, and its disambiguation.</summary>
   public interface Question : IVocabTevent
   {
      /// <summary>Which property of the question got which value: what the question's tevent appliers apply.</summary>
      public static class PropertyUpdated
      {
         /// <summary>The question's text became <see cref="Text"/>.</summary>
         public interface Text : Question
         {
            string Text { get; }
         }

         /// <summary>The question's disambiguation, which tells it apart from other vocab with the same text, became <see cref="Disambiguation"/>: empty when it needs none.</summary>
         public interface Disambiguation : Question
         {
            string Disambiguation { get; }
         }
      }

      /// <summary>
      /// The vocab was renamed from <see cref="From"/>: its question's text or disambiguation changed. What refers to the vocab by its old name, such<br/>
      /// as the word-references in other notes, is renamed with it.
      /// </summary>
      /// <remarks>The old name is in the tevent because nothing else has it once the vocab is renamed: a unit of work reads its own copy of the vocab.</remarks>
      public interface Renamed : PropertyUpdated.Text, PropertyUpdated.Disambiguation
      {
         WordReference From { get; }
      }
   }

   /// <summary>
   /// Something happened to the vocab's entered forms (<see cref="VocabNoteForms"/>): the texts besides its question that, found in a sentence,<br/>
   /// are this vocab, each owned or not. What the vocab matches in sentences changes with them.
   /// </summary>
   public interface Forms : IVocabTevent
   {
      /// <summary>Which property of the forms got which value: what the forms' tevent appliers apply.</summary>
      public static class PropertyUpdated
      {
         /// <summary>The entered forms became <see cref="EnteredForms"/>, in their order.</summary>
         public interface EnteredForms : Forms
         {
            IReadOnlyList<EnteredForm> EnteredForms { get; }
         }
      }

      /// <summary><see cref="Form"/> became one of the entered forms: the vocab whose question it is get this vocab's question among theirs in turn.</summary>
      public interface Added : PropertyUpdated.EnteredForms
      {
         string Form { get; }
      }

      /// <summary><see cref="Form"/> is no longer one of the entered forms: the vocab whose question it is lose this vocab's question from theirs in turn.</summary>
      public interface Removed : PropertyUpdated.EnteredForms
      {
         string Form { get; }
      }
   }

   /// <summary>
   /// Something happened to the corpus's own answer for the vocab (<see cref="VocabNote.CorpusAnswer"/>): the field's own <typeparamref name="TTevent"/>,<br/>
   /// such as <see cref="IStringValueTevent.PropertyUpdated.Value"/>, adopted into the vocab's tevents by the field's place in the vocab.<br/>
   /// The vocab's other text fields are the same, each by its own name.
   /// </summary>
   public interface CorpusAnswer<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;

   /// <summary>Something happened to the corpus's explanation of the vocab (<see cref="VocabNote.Explanation"/>): see <see cref="IVocabTevent.CorpusAnswer{TTevent}"/>.</summary>
   public interface Explanation<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;

   /// <summary>Something happened to the corpus's longer explanation of the vocab (<see cref="VocabNote.ExplanationLong"/>): see <see cref="IVocabTevent.CorpusAnswer{TTevent}"/>.</summary>
   public interface ExplanationLong<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;

   /// <summary>Something happened to the corpus's own mnemonic for the vocab (<see cref="VocabNote.Mnemonic"/>): see <see cref="IVocabTevent.CorpusAnswer{TTevent}"/>.</summary>
   public interface Mnemonic<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;

   /// <summary>Something happened to the vocab's technical notes (<see cref="VocabNote.TechnicalNotes"/>): see <see cref="IVocabTevent.CorpusAnswer{TTevent}"/>.</summary>
   public interface TechnicalNotes<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;

   /// <summary>Something happened to the vocab's references (<see cref="VocabNote.References"/>): see <see cref="IVocabTevent.CorpusAnswer{TTevent}"/>.</summary>
   public interface References<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;
}

/// <summary>
/// A tevent as a vocab published it: see <see cref="INoteTevent{TTevent}"/>. Any <see cref="INoteTevent"/>, since a vocab also publishes the tevents<br/>
/// every note has.
/// </summary>
public interface IVocabTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
