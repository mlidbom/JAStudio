using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.UI.Utils;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>
/// The notes of the type <typeparamref name="TNote"/> that a search found, each shown as a row of the type <typeparamref name="TSearchResult"/>,<br/>
/// such as <see cref="VocabSearchResultViewModel"/> for <c>VocabNote</c>.
/// </summary>
/// <remarks>
/// The rows are replaced in place, in the same <see cref="Results"/> collection, so the grid keeps the sort it has when the search changes,<br/>
/// and sorts the new rows the same way.
/// </remarks>
sealed class SearchResultsOfOneTypeViewModel<TNote, TSearchResult> : SearchResultsOfOneTypeViewModel
   where TNote : JPNote
   where TSearchResult : SearchResultViewModel
{
   readonly Func<TNote, TSearchResult> _rowFor;
   readonly Dictionary<NoteId, TSearchResult> _rowsByNoteId = [];

   /// <param name="workspace">The workspace the notes open in.</param>
   /// <param name="noteType">The type's name, as its tab shows it, such as "Vocab".</param>
   /// <param name="rowFor">Creates the row that shows a note.</param>
   public SearchResultsOfOneTypeViewModel(WorkspaceViewModel workspace, string noteType, Func<TNote, TSearchResult> rowFor) : base(workspace, noteType) =>
      _rowFor = rowFor;

   public ReplaceableObservableCollection<TSearchResult> Results { get; } = [];

   public override int Count => Results.Count;

   public override void Show(IReadOnlyList<JPNote> found)
   {
      Results.ReplaceAllWith(found.OfType<TNote>().Select(_rowFor));
      _rowsByNoteId.Clear();
      foreach(var row in Results)
         _rowsByNoteId.Add(row.Note.GetId(), row);
      OnTheNumberOfRowsChanged();
   }

   /// <remarks>The rows that stay read their values again, so a refresh shows every note as it is now, even a change no notification reported.</remarks>
   public override void ShowWhatTheRefreshedSearchFound(IReadOnlyList<JPNote> found)
   {
      var foundNow = found.OfType<TNote>().ToDictionary(it => it.GetId());
      foreach(var row in _rowsByNoteId.Values.ToList())
      {
         if(foundNow.TryGetValue(row.Note.GetId(), out var stillFound))
            row.ShowTheCurrentDataOf(stillFound);
         else
            Remove(row);
      }
      foreach(var newlyFound in foundNow.Values.Where(it => !_rowsByNoteId.ContainsKey(it.GetId())).ToList())
         AddARowFor(newlyFound);
      OnTheNumberOfRowsChanged();
   }

   public override void ShowTheChangeTo(JPNote updated)
   {
      if(_rowsByNoteId.TryGetValue(updated.GetId(), out var row))
         row.ShowTheCurrentDataOf(updated);
   }

   public override void ShowTheReloadedNotesOf(JPCollection collection)
   {
      foreach(var row in _rowsByNoteId.Values.ToList())
      {
         if(collection.NoteFromNoteId(row.Note.GetId()) is {} reloaded)
            row.ShowTheCurrentDataOf(reloaded);
         else
            Remove(row);
      }
      OnTheNumberOfRowsChanged();
   }

   void AddARowFor(TNote note)
   {
      var row = _rowFor(note);
      Results.Add(row);
      _rowsByNoteId.Add(note.GetId(), row);
   }

   void Remove(TSearchResult row)
   {
      Results.Remove(row);
      _rowsByNoteId.Remove(row.Note.GetId());
   }
}
