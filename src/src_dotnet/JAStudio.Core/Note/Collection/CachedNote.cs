namespace JAStudio.Core.Note.Collection;

/// <summary>
/// What a note cache keeps of a note as it was cached: the values its indexes file the note under, so that when the note changes, the cache<br/>
/// knows where it was filed.
/// </summary>
class CachedNote
{
   public NoteId Id { get; }
   public string Question { get; }

   public CachedNote(JPNote note)
   {
      Id = note.GetId();
      Question = note.GetQuestion();
   }
}
