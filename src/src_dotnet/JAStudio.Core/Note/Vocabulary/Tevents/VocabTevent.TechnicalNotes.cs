using JAStudio.Core.Note.NoteFields.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   public class TechnicalNotes<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.TechnicalNotes<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }
}
