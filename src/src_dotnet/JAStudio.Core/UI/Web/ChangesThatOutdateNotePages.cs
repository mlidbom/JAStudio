using System;
using System.Collections.Generic;
using Compze.Internals.SystemCE;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.UI.Web;

/// <summary>
/// The changes after which a page that shows a note no longer shows things as they are, so that it has to be shown anew. Such a page is the<br/>
/// page of one of the note's cards, which Anki shows, or the note's own page (<see cref="CardServerUrl"/>).
/// </summary>
/// <remarks>
/// The changes are: the note was updated or deleted, its study-note changed, such as its tags in Anki, the collection was loaded again, which<br/>
/// replaces every note, and a setting changed, such as how sentence breakdowns are shown.<br/>
/// A change to another note that the page shows something of, such as a vocab in a sentence's breakdown, isn't among them: it shows the next time<br/>
/// the page is shown.
/// </remarks>
public sealed class ChangesThatOutdateNotePages(JPCollection collection, JapaneseConfig config)
{
   readonly JPCollection _collection = collection;
   readonly JapaneseConfig _config = config;

   /// <summary>
   /// Calls <paramref name="thePageIsOutdated"/> after each change that outdates a page showing the note with the id <paramref name="shownNote"/>,<br/>
   /// on the thread that made the change, until what this returns is disposed.
   /// </summary>
   public IDisposable Subscribe(NoteId shownNote, Action thePageIsOutdated)
   {
      List<IDisposable> toEachKindOfChange =
      [
         _collection.NoteUpdated.Subscribe(ThePageIsOutdatedIfItShows),
         _collection.NoteDeleted.Subscribe(ThePageIsOutdatedIfItShows),
         _collection.StudyNoteChanged.Subscribe(ThePageIsOutdatedIfItShows),
         _collection.ReloadedFromBackend.Subscribe(_ => thePageIsOutdated()),
         _config.Changed.Subscribe(_ => thePageIsOutdated())
      ];
      return new Disposable(() => toEachKindOfChange.ForEach(it => it.Dispose()));

      void ThePageIsOutdatedIfItShows(JPNote changedNote)
      {
         if(changedNote.GetId().Equals(shownNote)) thePageIsOutdated();
      }
   }
}
