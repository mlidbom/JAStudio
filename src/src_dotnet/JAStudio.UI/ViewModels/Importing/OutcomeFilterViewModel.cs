using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Importing.Checking;

namespace JAStudio.UI.ViewModels.Importing;

/// <summary>
/// A check box above the import report that shows or hides the rows of one <see cref="RawNoteOutcome"/>. It shows the outcome's <see cref="ReportMark"/>,<br/>
/// its name and how many raw notes have it, so the check boxes are also the legend of the outcome marks.
/// </summary>
class OutcomeFilterViewModel(RawNoteOutcome outcome, int count, Action showTheRowsOfTheShownOutcomes) : ObservableObject
{
   readonly Action _showTheRowsOfTheShownOutcomes = showTheRowsOfTheShownOutcomes;

   public RawNoteOutcome Outcome { get; } = outcome;
   public ReportMark Mark { get; } = ReportMark.Of(outcome);
   public string Description { get; } = $"{ReportMark.NameOf(outcome)} ({count})";

   /// <summary>Whether the report shows the rows of <see cref="Outcome"/>. Every outcome is shown after each check.</summary>
   public bool IsShown
   {
      get;
      set
      {
         if(SetProperty(ref field, value)) _showTheRowsOfTheShownOutcomes();
      }
   } = true;
}
