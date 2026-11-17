namespace JAStudio.Core.Note.Tevents;

/// <summary>What every <see cref="INoteTevent"/> tells of the note that published it.</summary>
public static class NoteTeventExtensions
{
   extension(INoteTevent tevent)
   {
      /// <summary>The id of the note that published the tevent: its taggregate id, which a note's always is (<see cref="JPNote.Id"/>).</summary>
      public NoteId NoteId => (NoteId)tevent.TaggregateId;
   }
}
