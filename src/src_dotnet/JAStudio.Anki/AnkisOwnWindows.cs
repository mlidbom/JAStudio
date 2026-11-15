using System;
using System.Collections.Generic;
using JAStudio.Anki.AnkisFunctions;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>
/// The spaced repetition system's own windows (<see cref="ISRSWindows"/>) when it's Anki: Anki's reviewer, browser and previewer, which JAStudio<br/>
/// drives by calling Anki (<see cref="IAnkisWindows"/>).
/// </summary>
public sealed class AnkisOwnWindows(IAnkisWindows ankisWindows) : ISRSWindows
{
   readonly IAnkisWindows _ankisWindows = ankisWindows;

   public void Refresh() => _ankisWindows.Refresh();

   /// <summary>Shows <paramref name="message"/> in Anki's tooltip.</summary>
   public void ShowAPassingMessage(string message, TimeSpan shownFor) => _ankisWindows.ShowATooltip(message, shownFor);

   public void OpenTheBrowserOn(IReadOnlyCollection<long> studyNoteIds) => _ankisWindows.OpenTheBrowserOn(TheSearchThatFindsTheAnkiNotesWith(studyNoteIds));

   public void OpenThePreviewerOn(long studyNoteId) => _ankisWindows.OpenTheBrowserAndThePreviewerOn(TheSearchThatFindsTheAnkiNotesWith([studyNoteId]));

   /// <summary>
   /// The search, in Anki's search syntax, that finds the Anki notes with <paramref name="ankiNoteIds"/>, such as <c>nid:1234,5678</c>: a study-note's<br/>
   /// id is that of its Anki note. With no ids the search would be empty, and Anki's browser would show its default search instead of nothing.
   /// </summary>
   static string TheSearchThatFindsTheAnkiNotesWith(IReadOnlyCollection<long> ankiNoteIds) =>
      ankiNoteIds.Count > 0
         ? $"{Builtin.NoteId}:{string.Join(",", ankiNoteIds)}"
         : throw new ArgumentException("There are no Anki notes to search for.", nameof(ankiNoteIds));
}
