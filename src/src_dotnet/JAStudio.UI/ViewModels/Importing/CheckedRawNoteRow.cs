using System;
using System.Linq;
using JAStudio.Core.Importing.Checking;

namespace JAStudio.UI.ViewModels.Importing;

/// <summary>A row of the import profiles window's report: one <see cref="CheckedRawNote"/>, its outcome and every finding behind it.</summary>
class CheckedRawNoteRow(CheckedRawNote checkedRawNote)
{
   public string Question { get; } = checkedRawNote.Question;
   public string ExternalId { get; } = checkedRawNote.ExternalId;
   public string Outcome { get; } = DescriptionOf(checkedRawNote.Outcome);

   /// <summary>The findings, a line each.</summary>
   public string Findings { get; } = string.Join(Environment.NewLine, checkedRawNote.Findings.Select(it => it.Description));

   public static string DescriptionOf(RawNoteOutcome outcome) => outcome switch
   {
      RawNoteOutcome.WillBeImported => "Will be imported",
      RawNoteOutcome.ImportedByAnEarlierRun => "Imported by an earlier run",
      RawNoteOutcome.AlreadyInJAStudio => "Skipped: already in JAStudio",
      RawNoteOutcome.Problem => "Problem",
      _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
   };
}
