namespace JAStudio.Core.Note.Search.NoteTypeProperties;

/// <summary>Whether a note is a vocab, a kanji or a sentence, as a property a search can have conditions on.</summary>
public sealed class NoteTypeProperty : NoteProperty
{
   internal NoteTypeProperty() : base("Note type") {}

   /// <summary>A condition that the note is a <typeparamref name="TNote"/>, such as a <see cref="Sentences.SentenceNote"/>.</summary>
   public NoteTypeCondition Is<TNote>() where TNote : JPNote => new(this, typeof(TNote));
}
