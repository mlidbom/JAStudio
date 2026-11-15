using System.Collections.Generic;
using System.Linq;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.ParsingCases;

/// <summary>
/// A group of the filters of the navigation pane's list of parsing cases (<see cref="ParsingCasesViewModel"/>), such as one for each outcome.<br/>
/// While none of its filters is on, the group lets every case through; otherwise only the cases that one of its filters that are on lets through.
/// </summary>
sealed class ParsingCaseFilterGroupViewModel
{
   public ParsingCaseFilterGroupViewModel(params IReadOnlyList<ParsingCaseFilterViewModel> filters) => Filters = filters;

   /// <summary>The group's filters, in the order their toggles come in.</summary>
   public IReadOnlyList<ParsingCaseFilterViewModel> Filters { get; }

   public bool LetsThrough(ListedParsingCaseViewModel listedCase) => Filters.All(it => !it.IsOn) || Filters.Any(it => it.IsOn && it.LetsThrough(listedCase));

   /// <summary>Has each filter count how many of <paramref name="allTheCases"/> it lets through.</summary>
   public void CountAmong(IReadOnlyCollection<ListedParsingCaseViewModel> allTheCases)
   {
      foreach(var filter in Filters) filter.CountAmong(allTheCases);
   }
}
