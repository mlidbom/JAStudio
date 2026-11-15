using System.Collections.Generic;

namespace JAStudio.Anki.AnkisFunctions;

/// <summary>
/// The tags in the collection Anki has open, changed as Anki operations: one group of the functions JAStudio calls in Anki (see<br/>
/// <see cref="IAnkisNotes"/> for the groups). Anki's Edit > Undo undoes each change, and Anki tells its windows of each, as of any operation.<br/>
/// The tags and paths are as Anki names them, and each call returns once Anki has made the change.
/// </summary>
public interface IAnkisTags
{
   /// <summary>Adds the tag <paramref name="tag"/> to the notes <paramref name="noteIds"/>. The add-on's <c>tags_add_to_notes</c>.</summary>
   void AddToNotes(IReadOnlyList<long> noteIds, string tag);

   /// <summary>
   /// Removes exactly the tag <paramref name="tag"/> from the notes <paramref name="noteIds"/>, and leaves the tags below it. The add-on's<br/>
   /// <c>tags_remove_from_notes</c>.
   /// </summary>
   void RemoveFromNotes(IReadOnlyList<long> noteIds, string tag);

   /// <summary>Renames the tag <paramref name="path"/>, and every tag below it, to <paramref name="renamedPath"/> on every note. The add-on's <c>tags_rename</c>.</summary>
   void Rename(string path, string renamedPath);

   /// <summary>Removes the tag <paramref name="path"/>, and every tag below it, from every note. The add-on's <c>tags_delete</c>.</summary>
   void Delete(string path);

   /// <summary>
   /// How many notes have the tag <paramref name="path"/> or a tag below it, as Anki's search for the tag finds them. The add-on's<br/>
   /// <c>tags_number_of_notes_with_a_tag_in</c>.
   /// </summary>
   int NumberOfNotesWithATagIn(string path);
}
