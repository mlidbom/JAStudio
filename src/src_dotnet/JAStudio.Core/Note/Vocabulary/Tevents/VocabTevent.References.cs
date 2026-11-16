using JAStudio.Core.Note.NoteFields.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   public class References<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.References<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }
}
