using Compze.Tessaging.TessageTypes;
using JAStudio.Core.Note.NoteFields.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial interface IVocabTevent
{
   /// <summary>
   /// A change to the corpus's explanation of the vocab (<see cref="VocabNote.Explanation"/>), such as <c>IVocabTevent.Explanation&lt;IStringValueTevent.Set&gt;</c>: the field's own<br/>
   /// <typeparamref name="TTevent"/>, adopted into the vocab's tevents by the field's place in the vocab.
   /// </summary>
   public interface Explanation<out TTevent> : IVocabTevent, IPublisherTevent<TTevent> where TTevent : IStringValueTevent;
}
