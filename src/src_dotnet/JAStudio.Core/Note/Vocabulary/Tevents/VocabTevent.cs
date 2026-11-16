using System.Collections.Generic;
using JAStudio.Core.Note.NoteFields.Tevents;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public abstract class VocabTevent : NoteTevent, IVocabTevent
{
   protected VocabTevent() {}

   VocabTevent(VocabId vocabId) : base(vocabId) {}

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

      public class Added(string form, IReadOnlyList<EnteredForm> enteredForms) : Forms, IVocabTevent.Forms.Added
      {
         public string Form { get; } = form;
         public IReadOnlyList<EnteredForm> EnteredForms { get; } = enteredForms;
      }

      public class Removed(string form, IReadOnlyList<EnteredForm> enteredForms) : Forms, IVocabTevent.Forms.Removed
      {
         public string Form { get; } = form;
         public IReadOnlyList<EnteredForm> EnteredForms { get; } = enteredForms;
      }
   }

   public class CorpusAnswer<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.CorpusAnswer<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }

   public class Explanation<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.Explanation<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }

   public class ExplanationLong<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.ExplanationLong<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }

   public class Mnemonic<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.Mnemonic<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }

   public class TechnicalNotes<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.TechnicalNotes<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }

   public class References<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.References<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }
}

public class VocabTevent<TTevent>(TTevent tevent) : NoteTevent<TTevent>(tevent), IVocabTevent<TTevent> where TTevent : INoteTevent;
