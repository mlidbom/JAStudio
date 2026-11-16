using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   /// <summary>The base class of the classes of the <see cref="IVocabTevent.Forms"/> tevents, which <see cref="VocabNoteForms"/> publishes.</summary>
   public abstract class Forms : VocabTevent, IVocabTevent.Forms
   {
      public class Added(string form) : Forms, IVocabTevent.Forms.Added
      {
         public string Form { get; } = form;
      }

      public class Removed(string form) : Forms, IVocabTevent.Forms.Removed
      {
         public string Form { get; } = form;
      }

      public class Owned(string form) : Forms, IVocabTevent.Forms.Owned
      {
         public string Form { get; } = form;
      }

      public class NoLongerOwned(string form) : Forms, IVocabTevent.Forms.NoLongerOwned
      {
         public string Form { get; } = form;
      }

      public class Replaced(IReadOnlyList<EnteredForm> enteredForms) : Forms, IVocabTevent.Forms.Replaced
      {
         public IReadOnlyList<EnteredForm> EnteredForms { get; } = enteredForms;
      }
   }
}
