using JAStudio.Core.Note.NoteFields.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   public class Mnemonic<TTevent>(TTevent tevent) : VocabTevent, IVocabTevent.Mnemonic<TTevent> where TTevent : IStringValueTevent
   {
      public TTevent Tevent { get; } = tevent;
   }
}
