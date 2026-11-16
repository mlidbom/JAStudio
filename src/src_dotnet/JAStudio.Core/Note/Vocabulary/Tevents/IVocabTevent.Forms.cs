using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial interface IVocabTevent
{
   /// <summary>
   /// The vocab's entered forms changed (<see cref="VocabNoteForms"/>): the texts besides its question that, found in a sentence, are this vocab,<br/>
   /// each owned or not. What the vocab matches in sentences changes with them, so what was parsed with the old forms may now parse otherwise.
   /// </summary>
   public interface Forms : IVocabTevent
   {
      /// <summary><see cref="Form"/> became one of the entered forms, not owned.</summary>
      /// <remarks>The vocab whose question is <see cref="Form"/> get this vocab's question among their entered forms in turn.</remarks>
      public interface Added : Forms
      {
         string Form { get; }
      }

      /// <summary><see cref="Form"/> is no longer one of the entered forms.</summary>
      /// <remarks>The vocab whose question is <see cref="Form"/> lose this vocab's question from their entered forms in turn.</remarks>
      public interface Removed : Forms
      {
         string Form { get; }
      }

      /// <summary>The vocab came to own its entered form <see cref="Form"/>, and so wins over the vocab that only match it.</summary>
      public interface Owned : Forms
      {
         string Form { get; }
      }

      /// <summary>The vocab no longer owns its entered form <see cref="Form"/>, and only matches it.</summary>
      public interface NoLongerOwned : Forms
      {
         string Form { get; }
      }

      /// <summary>The entered forms were replaced as a whole by <see cref="EnteredForms"/>, in their order: as a person edits them.</summary>
      public interface Replaced : Forms
      {
         IReadOnlyList<EnteredForm> EnteredForms { get; }
      }
   }
}
