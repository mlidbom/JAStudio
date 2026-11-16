using JAStudio.Core.Note.NoteFields.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   public class CorpusAnswer<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.CorpusAnswer<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }
}
