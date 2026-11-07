using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace.Navigation.SearchResults;
using Python.Runtime;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>
/// The notes a workspace's search found, below the search's condition tree (<see cref="SearchResultsView"/>):<br/>
/// a grid for each note type, with the type's columns, and a tab per type when the search found several.<br/>
/// Several notes can be selected, and the selection's menu acts on all of them at once, such as unsuspending all their cards.
/// </summary>
/// <remarks>
/// The type picked last stays shown when the search changes, as long as the search found notes of that type:<br/>
/// one usually knows which type of note one is after.
/// </remarks>
class SearchResultsViewModel : ObservableObject
{
   readonly TaskRunner _taskRunner;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<Task> _runTheSearchAgain;

   SearchResultsOfOneTypeViewModel? _shown;

   /// <summary>The type picked last in its tab. Null until one is picked.</summary>
   SearchResultsOfOneTypeViewModel? _pickedLast;

   /// <param name="workspace">The workspace the notes open in.</param>
   /// <param name="services">The services the selection's menu acts through.</param>
   /// <param name="runTheSearchAgain">Runs the search again after the selection's menu changed the notes, so the results show the change.</param>
   public SearchResultsViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services, Func<Task> runTheSearchAgain)
   {
      _taskRunner = services.TaskRunner;
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      _runTheSearchAgain = runTheSearchAgain;
      Vocab = new SearchResultsOfOneTypeViewModel<VocabSearchResultViewModel>(workspace, "Vocab");
      Kanji = new SearchResultsOfOneTypeViewModel<KanjiSearchResultViewModel>(workspace, "Kanji");
      Sentences = new SearchResultsOfOneTypeViewModel<SentenceSearchResultViewModel>(workspace, "Sentences");
      OfEachType = [Vocab, Kanji, Sentences];
   }

   public SearchResultsOfOneTypeViewModel<VocabSearchResultViewModel> Vocab { get; }
   public SearchResultsOfOneTypeViewModel<KanjiSearchResultViewModel> Kanji { get; }
   public SearchResultsOfOneTypeViewModel<SentenceSearchResultViewModel> Sentences { get; }

   /// <summary>The results of each type, in the order their tabs come in. A type without results has no tab.</summary>
   public IReadOnlyList<SearchResultsOfOneTypeViewModel> OfEachType { get; }

   /// <summary>Whether the search found notes of more than one type, so that each type gets a tab.</summary>
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

   /// <summary>Shows <paramref name="notes"/>, split by type, instead of the results shown so far.</summary>
   public void Show(IReadOnlyList<JPNote> notes)
   {
      Vocab.Show(notes.OfType<VocabNote>().Select(it => new VocabSearchResultViewModel(it)));
      Kanji.Show(notes.OfType<KanjiNote>().Select(it => new KanjiSearchResultViewModel(it)));
      Sentences.Show(notes.OfType<SentenceNote>().Select(it => new SentenceSearchResultViewModel(it)));
      OnPropertyChanged(nameof(ShowsSeveralTypes));
      ShowTheResultsOf(TypeToShow());
   }

   /// <summary>The menu of the <paramref name="selected"/> results: what can be done to all their notes at once, each picked by a single key.</summary>
   internal List<SpecMenuItem> SelectionMenu(IReadOnlyList<SearchResultViewModel> selected)
   {
      var withSuspendedCards = selected.Select(it => it.Note).Where(it => it.HasSuspendedCards()).ToList();
      var withActiveCards = selected.Select(it => it.Note).Where(it => it.HasActiveCards()).ToList();
      return
      [
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('u', "Unsuspend all cards"),
                              () => ChangeTheCardsOf(withSuspendedCards, "Unsuspending all cards", it => it.UnsuspendAllCards()),
                              enabled: withSuspendedCards.Count > 0),
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('s', "Suspend all cards"),
                              () => ChangeTheCardsOf(withActiveCards, "Suspending all cards", it => it.SuspendAllCards()),
                              enabled: withActiveCards.Count > 0)
      ];
   }

   /// <summary>
   /// Does <paramref name="changeTheCardsOfANote"/> to each of <paramref name="notes"/> in the background, showing its progress as <paramref name="whatIsDone"/>,<br/>
   /// and then runs the search again, so the results show the change, and drop the notes the search no longer finds.
   /// </summary>
   void ChangeTheCardsOf(List<JPNote> notes, string whatIsDone, Action<JPNote> changeTheCardsOfANote) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         await TaskCE.Run(() =>
         {
            using(var scope = _taskRunner.Current(whatIsDone))
               scope.RunBatch(notes, changeTheCardsOfANote, whatIsDone);
            ShowTheChangeInAnki();
         });
         await _runTheSearchAgain();
      });

   /// <summary>Refreshes Anki's own windows, such as its browser, as the note menus do after they change cards. There's no Anki when JAStudio runs without it.</summary>
   static void ShowTheChangeInAnki()
   {
      if(PythonEngine.IsInitialized)
         AnkiFacade.UIUtils.Refresh();
   }

   void ShowTheResultsOf(SearchResultsOfOneTypeViewModel? type)
   {
      SetProperty(ref _shown, type, nameof(Shown));
      foreach(var each in OfEachType)
         each.IsShown = each == type;
   }

   /// <summary>The type picked last if the search found notes of it, or else the first type the search found notes of.</summary>
   SearchResultsOfOneTypeViewModel? TypeToShow() => _pickedLast is { HasResults: true } ? _pickedLast : OfEachType.FirstOrDefault(it => it.HasResults);
}
