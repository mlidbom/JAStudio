using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.ParsingCases;

/// <summary>
/// One of the filters of the navigation pane's list of parsing cases (<see cref="ParsingCasesViewModel"/>): a toggle named for the cases it lets<br/>
/// through, such as "to review", which also says how many of all the parsing cases that is. The filters come in groups<br/>
/// (<see cref="ParsingCaseFilterGroupViewModel"/>).
/// </summary>
sealed partial class ParsingCaseFilterViewModel : ObservableObject
{
   readonly Func<ListedParsingCaseViewModel, bool> _letsThrough;
   readonly Action _whenSwitched;

   /// <param name="label">What the cases it lets through are, such as "to review".</param>
   /// <param name="letsThrough">Whether the filter lets a case through.</param>
   /// <param name="whenSwitched">Called when the filter is switched on or off, to list the cases the filters now let through.</param>
   public ParsingCaseFilterViewModel(string label, Func<ListedParsingCaseViewModel, bool> letsThrough, Action whenSwitched)
   {
      Label = label;
      _letsThrough = letsThrough;
      _whenSwitched = whenSwitched;
   }

   public string Label { get; }

   /// <summary>How many of all the parsing cases the filter lets through, whatever the other filters.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(Text))]
   public partial int Count { get; set; }

   /// <summary>The toggle's text: the <see cref="Label"/> and the <see cref="Count"/>, such as "to review (12)".</summary>
   public string Text => $"{Label} ({Count})";

   /// <summary>Whether the filter is on. Bound to its toggle.</summary>
   [ObservableProperty] public partial bool IsOn { get; set; }

   partial void OnIsOnChanged(bool value) => _whenSwitched();

   public bool LetsThrough(ListedParsingCaseViewModel listedCase) => _letsThrough(listedCase);

   /// <summary>Counts how many of <paramref name="allTheCases"/> the filter lets through.</summary>
   public void CountAmong(IEnumerable<ListedParsingCaseViewModel> allTheCases) => Count = allTheCases.Count(_letsThrough);
}
