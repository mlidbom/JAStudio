using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.ParsingCases;
using JAStudio.UI.Views.Workspace.Navigation.ParsingCases;
using JAStudio.Core.Note.Sentences.Tevents;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.ParsingCases;

/// <summary>
/// The "Parsing cases" tab of a workspace's <see cref="NavigationPaneViewModel"/> (<see cref="ParsingCasesView"/>): the corpus's parsing cases<br/>
/// (<see cref="ParsingCase"/>), each with how its sentence parses now compared with what the case expects, to find the ones that need a person:<br/>
/// those that fail, those to review, and those a person found broken. Filtered by outcome, verdict and text, and sorted by any column. A case's<br/>
/// sentence opens in the workspace's tabs.
/// </summary>
/// <remarks>
/// Every case is parsed and compared anew, in the background, each time the tab is shown and when it's refreshed<br/>
/// (<see cref="CompareTheCasesAsTheyParseNow"/>). Between those, a case is compared again whenever its sentence is updated, so a verdict given or a<br/>
/// result accepted shows at once. A change to a vocab that changes how a case's sentence parses shows only once the sentence is reparsed, or the<br/>
/// list is refreshed.
/// </remarks>
/// <remarks>
/// The cases listed stay listed until the filters change or the list is refreshed, each showing its case as it is now, even one the filters would<br/>
/// no longer let through: a case just accepted doesn't vanish from under the mouse.
/// </remarks>
partial class ParsingCasesViewModel : ObservableObject
{
   readonly WorkspaceViewModel _workspace;
   readonly JPCollection _collection;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Dictionary<NoteId, ListedParsingCaseViewModel> _casesBySentenceId = [];

   /// <summary>Whether the cases have been compared at least once: until the tab is first shown, nothing is listed, and nothing is kept up to date.</summary>
   bool _hasComparedTheCases;

   /// <summary>Whether a change to a sentence arrived while every case was being compared: the comparison may have read the sentence as it was before, so it runs once more.</summary>
   bool _compareAgainWhenCompared;

