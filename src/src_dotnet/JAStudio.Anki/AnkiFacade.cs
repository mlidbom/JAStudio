using System.Collections.Generic;
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
/// All calls go through anki_facade_backend.py which handles Qt thread marshaling,
/// making it safe to call from any .NET thread (including the Avalonia UI thread).
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

   /// <summary>Notes of any note type, for what only Anki does with them: its search, and tagging and deleting them. JAStudio reads notes straight from Anki's database.</summary>
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
      /// Returns the path to Anki's live collection database, after checkpointing the WAL.
      /// Anki uses WAL journaling, so without a checkpoint, recent writes (e.g. fields populated by TTS addons)
      /// remain in the .anki2-wal sidecar and are invisible to external read-only connections opened with immutable=1.
      /// Every code path that reads Anki's DB starts by getting the path here, so a checkpoint here guarantees freshness.
      /// </summary>
      public static string? DbFilePath()
      {
         CheckpointWal();
         return Backend.Use(it => (string?)it.col_db_file_path());
      }

      /// <summary>Force Anki to checkpoint its SQLite WAL into the main DB file.</summary>
      public static void CheckpointWal() => Backend.Use(it => it.col_checkpoint_wal());
   }

   /// <summary>Get note ID from card ID (requires Anki API).</summary>
   public static long GetNoteIdFromCardId(long cardId) => Backend.Use(it => (long)it.get_note_id_from_card_id(cardId));

   /// <summary>Get the addon root directory from the Python environment.</summary>
   public static string GetAddonRootDir() => Backend.Use(it => (string)it.addon_root_dir());

   /// <summary>Get the Anki media directory from the Python environment.</summary>
   public static string GetAnkiMediaDir() => Backend.Use(it => (string)it.anki_media_dir());
}
