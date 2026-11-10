using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.PythonInterop;
using JAStudio.PythonInterop.Utilities;

namespace JAStudio.Anki;

/// <summary>
/// Facade for calling Anki-specific functionality that cannot be implemented in pure C#.
/// Uses Python.NET to invoke Python code in the Anki environment.
/// 
/// IMPORTANT: This should only contain operations that REQUIRE Anki's Python API.
/// Anything that we can implement in C# and Avalonia should be.
/// 
/// All calls go through anki_facade_backend.py, and are safe to call from any .NET thread, the Avalonia UI thread included:
/// the functions that drive Anki's windows run their work on Qt's main thread themselves, and the others call Anki's collection
/// on the calling thread, which Anki's backend allows from any thread.
/// </summary>
public static class AnkiFacade
{
   static readonly PythonObjectWrapper Backend = PythonEnvironment.Import("jastudio.dotnet.anki_facade_backend");

   public static class Browser
   {
      /// <summary>Execute an Anki browser search query. </summary>
      public static void ExecuteLookup(string query) => Backend.Use(it => it.browser_execute_lookup(query));

      /// <summary>Execute an Anki browser search query and show the previewer. </summary>
      public static void ExecuteLookupAndShowPreviewer(string query) => Backend.Use(it => it.browser_execute_lookup_and_show_previewer(query));

      public static class MenuActions
      {
         /// <summary>Prioritize selected cards (sets card.due based on note type priority).</summary>
         public static void PrioritizeCards(IReadOnlyList<long> cardIds) => Backend.Use(it => it.browser_prioritize_cards(PythonDotNetShim.LongList.ToPython(cardIds)));

         /// <summary>Spread selected cards over days (distributes due dates across time range).</summary>
         public static void SpreadCardsOverDays(IReadOnlyList<long> cardIds, int startDay, int daysApart) => Backend.Use(it => it.browser_spread_cards_over_days(PythonDotNetShim.LongList.ToPython(cardIds), startDay, daysApart));
      }
   }

   public static class UIUtils
   {
      /// <summary>Show a tooltip message in Anki. </summary>
      public static void ShowTooltip(string message, int periodMs = 3000) => Backend.Use(it => it.ui_show_tooltip(message, periodMs));

      /// <summary>Refresh the currently displayed views in Anki. By it's nature it cannot be ported. </summary>
      public static void Refresh() => Backend.Use(it => it.ui_refresh());
   }

   /// <summary>Notes of any note type, for what only Anki does with them: its search, and tagging and deleting them. JAStudio reads notes from Anki's database (<see cref="Col.ReadJson"/>).</summary>
   internal static class Notes
   {
      /// <summary>The ids of the notes that the Anki search <paramref name="search"/> finds.</summary>
      public static List<long> Find(string search) => Backend.Use(it => PythonDotNetShim.LongList.ToDotNet(it.notes_find(search)));

      /// <summary>Gives the notes with the ids <paramref name="noteIds"/> the tag <paramref name="tag"/>.</summary>
      public static void AddTag(IReadOnlyList<long> noteIds, string tag) => Backend.Use(it => it.notes_add_tag(PythonDotNetShim.LongList.ToPython(noteIds), tag));

      /// <summary>Permanently deletes the notes with the ids <paramref name="noteIds"/> from Anki.</summary>
      public static void Delete(IReadOnlyList<long> noteIds) => Backend.Use(it => it.notes_remove(PythonDotNetShim.LongList.ToPython(noteIds)));
   }

   public static class Media
   {
      /// <summary>
      /// Copies the file at <paramref name="filePath"/> into Anki's media folder, so that Anki plays it and its media sync takes it to other devices,<br/>
      /// and returns the name Anki keeps it under. That's the file's own name, unless Anki already has a different file by that name.
      /// </summary>
      public static string AddFile(string filePath) => Backend.Use(it => (string)it.media_add_file(filePath));
   }

   /// <summary>
   /// Anki's tags, changed as Anki operations: Anki's Edit > Undo undoes each, and Anki is told of each as of any operation, so its windows show it.<br/>
   /// The tags and paths are as Anki names them. Each call returns once Anki has made the change.
   /// </summary>
   internal static class Tags
   {
      /// <summary>Adds the tag <paramref name="tag"/> to the Anki notes <paramref name="ankiNoteIds"/>.</summary>
      public static void AddToNotes(IReadOnlyList<long> ankiNoteIds, string tag) => Backend.Use(it => it.tags_add_to_notes(PythonDotNetShim.LongList.ToPython(ankiNoteIds), tag));

      /// <summary>Removes the tag <paramref name="tag"/> from the Anki notes <paramref name="ankiNoteIds"/>.</summary>
      public static void RemoveFromNotes(IReadOnlyList<long> ankiNoteIds, string tag) => Backend.Use(it => it.tags_remove_from_notes(PythonDotNetShim.LongList.ToPython(ankiNoteIds), tag));

      /// <summary>Renames the tag <paramref name="path"/>, and every tag below it, to <paramref name="renamedPath"/> on every Anki note.</summary>
      public static void Rename(string path, string renamedPath) => Backend.Use(it => it.tags_rename(path, renamedPath));

      /// <summary>Removes the tag <paramref name="path"/>, and every tag below it, from every Anki note.</summary>
      public static void Delete(string path) => Backend.Use(it => it.tags_delete(path));

      /// <summary>How many Anki notes have the tag <paramref name="path"/> or a tag below it, as Anki's search for the tag finds them.</summary>
      public static int NumberOfNotesWithATagIn(string path) => Backend.Use(it => (int)it.tags_number_of_notes_with_a_tag_in(path));
   }

   /// <summary>The study-notes: the Anki notes of JAStudio's note types, which JAStudio writes from the corpus.</summary>
   internal static class StudyNotes
   {
      /// <summary>Writes <paramref name="note"/>'s study-note into the Anki note with the id <paramref name="ankiNoteId"/>, as JAStudio writes it whenever it updates the note.</summary>
      public static void Write(long ankiNoteId, JPNote note) => Backend.Use(it => it.study_note_write(ankiNoteId, note));
   }

   internal static class NoteEx
   {
      /// <summary>Suspend all cards for the given note ID.</summary>
      public static void SuspendAllCardsForNote(long noteId) => Backend.Use(it => it.note_suspend_all_cards(noteId));

      /// <summary>Unsuspend all cards for the given note ID.</summary>
      public static void UnsuspendAllCardsForNote(long noteId) => Backend.Use(it => it.note_unsuspend_all_cards(noteId));

      /// <summary>Permanently delete the note from Anki.</summary>
      public static void RemoveNote(long noteId) => Backend.Use(it => it.note_remove(noteId));
   }

   internal static class Col
   {
      /// <summary>
      /// The one value the SELECT <paramref name="select"/> returns, a JSON text, from the database of the collection Anki has open, through<br/>
      /// Anki's own connection to it (<see cref="CollectionDatabase.AnkisOwnConnection"/>).
      /// </summary>
      public static string ReadJson(string select) => Backend.Use(it => (string)it.col_read_json(select));
   }

   /// <summary>Get note ID from card ID (requires Anki API).</summary>
   public static long GetNoteIdFromCardId(long cardId) => Backend.Use(it => (long)it.get_note_id_from_card_id(cardId));

   /// <summary>Get the addon root directory from the Python environment.</summary>
   public static string GetAddonRootDir() => Backend.Use(it => (string)it.addon_root_dir());

   /// <summary>Get the Anki media directory from the Python environment.</summary>
   public static string GetAnkiMediaDir() => Backend.Use(it => (string)it.anki_media_dir());
}
