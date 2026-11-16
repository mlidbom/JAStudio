using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   public static class Forms
   {
      public class Added(string form) : VocabTevent, IVocabTevent.Forms.Added
      {
         public string Form { get; } = form;
      }

      public class Removed(string form) : VocabTevent, IVocabTevent.Forms.Removed
      {
         public string Form { get; } = form;
      }

      public class Owned(string form) : VocabTevent, IVocabTevent.Forms.Owned
      {
         public string Form { get; } = form;
      }

      public class NoLongerOwned(string form) : VocabTevent, IVocabTevent.Forms.NoLongerOwned
      {
         public string Form { get; } = form;
      }

      public class Replaced(IReadOnlyList<EnteredForm> enteredForms) : VocabTevent, IVocabTevent.Forms.Replaced
      {
         public IReadOnlyList<EnteredForm> EnteredForms { get; } = enteredForms;
      }
   }
}
