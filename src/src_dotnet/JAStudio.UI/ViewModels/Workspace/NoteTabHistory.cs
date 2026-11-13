using System;
using System.Collections.Generic;
using Compze.Contracts;
using JAStudio.Core.Note;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// The notes a workspace tab (<see cref="NoteTabViewModel"/>) has shown, in order, and the one it shows now: what its back and forward go through,<br/>
/// as a browser tab's history does. Showing another note, as following a link does, drops the notes ahead of the shown one, and adds it after it.
/// </summary>
/// <remarks>
/// The tab keeps the history, rather than the web view that shows a note's page, so that it can show one note's page in Avalonia and the next one's<br/>
/// in the web view, and go back and forward between them. See <c>dev_docs/jastudio-workspace.md</c>, "Technology".
/// </remarks>
/// <remarks>
/// It holds the notes' ids, as views do, since the collection replaces the note objects when it's reloaded. Every note in it is in the collection: the<br/>
/// tab forgets a note that leaves the collection, and closes when it's the one it shows.
/// </remarks>
sealed class NoteTabHistory
{
   readonly List<NoteId> _notes;
   int _shownIndex;

   public NoteTabHistory(NoteId firstNote) => _notes = [firstNote];

   /// <summary>The note the tab shows now.</summary>
   public NoteId Shown => _notes[_shownIndex];

   public bool CanGoBack => _shownIndex > 0;

   public bool CanGoForward => _shownIndex < _notes.Count - 1;

   public void GoBack()
   {
      Contract.State.Assert(CanGoBack, () => "There is no note before the one shown.");
      _shownIndex--;
   }

   public void GoForward()
   {
      Contract.State.Assert(CanGoForward, () => "There is no note after the one shown.");
      _shownIndex++;
   }

   /// <summary>
   /// Shows <paramref name="note"/> after the one shown now, as following a link does: the notes ahead of the shown one are dropped. Showing the note<br/>
   /// already shown changes nothing.
   /// </summary>
   public void Show(NoteId note)
   {
      if(note.Equals(Shown)) return;

      _notes.RemoveRange(_shownIndex + 1, _notes.Count - _shownIndex - 1);
      _notes.Add(note);
      _shownIndex++;
   }

   /// <summary>
   /// Forgets the notes that <paramref name="leftTheCollection"/>, such as a note deleted, so that going back or forward never reaches one. Where that<br/>
   /// leaves the same note twice in a row, it's kept once, as going back over a forgotten note would otherwise show the same note again.
   /// </summary>
   /// <param name="leftTheCollection">Whether a note is no longer in the collection. Never the note shown: the tab closes instead.</param>
   public void Forget(Func<NoteId, bool> leftTheCollection)
   {
      Contract.State.Assert(!leftTheCollection(Shown), () => "The tab shows the note to forget: it closes instead.");

      var kept = new List<NoteId>();
      var shownIndexInKept = 0;
      for(var index = 0; index < _notes.Count; index++)
      {
         var note = _notes[index];
         if(leftTheCollection(note)) continue;

         if(kept.Count == 0 || !kept[^1].Equals(note)) kept.Add(note);
         if(index == _shownIndex) shownIndexInKept = kept.Count - 1;
      }

      _notes.Clear();
      _notes.AddRange(kept);
      _shownIndex = shownIndexInKept;
   }
}