   public ParsingCasesViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services)
   {
      _workspace = workspace;
      _collection = services.CoreApp.Collection;
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();

      ByOutcome = new ParsingCaseFilterGroupViewModel(Filter(ParsingCaseOutcome.Fails.InWords(), it => it.Outcome == ParsingCaseOutcome.Fails),
                                                      Filter(ParsingCaseOutcome.IsToReview.InWords(), it => it.Outcome == ParsingCaseOutcome.IsToReview),
                                                      Filter(ParsingCaseOutcome.ParsesAsExpected.InWords(), it => it.Outcome == ParsingCaseOutcome.ParsesAsExpected));
      ByVerdict = new ParsingCaseFilterGroupViewModel(Filter(Verdict.Broken.InWords(), it => it.Verdict == Verdict.Broken),
                                                      Filter(Verdict.Unverified.InWords(), it => it.Verdict == Verdict.Unverified),
                                                      Filter(Verdict.Verified.InWords(), it => it.Verdict == Verdict.Verified));
      FollowTheChangesToTheSentencesIn(services);
   }

   /// <summary>The cases listed: those the filters let through when the filters last changed or the list was last refreshed.</summary>
   public ReplaceableObservableCollection<ListedParsingCaseViewModel> Cases { get; } = [];

   /// <summary>The case selected in the list, whose why and differences show below it.</summary>
   [ObservableProperty] public partial ListedParsingCaseViewModel? SelectedCase { get; set; }

   /// <summary>The filters by how a case's sentence parses compared with what the case expects.</summary>
   public ParsingCaseFilterGroupViewModel ByOutcome { get; }

   /// <summary>The filters by a person's verdict on the vocab the case expects the breakdown to display.</summary>
   public ParsingCaseFilterGroupViewModel ByVerdict { get; }

   /// <summary>Lists only the cases whose name, sentence or why contains this text, in any letter case. Empty lists them whatever their text.</summary>
   [ObservableProperty] public partial string TextFilter { get; set; } = "";

   partial void OnTextFilterChanged(string value) => _fatalErrorHandler.HandleExceptionsFrom(ListTheCasesTheFiltersLetThrough);

   /// <summary>How many cases are listed, of how many, or that they're being compared.</summary>
   [ObservableProperty] public partial string StatusText { get; set; } = "The parsing cases are compared when the tab is shown";

   /// <summary>Whether every case is being parsed and compared, in the background.</summary>
   [ObservableProperty] public partial bool IsComparing { get; set; }

   /// <summary>
   /// Parses every parsing case of the corpus now, in the background, compares each with what it expects, and lists those the filters let through.<br/>
   /// The rows of the cases still there stay, selected if they were.
   /// </summary>
   public void CompareTheCasesAsTheyParseNow()
   {
      if(IsComparing)
      {
         _compareAgainWhenCompared = true;
         return;
      }

      IsComparing = true;
      StatusText = "Parsing every parsing case...";
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         do
         {
            _compareAgainWhenCompared = false;
            Show(await TaskCE.Run(() => _collection.Sentences.ParsingCasesCompared()));
         } while(_compareAgainWhenCompared);

         IsComparing = false;
         ShowHowManyAreListed();
      });
   }

   /// <summary>Compares every case again, F5 or the Refresh button, and lists the cases the filters let through now.</summary>
   [RelayCommand]
   void Refresh() => CompareTheCasesAsTheyParseNow();

   /// <summary>Shows the selected case's sentence in the workspace, in the tab already showing it or else in a new tab, as Enter does.</summary>
   [RelayCommand]
   public void OpenTheSelectedCase()
   {
      if(SelectedCase != null) Open(SelectedCase);
   }

   /// <summary>Shows <paramref name="listedCase"/>'s sentence in the workspace, in the tab already showing it or else in a new tab, as a double-click does.</summary>
   public void Open(ListedParsingCaseViewModel listedCase) => _workspace.OpenNote(listedCase.Sentence);

   /// <summary>Opens <paramref name="listedCase"/>'s sentence in a new tab in the background, as middle-clicking a link does.</summary>
   public void OpenInBackgroundTab(ListedParsingCaseViewModel listedCase) => _workspace.OpenNoteInBackgroundTab(listedCase.Sentence);

   ParsingCaseFilterViewModel Filter(string label, Func<ListedParsingCaseViewModel, bool> letsThrough) =>
      new(label, letsThrough, whenSwitched: () => _fatalErrorHandler.HandleExceptionsFrom(ListTheCasesTheFiltersLetThrough));

   /// <summary>
   /// Keeps the rows showing their cases as they are now, on the UI thread: when a unit of work changed a sentence, its case is compared again;<br/>
   /// when one is deleted, its row goes; and after the collection is reloaded, which replaces every note object, every case is compared again.
   /// </summary>
   void FollowTheChangesToTheSentencesIn(TemporaryServiceCollection services) =>
      _ = new TeventsObservedOnTheUIThread(services, observe => observe
                                                               .ForTeventsOfEachUnitOfWork<ISentenceTevent>(ShowTheChangesOfAUnitOfWork)
                                                               .ForTevent<ICollectionReloaded>(_ =>
                                                                {
                                                                   if(_hasComparedTheCases) CompareTheCasesAsTheyParseNow();
                                                                }));

   /// <summary>After a unit of work changed or deleted the sentences that published <paramref name="changes"/>: shows each change (<see cref="ShowTheChangeTo"/>).</summary>
   void ShowTheChangesOfAUnitOfWork(IReadOnlyList<ISentenceTevent> changes)
   {
      foreach(var changed in changes.Select(it => it.NoteId).Distinct())
         if(_collection.Sentences.WithIdOrNone(changed) is {} sentence)
            ShowTheChangeTo(sentence);
         else
            StopListing(changed);
   }

   /// <summary>Shows <paramref name="compared"/>, every parsing case of the corpus, just compared, and lists those the filters let through.</summary>
   void Show(IReadOnlyList<ComparedParsingCase> compared)
   {
      var sentenceIdsCompared = compared.Select(it => it.Sentence.GetId()).ToHashSet();
      foreach(var noLongerACase in _casesBySentenceId.Keys.Where(it => !sentenceIdsCompared.Contains(it)).ToList())
         _casesBySentenceId.Remove(noLongerACase);

      foreach(var comparedCase in compared) ShowInItsRow(comparedCase);

      _hasComparedTheCases = true;
      CountTheCasesEachFilterLetsThrough();
      ListTheCasesTheFiltersLetThrough();
   }

   /// <summary>
   /// After <paramref name="sentence"/> was updated: compares its case again, in its row, gives it a row if it has just become a parsing case, and<br/>
   /// takes its row away if it no longer is one. A row listed stays listed, whatever the filters.
   /// </summary>
   void ShowTheChangeTo(SentenceNote sentence)
   {
      if(!_hasComparedTheCases) return;

      if(IsComparing)
      {
         _compareAgainWhenCompared = true;
         return;
      }

      if(sentence.ParsingCase == null)
      {
         StopListing(sentence.GetId());
         return;
      }

      var isNewlyACase = !_casesBySentenceId.ContainsKey(sentence.GetId());
      var row = ShowInItsRow(ComparedParsingCase.Of(sentence));
      if(isNewlyACase && TheFiltersLetThrough(row)) Cases.Add(row);

      CountTheCasesEachFilterLetsThrough();
      ShowHowManyAreListed();
   }

   /// <summary>Shows <paramref name="compared"/> in the row of its case, which is made if the case has none. The row.</summary>
   ListedParsingCaseViewModel ShowInItsRow(ComparedParsingCase compared)
   {
      if(_casesBySentenceId.TryGetValue(compared.Sentence.GetId(), out var row))
         row.Show(compared);
      else
         _casesBySentenceId.Add(compared.Sentence.GetId(), row = new ListedParsingCaseViewModel(compared));

      return row;
   }

   /// <summary>Takes away the row of the sentence with the id <paramref name="sentenceId"/>, if it has one: it was deleted, or is no longer a parsing case.</summary>
   void StopListing(NoteId sentenceId)
   {
      if(!_casesBySentenceId.Remove(sentenceId, out var row)) return;

      Cases.Remove(row);
      CountTheCasesEachFilterLetsThrough();
      ShowHowManyAreListed();
   }

   /// <summary>
   /// Lists exactly the cases the filters let through now. The rows already listed stay where they are, selected if they were: the others are taken<br/>
   /// away, and those missing added.
   /// </summary>
   void ListTheCasesTheFiltersLetThrough()
   {
      var toList = _casesBySentenceId.Values.Where(TheFiltersLetThrough).ToHashSet();
      if(Cases.Count == 0)
         Cases.ReplaceAllWith(toList);
      else
      {
         foreach(var notLetThrough in Cases.Where(it => !toList.Contains(it)).ToList()) Cases.Remove(notLetThrough);
         var listed = Cases.ToHashSet();
         foreach(var missing in toList.Where(it => !listed.Contains(it))) Cases.Add(missing);
      }

      ShowHowManyAreListed();
   }

   bool TheFiltersLetThrough(ListedParsingCaseViewModel listedCase) => ByOutcome.LetsThrough(listedCase) && ByVerdict.LetsThrough(listedCase) && TheTextFilterLetsThrough(listedCase);

   bool TheTextFilterLetsThrough(ListedParsingCaseViewModel listedCase)
   {
      return TextFilter.Length == 0 || ContainsTheText(listedCase.Name) || ContainsTheText(listedCase.Question) || ContainsTheText(listedCase.Why);

      bool ContainsTheText(string text) => text.Contains(TextFilter, StringComparison.OrdinalIgnoreCase);
   }

   void CountTheCasesEachFilterLetsThrough()
   {
      ByOutcome.CountAmong(_casesBySentenceId.Values);
      ByVerdict.CountAmong(_casesBySentenceId.Values);
   }

   void ShowHowManyAreListed()
   {
      if(IsComparing) return;

      var allTheCases = _casesBySentenceId.Count == 1 ? "1 parsing case" : $"{_casesBySentenceId.Count} parsing cases";
      StatusText = Cases.Count == _casesBySentenceId.Count ? allTheCases : $"{Cases.Count} of {allTheCases}";
   }
}
