using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Studying;

/// <summary>
/// The changes JAStudio makes to the tags in the spaced repetition system, which are the student's: adding a tag to study-notes, removing one<br/>
/// from them, and renaming or deleting a branch of tags on every note there. Each is made there as one operation, which the spaced repetition<br/>
/// system can undo: in Anki, with Edit > Undo. The integration with a spaced repetition system implements it, so that<br/>
/// <see cref="AnkiTagChanges"/>, which a person's changes to the notes' Anki tags go through, needs no Anki.
/// </summary>
/// <remarks>
/// The tags and paths are as the spaced repetition system names them: Anki's are without the <c>anki::</c> that JAStudio shows them below<br/>
/// (<see cref="Tags.Anki"/>).
/// </remarks>
public interface ISpacedRepetitionSystemTagOperations
{
   //review-naming:studying: ISpacedRepetitionSystemTagOperations was IAnkiTagOperations (Claude, 2026-11-15). Its members said "every Anki note" where
   //   they now say "every note in the spaced repetition system", and its parameters "in Anki" where they now say "in the spaced repetition system".
   //   What uses it still says Anki, since it names the branch anki:: that the student sees: AnkiTagChanges, AnkiTags and Tags.Anki.
   /// <summary>Adds the tag <paramref name="tagInTheSpacedRepetitionSystem"/> to the study-note of each of <paramref name="notes"/>, each of which has one.</summary>
   void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInTheSpacedRepetitionSystem);

   /// <summary>
   /// Removes exactly the tag <paramref name="tagInTheSpacedRepetitionSystem"/> from the study-note of each of <paramref name="notes"/>, each of which<br/>
   /// has one: the tags below it stay, unlike with Anki's own Remove Tags, which removes them too.
   /// </summary>
   void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInTheSpacedRepetitionSystem);

   /// <summary>
   /// Renames the tag <paramref name="pathInTheSpacedRepetitionSystem"/>, and every tag below it, to<br/>
   /// <paramref name="renamedPathInTheSpacedRepetitionSystem"/> on every note in the spaced repetition system, the study-notes and any other, as<br/>
   /// Anki's own rename does.
   /// </summary>
   void RenameTheBranchOnEveryNoteInTheSpacedRepetitionSystem(string pathInTheSpacedRepetitionSystem, string renamedPathInTheSpacedRepetitionSystem);

   /// <summary>
   /// Removes the tag <paramref name="pathInTheSpacedRepetitionSystem"/>, and every tag below it, from every note in the spaced repetition system,<br/>
   /// the study-notes and any other, as Anki's own delete does.
   /// </summary>
   void DeleteTheBranchFromEveryNoteInTheSpacedRepetitionSystem(string pathInTheSpacedRepetitionSystem);

   /// <summary>
   /// How many notes in the spaced repetition system, the study-notes and any other, have the tag<br/>
   /// <paramref name="pathInTheSpacedRepetitionSystem"/> or a tag below it.
   /// </summary>
   int NumberOfNotesInTheSpacedRepetitionSystemWithATagIn(string pathInTheSpacedRepetitionSystem);
}
