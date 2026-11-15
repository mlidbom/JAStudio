using System;
using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>The Anki windows of JAStudio running inside Anki: Anki's own, which JAStudio drives through Python (<see cref="AnkiFacade"/>).</summary>
public sealed class AnkisOwnWindows : ISRSWindows
{
   public void Refresh() => AnkiFacade.UIUtils.Refresh();

   /// <summary>Shows <paramref name="message"/> in Anki's tooltip.</summary>
   public void ShowAPassingMessage(string message) => AnkiFacade.UIUtils.ShowTooltip(message);

   public void OpenTheBrowserOn(IReadOnlyCollection<long> studyNoteIds) => AnkiFacade.Browser.ExecuteLookup(TheSearchThatFindsTheAnkiNotesWith(studyNoteIds));

   public void OpenThePreviewerOn(long studyNoteId) => AnkiFacade.Browser.ExecuteLookupAndShowPreviewer(TheSearchThatFindsTheAnkiNotesWith([studyNoteId]));

   /// <summary>
   /// The search, in Anki's search syntax, that finds the Anki notes with <paramref name="ankiNoteIds"/>, such as <c>nid:1234,5678</c>: a study-note's<br/>
   /// id is that of its Anki note. With no ids the search would be empty, and Anki's browser would show its default search instead of nothing.
   /// </summary>
   static string TheSearchThatFindsTheAnkiNotesWith(IReadOnlyCollection<long> ankiNoteIds) =>
      ankiNoteIds.Count > 0
         ? $"{Builtin.NoteId}:{string.Join(",", ankiNoteIds)}"
         : throw new ArgumentException("There are no Anki notes to search for.", nameof(ankiNoteIds));
}
