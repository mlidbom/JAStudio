using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Studying;

/// <summary>
/// The changes JAStudio makes to the tags in the SRS, the spaced repetition system, which are the student's: adding a tag to study-notes,<br/>
/// removing one from them, and renaming or deleting a branch of tags on every note there. Each is made there as one operation, which the spaced repetition<br/>
/// system can undo: in Anki, with Edit > Undo. The integration with a spaced repetition system implements it, so that<br/>
/// <see cref="AnkiTagChanges"/>, which a person's changes to the notes' Anki tags go through, needs no Anki.
/// </summary>
/// <remarks>
/// The tags and paths are as the spaced repetition system names them: Anki's are without the <c>anki::</c> that JAStudio shows them below<br/>
/// (<see cref="Tags.Anki"/>).
/// </remarks>
/// <remarks>
/// review-naming:studying: ISRSTagOperations was IAnkiTagOperations (Claude, 2026-11-15). Its members said "every Anki note" where they now say<br/>
/// "every note in the SRS", and its parameters "in Anki" where they now say "in the SRS". What uses it still says Anki, since it names the<br/>
/// branch anki:: that the student sees: AnkiTagChanges, AnkiTags and Tags.Anki.
/// </remarks>
public interface ISRSTagOperations
{
   /// <summary>Adds the tag <paramref name="tagInTheSRS"/> to the study-note of each of <paramref name="notes"/>, each of which has one.</summary>
   void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInTheSRS);

   /// <summary>
   /// Removes exactly the tag <paramref name="tagInTheSRS"/> from the study-note of each of <paramref name="notes"/>, each of which<br/>
   /// has one: the tags below it stay, unlike with Anki's own Remove Tags, which removes them too.
   /// </summary>
   void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInTheSRS);

   /// <summary>
   /// Renames the tag <paramref name="pathInTheSRS"/>, and every tag below it, to<br/>
   /// <paramref name="renamedPathInTheSRS"/> on every note in the spaced repetition system, the study-notes and any other, as<br/>
   /// Anki's own rename does.
   /// </summary>
   void RenameTheBranchOnEveryNoteInTheSRS(string pathInTheSRS, string renamedPathInTheSRS);

   /// <summary>
   /// Removes the tag <paramref name="pathInTheSRS"/>, and every tag below it, from every note in the spaced repetition system,<br/>
   /// the study-notes and any other, as Anki's own delete does.
   /// </summary>
   void DeleteTheBranchFromEveryNoteInTheSRS(string pathInTheSRS);

   /// <summary>
   /// How many notes in the spaced repetition system, the study-notes and any other, have the tag<br/>
   /// <paramref name="pathInTheSRS"/> or a tag below it.
   /// </summary>
   int NumberOfNotesInTheSRSWithATagIn(string pathInTheSRS);
}
