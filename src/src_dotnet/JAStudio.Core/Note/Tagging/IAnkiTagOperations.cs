using System.Collections.Generic;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// The changes JAStudio makes to Anki's tags, each made in Anki as one Anki operation, so Anki's Edit > Undo undoes it. The integration layer<br/>
/// implements it, so that <see cref="AnkiTagChanges"/>, which a person's changes to the notes' Anki tags go through, needs no Anki.<br/>
/// The tags and paths are as Anki names them, without the <c>anki::</c> that JAStudio shows them below (<see cref="Tags.Anki"/>).
/// </summary>
public interface IAnkiTagOperations
{
   /// <summary>Adds the tag <paramref name="tagInAnki"/> to the study-note of each of <paramref name="notes"/>, each of which has one.</summary>
   void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki);

   /// <summary>
   /// Removes exactly the tag <paramref name="tagInAnki"/> from the study-note of each of <paramref name="notes"/>, each of which has one: the tags<br/>
   /// below it stay, unlike with Anki's own Remove Tags, which removes them too.
   /// </summary>
   void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki);

   /// <summary>Renames the tag <paramref name="pathInAnki"/>, and every tag below it, to <paramref name="renamedPathInAnki"/> on every Anki note, as Anki's own rename does.</summary>
   void RenameTheBranchOnEveryAnkiNote(string pathInAnki, string renamedPathInAnki);

   /// <summary>Removes the tag <paramref name="pathInAnki"/>, and every tag below it, from every Anki note, as Anki's own delete does.</summary>
   void DeleteTheBranchFromEveryAnkiNote(string pathInAnki);

   /// <summary>How many Anki notes, of any note type, have the tag <paramref name="pathInAnki"/> or a tag below it.</summary>
   int NumberOfAnkiNotesWithATagIn(string pathInAnki);
}
