using System.Collections.Generic;
using JAStudio.Core.Note.Tevents;

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

   public class Created(VocabId vocabId, string question) : VocabTevent(vocabId), IVocabTevent.Created
   {
      public string Text { get; } = question;
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
