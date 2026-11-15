using System.Collections.Generic;

namespace JAStudio.Anki.AnkisFunctions;

/// <summary>
/// The notes in the collection Anki has open, of any note type, with their cards: one group of the functions JAStudio calls in Anki. The others<br/>
/// are <see cref="IAnkisTags"/>, <see cref="IAnkisMedia"/>, <see cref="IAnkisDatabase"/> and <see cref="IAnkisWindows"/>. A note is named by Anki's id of it.
/// </summary>
/// <remarks>
/// Each group has one implementation for each way JAStudio reaches Anki: through pythonnet while JAStudio runs inside Anki<br/>
/// (<see cref="ThroughPythonnet.AnkiThroughPythonnet"/>), and over HTTP once JAStudio is a program of its own<br/>
/// (<c>dev_docs/moving-jastudio-out-of-anki.md</c>). Each member is one function of the Anki add-on, which its doc comment names.
/// </remarks>
/// <remarks>
/// review-naming:studying: the namespace AnkisFunctions and its five interfaces are Claude's (2026-11-15). Until then the classes of the Anki<br/>
/// integration called the static AnkiFacade.
/// </remarks>
public interface IAnkisNotes
{
   /// <summary>The ids of the notes that the Anki search <paramref name="search"/> finds. The add-on's <c>notes_find</c>.</summary>
   List<long> Find(string search);

   /// <summary>Gives the notes <paramref name="noteIds"/> the tag <paramref name="tag"/>. The add-on's <c>notes_add_tag</c>.</summary>
   void AddTag(IReadOnlyList<long> noteIds, string tag);

   /// <summary>Permanently deletes the notes <paramref name="noteIds"/> from Anki. The add-on's <c>notes_remove</c>.</summary>
   void Delete(IReadOnlyList<long> noteIds);

   /// <summary>Permanently deletes the note <paramref name="noteId"/> from Anki, with its cards and their review history. The add-on's <c>note_remove</c>.</summary>
   void Delete(long noteId);

   /// <summary>
   /// Writes <paramref name="fields"/>, by their names in Anki, into the note <paramref name="noteId"/>: the note's other fields, and its tags, stay as<br/>
   /// they are. The add-on's <c>study_note_write</c>, by which JAStudio writes a note's study-note.
   /// </summary>
   void WriteTheFields(long noteId, IReadOnlyDictionary<string, string> fields);

   /// <summary>Suspends every card of the note <paramref name="noteId"/>. The add-on's <c>note_suspend_all_cards</c>.</summary>
   void SuspendAllCardsOf(long noteId);

   /// <summary>Unsuspends every card of the note <paramref name="noteId"/>. The add-on's <c>note_unsuspend_all_cards</c>.</summary>
   void UnsuspendAllCardsOf(long noteId);
}
