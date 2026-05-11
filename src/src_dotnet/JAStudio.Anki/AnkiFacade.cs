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

   public static class Batches
   {
      /// <summary>This is about converting between Anki note types. There's no way to avoid interfacing with anki for that. </summary>
      public static void ConvertImmersionKitSentences() => Backend.Use(it => it.batches_convert_immersion_kit_sentences());

      /// <summary>Loads an Anki note by external ID and calls update_note to flush it, triggering the ExternalNoteWillFlush merge path.</summary>
      public static void FlushAnkiNote(long externalNoteId) => Backend.Use(it => it.batches_flush_anki_note(externalNoteId));
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
      public static string? DbFilePath() => Backend.Use(it => (string)it.col_db_file_path());
   }

   /// <summary>Get note ID from card ID (requires Anki API).</summary>
   public static long GetNoteIdFromCardId(long cardId) => Backend.Use(it => (long)it.get_note_id_from_card_id(cardId));

   /// <summary>Get the addon root directory from the Python environment.</summary>
   public static string GetAddonRootDir() => Backend.Use(it => (string)it.addon_root_dir());

   /// <summary>Get the Anki media directory from the Python environment.</summary>
   public static string GetAnkiMediaDir() => Backend.Use(it => (string)it.anki_media_dir());
}
