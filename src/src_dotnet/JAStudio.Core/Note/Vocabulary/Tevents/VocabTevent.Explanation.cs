using JAStudio.Core.Note.NoteFields.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   public class Explanation<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.Explanation<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }
}
