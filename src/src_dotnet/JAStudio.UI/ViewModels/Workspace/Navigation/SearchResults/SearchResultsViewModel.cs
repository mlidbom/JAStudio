using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Views.Workspace.Navigation.SearchResults;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>
/// The notes a workspace's search found, below the search's condition tree (<see cref="SearchResultsView"/>):<br/>
/// a grid for each note type, with the type's columns, and a tab per type when the search found several.<br/>
/// Several notes can be selected, and <see cref="ActionsOnTheSelected"/> act on all of them at once, such as adding a tag.
/// </summary>
/// <remarks>
/// The results keep showing the same notes until the search changes or is refreshed, so nothing disappears while one works on them.<br/>
/// Each row shows its note as it is now: it's updated whenever its note is, whatever updated it, and after the collection is reloaded.
/// </remarks>
/// <remarks>
/// The type picked last stays shown when the search changes, as long as the search found notes of that type:<br/>
/// one usually knows which type of note one is after.<br/>
/// The results follow the collection for as long as they live, which is as long as their workspace.
/// </remarks>
class SearchResultsViewModel : ObservableObject
{
   SearchResultsOfOneTypeViewModel? _shown;

   /// <summary>The type picked last in its tab. Null until one is picked.</summary>
   SearchResultsOfOneTypeViewModel? _pickedLast;

   /// <param name="workspace">The workspace the notes open in.</param>
   /// <param name="services">The collection the notes are in, and the services the selection's menu acts through.</param>
   public SearchResultsViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services)
   {
      ActionsOnTheSelected = new ActionsOnSelectedResults(services);
      Vocab = new SearchResultsOfOneTypeViewModel<VocabNote, VocabSearchResultViewModel>(workspace, "Vocab", it => new VocabSearchResultViewModel(it));
      Kanji = new SearchResultsOfOneTypeViewModel<KanjiNote, KanjiSearchResultViewModel>(workspace, "Kanji", it => new KanjiSearchResultViewModel(it));
      Sentences = new SearchResultsOfOneTypeViewModel<SentenceNote, SentenceSearchResultViewModel>(workspace, "Sentences", it => new SentenceSearchResultViewModel(it));
      OfEachType = [Vocab, Kanji, Sentences];
      FollowTheChangesToTheNotesIn(services);
   }

   public SearchResultsOfOneTypeViewModel<VocabNote, VocabSearchResultViewModel> Vocab { get; }
   public SearchResultsOfOneTypeViewModel<KanjiNote, KanjiSearchResultViewModel> Kanji { get; }
   public SearchResultsOfOneTypeViewModel<SentenceNote, SentenceSearchResultViewModel> Sentences { get; }

   /// <summary>The results of each type, in the order their tabs come in. A type without results has no tab.</summary>
   public IReadOnlyList<SearchResultsOfOneTypeViewModel> OfEachType { get; }

   /// <summary>Whether notes of more than one type are shown, so that each type gets a tab.</summary>
   public bool ShowsSeveralTypes => OfEachType.Count(it => it.HasResults) > 1;

   /// <summary>The type whose results are shown. Set by its tab when the tab is clicked.</summary>
   public SearchResultsOfOneTypeViewModel? Shown
   {
      get => _shown;
      set
      {
         ShowTheResultsOf(value);
         _pickedLast = value;
      }
   }

   /// <summary>Shows <paramref name="found"/>, the notes a changed search found, split by type, instead of the results shown so far.</summary>
   public void Show(IReadOnlyList<JPNote> found) => ShowEachType(it => it.Show(found));

   /// <summary>
   /// Shows <paramref name="found"/>, the notes the same search found when it was refreshed: the rows of the notes still found stay,<br/>
   /// where they are and selected if they were, the rows of the notes no longer found go, and the newly found notes get rows.
   /// </summary>
   public void ShowWhatTheRefreshedSearchFound(IReadOnlyList<JPNote> found) => ShowEachType(it => it.ShowWhatTheRefreshedSearchFound(found));

   /// <summary>What can be done to the notes of the selected results, all at once, such as adding a tag. The view passes the selection.</summary>
   public ActionsOnSelectedResults ActionsOnTheSelected { get; }

   /// <summary>
   /// Keeps the rows showing their notes as they are now: when a note is updated, or its studying status changes, on whatever thread,<br/>
   /// its row is updated on the UI thread, and when a note is deleted, its row goes. After the collection is reloaded, every row shows the object<br/>
   /// that replaced its note.
   /// </summary>
   void FollowTheChangesToTheNotesIn(TemporaryServiceCollection services)
   {
      var collection = services.CoreApp.Collection;
      var uiThread = services.Resolve<IUIThreadDispatcher>();
      collection.NoteUpdated.Subscribe(ShowTheChangeTo);
      collection.StudyNoteChanged.Subscribe(ShowTheChangeTo);
      collection.NoteDeleted.Subscribe(deleted => uiThread.PostToUIThread(() => ShowEachType(it => it.ShowThatItWasDeleted(deleted))));
      collection.ReloadedFromBackend.Subscribe(_ => uiThread.PostToUIThread(() => ShowEachType(it => it.ShowTheReloadedNotesOf(collection))));
      return;

      void ShowTheChangeTo(JPNote changed) => uiThread.PostToUIThread(() => ForEachType(it => it.ShowTheChangeTo(changed)));
   }

   /// <summary>Changes the rows of each type with <paramref name="changeTheRows"/>, and then shows the tabs and the type that fit the rows now.</summary>
   void ShowEachType(Action<SearchResultsOfOneTypeViewModel> changeTheRows)
   {
      ForEachType(changeTheRows);
      OnPropertyChanged(nameof(ShowsSeveralTypes));
      ShowTheResultsOf(TypeToShow());
   }

   void ForEachType(Action<SearchResultsOfOneTypeViewModel> action)
   {
      foreach(var type in OfEachType)
         action(type);
   }

   void ShowTheResultsOf(SearchResultsOfOneTypeViewModel? type)
   {
      SetProperty(ref _shown, type, nameof(Shown));
      foreach(var each in OfEachType)
         each.IsShown = each == type;
   }

   /// <summary>
   /// The type picked last if notes of it are shown, or else the type shown so far if notes of it still are, or else the first type with notes shown.<br/>
   /// So a refresh that finds a vocab doesn't switch away from the sentences being worked on.
   /// </summary>
   SearchResultsOfOneTypeViewModel? TypeToShow() =>
      new[] { _pickedLast, _shown }.FirstOrDefault(it => it is { HasResults: true }) ?? OfEachType.FirstOrDefault(it => it.HasResults);
}
