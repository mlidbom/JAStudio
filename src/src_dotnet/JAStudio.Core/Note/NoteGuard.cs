using System;

namespace JAStudio.Core.Note;

/// <summary>
/// What a part of a note changes the note's state through: the change fails before it is made when the note may not be changed, such as a note<br/>
/// a taggregate store serves to readers, and once it is made, the note is flushed: saved, and its interim tevent published, when its stored data changed.
/// </summary>
public class NoteGuard
{
   readonly Action _assertCanBeChanged;
   readonly Action _flush;

   public NoteGuard(Action assertCanBeChanged, Action flush)
   {
      _assertCanBeChanged = assertCanBeChanged;
      _flush = flush;
   }

   public void Update(Action mutation)
   {
      _assertCanBeChanged();
      mutation();
      _flush();
   }
}
