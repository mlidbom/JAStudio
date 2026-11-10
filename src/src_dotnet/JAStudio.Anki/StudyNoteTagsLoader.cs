using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using LinqToDB;

namespace JAStudio.Anki;

/// <summary>
/// Reads the tags of each study-note, each Anki note of JAStudio's note types, straight from Anki's database: the student's tags, which JAStudio<br/>
/// shows as each note's Anki tags (<see cref="Core.Note.Tagging.AnkiTags"/>) and never writes.
/// </summary>
public static class StudyNoteTagsLoader
{
   /// <summary>The tags of every study-note in the collection whose database is <paramref name="dbFilePath"/>, by its Anki note id. Opens and closes its own connection.</summary>
   public static Dictionary<long, IReadOnlyList<string>> Load(string dbFilePath)
   {
      using var db = AnkiDb.OpenReadOnly(dbFilePath);
      var noteTypeIdsOfStudyNotes = db.NoteTypes.ToList().Where(it => NoteTypes.All.Contains(it.Name)).Select(it => it.Id).ToList();

      return db.Notes.Where(it => noteTypeIdsOfStudyNotes.Contains(it.NoteTypeId))
               .Select(it => new { it.Id, it.Tags })
               .ToList()
               .ToDictionary(it => it.Id, IReadOnlyList<string> (it) => it.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries));
   }
}
